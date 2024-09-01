using api.Shared.Models.Domain;
using api.Shared.Repositories;
using api.Shared.Services;
using Moq;
using Microsoft.Extensions.Logging;
using api.Shared.Models.Errors;
using api.Shared.Extensions;

namespace api.Tests.Services
{
    public class ReservationServiceTests
    {
        private readonly Mock<IReservationRepository> _mockRepository;
        private readonly Mock<ILogger<ReservationService>> _mockLogger;
        private readonly Mock<IGuestService> _mockGuestService;
        private readonly ReservationService _reservationService;

        public ReservationServiceTests()
        {
            _mockRepository = new Mock<IReservationRepository>();
            _mockLogger = new Mock<ILogger<ReservationService>>();
            _mockGuestService = new Mock<IGuestService>();
            _reservationService = new ReservationService(_mockLogger.Object, _mockGuestService.Object, _mockRepository.Object);
        }

        [Fact]
        public async Task GetReservations_ShouldReturnReservations()
        {
            // Arrange
            var reservations = new List<api.Shared.Models.DB.Reservation>
            {
                new() { Id = Guid.NewGuid().ToString(), RoomNumber = 101, GuestEmail = "guest1@example.com" },
                new() { Id = Guid.NewGuid().ToString(), RoomNumber = 102, GuestEmail = "guest2@example.com" }
            };

            _mockRepository.Setup(repo => repo.GetReservations()).ReturnsAsync(reservations);

            // Act
            var result = await _reservationService.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetReservationById_ShouldReturnReservation_WhenReservationExists()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var reservation = new api.Shared.Models.DB.Reservation
            {
                Id = reservationId.ToString(),
                RoomNumber = 101,
                GuestEmail = "guest1@example.com"
            };

            _mockRepository.Setup(repo => repo.GetReservation(reservationId)).ReturnsAsync(reservation);

            // Act
            var result = await _reservationService.GetByReservationId(reservationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reservationId, result.Id);
        }

        [Fact]
        public async Task GetReservationById_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            // Arrange
            var reservationId = Guid.NewGuid();

            _mockRepository
                .Setup(repo => repo.GetReservation(reservationId))
                .ThrowsAsync(new NotFoundException($"Reservation {reservationId} not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _reservationService.GetByReservationId(reservationId));
        }

        [Theory]
        [InlineData("000")]
        [InlineData("001")]
        [InlineData("1000")]
        [InlineData("InvalidData")]
        [InlineData("-101")]
        [InlineData("2020")]
        [InlineData("01")]
        public async Task Create_ShouldThrowValidationException_WhenRoomNumberIsInvalid(string roomNumber)
        {
            // Arrange
            var invalidReservation = new Reservation
            {
                Id = Guid.NewGuid(),
                RoomNumber = roomNumber, // Invalid room number
                GuestEmail = "guest@example.com",
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ServiceValidationException>(() => _reservationService.Create(invalidReservation));
        }

        [Fact]
        public async Task Create_ShouldThrowValidationException_WhenStartDateIsAfterEndDate()
        {
            // Arrange
            var invalidReservation = new Reservation
            {
                Id = Guid.NewGuid(),
                RoomNumber = "111",
                GuestEmail = "guest@example.com",
                Start = DateTime.Now.AddDays(1),
                End = DateTime.Now
            };

            // Act & Assert
            await Assert.ThrowsAsync<ServiceValidationException>(() => _reservationService.Create(invalidReservation));
        }

        [Fact]
        public async Task Create_ShouldThrowValidationException_WhenStartDateIsMinValue()
        {
            // Arrange
            var invalidReservation = new Reservation
            {
                Id = Guid.NewGuid(),
                RoomNumber = "111",
                GuestEmail = "guest@example.com",
                Start = DateTime.MinValue,
                End = DateTime.Now
            };

            // Act & Assert
            await Assert.ThrowsAsync<ServiceValidationException>(() => _reservationService.Create(invalidReservation));
        }

        [Fact]
        public async Task CreateReservation_ShouldLogInformationAndReturnCreatedReservation()
        {
            // Arrange
            var newReservation = new api.Shared.Models.DB.Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = 101,
                GuestEmail = "guest1@example.com",
                Start = DateTime.Now,
                End = DateTime.Now.AddDays(1)
            };

            _mockRepository.Setup(repo => repo.CreateReservation(It.IsAny<api.Shared.Models.DB.Reservation>()))
                .ReturnsAsync(newReservation);

            // Act
            var result = await _reservationService.Create(newReservation.ToDomain());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newReservation.Id, result.Id.ToString());
        }

        [Fact]
        public async Task DeleteReservation_ShouldLogInformation_WhenReservationIsDeleted()
        {
            // Arrange
            var reservationId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.DeleteReservation(reservationId)).ReturnsAsync(true);

            // Act
            var result = await _reservationService.Delete(reservationId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteReservation_ShouldReturnFalse_WhenReservationDoesNotExist()
        {
            // Arrange
            var reservationId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.DeleteReservation(reservationId)).ReturnsAsync(false);

            // Act
            var result = await _reservationService.Delete(reservationId);

            // Assert
            Assert.False(result);
        }
    }
}
