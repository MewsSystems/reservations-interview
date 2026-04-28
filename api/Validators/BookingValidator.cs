using System.Text.RegularExpressions;
using Models;
using Models.Errors;

namespace Validators
{
    public static class BookingValidator
    {
        public static void Validate(Reservation booking)
        {
            if (string.IsNullOrWhiteSpace(booking.RoomNumber))
            {
                throw new InvalidBooking("Room number is required.");
            }

            if (string.IsNullOrWhiteSpace(booking.GuestEmail))
            {
                throw new InvalidBooking("Guest email is required.");
            }

            if (!Room.IsValidRoomNumber(booking.RoomNumber))
            {
                throw new InvalidBooking($"'{booking.RoomNumber}' is not a valid room number - must be exactly 3 digits and the last two digits cannot be 00 (e.g. 101, 202).");
            }

            if (!Regex.IsMatch(booking.GuestEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new InvalidBooking("Email must include a domain.");
            }

            var start = booking.Start.Date;
            var end = booking.End.Date;

            if (start >= end)
            {
                throw new InvalidBooking("Start date must be before end date.");
            }

            var duration = (end - start).Days;

            if (duration < 1)
            {
                throw new InvalidBooking("Minimum booking duration is 1 day.");
            }

            if (duration > 30)
            {
                throw new InvalidBooking("Maximum booking duration is 30 days.");
            }
        }
    }
}
