using System.Threading.Tasks;
using System;
using api.Shared.Models.Domain;
using System.Collections.Generic;

namespace api.Shared.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> Get();

        Task<Reservation> GetByReservationId(Guid reservationId);

        Task<Reservation> Create(Reservation reservation);

        Task<bool> Delete(Guid reservationId);
    }
}