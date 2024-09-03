using api.Shared.Models.Errors;
using api.Shared.Constants;
using api.Shared.Repositories.Dapper;
using api.Shared.Models.DB;

namespace api.Tests.Integration
{
    [Collection(nameof(DatabaseCollection))]
    public class RoomRepositoryTests
    {
        private readonly RoomRepository _roomRepository;
        private readonly DatabaseFixture _fixture;

        public RoomRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _roomRepository = new RoomRepository(fixture.DbConnection);
        }

        [Fact]
        public async Task GetRoom_ShouldReturnRoom_WhenRoomExists()
        {
            // Arrange
            var room = new Room { Number = 101, State = State.Ready };
            await _roomRepository.CreateRoom(room);

            // Act
            var fetchedRoom = await _roomRepository.GetRoom(101);

            // Assert
            Assert.Equal(room.Number, fetchedRoom.Number);
            Assert.Equal(room.State, fetchedRoom.State);
        }

        [Fact]
        public async Task GetRoom_ShouldThrowNotFoundException_WhenRoomDoesNotExist()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _roomRepository.GetRoom(999));
        }

        [Fact]
        public async Task CreateRoom_ShouldInsertRoomIntoDatabase()
        {
            // Arrange
            var room = new Room { Number = 102, State = State.Ready };

            // Act
            var createdRoom = await _roomRepository.CreateRoom(room);

            // Assert
            Assert.Equal(room.Number, createdRoom.Number);
            Assert.Equal(room.State, createdRoom.State);
        }

        [Fact]
        public async Task DeleteRoom_ShouldRemoveRoomFromDatabase()
        {
            // Arrange
            var room = new Room { Number = 103, State = State.Ready };
            await _roomRepository.CreateRoom(room);

            // Act
            var result = await _roomRepository.DeleteRoom(103);

            // Assert
            Assert.True(result);
            await Assert.ThrowsAsync<NotFoundException>(() => _roomRepository.GetRoom(103));
        }

        [Fact]
        public async Task GetRooms_ShouldReturnAllRooms()
        {
            // Arrange
            var room1 = new Room { Number = 104, State = State.Ready };
            var room2 = new Room { Number = 105, State = State.Occupied };
            await _roomRepository.CreateRoom(room1);
            await _roomRepository.CreateRoom(room2);

            // Act
            var rooms = await _roomRepository.GetRooms();

            // Assert
            Assert.Contains(rooms, r => r.Number == room1.Number);
            Assert.Contains(rooms, r => r.Number == room2.Number);
        }

        [Fact]
        public async Task UpdateRoomStatus_ShouldSucced()
        {
            // Arrange
            var room = new Room { Number = 123, State = State.Ready };
            await _roomRepository.CreateRoom(room);

            // Act
            var result = await _roomRepository.UpdateRoomStatus(room.Number, State.Dirty);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateRoomStatus_ShouldFail()
        {
            // Act
            var result = await _roomRepository.UpdateRoomStatus(999, State.Dirty);

            // Assert
            Assert.False(result);
        }
    }
}
