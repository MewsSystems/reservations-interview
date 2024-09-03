using FluentValidation;

namespace api.Shared.Validation.Domain
{
    public class RoomNumberValidator : AbstractValidator<string>
    {
        public RoomNumberValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Room number cannot be null.")
                .NotEmpty().WithMessage("Room number cannot be empty.")
                .Length(3).WithMessage("Room number must be exactly 3 digits.")
                .Matches(@"^[0-9]{3}$").WithMessage("Room number must be exactly 3 digits.")
                .Must(ValidateRoomNumber).WithMessage("Room number must be a valid three-digit number with a non-zero floor number and non-'00' door number.");
        }

        private static bool ValidateRoomNumber(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber) || roomNumber.Length != 3)
                return false;

            if (!int.TryParse(roomNumber, out int number))
                return false;

            var floorNumber = number / 100;
            var doorNumber = number % 100;

            return floorNumber >= 1 && doorNumber != 0;
        }
    }
}
