using System.Threading.Tasks;
using System;
using api.Shared.Models.Domain;
using System.Collections.Generic;
using System.Data;

namespace api.Shared.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> Get();

        Task<IEnumerable<ReservationWithRoomState>> GetStaffReservations();

        Task<Reservation> GetByReservationId(Guid reservationId);

        Task<Reservation> Create(Reservation reservation, IDbTransaction? transaction = null);

        Task<bool> Delete(Guid reservationId);

        Task<bool> CheckIn(Guid reservationId, string emailAddress);
    }
}