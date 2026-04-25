using System.Data;
using Microsoft.Data.Sqlite;
using Models;
using Repositories;
using Xunit;

namespace api.tests.Repositories
{
    public class ReservationRepositoryTests : IDisposable
    {
        private readonly IDbConnection _db;
        private readonly ReservationRepository _repo;

        public ReservationRepositoryTests()
        {
            // Set up a fresh in-memory SQLite database for every test
            _db = new SqliteConnection("Data Source=:memory:");
            _db.Open();

            // Initialize the schema (Tables: Guests, Rooms, Reservations)
            // You can use your existing Setup.cs logic or a simplified script
            InitializeSchema();

            _repo = new ReservationRepository(_db);
        }

        private void InitializeSchema()
        {
            using var command = _db.CreateCommand();
            command.CommandText =
                @"
                CREATE TABLE Guests (Email TEXT PRIMARY KEY, Name TEXT);
                CREATE TABLE Rooms (Number TEXT PRIMARY KEY, State INTEGER);
                CREATE TABLE Reservations (
                    Id GUID PRIMARY KEY, 
                    RoomNumber TEXT, 
                    GuestEmail TEXT, 
                    Start DATETIME, 
                    End DATETIME,
                    CheckedIn INTEGER DEFAULT 0,
                    CheckedOut INTEGER DEFAULT 0
                );
                INSERT INTO Rooms (Number, State) VALUES ('101', 1);
                INSERT INTO Guests (Email, Name) VALUES ('test@test.com', 'Test Guest');
            ";
            command.ExecuteNonQuery();
        }

        [Fact]
        public async Task CreateReservation_SavesToDatabase()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                RoomNumber = "101",
                GuestEmail = "test@test.com",
                Start = DateTime.Now.AddDays(1),
                End = DateTime.Now.AddDays(2),
            };

            // Act
            var result = await _repo.CreateReservation(reservation);

            // Assert
            Assert.NotNull(result);
            var all = await _repo.GetReservations();
            Assert.Single(all);
        }

        [Fact]
        public async Task HasConflict_ReturnsTrue_WhenDatesOverlap()
        {
            // Arrange: Existing booking for tomorrow
            var start = DateTime.Today.AddDays(1);
            var end = DateTime.Today.AddDays(3);

            await _repo.CreateReservation(
                new Reservation
                {
                    Id = Guid.NewGuid(),
                    RoomNumber = "101",
                    GuestEmail = "a@a.com",
                    Start = start,
                    End = end,
                }
            );

            // Act: Try to book an overlapping slot
            var overlap = new Reservation
            {
                RoomNumber = "101",
                Start = start.AddDays(1),
                End = end.AddDays(1),
                GuestEmail = "",
            };
            var hasConflict = await _repo.HasConflict(overlap);

            // Assert
            Assert.True(hasConflict);
        }

        public void Dispose()
        {
            _db.Close();
            _db.Dispose();
        }
    }
}
