using System.Data;
using Dapper;
using Models;
using Models.Errors;
using Repositories.Interfaces;

namespace Repositories
{
    public class ReservationRepository : IReservationRepository
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
            // Check for overlapping reservations
            var hasConflict = await HasConflict(newReservation);
            if (hasConflict)
            {
                throw new InvalidOperationException("Room is already booked for these dates");
            }

            var createdReservation = await _db.QuerySingleAsync<ReservationDb>(
                @"INSERT INTO Reservations(Id, RoomNumber, GuestEmail, Start, End, CheckedIn, CheckedOut) 
                  Values(@Id, @RoomNumber, @GuestEmail, @Start, @End, @CheckedIn, @CheckedOut) 
                  RETURNING *",
                new ReservationDb(newReservation)
            );

            return createdReservation.ToDomain();
        }

        /// <summary>
        /// Check if a reservation conflicts with existing reservations for the same room
        /// </summary>
        public async Task<bool> HasConflict(Reservation reservation)
        {
            var roomNumberInt = Room.ConvertRoomNumberToInt(reservation.RoomNumber);

            // Overlap formula: (newStart < existingEnd) AND (newEnd > existingStart)
            var existingReservations = await _db.QueryAsync<ReservationDb>(
                @"SELECT * FROM Reservations 
                  WHERE RoomNumber = @roomNumberInt 
                  AND Start < @endDate 
                  AND End > @startDate",
                new
                {
                    roomNumberInt,
                    startDate = reservation.Start,
                    endDate = reservation.End,
                }
            );

            return existingReservations.Any();
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
            var sql = "SELECT * FROM Reservations WHERE End >= date('now') ORDER BY Start ASC";
            return await _db.QueryAsync<Reservation>(sql);
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
                    CheckedOut = CheckedOut,
                };
            }
        }
    }
}
