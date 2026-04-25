using System.Text.RegularExpressions;
using Models;
using Validators.Interfaces;

namespace Validators
{
    public class ReservationValidator : IReservationValidator
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled
        );
        private static readonly Regex RoomNumberRegex = new Regex(
            @"^[0-9](0[1-9]|[1-9][0-9])$",
            RegexOptions.Compiled
        );

        public List<string> Validate(Reservation reservation)
        {
            var errors = new List<string>();

            if (reservation == null)
            {
                errors.Add("Reservation payload is missing or invalid.");
                return errors;
            }

            if (reservation.Start >= reservation.End)
                errors.Add("Start date must be before end date");

            var duration = (reservation.End - reservation.Start).Days;
            if (duration is < 1 or > 30)
                errors.Add("Duration must be between 1 and 30 days");

            if (
                string.IsNullOrWhiteSpace(reservation.GuestEmail)
                || !EmailRegex.IsMatch(reservation.GuestEmail)
            )
                errors.Add("Email must be valid and include a domain");

            if (
                string.IsNullOrWhiteSpace(reservation.RoomNumber)
                || !RoomNumberRegex.IsMatch(reservation.RoomNumber)
            )
                errors.Add(
                    "Room number must be 3 digits, floor 0-9, and door 01-99 (e.g., 001, 102)"
                );

            return errors;
        }
    }
}
