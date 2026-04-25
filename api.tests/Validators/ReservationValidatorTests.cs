using Models;
using Validators;

namespace api.tests.Validators
{
    public class ReservationValidatorTests
    {
        private readonly ReservationValidator _validator;

        public ReservationValidatorTests()
        {
            _validator = new ReservationValidator();
        }

        [Fact]
        public void Validate_WithValidReservation_ReturnsEmptyList()
        {
            // Arrange
            var validReservation = new Reservation
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(3),
                GuestEmail = "test@example.com",
                RoomNumber = "101",
            };

            // Act
            var errors = _validator.Validate(validReservation);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_WithInvalidDates_ReturnsDateError()
        {
            // Arrange (Start is after End)
            var invalidReservation = new Reservation
            {
                Start = DateTime.Now.AddDays(5),
                End = DateTime.Now,
                GuestEmail = "test@example.com",
                RoomNumber = "101",
            };

            // Act
            var errors = _validator.Validate(invalidReservation);

            // Assert
            Assert.Contains("Start date must be before end date", errors);
            Assert.Contains("Duration must be between 1 and 30 days", errors);
        }

        [Theory]
        [InlineData("testexample.com")] // Missing @
        [InlineData("test@example")] // Missing TLD
        [InlineData(" test@example.com")] // Leading space
        public void Validate_WithInvalidEmail_ReturnsEmailError(string invalidEmail)
        {
            var reservation = new Reservation
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(2),
                GuestEmail = invalidEmail,
                RoomNumber = "101",
            };

            var errors = _validator.Validate(reservation);

            Assert.Contains("Email must be valid and include a domain", errors);
        }

        [Theory]
        [InlineData("000")] // Door 00 is invalid
        [InlineData("10")] // Only 2 digits
        [InlineData("-101")] // Negative
        [InlineData("A01")] // Letters
        public void Validate_WithInvalidRoomNumber_ReturnsRoomError(string invalidRoom)
        {
            var reservation = new Reservation
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(2),
                GuestEmail = "test@example.com",
                RoomNumber = invalidRoom,
            };

            var errors = _validator.Validate(reservation);

            Assert.Contains(
                "Room number must be 3 digits, floor 0-9, and door 01-99 (e.g., 001, 102)",
                errors
            );
        }
    }
}
