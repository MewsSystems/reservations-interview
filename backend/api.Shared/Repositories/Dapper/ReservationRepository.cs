using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using api.Shared.Extensions;
using api.Shared.Models.DB;
using api.Shared.Models.Errors;
using Dapper;

namespace api.Shared.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly IDbConnection _db;

        public ReservationRepository(IDbConnection db)
        {
            _db = db.ThrowIfNull(nameof(IDbConnection));
        }

        public async Task<IEnumerable<Reservation>> GetReservations()
        {
            var reservations = await _db.QueryAsync<Reservation>("SELECT * FROM Reservations");

            if (reservations == null)
            {
                return [];
            }

            return reservations;
        }

        public async Task<IEnumerable<ReservationWithRoomState>> GetStaffReservations()
        {
            var reservations = await _db.QueryAsync<ReservationWithRoomState>(@"
                SELECT Reservations.*, Rooms.State 
                FROM Reservations 
                JOIN Rooms on Rooms.Number = Reservations.RoomNumber 
                WHERE Start >= DATE('now')
            ");

            if (reservations == null)
            {
                return [];
            }

            return reservations;
        }

        /// <inheritdoc/>
        public async Task<Reservation> GetReservation(Guid reservationId)
        {
            var reservation = await _db.QueryFirstOrDefaultAsync<Reservation>(
                "SELECT * FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            // I don't like this, I would rather support nullable
            if (reservation == null)
            {
                throw new NotFoundException($"Room {reservationId} not found");
            }

            return reservation;
        }

        public async Task<Reservation> CreateReservation(Reservation newReservation, IDbTransaction transaction)
        {
            var count = await _db.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*)
                    FROM Reservations
                    WHERE RoomNumber = @roomNumber
                      AND Start < @newEnd
                      AND End > @newStart", new
            {
                roomNumber = newReservation.RoomNumber,
                newStart = newReservation.Start,
                newEnd = newReservation.End
            }, transaction);

            if (count > 0)
            {
                throw new ReservationUnavailableException(newReservation.RoomNumber, newReservation.Start, newReservation.End);
            }

            var reservation = await _db.QuerySingleAsync<Reservation>(
                @"INSERT INTO Reservations(Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                    Values(@Id, @GuestEmail, @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut) RETURNING *",
                newReservation,
                transaction
            );
            return reservation;
        }

        public async Task<bool> DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            return deleted > 0;
        }

        public async Task<bool> CheckIn(Guid reservationId, string emailAddress, IDbTransaction? dbTransaction = null)
        {
            var updated = await _db.ExecuteAsync(
                "UPDATE Reservations SET CheckedIn = 1 WHERE GuestEmail = @emailAddress AND Id = @reservationId;",
                new { emailAddress, reservationId = reservationId.ToString() },
                dbTransaction
            );

            return updated > 0;
        }
    }
}
