using api.Shared.Models.Domain;
using FluentValidation;
using System;

namespace api.Shared.Validation.Domain
{
    public class RoomValidator : AbstractValidator<Room>
    {
        public RoomValidator()
        {
            RuleFor(x => x.Number)
                .NotNull()
                .NotEmpty()
                .Length(3)
                .Matches(@"^[1-9][0-9][1-9]$")
                .WithMessage("Room number must be a three-digit number with a non-zero floor number and non-'00' door number.");
        }
    }
}
