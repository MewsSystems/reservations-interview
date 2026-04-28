using System.Data;
using Dapper;
using Models;
using Models.Errors;
using Validators;

namespace Repositories
{
    public class ReservationRepository
    {
        private IDbConnection _db { get; set; }

        public ReservationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reservation>> GetReservations()
        {
            var reservations = await _db.QueryAsync<ReservationDb>("SELECT * FROM Reservations");

            if (reservations == null)
            {
                return [];
            }

            return reservations.Select(r => r.ToDomain());
        }

        public async Task<IEnumerable<Reservation>> GetUpcomingReservations()
        {
            var today = DateTime.UtcNow.Date;

            var reservations = await _db.QueryAsync<ReservationDb>(
                "SELECT * FROM Reservations WHERE End >= @today ORDER BY Start ASC;",
                new { today }
            );

            return reservations?.Select(r => r.ToDomain()) ?? [];
        }

        /// <summary>
        /// Find a reservation by its Guid ID, throwing if not found
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns cref="Reservation">An existing reservation</returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Reservation> GetReservation(Guid reservationId)
        {
            var reservation = await _db.QueryFirstOrDefaultAsync<ReservationDb>(
                "SELECT * FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            if (reservation == null)
            {
                throw new NotFoundException($"Reservation {reservationId} not found");
            }

            return reservation.ToDomain();
        }

        public async Task<Reservation> CreateReservation(Reservation newReservation)
        {
            await CheckForConflict(newReservation);

            var db = new ReservationDb(newReservation);

            var created = await _db.QuerySingleAsync<ReservationDb>(
                @"INSERT INTO Reservations (Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                  VALUES (@Id, @GuestEmail, @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut)
                  RETURNING *;",
                db
            );

            return created.ToDomain();
        }

        private async Task CheckForConflict(Reservation newReservation)
        {
            var conflict = await _db.QueryFirstOrDefaultAsync<ReservationDb>(
                @"SELECT * FROM Reservations
                  WHERE RoomNumber = @RoomNumber
                  AND Start < @End
                  AND End > @Start
                  LIMIT 1;",
                new
                {
                    RoomNumber = Room.ConvertRoomNumberToInt(newReservation.RoomNumber),
                    newReservation.Start,
                    newReservation.End
                }
            );

            ReservationConflictValidator.ValidateNoConflict(newReservation, conflict != null);
        }

        public async Task<bool> DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            return deleted > 0;
        }

        public async Task<Reservation> CheckIn(Guid reservationId, string guestEmail)
        {
            var reservation = await GetReservation(reservationId);

            if (!string.Equals(reservation.GuestEmail, guestEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Email does not match reservation.");
            }

            if (reservation.CheckedOut)
            {
                throw new InvalidOperationException("Reservation has already been checked out.");
            }

            if (reservation.CheckedIn)
            {
                throw new InvalidOperationException("Reservation is already checked in.");
            }

            var updated = await _db.QuerySingleAsync<ReservationDb>(
                "UPDATE Reservations SET CheckedIn = 1 WHERE Id = @id RETURNING *;",
                new { id = reservationId.ToString() }
            );

            return updated.ToDomain();
        }

        /// <summary>
        /// Validates the check-in conditions against the already-loaded <paramref name="reservation"/>,
        /// then atomically marks the reservation as checked-in and sets the room state to Occupied
        /// inside a single transaction, avoiding partial-update inconsistencies.
        /// </summary>
        public async Task<Reservation> CheckInWithRoomUpdate(Reservation reservation, string guestEmail)
        {
            if (!string.Equals(reservation.GuestEmail, guestEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Email does not match reservation.");
            }

            if (reservation.CheckedOut)
            {
                throw new InvalidOperationException("Reservation has already been checked out.");
            }

            if (reservation.CheckedIn)
            {
                throw new InvalidOperationException("Reservation is already checked in.");
            }

            if (_db.State != System.Data.ConnectionState.Open)
            {
                _db.Open();
            }

            using var tx = _db.BeginTransaction();

            try
            {
                var updated = await _db.QuerySingleAsync<ReservationDb>(
                    "UPDATE Reservations SET CheckedIn = 1 WHERE Id = @id RETURNING *;",
                    new { id = reservation.Id.ToString() },
                    tx
                );
                var roomNumberInt = Room.ConvertRoomNumberToInt(reservation.RoomNumber);
                var updatedRooms = await _db.ExecuteAsync(
                           "UPDATE Rooms SET State = @state WHERE Number = @roomNumberInt;",
                           new { state = Models.State.Occupied, roomNumberInt },
                           tx
                       );
                if (updatedRooms != 1)
                {
                    throw new InvalidOperationException("Room update failed for reservation check-in.");
                }
                tx.Commit();
                return updated.ToDomain();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private class ReservationDb
        {
            public string Id { get; set; }
            public int RoomNumber { get; set; }

            public string GuestEmail { get; set; }

            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public bool CheckedIn { get; set; }
            public bool CheckedOut { get; set; }

            public ReservationDb()
            {
                Id = Guid.Empty.ToString();
                RoomNumber = 0;
                GuestEmail = "";
            }

            public ReservationDb(Reservation reservation)
            {
                Id = reservation.Id.ToString();
                RoomNumber = Room.ConvertRoomNumberToInt(reservation.RoomNumber);
                GuestEmail = reservation.GuestEmail;
                Start = reservation.Start;
                End = reservation.End;
                CheckedIn = reservation.CheckedIn;
                CheckedOut = reservation.CheckedOut;
            }

            public Reservation ToDomain()
            {
                return new Reservation
                {
                    Id = Guid.Parse(Id),
                    RoomNumber = Room.FormatRoomNumber(RoomNumber),
                    GuestEmail = GuestEmail,
                    Start = Start,
                    End = End,
                    CheckedIn = CheckedIn,
                    CheckedOut = CheckedOut
                };
            }
        }
    }
}
