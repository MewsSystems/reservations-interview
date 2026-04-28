using Dapper;
using Microsoft.Data.Sqlite;
using Models;
using NUnit.Framework;
using Repositories;

namespace api.Tests
{
    [TestFixture]
    public class ReservationRepositoryTests
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
                INSERT INTO Guests (Email, Name) VALUES ('guest@example.com', 'Test Guest');
                INSERT INTO Rooms  (Number, State) VALUES (101, 0);
                INSERT INTO Rooms  (Number, State) VALUES (202, 0);
            ");

            _repo = new ReservationRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        private async Task<Reservation> Insert(string room, string start, string end,
            bool checkedIn = false, bool checkedOut = false)
        {
            var id = Guid.NewGuid();
            await _db.ExecuteAsync(
                @"INSERT INTO Reservations (Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                  VALUES (@Id, 'guest@example.com', @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut);",
                new
                {
                    Id = id.ToString(),
                    RoomNumber = int.Parse(room),
                    // Pass DateTime so Dapper serialises to "yyyy-MM-dd HH:mm:ss", matching
                    // what CreateReservation stores and what GetUpcomingReservations compares against.
                    Start = DateTime.Parse(start),
                    End = DateTime.Parse(end),
                    CheckedIn = checkedIn ? 1 : 0,
                    CheckedOut = checkedOut ? 1 : 0
                });
            return await _repo.GetReservation(id);
        }

        // ── GetReservations ──────────────────────────────────────────────────────

        [Test]
        public async Task GetReservations_EmptyTable_ReturnsEmpty()
        {
            var result = await _repo.GetReservations();
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetReservations_MultipleReservations_ReturnsAll()
        {
            await Insert("101", "2025-06-01", "2025-06-05");
            await Insert("202", "2025-07-01", "2025-07-05");

            var result = await _repo.GetReservations();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        // ── GetUpcomingReservations ──────────────────────────────────────────────
        //
        // startDaysFromNow / endDaysFromNow are offsets relative to DateTime.UtcNow.Date
        // so the tests stay correct regardless of when they are run.

        // Single-reservation inclusion / exclusion
        [TestCase(-10, -1, 0, TestName = "Upcoming_EndedYesterday_IsExcluded")]
        [TestCase(-5,  -5, 0, TestName = "Upcoming_EndedFiveDaysAgo_IsExcluded")]
        [TestCase(-1,   0, 1, TestName = "Upcoming_EndsToday_IsIncluded")]
        [TestCase( 0,   1, 1, TestName = "Upcoming_StartsTodayEndsTomorrow_IsIncluded")]
        [TestCase( 1,   5, 1, TestName = "Upcoming_StartsAndEndsFuture_IsIncluded")]
        [TestCase( 0,  30, 1, TestName = "Upcoming_EndsThirtyDaysAhead_IsIncluded")]
        public async Task GetUpcomingReservations_SingleReservation_CountMatchesExpected(
            int startDaysFromNow, int endDaysFromNow, int expectedCount)
        {
            var today = DateTime.UtcNow.Date;
            var start = today.AddDays(startDaysFromNow).ToString("yyyy-MM-dd");
            var end   = today.AddDays(endDaysFromNow).ToString("yyyy-MM-dd");

            await Insert("101", start, end);

            var result = await _repo.GetUpcomingReservations();

            Assert.That(result.Count(), Is.EqualTo(expectedCount));
        }

        // Mixed past + future — only upcoming ones come back
        [TestCase(1, 1, TestName = "Upcoming_OnePastOneFuture_ReturnsOneFuture")]
        [TestCase(2, 2, TestName = "Upcoming_TwoPastTwoFuture_ReturnsTwoFuture")]
        public async Task GetUpcomingReservations_MixedReservations_ReturnsOnlyUpcoming(
            int pastCount, int futureCount)
        {
            var today = DateTime.UtcNow.Date;

            // Insert past reservations (room 101)
            for (int i = 0; i < pastCount; i++)
            {
                var s = today.AddDays(-20 - i).ToString("yyyy-MM-dd");
                var e = today.AddDays(-10 - i).ToString("yyyy-MM-dd");
                await Insert("101", s, e);
            }

            // Insert future reservations (room 202)
            for (int i = 0; i < futureCount; i++)
            {
                var s = today.AddDays(10 + i).ToString("yyyy-MM-dd");
                var e = today.AddDays(20 + i).ToString("yyyy-MM-dd");
                await Insert("202", s, e);
            }

            var result = await _repo.GetUpcomingReservations();

            Assert.That(result.Count(), Is.EqualTo(futureCount));
            Assert.That(result.All(r => r.RoomNumber == "202"), Is.True);
        }

        // Results must be ordered by Start ASC
        [TestCase("101", 5, 10, "202", 1, 4,  "202", "101", TestName = "Upcoming_OrderedByStart_EarlierStartFirst")]
        [TestCase("101", 1,  3, "202", 4, 8,  "101", "202", TestName = "Upcoming_OrderedByStart_LaterStartSecond")]
        public async Task GetUpcomingReservations_MultipleReservations_ReturnedOrderedByStartAsc(
            string room1, int start1, int end1,
            string room2, int start2, int end2,
            string expectedFirst, string expectedSecond)
        {
            var today = DateTime.UtcNow.Date;

            await Insert(room1,
                today.AddDays(start1).ToString("yyyy-MM-dd"),
                today.AddDays(end1).ToString("yyyy-MM-dd"));

            await Insert(room2,
                today.AddDays(start2).ToString("yyyy-MM-dd"),
                today.AddDays(end2).ToString("yyyy-MM-dd"));

            var result = (await _repo.GetUpcomingReservations()).ToList();

            Assert.That(result[0].RoomNumber, Is.EqualTo(expectedFirst));
            Assert.That(result[1].RoomNumber, Is.EqualTo(expectedSecond));
        }

        // Empty table edge case
        [Test]
        public async Task GetUpcomingReservations_EmptyTable_ReturnsEmpty()
        {
            var result = await _repo.GetUpcomingReservations();
            Assert.That(result, Is.Empty);
        }

        // ── DeleteReservation ────────────────────────────────────────────────────

        [Test]
        public async Task DeleteReservation_ExistingReservation_ReturnsTrue()
        {
            var reservation = await Insert("101", "2025-06-01", "2025-06-05");

            var result = await _repo.DeleteReservation(reservation.Id);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteReservation_ExistingReservation_IsActuallyDeleted()
        {
            var reservation = await Insert("101", "2025-06-01", "2025-06-05");
            await _repo.DeleteReservation(reservation.Id);

            var all = await _repo.GetReservations();
            Assert.That(all, Is.Empty);
        }

        [Test]
        public async Task DeleteReservation_UnknownId_ReturnsFalse()
        {
            var result = await _repo.DeleteReservation(Guid.NewGuid());

            Assert.That(result, Is.False);
        }
    }
}
