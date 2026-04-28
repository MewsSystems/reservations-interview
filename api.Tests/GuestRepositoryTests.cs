using Dapper;
using Microsoft.Data.Sqlite;
using Models;
using Models.Errors;
using NUnit.Framework;
using Repositories;

namespace api.Tests
{
    [TestFixture]
    public class GuestRepositoryTests
    {
        private SqliteConnection _db = null!;
        private GuestRepository _repo = null!;

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
            ");

            _repo = new GuestRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        // ── GetGuests ────────────────────────────────────────────────────────────

        [Test]
        public async Task GetGuests_EmptyTable_ReturnsEmpty()
        {
            var result = await _repo.GetGuests();
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetGuests_MultipleGuests_ReturnsAll()
        {
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name) VALUES ('a@a.com', 'Alice');");
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name) VALUES ('b@b.com', 'Bob');");

            var result = await _repo.GetGuests();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        // ── GetGuestByEmail ──────────────────────────────────────────────────────

        [Test]
        public async Task GetGuestByEmail_ExistingGuest_ReturnsGuest()
        {
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name, Surname) VALUES ('alice@example.com', 'Alice', 'Smith');");

            var guest = await _repo.GetGuestByEmail("alice@example.com");

            Assert.That(guest.Email, Is.EqualTo("alice@example.com"));
            Assert.That(guest.Name, Is.EqualTo("Alice"));
            Assert.That(guest.Surname, Is.EqualTo("Smith"));
        }

        [Test]
        public void GetGuestByEmail_UnknownEmail_ThrowsNotFoundException()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                _repo.GetGuestByEmail("nobody@example.com"));
        }

        // ── GuestExists ──────────────────────────────────────────────────────────

        [Test]
        public async Task GuestExists_ExistingGuest_ReturnsTrue()
        {
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name) VALUES ('alice@example.com', 'Alice');");

            var exists = await _repo.GuestExists("alice@example.com");

            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task GuestExists_UnknownEmail_ReturnsFalse()
        {
            var exists = await _repo.GuestExists("nobody@example.com");

            Assert.That(exists, Is.False);
        }

        // ── CreateGuest ──────────────────────────────────────────────────────────

        [Test]
        public async Task CreateGuest_NewGuest_ReturnsCreated()
        {
            var newGuest = new Guest { Email = "new@example.com", Name = "New", Surname = "Guest" };

            var created = await _repo.CreateGuest(newGuest);

            Assert.That(created.Email, Is.EqualTo("new@example.com"));
            Assert.That(created.Name, Is.EqualTo("New"));
            Assert.That(created.Surname, Is.EqualTo("Guest"));
        }

        [Test]
        public async Task CreateGuest_NewGuestNoSurname_ReturnsCreated()
        {
            var newGuest = new Guest { Email = "nosurname@example.com", Name = "NoSurname" };

            var created = await _repo.CreateGuest(newGuest);

            Assert.That(created.Email, Is.EqualTo("nosurname@example.com"));
            Assert.That(created.Surname, Is.Null);
        }

        [Test]
        public async Task CreateGuest_DuplicateEmail_ThrowsConflictException()
        {
            var guest = new Guest { Email = "dup@example.com", Name = "Dup" };
            await _repo.CreateGuest(guest);

            Assert.ThrowsAsync<ConflictException>(() =>
                _repo.CreateGuest(new Guest { Email = "dup@example.com", Name = "Other" }));
        }

        // ── UpdateGuest ──────────────────────────────────────────────────────────

        [Test]
        public async Task UpdateGuest_ExistingGuest_ReturnsUpdated()
        {
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name, Surname) VALUES ('alice@example.com', 'Alice', 'Old');");

            var updated = await _repo.UpdateGuest("alice@example.com",
                new Guest { Email = "alice@example.com", Name = "Alice", Surname = "New" });

            Assert.That(updated.Surname, Is.EqualTo("New"));
        }

        [Test]
        public async Task UpdateGuest_ExistingGuest_CanClearSurname()
        {
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name, Surname) VALUES ('alice@example.com', 'Alice', 'Smith');");

            var updated = await _repo.UpdateGuest("alice@example.com",
                new Guest { Email = "alice@example.com", Name = "Alice Updated", Surname = null });

            Assert.That(updated.Name, Is.EqualTo("Alice Updated"));
            Assert.That(updated.Surname, Is.Null);
        }

        [Test]
        public void UpdateGuest_UnknownEmail_ThrowsNotFoundException()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                _repo.UpdateGuest("nobody@example.com",
                    new Guest { Email = "nobody@example.com", Name = "Nobody" }));
        }

        // ── DeleteGuestByEmail ───────────────────────────────────────────────────

        [Test]
        public async Task DeleteGuestByEmail_ExistingGuest_ReturnsTrue()
        {
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name) VALUES ('alice@example.com', 'Alice');");

            var result = await _repo.DeleteGuestByEmail("alice@example.com");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteGuestByEmail_ExistingGuest_IsActuallyDeleted()
        {
            await _db.ExecuteAsync("INSERT INTO Guests (Email, Name) VALUES ('alice@example.com', 'Alice');");
            await _repo.DeleteGuestByEmail("alice@example.com");

            var exists = await _repo.GuestExists("alice@example.com");
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task DeleteGuestByEmail_UnknownEmail_ReturnsFalse()
        {
            var result = await _repo.DeleteGuestByEmail("nobody@example.com");

            Assert.That(result, Is.False);
        }
    }
}
