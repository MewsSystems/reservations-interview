using System.Data;
using Dapper;
using Models;
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

        public async Task<IEnumerable<Reservation>> GetRoomReservations(string roomNumber)
        {
            var reservations = await _db.QueryAsync<ReservationDb>("SELECT * FROM Reservations WHERE RoomNumber = @roomNumber", new { roomNumber });

            return reservations.Select(r => r.ToDomain());
        }

        public async Task<IEnumerable<Reservation>> GetUpcomingReservations()
        {
            var today = DateTime.UtcNow.Date;
            var query = """
                SELECT * 
                FROM Reservations 
                WHERE Start >= @today
                ORDER BY Start
                """;
            var reservations = await _db.QueryAsync<ReservationDb>(query, new {today});

            return reservations.Select(r => r.ToDomain());
        }

        public async Task<Reservation> CreateReservation(Reservation newReservation)
        {
            const string queryCheckOverlap = """
                SELECT COUNT(*) FROM Reservations 
                WHERE RoomNumber = @RoomNumber 
                AND Start < @End 
                AND End > @Start;
                """;
            const string queryInsert = """
                INSERT INTO Reservations (Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut) 
                VALUES (@Id, @GuestEmail, @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut)
                RETURNING *
                """;

            _db.Open();
            using var transaction = _db.BeginTransaction();
            try
            {
                var reservationDb = new ReservationDb(newReservation);

                var overlapCount = await _db.QuerySingleAsync<int>(queryCheckOverlap, reservationDb, transaction);

                if (overlapCount > 0)
                {
                    throw new ReservationConflictException($"Room {newReservation.RoomNumber} is already reserved for the specified time period");
                }

                var createdReservation = await _db.QuerySingleAsync<ReservationDb>(queryInsert, reservationDb, transaction);

                transaction.Commit();

                return createdReservation.ToDomain();
            }
            catch
            {
                transaction.Rollback();
                throw;
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
