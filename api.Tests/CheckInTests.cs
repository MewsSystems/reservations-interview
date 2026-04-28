using Dapper;
using Microsoft.Data.Sqlite;
using Models;
using Models.Errors;
using NUnit.Framework;
using Repositories;

namespace api.Tests
{
    [TestFixture]
    public class CheckInTests
    {
        private SqliteConnection _db = null!;
        private ReservationRepository _reservationRepo = null!;
        private RoomRepository _roomRepo = null!;

        [SetUp]
        public async Task SetUp()
        {
            _db = new SqliteConnection("Data Source=:memory:");
            await _db.OpenAsync();

            await _db.ExecuteAsync(@"
                CREATE TABLE Guests (
                    Email TEXT PRIMARY KEY NOT NULL,
                    Name  TEXT NOT NULL,
                    Surname TEXT
                );
                CREATE TABLE Rooms (
                    Number INT PRIMARY KEY NOT NULL,
                    State  INT NOT NULL
                );
                CREATE TABLE Reservations (
                    Id          TEXT PRIMARY KEY NOT NULL,
                    GuestEmail  TEXT NOT NULL,
                    RoomNumber  INT  NOT NULL,
                    Start       TEXT NOT NULL,
                    End         TEXT NOT NULL,
                    CheckedIn   INT  NOT NULL DEFAULT 0,
                    CheckedOut  INT  NOT NULL DEFAULT 0
                );
                INSERT INTO Guests (Email, Name) VALUES ('guest@example.com', 'Test Guest');
                INSERT INTO Rooms  (Number, State) VALUES (101, 0);
            ");

            _reservationRepo = new ReservationRepository(_db);
            _roomRepo = new RoomRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        private async Task<Reservation> CreateReservation(
            string email = "guest@example.com",
            bool checkedIn = false,
            bool checkedOut = false)
        {
            var id = Guid.NewGuid();
            await _db.ExecuteAsync(
                @"INSERT INTO Reservations (Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                  VALUES (@Id, @GuestEmail, 101, '2025-06-01', '2025-06-05', @CheckedIn, @CheckedOut);",
                new { Id = id.ToString(), GuestEmail = email, CheckedIn = checkedIn ? 1 : 0, CheckedOut = checkedOut ? 1 : 0 }
            );
            return await _reservationRepo.GetReservation(id);
        }

        // -- successful check-in ---------------------------------------------

        [TestCase("guest@example.com", TestName = "CheckIn_ExactEmail_ReturnsCheckedInReservation")]
        [TestCase("GUEST@EXAMPLE.COM", TestName = "CheckIn_UpperCaseEmail_Succeeds")]
        [TestCase("Guest@Example.Com", TestName = "CheckIn_MixedCaseEmail_Succeeds")]
        public async Task CheckIn_ValidEmail_ReturnsCheckedInReservation(string email)
        {
            var reservation = await CreateReservation();

            var result = await _reservationRepo.CheckIn(reservation.Id, email);

            Assert.That(result.CheckedIn, Is.True);
            Assert.That(result.Id, Is.EqualTo(reservation.Id));
        }

        // -- error cases -----------------------------------------------------

        [TestCase(false, false, "wrong@example.com", "Email does not match",        TestName = "CheckIn_WrongEmail_ThrowsInvalidOperationException")]
        [TestCase(true,  false, "guest@example.com", "already checked in",          TestName = "CheckIn_AlreadyCheckedIn_ThrowsInvalidOperationException")]
        [TestCase(true,  true,  "guest@example.com", "already been checked out",    TestName = "CheckIn_AlreadyCheckedOut_ThrowsInvalidOperationException")]
        public async Task CheckIn_InvalidState_ThrowsInvalidOperationException(
            bool checkedIn, bool checkedOut, string email, string expectedMessage)
        {
            var reservation = await CreateReservation(checkedIn: checkedIn, checkedOut: checkedOut);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reservationRepo.CheckIn(reservation.Id, email));

            Assert.That(ex!.Message, Does.Contain(expectedMessage));
        }

        // -- reservation not found -------------------------------------------

        [Test]
        public void CheckIn_UnknownReservationId_ThrowsNotFoundException()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                _reservationRepo.CheckIn(Guid.NewGuid(), "guest@example.com"));
        }

        // -- room state persistence ------------------------------------------

        [TestCase(State.Dirty,    TestName = "RoomState_SetDirty_Persists")]
        [TestCase(State.Occupied, TestName = "RoomState_SetOccupied_Persists")]
        [TestCase(State.Ready,    TestName = "RoomState_SetReady_Persists")]
        public async Task SetRoomState_Persists(State state)
        {
            await _roomRepo.SetRoomState("101", state);

            var room = await _roomRepo.GetRoom("101");

            Assert.That(room.State, Is.EqualTo(state));
        }
    }
}
