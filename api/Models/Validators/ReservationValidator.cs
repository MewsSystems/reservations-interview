using System.Text.RegularExpressions;
using Models;
using Models.Validation;

public class ReservationValidator
{
    public ValidationResult ValidateReservation(Reservation reservation)
    {
        if (reservation.Start >= reservation.End)
        {
            return ValidationResult.Invalid("Start date must be before the end date.");
        }

        var durationInDays = (reservation.End - reservation.Start).Days;
        if (durationInDays < 1 || durationInDays > 30)
        {
            return ValidationResult.Invalid("Reservation duration must be between 1 and 30 days.");
        }

        if (!IsValidEmail(reservation.GuestEmail))
        {
            return ValidationResult.Invalid("Invalid email address. Ensure the email has a valid domain.");
        }

        return ValidationResult.Valid();
    }

    private bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }
}
