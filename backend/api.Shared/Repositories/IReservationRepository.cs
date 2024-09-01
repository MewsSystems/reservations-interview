using api.Shared.Models.Errors;
using System.Threading.Tasks;
using System;
using api.Shared.Models.DB;
using System.Collections.Generic;
using System.Data;

namespace api.Shared.Repositories
{
    public interface IReservationRepository
    {
        Task<IEnumerable<Reservation>> GetReservations();

        /// <summary>
        /// Find a reservation by its Guid ID, throwing if not found
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns cref="Reservation">An existing reservation</returns>
        /// <exception cref="NotFoundException"></exception>
        Task<Reservation> GetReservation(Guid reservationId);

        Task<Reservation> CreateReservation(Reservation newReservation, IDbTransaction transaction);

        Task<bool> DeleteReservation(Guid reservationId);

        Task<IEnumerable<Reservation>> GetStaffReservations();
    }
}