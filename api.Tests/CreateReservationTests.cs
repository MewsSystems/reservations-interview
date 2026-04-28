using Dapper;
using Microsoft.Data.Sqlite;
using Models;
using Models.Errors;
using NUnit.Framework;
using Repositories;

namespace api.Tests
{
    [TestFixture]
    public class CreateReservationTests
    {
        private SqliteConnection _db = null!;
        private ReservationRepository _repo = null!;

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
                INSERT INTO Guests  (Email, Name) VALUES ('guest@example.com', 'Test Guest');
                INSERT INTO Rooms   (Number, State) VALUES (101, 0);
            ");

            _repo = new ReservationRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        private static Reservation Make(string room, string start, string end) => new()
        {
            Id = Guid.NewGuid(),
            RoomNumber = room,
            GuestEmail = "guest@example.com",
            Start = DateTime.Parse(start),
            End = DateTime.Parse(end),
        };

        // -- successful creation ---------------------------------------------

        [TestCase("101", "2025-06-01", "2025-06-05", TestName = "Create_ValidReservation_ReturnsCreated")]
        [TestCase("101", "2025-07-01", "2025-07-10", TestName = "Create_DifferentDates_ReturnsCreated")]
        public async Task CreateReservation_NoConflict_ReturnsReservation(string room, string start, string end)
        {
            var reservation = Make(room, start, end);
            var result = await _repo.CreateReservation(reservation);

            Assert.That(result.Id, Is.EqualTo(reservation.Id));
            Assert.That(result.RoomNumber, Is.EqualTo(room));
            Assert.That(result.Start.Date, Is.EqualTo(DateTime.Parse(start).Date));
            Assert.That(result.End.Date, Is.EqualTo(DateTime.Parse(end).Date));
        }

        // -- conflict detection ----------------------------------------------

        [TestCase("101", "2025-06-01", "2025-06-10", "2025-06-05", "2025-06-15", TestName = "Conflict_PartialOverlapAfter_ThrowsConflict")]
        [TestCase("101", "2025-06-05", "2025-06-15", "2025-06-01", "2025-06-10", TestName = "Conflict_PartialOverlapBefore_ThrowsConflict")]
        [TestCase("101", "2025-06-01", "2025-06-10", "2025-06-01", "2025-06-10", TestName = "Conflict_ExactMatch_ThrowsConflict")]
        [TestCase("101", "2025-06-01", "2025-06-10", "2025-06-03", "2025-06-07", TestName = "Conflict_NewInsideExisting_ThrowsConflict")]
        [TestCase("101", "2025-06-03", "2025-06-07", "2025-06-01", "2025-06-10", TestName = "Conflict_ExistingInsideNew_ThrowsConflict")]
        public async Task CreateReservation_ConflictingReservation_ThrowsConflictException(
            string room, string existStart, string existEnd,
            string newStart, string newEnd)
        {
            await _repo.CreateReservation(Make(room, existStart, existEnd));

            var conflicting = Make(room, newStart, newEnd);
            Assert.ThrowsAsync<ConflictException>(() => _repo.CreateReservation(conflicting));
        }

        // -- no conflict different room --------------------------------------

        [TestCase("101", "2025-06-01", "2025-06-10", "2025-06-01", "2025-06-10", TestName = "NoConflict_DifferentRoom_BothCreated")]
        public async Task CreateReservation_DifferentRoom_DoesNotThrow(
            string room1, string existStart, string existEnd,
            string newStart, string newEnd)
        {
            // Insert a second room so the FK constraint is satisfied
            await _db.ExecuteAsync("INSERT INTO Rooms (Number, State) VALUES (202, 0);");

            await _repo.CreateReservation(Make(room1, existStart, existEnd));

            var other = Make("202", newStart, newEnd);
            Assert.DoesNotThrowAsync(() => _repo.CreateReservation(other));
        }

        // -- adjacent (touching) boundaries ---------------------------------

        [TestCase("101", "2025-06-01", "2025-06-05", "2025-06-05", "2025-06-10", TestName = "NoConflict_AdjacentCheckoutCheckin_DoesNotThrow")]
        public async Task CreateReservation_AdjacentReservations_DoesNotThrow(
            string room, string existStart, string existEnd,
            string newStart, string newEnd)
        {
            await _repo.CreateReservation(Make(room, existStart, existEnd));

            var adjacent = Make(room, newStart, newEnd);
            Assert.DoesNotThrowAsync(() => _repo.CreateReservation(adjacent));
        }
    }
}
