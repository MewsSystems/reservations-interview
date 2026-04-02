using System.Data;
using Dapper;
using Models;
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

        /// <summary>
        /// Returns reservations with optional date filter and offset-based pagination.
        /// When <paramref name="from"/> is provided, only reservations whose End &gt;= from are returned, ordered by Start ASC.
        /// </summary>
        public async Task<(IEnumerable<Reservation> Items, int TotalCount)> GetReservations(
            DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var conditions = new List<string>();
            if (from.HasValue) conditions.Add("[End] >= @from");
            if (to.HasValue) conditions.Add("[Start] <= @to");
            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
            var orderClause = from.HasValue || to.HasValue ? "ORDER BY [Start] ASC" : "";
            var offset = (page - 1) * pageSize;

            var totalCount = await _db.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM Reservations {whereClause}",
                new { from, to }
            );

            var reservations = await _db.QueryAsync<ReservationDb>(
                $"SELECT * FROM Reservations {whereClause} {orderClause} LIMIT @pageSize OFFSET @offset",
                new { from, to, pageSize, offset }
            );

            return (reservations?.Select(r => r.ToDomain()) ?? [], totalCount);
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

        /// <summary>
        /// Atomically checks for overlapping reservations and inserts a new one inside a transaction.
        /// Throws <see cref="ValidationException"/> if the room is already booked for the selected dates.
        /// Uses strict inequality so same-day checkout/checkin is allowed.
        /// </summary>
        public async Task<Reservation> CreateReservation(Reservation newReservation)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using var txn = _db.BeginTransaction();

            var dbModel = new ReservationDb(newReservation);

            // Check for overlap inside the transaction
            var hasOverlap = await _db.ExecuteScalarAsync<bool>(
                @"SELECT EXISTS(
                    SELECT 1 FROM Reservations
                    WHERE RoomNumber = @RoomNumber
                      AND [Start] < @End
                      AND [End] > @Start
                    LIMIT 1
                  )",
                new { dbModel.RoomNumber, dbModel.Start, dbModel.End },
                transaction: txn
            );

            if (hasOverlap)
            {
                txn.Rollback();
                throw new ValidationException(
                    $"Room {newReservation.RoomNumber} is already booked for the selected dates."
                );
            }

            var created = await _db.QuerySingleAsync<ReservationDb>(
                @"INSERT INTO Reservations(Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                  VALUES(@Id, @GuestEmail, @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut)
                  RETURNING *",
                dbModel,
                transaction: txn
            );

            txn.Commit();
            return created.ToDomain();
        }

        /// <summary>
        /// Atomically sets CheckedIn = 1, room State = Occupied, and IsDirty = 1 inside a transaction.
        /// The UPDATE uses WHERE CheckedIn = 0 as a guard against concurrent check-ins.
        /// Returns false if the reservation was already checked in (no rows updated).
        /// </summary>
        public async Task<bool> CheckIn(Guid reservationId)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using var txn = _db.BeginTransaction();

            var updated = await _db.ExecuteAsync(
                "UPDATE Reservations SET CheckedIn = 1 WHERE Id = @id AND CheckedIn = 0;",
                new { id = reservationId.ToString() },
                transaction: txn
            );

            if (updated == 0)
            {
                txn.Rollback();
                return false;
            }

            // Get room number to update its state
            var roomNumber = await _db.ExecuteScalarAsync<int>(
                "SELECT RoomNumber FROM Reservations WHERE Id = @id;",
                new { id = reservationId.ToString() },
                transaction: txn
            );

            await _db.ExecuteAsync(
                "UPDATE Rooms SET State = @state, IsDirty = 1 WHERE Number = @roomNumber;",
                new { state = (int)State.Occupied, roomNumber },
                transaction: txn
            );

            txn.Commit();
            return true;
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
                RoomNumber = RoomExtensions.ConvertRoomNumberToInt(reservation.RoomNumber);
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
                    RoomNumber = RoomExtensions.FormatRoomNumber(RoomNumber),
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
