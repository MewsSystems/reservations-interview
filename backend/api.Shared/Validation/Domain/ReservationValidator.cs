using api.Shared.Models.Domain;
using FluentValidation;
using System;

namespace api.Shared.Validation.Domain
{
    public class ReservationValidator : AbstractValidator<Reservation>
    {
        public ReservationValidator()
        {
            RuleFor(x => x.GuestEmail)
                .NotNull().WithMessage("Guest email cannot be null.")
                .NotEmpty().WithMessage("Guest email cannot be empty.")
                .EmailAddress().WithMessage("Guest email has to eb valid email address.")
                .WithMessage("Invalid guest email address.");
            RuleFor(x => x.Id).Must((id) => id != Guid.Empty).WithMessage("Invalid reservation unique identifier.");
            RuleFor(r => r)
                .Must(r => r.Start <= r.End)
                .WithMessage("The start date must be before the end date.");

            RuleFor(r => r)
                .Must(r =>
                    (r.End - r.Start).TotalDays >= 0 &&
                    (r.End - r.Start).TotalDays <= 30)
                .WithMessage("Reservation must be between 1 day and 30 days.");

            RuleFor(x => x.RoomNumber)
                .NotNull().WithMessage("Room number cannot be null.")
                .NotEmpty().WithMessage("Room number cannot be empty.")
                .Length(3).WithMessage("Room number must be exactly 3 digits.")
                .Matches(@"^[0-9]{3}$").WithMessage("Room number must be exactly 3 digits.")
                .Must(ValidateRoomNumber).WithMessage("Room number must be a valid three-digit number with a non-zero floor number and non-'00' door number.");
        }

        private bool ValidateRoomNumber(string roomNumber)
        {
            var floorNumber = int.Parse(roomNumber[0].ToString());
            var doorNumber = int.Parse(roomNumber[1..]);
            return floorNumber >= 0 && floorNumber <= 9 && doorNumber != 0;
        }
    }
}
