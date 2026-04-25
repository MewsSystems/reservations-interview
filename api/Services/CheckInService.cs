using Models;
using Repositories.Interfaces;
using Validators.Interfaces;

namespace Services
{
    public class CheckInService : ICheckInService
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IRoomRepository _roomRepo;

        public CheckInService(IReservationRepository reservationRepo, IRoomRepository roomRepo)
        {
            _reservationRepo = reservationRepo;
            _roomRepo = roomRepo;
        }

        public async Task<(bool Success, string Error)> ProcessCheckIn(
            Guid reservationId,
            string emailConfirmation
        )
        {
            var reservation = await _reservationRepo.GetReservation(reservationId);
            if (reservation == null)
                return (false, "Reservation not found.");

            if (reservation.GuestEmail != emailConfirmation)
                return (false, "Email confirmation does not match the reservation.");

            var room = await _roomRepo.GetRoom(reservation.RoomNumber);
            if (room?.State == State.Dirty)
                return (false, "Staff cannot check in a guest to a dirty room.");

            var success = await _reservationRepo.ExecuteCheckInTransaction(
                reservationId,
                reservation.RoomNumber
            );

            return success
                ? (true, string.Empty)
                : (false, "An error occurred during database update.");
        }
    }
}
