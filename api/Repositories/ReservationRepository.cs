using System.Data;
using Dapper;
using Models;
using Models.Database;
using Models.Errors;

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
                throw new NotFoundException($"Room {reservationId} not found");
            }

            return reservation.ToDomain();
        }

        public async Task<Reservation> CreateReservation(Reservation newReservation)
        {

            if (newReservation.Id == Guid.Empty)
            {
                newReservation.Id = Guid.NewGuid();
            }

            if (_db is Microsoft.Data.Sqlite.SqliteConnection sqliteConnection)
            {
                if (sqliteConnection.State != ConnectionState.Open)
                {
                    await sqliteConnection.OpenAsync();
                }
            }
            else
            {
                throw new InvalidOperationException("Database connection is not of type SQLiteConnection.");
            }

            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    var guestExists = await _db.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM Guests WHERE Email = @GuestEmail",
                        new { GuestEmail = newReservation.GuestEmail }
                    );

                    if (guestExists == 0)
                    {
                        await _db.ExecuteAsync(
                            "INSERT INTO Guests (Email, Name) VALUES (@Email, @Name)",
                            new { Email = newReservation.GuestEmail, Name = newReservation.GuestEmail }
                        );
                    }

                    var roomExists = await _db.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM Rooms WHERE Number = @RoomNumber",
                        new { RoomNumber = Room.ConvertRoomNumberToInt(newReservation.RoomNumber) }
                    );

                    if (roomExists == 0)
                    {
                        throw new Exception("Room not found.");
                    }

                    var result = await _db.ExecuteAsync(
                        @"
                        INSERT INTO Reservations (Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                        VALUES (@Id, @GuestEmail, @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut);
                        ",
                        new
                        {
                            Id = newReservation.Id.ToString(),
                            GuestEmail = newReservation.GuestEmail,
                            RoomNumber = Room.ConvertRoomNumberToInt(newReservation.RoomNumber),
                            Start = newReservation.Start,
                            End = newReservation.End,
                            CheckedIn = newReservation.CheckedIn,
                            CheckedOut = newReservation.CheckedOut
                        },
                        transaction: transaction
                    );

                    if (result == 0)
                    {
                        throw new Exception("Failed to insert the reservation.");
                    }

                    transaction.Commit();

                    return new Reservation
                    {
                        Id = newReservation.Id,
                        RoomNumber = newReservation.RoomNumber,
                        GuestEmail = newReservation.GuestEmail,
                        Start = newReservation.Start,
                        End = newReservation.End,
                        CheckedIn = newReservation.CheckedIn,
                        CheckedOut = newReservation.CheckedOut
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"An error occurred while creating the reservation: {ex.Message}", ex);
                }
            }
        }

        public async Task<bool> DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            return deleted > 0;
        }

        public async Task<IEnumerable<Reservation>> GetUpcomingReservations()
        {
            var now = DateTime.UtcNow;
            var query = @"
                SELECT * 
                FROM Reservations 
                WHERE Start > @Now
                ORDER BY Start ASC
            ";

            var reservations = await _db.QueryAsync<ReservationDb>(query, new { Now = now });

            if (reservations == null || !reservations.Any())
            {
                return new List<Reservation>();
            }

            return reservations.Select(r => r.ToDomain());
        }
    }
}
