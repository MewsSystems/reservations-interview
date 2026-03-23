using FluentValidation;
using Contracts;
using Models;

namespace Validators
{
    public class ReservationRequestValidator : AbstractValidator<ReservationRequest>
    {
        public ReservationRequestValidator()
        {
            RuleFor(x => x.GuestEmail)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .Must(email => email.Contains('.')).WithMessage("Email must include a domain");

            RuleFor(x => x.RoomNumber)
                .NotEmpty().WithMessage("Room number is required")
                .Must(Room.IsValidRoomNumber).WithMessage("Room number must be in format '###' (e.g. 101, 202)");

            RuleFor(x => x.Start)
                .NotEmpty().WithMessage("Start date is required")
                .LessThan(x => x.End).WithMessage("Start date must be before end date")
                .Must(start => start >= DateTime.Today).WithMessage("Time travellers are not welcomed in Mewstel");

            RuleFor(x => x.End)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.Start).WithMessage("End date must be after start date");

            RuleFor(x => x)
                .Must(HaveMinimumDuration).WithMessage("Minimum booking duration is 1 day")
                .Must(HaveMaximumDuration).WithMessage("Maximum booking duration is 30 days");
        }

        private static bool HaveMinimumDuration(ReservationRequest request)
        {
            return (request.End - request.Start).TotalDays >= 1;
        }

        private static bool HaveMaximumDuration(ReservationRequest request)
        {
            return (request.End - request.Start).TotalDays <= 30;
        }
    }
}
