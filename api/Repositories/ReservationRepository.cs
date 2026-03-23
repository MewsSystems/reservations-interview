using System.Data;
using Dapper;
using Models;
using Contracts;
using Models.Errors;
using Extensions;

namespace Repositories
{
    public class ReservationRepository
    {
        private IDbConnection _db { get; set; }

        public ReservationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reservation>> GetReservations(
            DateTime? from = null,
            DateTime? to = null,
            string? roomNumber = null,
            string? guestEmail = null)
        {
            var sql = "SELECT * FROM Reservations WHERE 1=1";
            var parameters = new DynamicParameters();

            if (from.HasValue)
            {
                sql += " AND [End] >= @From";
                parameters.Add("From", from.Value);
            }

            if (to.HasValue)
            {
                sql += " AND Start <= @To";
                parameters.Add("To", to.Value);
            }

            if (!string.IsNullOrEmpty(roomNumber))
            {
                sql += " AND RoomNumber = @RoomNumber";
                parameters.Add("RoomNumber", roomNumber);
            }

            if (!string.IsNullOrEmpty(guestEmail))
            {
                sql += " AND GuestEmail LIKE @GuestEmail";
                parameters.Add("GuestEmail", $"%{guestEmail}%");
            }

            sql += " ORDER BY Start";

            var reservations = await _db.QueryAsync<Reservation>(sql, parameters);
            return reservations ?? [];
        }

        /// <summary>
        /// Find a reservation by its Guid ID, throwing if not found
        /// </summary>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Reservation> GetReservation(Guid reservationId)
        {
            var reservation = await _db.QueryFirstOrDefaultAsync<Reservation>(
                "SELECT * FROM Reservations WHERE Id = @reservationId;",
                new { reservationId }
            );

            if (reservation == null)
            {
                throw new NotFoundException(nameof(Reservation), reservationId.ToString());
            }

            return reservation;
        }

        public async Task<Reservation> CreateReservation(ReservationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            

            using var transaction = _db.BeginSerializableTransaction();

            try
            {
                var room = await _db.QueryFirstOrDefaultAsync<Room>(
                    "SELECT * FROM Rooms WHERE Number = @RoomNumber;",
                    new { request.RoomNumber },
                    transaction
                ) ?? throw new NotFoundException(nameof(Room), request.RoomNumber);

                var conflict = await _db.QueryFirstOrDefaultAsync<Reservation>(
                    @"SELECT * FROM Reservations
                      WHERE RoomNumber = @RoomNumber
                      AND Start < @End
                      AND [End] > @Start",
                    new { request.RoomNumber, request.Start, request.End },
                    transaction
                );

                if (conflict != null)
                {
                    throw new ConflictException(
                        nameof(Reservation),
                        request.RoomNumber,
                        $"Room {request.RoomNumber} is already booked from {conflict.Start:yyyy-MM-dd} to {conflict.End:yyyy-MM-dd}"
                    );
                }

                await _db.ExecuteAsync(
                    "INSERT OR IGNORE INTO Guests(Email, Name) VALUES(@GuestEmail, @GuestEmail)",
                    new { request.GuestEmail },
                    transaction
                );

                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    RoomNumber = request.RoomNumber,
                    GuestEmail = request.GuestEmail,
                    Start = request.Start,
                    End = request.End
                };

                var created = await _db.QuerySingleAsync<Reservation>(
                    @"INSERT INTO Reservations(Id, GuestEmail, RoomNumber, Start, End)
                      VALUES(@Id, @GuestEmail, @RoomNumber, @Start, @End)
                      RETURNING *",
                    reservation,
                    transaction
                );

                transaction.Commit();
                return created;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Reservation> CheckIn(Guid reservationId, string guestEmail)
        {
            var reservation = await GetReservation(reservationId);

            if (!string.Equals(reservation.GuestEmail, guestEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(nameof(Reservation), reservationId.ToString(), "Guest email does not match the reservation");
            }

            if (reservation.CheckedIn)
            {
                throw new ConflictException(nameof(Reservation), reservationId.ToString(), "Reservation is already checked in");
            }

            if (reservation.Start > DateTime.Today || reservation.End <= DateTime.Today)
            {
                throw new ValidationException(nameof(Reservation), reservationId.ToString(), "Check-in is only allowed during the reservation period");
            }

            var room = await _db.QueryFirstOrDefaultAsync<Room>(
                "SELECT * FROM Rooms WHERE Number = @RoomNumber",
                new { reservation.RoomNumber }
            );

            if (room != null && room.State == State.Dirty)
            {
                throw new ValidationException(nameof(Reservation), reservationId.ToString(), "Cannot check in to a dirty room. Room must be cleaned first");
            }

            using var transaction = _db.BeginSerializableTransaction();
            try
            {
                await _db.ExecuteAsync(
                    "UPDATE Reservations SET CheckedIn = 1 WHERE Id = @reservationId",
                    new { reservationId },
                    transaction
                );

                await _db.ExecuteAsync(
                    "UPDATE Rooms SET State = @State WHERE Number = @RoomNumber",
                    new { State = (int)State.Occupied, reservation.RoomNumber },
                    transaction
                );

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            reservation.CheckedIn = true;
            return reservation;
        }

        public async Task<Reservation> CheckOut(Guid reservationId, string guestEmail)
        {
            var reservation = await GetReservation(reservationId);

            if (!string.Equals(reservation.GuestEmail, guestEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(nameof(Reservation), reservationId.ToString(), "Guest email does not match the reservation");
            }

            if (!reservation.CheckedIn)
            {
                throw new ValidationException(nameof(Reservation), reservationId.ToString(), "Guest has not checked in yet");
            }

            if (reservation.CheckedOut)
            {
                throw new ConflictException(nameof(Reservation), reservationId.ToString(), "Reservation is already checked out");
            }

            var checkedOutAt = DateTime.Now;

            using var transaction = _db.BeginSerializableTransaction();
            try
            {
                await _db.ExecuteAsync(
                    "UPDATE Reservations SET CheckedOut = 1, CheckedOutAt = @checkedOutAt WHERE Id = @reservationId",
                    new { reservationId, checkedOutAt },
                    transaction
                );

                await _db.ExecuteAsync(
                    "UPDATE Rooms SET State = @State WHERE Number = @RoomNumber",
                    new { State = (int)State.Dirty, reservation.RoomNumber },
                    transaction
                );

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            reservation.CheckedOut = true;
            reservation.CheckedOutAt = checkedOutAt;
            return reservation;
        }

        public async Task DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationId;",
                new { reservationId }
            );

            if (deleted == 0)
            {
                throw new NotFoundException(nameof(Reservation), reservationId.ToString());
            }
        }
    }
}
