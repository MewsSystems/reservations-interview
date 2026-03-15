using FluentValidation;
using FluentValidation.AspNetCore;
using Models;
using System;
using Repositories;

namespace Controllers.Validators
{
    public class ReservationValidator : AbstractValidator<Reservation>
    {
        public ReservationValidator(RoomRepository roomRepository)
        {
            RuleFor(x => x.GuestEmail)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Valid email is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Guest name is required");

            RuleFor(x => x.Start)
                .LessThan(x => x.End)
                .WithMessage("Start date must be before End date.");

            RuleFor(x => x)
                .Must(x => (x.End - x.Start).TotalDays >= 1)
                .WithMessage("Minimum booking duration is 1 day.");

            RuleFor(x => x)
                .Must(x => (x.End - x.Start).TotalDays <= 30)
                .WithMessage("Maximum booking duration is 30 days.");
        }
    }
}
