using api.Shared.Models.Domain;
using FluentValidation;

namespace api.Shared.Validation.Domain
{
    public class RoomValidator : AbstractValidator<Room>
    {
        public RoomValidator()
        {
            RuleFor(x => x.Number).SetValidator(new RoomNumberValidator());

        }
    }
}
