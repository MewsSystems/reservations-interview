using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Models;
using Repositories;

namespace Services
{
    public class ReservationService(ReservationRepository reservationRepository, GuestRepository guestRepository, RoomRepository roomRepository) : IReservationService
    {
        public async Task<Reservation> CreateReservation(Reservation newBooking)
        {
            var room = await roomRepository.GetRoom(newBooking.RoomNumber);
            if (room == null)
            {
                throw new ValidationException("Room does not exists, cannot continue booking");
            }

            var guest = await guestRepository.GetGuestByEmail(newBooking.GuestEmail);
            if (guest == null)
            {
                if(string.IsNullOrEmpty(newBooking.Name))
                {
                    throw new ValidationException("Name must me filled, if email does not exists yet");
                }

                guest = await guestRepository.CreateGuest(new Guest() { Email = newBooking.GuestEmail, Name = newBooking.Name, Surname = newBooking.Surname });
            }

            if (guest == null)
            {
                throw new Exception("Reservation cant be filled");
            }

            var isOverlapingReservation = await isOverlapingWithExistingReservation(newBooking);
            if (isOverlapingReservation)
            {
                throw new ValidationException("Room is already booked during this time period");
            }

            var createdReservation = await reservationRepository.CreateReservation(newBooking);

            return createdReservation;
        }

        public async Task<IEnumerable<Reservation>> GetReservations(DateTime? from, DateTime? to)
        {
            var reservations = await reservationRepository.GetReservations();

            if (from != null)
            {
                reservations = reservations.Where(r => r.Start >= from);
            }

            if (to != null)
            {
                reservations = reservations.Where(r => r.End <= to);
            }

            return reservations;
        }

        private async Task<bool> isOverlapingWithExistingReservation(Reservation newBooking) {
            var existingReservations = await reservationRepository.GetReservations(newBooking.RoomNumber);

            // Check for overlaping reservations
            return (existingReservations.Any(x => x.Start < newBooking.End && x.End > newBooking.Start))
                ? true
                : false;
        }
    }
}