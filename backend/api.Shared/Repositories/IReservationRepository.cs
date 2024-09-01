using api.Shared.Models.Errors;
using System.Threading.Tasks;
using System;
using api.Shared.Models.DB;
using System.Collections.Generic;

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

        Task<Reservation> CreateReservation(Reservation newReservation);

        Task<bool> DeleteReservation(Guid reservationId);
    }
}