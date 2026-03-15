using System;
using Models;

namespace Services
{
    public interface IReservationService
    {
        public Task<Reservation> CreateReservation(Reservation newBooking);
    }
}