using Dapper;
using Microsoft.Data.Sqlite;
using Models;
using Models.Errors;
using NUnit.Framework;
using Repositories;

namespace api.Tests
{
    [TestFixture]
    public class RoomRepositoryTests
    {
        private SqliteConnection _db = null!;
        private RoomRepository _repo = null!;

        [SetUp]
        public async Task SetUp()
        {
            _db = new SqliteConnection("Data Source=:memory:");
            await _db.OpenAsync();

            await _db.ExecuteAsync(@"
                CREATE TABLE Rooms (
                    Number INT PRIMARY KEY NOT NULL,
                    State  INT NOT NULL
                );
            ");

            _repo = new RoomRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        // ── GetRooms ─────────────────────────────────────────────────────────────

        [Test]
        public async Task GetRooms_EmptyTable_ReturnsEmpty()
        {
            var rooms = await _repo.GetRooms();
            Assert.That(rooms, Is.Empty);
        }

        [Test]
        public async Task GetRooms_MultipleRooms_ReturnsAll()
        {
            await _db.ExecuteAsync("INSERT INTO Rooms (Number, State) VALUES (101, 0);");
            await _db.ExecuteAsync("INSERT INTO Rooms (Number, State) VALUES (202, 0);");

            var rooms = await _repo.GetRooms();

            Assert.That(rooms.Count(), Is.EqualTo(2));
        }

        // ── GetRoom ──────────────────────────────────────────────────────────────

        [Test]
        public async Task GetRoom_ExistingRoom_ReturnsRoom()
        {
            await _db.ExecuteAsync("INSERT INTO Rooms (Number, State) VALUES (101, 0);");

            var room = await _repo.GetRoom("101");

            Assert.That(room.Number, Is.EqualTo("101"));
            Assert.That(room.State, Is.EqualTo(State.Ready));
        }

        [Test]
        public void GetRoom_UnknownRoomNumber_ThrowsNotFoundException()
        {
            Assert.ThrowsAsync<NotFoundException>(() => _repo.GetRoom("999"));
        }

        // ── CreateRoom ───────────────────────────────────────────────────────────

        [Test]
        public async Task CreateRoom_NewRoom_ReturnsCreated()
        {
            var newRoom = new Room { Number = "303", State = State.Ready };

            var created = await _repo.CreateRoom(newRoom);

            Assert.That(created.Number, Is.EqualTo("303"));
            Assert.That(created.State, Is.EqualTo(State.Ready));
        }

        [TestCase(State.Ready,    TestName = "CreateRoom_StateReady_Persists")]
        [TestCase(State.Occupied, TestName = "CreateRoom_StateOccupied_Persists")]
        [TestCase(State.Dirty,    TestName = "CreateRoom_StateDirty_Persists")]
        public async Task CreateRoom_WithState_PersistsState(State state)
        {
            var newRoom = new Room { Number = "401", State = state };

            var created = await _repo.CreateRoom(newRoom);

            Assert.That(created.State, Is.EqualTo(state));
        }

        // ── DeleteRoom ───────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteRoom_ExistingRoom_ReturnsTrue()
        {
            await _db.ExecuteAsync("INSERT INTO Rooms (Number, State) VALUES (101, 0);");

            var result = await _repo.DeleteRoom("101");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteRoom_ExistingRoom_IsActuallyDeleted()
        {
            await _db.ExecuteAsync("INSERT INTO Rooms (Number, State) VALUES (101, 0);");
            await _repo.DeleteRoom("101");

            Assert.ThrowsAsync<NotFoundException>(() => _repo.GetRoom("101"));
        }

        [Test]
        public async Task DeleteRoom_UnknownRoom_ReturnsFalse()
        {
            var result = await _repo.DeleteRoom("999");

            Assert.That(result, Is.False);
        }

        // ── SetRoomState (not-found path) ────────────────────────────────────────

        [Test]
        public void SetRoomState_UnknownRoom_ThrowsNotFoundException()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                _repo.SetRoomState("999", State.Dirty));
        }
    }
}
