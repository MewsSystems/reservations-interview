using api.Shared.Models.DB;
using api.Shared.Models.Errors;
using api.Shared.Repositories;

namespace api.Tests.Integration
{
    [Collection(nameof(DatabaseCollection))]
    public class GuestRepositoryTests
    {
        private readonly GuestRepository _guestRepository;
        private readonly DatabaseFixture _fixture;

        public GuestRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _guestRepository = new GuestRepository(fixture.DbConnection);
        }

        [Fact]
        public async Task GetGuests_ShouldReturnAllGuests()
        {
            // Arrange
            var guest1 = new Guest { Email = "guest1@example.com", Name = "Guest One" };
            var guest2 = new Guest { Email = "guest2@example.com", Name = "Guest Two" };
            await _guestRepository.CreateGuest(guest1);
            await _guestRepository.CreateGuest(guest2);

            // Act
            var guests = await _guestRepository.GetGuests();

            // Assert
            Assert.Contains(guests, g => g.Email == guest1.Email);
            Assert.Contains(guests, g => g.Email == guest2.Email);
        }

        [Fact]
        public async Task GetGuestByEmail_ShouldReturnGuest_WhenGuestExists()
        {
            // Arrange
            var guest = new Guest { Email = "guest@example.com", Name = "Guest" };
            await _guestRepository.CreateGuest(guest);

            // Act
            var fetchedGuest = await _guestRepository.GetGuestByEmail(guest.Email);

            // Assert
            Assert.Equal(guest.Email, fetchedGuest.Email);
            Assert.Equal(guest.Name, fetchedGuest.Name);
        }

        [Fact]
        public async Task GetGuestByEmail_ShouldThrowNotFoundException_WhenGuestDoesNotExist()
        {
            // Arrange
            var nonExistentEmail = "nonexistent@example.com";

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _guestRepository.GetGuestByEmail(nonExistentEmail));
        }

        [Fact]
        public async Task CreateGuest_ShouldInsertGuestIntoDatabase()
        {
            // Arrange
            var guest = new Guest { Email = "newguest@example.com", Name = "New Guest" };

            // Act
            var createdGuest = await _guestRepository.CreateGuest(guest);

            // Assert
            Assert.Equal(guest.Email, createdGuest.Email);
            Assert.Equal(guest.Name, createdGuest.Name);
        }

        [Fact]
        public async Task DeleteGuestByEmail_ShouldRemoveGuestFromDatabase()
        {
            // Arrange
            var guest = new Guest { Email = "deletethisguest@example.com", Name = "Delete Me" };
            await _guestRepository.CreateGuest(guest);

            // Act
            var result = await _guestRepository.DeleteGuestByEmail(guest.Email);

            // Assert
            Assert.True(result);
            await Assert.ThrowsAsync<NotFoundException>(() => _guestRepository.GetGuestByEmail(guest.Email));
        }
    }
}
