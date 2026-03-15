using System;
using Models;
using Repositories;

namespace Services
{
    public class ReservationService(ReservationRepository reservationRepository, GuestRepository guestRepository) : IReservationService
    {
        public async Task<Reservation> CreateReservation(Reservation newBooking)
        {
            var guest = await guestRepository.GetGuestByEmail(newBooking.GuestEmail);
            if (guest == null)
            {
                if(string.IsNullOrEmpty(newBooking.Name))
                {
                    throw new Exception("Name must me filled, if email does not exists yet");
                }

                guest = await guestRepository.CreateGuest(new Guest() { Email = newBooking.GuestEmail, Name = newBooking.Name, Surname = newBooking.Surname });
            }

            if (guest == null)
            {
                throw new Exception("Reservation cant be filled");
            }

            var createdReservation = await reservationRepository.CreateReservation(newBooking);

            return createdReservation;
        }
    }
}