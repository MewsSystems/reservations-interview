using Controllers;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Moq;
using Repositories.Interfaces;
using Validators.Interfaces;

namespace api.tests.Controllers
{
    public class ReservationControllerTests
    {
        private readonly Mock<IReservationRepository> _mockRepo;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IGuestRepository> _mockGuestRepo;
        private readonly Mock<IReservationValidator> _mockValidator;
        private readonly ReservationController _controller;

        public ReservationControllerTests()
        {
            _mockRepo = new Mock<IReservationRepository>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockGuestRepo = new Mock<IGuestRepository>();
            _mockValidator = new Mock<IReservationValidator>();

            _controller = new ReservationController(
                _mockRepo.Object,
                _mockRoomRepo.Object,
                _mockGuestRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task BookReservation_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var booking = new Reservation { RoomNumber = "101", GuestEmail = "test@test.com" };
            _mockValidator
                .Setup(v => v.Validate(booking))
                .Returns(new List<string> { "Invalid date range" });

            // Act
            var result = await _controller.BookReservation(booking);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task BookReservation_ReturnsBadRequest_WhenRoomDoesNotExist()
        {
            // Arrange
            var booking = new Reservation { RoomNumber = "999", GuestEmail = "test@test.com" };
            _mockValidator.Setup(v => v.Validate(booking)).Returns(new List<string>());
            _mockRoomRepo
                .Setup(r => r.GetRoom(booking.RoomNumber))
                .ThrowsAsync(new NotFoundException("Room not found"));

            // Act
            var result = await _controller.BookReservation(booking);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task BookReservation_CreatesGuest_WhenGuestIsNew()
        {
            // Arrange
            var booking = new Reservation
            {
                RoomNumber = "101",
                GuestEmail = "new@guest.com",
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(1),
            };

            _mockValidator.Setup(v => v.Validate(booking)).Returns(new List<string>());
            _mockRoomRepo
                .Setup(r => r.GetRoom(booking.RoomNumber))
                .ReturnsAsync(new Room { Number = "101" });

            // Simulate Guest NOT found
            _mockGuestRepo
                .Setup(g => g.GetGuestByEmail(booking.GuestEmail))
                .ThrowsAsync(new NotFoundException("Guest not found"));

            _mockRepo
                .Setup(r => r.CreateReservation(It.IsAny<Reservation>()))
                .ReturnsAsync(booking);

            // Act
            await _controller.BookReservation(booking);

            // Assert
            _mockGuestRepo.Verify(
                g => g.CreateGuest(It.Is<Guest>(gt => gt.Email == booking.GuestEmail)),
                Times.Once
            );
        }

        [Fact]
        public async Task BookReservation_ReturnsConflict_WhenOverlapExists()
        {
            // Arrange
            var booking = new Reservation { RoomNumber = "101", GuestEmail = "test@test.com" };
            _mockValidator.Setup(v => v.Validate(booking)).Returns(new List<string>());
            _mockRoomRepo
                .Setup(r => r.GetRoom(booking.RoomNumber))
                .ReturnsAsync(new Room { Number = "101" });
            _mockGuestRepo
                .Setup(g => g.GetGuestByEmail(booking.GuestEmail))
                .ReturnsAsync(new Guest { Email = "test@test.com", Name = "Test" });

            _mockRepo
                .Setup(r => r.CreateReservation(It.IsAny<Reservation>()))
                .ThrowsAsync(new InvalidOperationException("Room is already booked"));

            // Act
            var result = await _controller.BookReservation(booking);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }
    }
}
