using Models;
using Models.Errors;

namespace Validators
{
    public static class ReservationConflictValidator
    {
        public static void ValidateNoConflict(Reservation newReservation, bool hasConflict)
        {
            if (hasConflict)
            {
                throw new ConflictException($"Room {newReservation.RoomNumber} is already booked between {newReservation.Start:d} and {newReservation.End:d}.");
            }
        }
    }
}
