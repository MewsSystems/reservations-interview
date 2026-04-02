using Models;
using Models.Errors;

namespace Extensions
{
    public static class ReservationExtensions
    {
        /// <summary>
        /// Validates a reservation against RE-001 booking rules.
        /// Throws <see cref="ValidationException"/> if any rules are violated.
        /// </summary>
        public static void Validate(this Reservation reservation)
        {
            var errors = new List<string>();

            // Room number validation
            errors.AddRange(new Room { Number = reservation.RoomNumber }.Validate());

            // Email must include a domain
            if (string.IsNullOrWhiteSpace(reservation.GuestEmail))
            {
                errors.Add("Email is required.");
            }
            else if (
                !reservation.GuestEmail.Contains('@')
                || reservation.GuestEmail.IndexOf('@') == reservation.GuestEmail.Length - 1
            )
            {
                errors.Add("Email must include a domain (e.g. user@example.com).");
            }

            // Start date must not be in the past (compare local server date — dates are calendar dates at the hotel)
            if (reservation.Start.Date < DateTime.Today)
            {
                errors.Add("Start date cannot be in the past.");
            }

            // Start date must be before End date
            if (reservation.Start >= reservation.End)
            {
                errors.Add("Start date must be before the end date.");
            }

            // Duration constraints
            var duration = (reservation.End - reservation.Start).TotalDays;
            if (duration < 1)
            {
                errors.Add("Reservation must be at least 1 day.");
            }
            else if (duration > 30)
            {
                errors.Add("Reservation cannot exceed 30 days.");
            }

            if (errors.Count > 0)
            {
                throw new ValidationException(errors);
            }
        }
    }
}
