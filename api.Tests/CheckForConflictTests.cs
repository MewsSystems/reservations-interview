using Dapper;
using Microsoft.Data.Sqlite;
using Models;
using Models.Errors;
using NUnit.Framework;
using Repositories;

namespace api.Tests
{
    /// <summary>
    /// Tests for the conflict-detection logic inside ReservationRepository.CheckForConflict,
    /// exercised indirectly via CreateReservation (the only public entry point).
    ///
    /// Interval model: [Start, End) — the standard half-open hotel night convention.
    /// Two reservations on the same room conflict when:  existingStart < newEnd AND existingEnd > newStart
    /// </summary>
    [TestFixture]
    public class CheckForConflictTests
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
                    Email   TEXT PRIMARY KEY NOT NULL,
                    Name    TEXT NOT NULL,
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
                INSERT INTO Rooms  (Number, State) VALUES (102, 0);
            ");

            _repo = new ReservationRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        private static Reservation Make(string room, string start, string end) => new()
        {
            Id         = Guid.NewGuid(),
            RoomNumber = room,
            GuestEmail = "guest@example.com",
            Start      = DateTime.Parse(start),
            End        = DateTime.Parse(end),
        };

        // ── Overlapping — must throw ConflictException ────────────────────────────

        // existing:  |-------|
        // new:           |-------|
        [TestCase("101", "2025-06-01", "2025-06-10",  "2025-06-07", "2025-06-15",
            TestName = "Conflict_NewStartsInsideExisting_Throws")]

        // existing:      |-------|
        // new:       |-------|
        [TestCase("101", "2025-06-07", "2025-06-15",  "2025-06-01", "2025-06-10",
            TestName = "Conflict_NewEndsInsideExisting_Throws")]

        // existing:  |---------------|
        // new:           |-------|
        [TestCase("101", "2025-06-01", "2025-06-20",  "2025-06-05", "2025-06-15",
            TestName = "Conflict_NewContainedByExisting_Throws")]

        // existing:      |-------|
        // new:       |---------------|
        [TestCase("101", "2025-06-05", "2025-06-15",  "2025-06-01", "2025-06-20",
            TestName = "Conflict_NewContainsExisting_Throws")]

        // existing:  |-------|
        // new:       |-------|
        [TestCase("101", "2025-06-01", "2025-06-10",  "2025-06-01", "2025-06-10",
            TestName = "Conflict_ExactSameDates_Throws")]

        // existing:  |-------|
        // new:       |---|
        [TestCase("101", "2025-06-01", "2025-06-10",  "2025-06-01", "2025-06-05",
            TestName = "Conflict_NewSameStartShorter_Throws")]

        // existing:  |-------|
        // new:           |---|
        [TestCase("101", "2025-06-01", "2025-06-10",  "2025-06-05", "2025-06-10",
            TestName = "Conflict_NewSameEndLonger_Throws")]

        // existing:  |-|
        // new:       |-------|
        [TestCase("101", "2025-06-03", "2025-06-05",  "2025-06-01", "2025-06-10",
            TestName = "Conflict_ExistingShorterInsideNew_Throws")]
        public async Task CheckForConflict_OverlappingRanges_ThrowsConflictException(
            string room,
            string existStart, string existEnd,
            string newStart,   string newEnd)
        {
            await _repo.CreateReservation(Make(room, existStart, existEnd));

            Assert.ThrowsAsync<ConflictException>(
                () => _repo.CreateReservation(Make(room, newStart, newEnd)));
        }

        // ── Non-overlapping — must NOT throw ─────────────────────────────────────

        // existing:  |-------|
        // new:                |-------|   (new starts exactly when existing ends)
        [TestCase("101", "2025-06-01", "2025-06-10",  "2025-06-10", "2025-06-20",
            TestName = "NoConflict_AdjacentNewAfter_DoesNotThrow")]

        // existing:          |-------|
        // new:       |-------|             (new ends exactly when existing starts)
        [TestCase("101", "2025-06-10", "2025-06-20",  "2025-06-01", "2025-06-10",
            TestName = "NoConflict_AdjacentNewBefore_DoesNotThrow")]

        // existing:  |-------|
        // new:                  |--|       (gap between)
        [TestCase("101", "2025-06-01", "2025-06-05",  "2025-06-10", "2025-06-15",
            TestName = "NoConflict_NewStartsAfterGap_DoesNotThrow")]

        // existing:              |-------|
        // new:       |--|                  (gap between)
        [TestCase("101", "2025-06-10", "2025-06-15",  "2025-06-01", "2025-06-05",
            TestName = "NoConflict_NewEndsBeforeGap_DoesNotThrow")]

        // same dates but different room — never a conflict
        [TestCase("101", "2025-06-01", "2025-06-10",  "2025-06-01", "2025-06-10",
            TestName = "NoConflict_SameDatesButDifferentRoom_DoesNotThrow")]
        public async Task CheckForConflict_NonOverlappingRanges_DoesNotThrow(
            string room,
            string existStart, string existEnd,
            string newStart,   string newEnd)
        {
            await _repo.CreateReservation(Make(room, existStart, existEnd));

            // For the different-room case, point the new reservation at room 102
            var newRoom = (room == "101" && newStart == existStart && newEnd == existEnd) ? "102" : room;

            Assert.DoesNotThrowAsync(
                () => _repo.CreateReservation(Make(newRoom, newStart, newEnd)));
        }

        // ── Multiple existing reservations ───────────────────────────────────────

        // Ensures the query finds a conflict even when there are multiple rows
        // and the conflict is not with the first-inserted one.
        [TestCase("101", "2025-06-01", "2025-06-05",
                         "2025-06-10", "2025-06-15",
                         "2025-06-12", "2025-06-18",
            TestName = "Conflict_SecondOfTwoExistingReservations_Throws")]
        public async Task CheckForConflict_ConflictsWithSecondExistingReservation_Throws(
            string room,
            string exist1Start, string exist1End,
            string exist2Start, string exist2End,
            string newStart,    string newEnd)
        {
            await _repo.CreateReservation(Make(room, exist1Start, exist1End));
            await _repo.CreateReservation(Make(room, exist2Start, exist2End));

            Assert.ThrowsAsync<ConflictException>(
                () => _repo.CreateReservation(Make(room, newStart, newEnd)));
        }

        // A reservation that fits cleanly in a gap between two existing ones must succeed.
        [TestCase("101", "2025-06-01", "2025-06-05",
                         "2025-06-10", "2025-06-15",
                         "2025-06-05", "2025-06-10",
            TestName = "NoConflict_FitsInGapBetweenTwoExisting_DoesNotThrow")]
        public async Task CheckForConflict_FitsInGapBetweenTwoReservations_DoesNotThrow(
            string room,
            string exist1Start, string exist1End,
            string exist2Start, string exist2End,
            string newStart,    string newEnd)
        {
            await _repo.CreateReservation(Make(room, exist1Start, exist1End));
            await _repo.CreateReservation(Make(room, exist2Start, exist2End));

            Assert.DoesNotThrowAsync(
                () => _repo.CreateReservation(Make(room, newStart, newEnd)));
        }
    }
}
