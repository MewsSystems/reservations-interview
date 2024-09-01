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
            var reservations = await _db.QueryAsync<Models.DB.Reservation>("SELECT * FROM Reservations");

            if (reservations == null)
            {
                return [];
            }

            return reservations;
        }

        /// <inheritdoc/>
        public async Task<Reservation> GetReservation(Guid reservationId)
        {
            var reservation = await _db.QueryFirstOrDefaultAsync<Models.DB.Reservation>(
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

        public async Task<Reservation> CreateReservation(Reservation newReservation)
        {
            return await _db.QuerySingleAsync<Reservation>(
                @"INSERT INTO Reservations(Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                    Values(@Id, @GuestEmail, @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut) RETURNING *",
                newReservation
            );
        }

        public async Task<bool> DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            return deleted > 0;
        }
    }
}
