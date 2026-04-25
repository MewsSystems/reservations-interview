using Models;
using Moq;
using Repositories.Interfaces;
using Services;
using Xunit;

namespace api.tests.Services
{
    public class CheckInServiceTests
    {
        private readonly Mock<IReservationRepository> _resRepoMock;
        private readonly Mock<IRoomRepository> _roomRepoMock;
        private readonly CheckInService _service;

        public CheckInServiceTests()
        {
            _resRepoMock = new Mock<IReservationRepository>();
            _roomRepoMock = new Mock<IRoomRepository>();
            _service = new CheckInService(_resRepoMock.Object, _roomRepoMock.Object);
        }

        [Fact]
        public async Task ProcessCheckIn_WrongEmail_ReturnsError()
        {
            // Arrange
            var resId = Guid.NewGuid();
            var reservation = new Reservation
            {
                GuestEmail = "correct@test.com",
                RoomNumber = "101",
            };
            _resRepoMock.Setup(r => r.GetReservation(resId)).ReturnsAsync(reservation);

            // Act
            var (success, error) = await _service.ProcessCheckIn(resId, "wrong@test.com");

            // Assert
            Assert.False(success);
            Assert.Equal("Email confirmation does not match the reservation.", error); //
        }

        [Fact]
        public async Task ProcessCheckIn_DirtyRoom_ReturnsError()
        {
            // Arrange
            var resId = Guid.NewGuid();
            var reservation = new Reservation { GuestEmail = "test@test.com", RoomNumber = "101" };
            var room = new Room { Number = "101", State = State.Dirty };

            _resRepoMock.Setup(r => r.GetReservation(resId)).ReturnsAsync(reservation);
            _roomRepoMock.Setup(r => r.GetRoom("101")).ReturnsAsync(room);

            // Act
            var (success, error) = await _service.ProcessCheckIn(resId, "test@test.com");

            // Assert
            Assert.False(success);
            Assert.Equal("Staff cannot check in a guest to a dirty room.", error); //
        }

        [Fact]
        public async Task ProcessCheckIn_Valid_ExecutesTransaction()
        {
            // Arrange
            var resId = Guid.NewGuid();
            var reservation = new Reservation { GuestEmail = "test@test.com", RoomNumber = "101" };
            var room = new Room { Number = "101", State = State.Ready };

            _resRepoMock.Setup(r => r.GetReservation(resId)).ReturnsAsync(reservation);
            _roomRepoMock.Setup(r => r.GetRoom("101")).ReturnsAsync(room);
            _resRepoMock.Setup(r => r.ExecuteCheckInTransaction(resId, "101")).ReturnsAsync(true);

            // Act
            var (success, _) = await _service.ProcessCheckIn(resId, "test@test.com");

            // Assert
            Assert.True(success);
            _resRepoMock.Verify(r => r.ExecuteCheckInTransaction(resId, "101"), Times.Once); //
        }
    }
}
