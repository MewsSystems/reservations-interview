using api.Shared.Models.Errors;
using api.Shared.Models;
using api.Shared.Repositories;
using api.Shared.Models.DB;

namespace api.Tests.Integration
{
    [Collection(nameof(DatabaseCollection))]
    public class ReservationRepositoryTests
    {
        private readonly ReservationRepository _reservationRepository;
        private readonly DatabaseFixture _fixture;

        public ReservationRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _reservationRepository = new ReservationRepository(fixture.DbConnection);
        }

        [Fact]
        public async Task GetReservations_ShouldReturnAllReservations()
        {
            // Arrange
            var reservation1 = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = 101,
                GuestEmail = "guest1@example.com",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(2),
                CheckedIn = false,
                CheckedOut = false
            };
            var reservation2 = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = 102,
                GuestEmail = "guest2@example.com",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(3),
                CheckedIn = false,
                CheckedOut = false
            };

            /* TBA
             await _reservationRepository.CreateReservation(reservation1);
             await _reservationRepository.CreateReservation(reservation2);

            // Act
            var reservations = await _reservationRepository.GetReservations();

            // Assert
            Assert.Contains(reservations, r => r.Id == reservation1.Id);
            Assert.Contains(reservations, r => r.Id == reservation2.Id);
            */
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetReservation_ShouldReturnReservation_WhenReservationExists()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = 101,
                GuestEmail = "guest@example.com",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(2),
                CheckedIn = false,
                CheckedOut = false
            };

            /* TBA
            using var trans = _fixture.DbConnection.BeginTransaction();
            await _reservationRepository.CreateReservation(reservation);
            
            // Act
            var fetchedReservation = await _reservationRepository.GetReservation(reservation.Id);
            trans.Rollback();
            // Assert
            Assert.Equal(reservation.Id, fetchedReservation.Id);
            Assert.Equal(reservation.RoomNumber, fetchedReservation.RoomNumber);
            Assert.Equal(reservation.GuestEmail, fetchedReservation.GuestEmail);
            */

            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetReservation_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            // Arrange
            var reservationId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _reservationRepository.GetReservation(reservationId));
        }

        [Fact]
        public async Task CreateReservation_ShouldInsertReservationIntoDatabase()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = 103,
                GuestEmail = "guest3@example.com",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(4),
                CheckedIn = false,
                CheckedOut = false
            };

            /* TBA
            // Act
            var createdReservation = await _reservationRepository.CreateReservation(reservation);

            // Assert
            Assert.Equal(reservation.Id, createdReservation.Id);
            Assert.Equal(reservation.RoomNumber, createdReservation.RoomNumber);
            Assert.Equal(reservation.GuestEmail, createdReservation.GuestEmail);
            */

            await Task.CompletedTask;
        }

        [Fact]
        public async Task DeleteReservation_ShouldRemoveReservationFromDatabase()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = 104,
                GuestEmail = "guest4@example.com",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(1),
                CheckedIn = false,
                CheckedOut = false
            };

            /* TBA
            await _reservationRepository.CreateReservation(reservation);

            // Act
            var result = await _reservationRepository.DeleteReservation(reservation.Id);
            
            // Assert
            Assert.True(result);
            await Assert.ThrowsAsync<NotFoundException>(() => _reservationRepository.GetReservation(reservation.Id));
            */

            await Task.CompletedTask;
        }
    }
}
