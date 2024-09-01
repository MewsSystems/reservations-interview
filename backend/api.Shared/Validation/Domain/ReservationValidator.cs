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
                .NotNull()
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Invalid guest email address.");
            RuleFor(x => x.Id).Must((id) => id != Guid.Empty).WithMessage("Invalid reservation unique identifier.");
            RuleFor(r => r)
                .Must(r => r.Start < r.End)
                .WithMessage("The start date must be before the end date.");

            RuleFor(r => r)
                .Must(r =>
                    (r.End - r.Start).TotalDays >= 0 &&
                    (r.End - r.Start).TotalDays <= 30)
                .WithMessage("Reservation must be between 1 day and 30 days.");

            RuleFor(x => x.RoomNumber)
                .NotNull()
                .NotEmpty()
                .Length(3)
                .Matches(@"^[1-9][0-9][1-9]$")
                .WithMessage("Room number must be a three-digit number with a non-zero floor number and non-'00' door number.");
        }
    }
}
