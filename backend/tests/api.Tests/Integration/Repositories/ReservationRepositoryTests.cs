using api.Shared.Models.Errors;
using api.Shared.Repositories;
using api.Shared.Models.DB;
using Microsoft.Data.Sqlite;

namespace api.Tests.Integration
{
    [Collection(nameof(DatabaseSeedCollection))]
    public abstract class SequentialTestBase
    {
        public string[] Guests => _fixture.Guests;
        public int[] Rooms => _fixture.Rooms;

        protected readonly ReservationRepository _reservationRepository;
        protected readonly DatabaseSeedFixture _fixture;

        public SequentialTestBase(DatabaseSeedFixture fixture)
        {
            _fixture = fixture;
            _reservationRepository = new ReservationRepository(fixture.DbConnection);
        }
    }

    public class GetReservationsTest : SequentialTestBase
    {
        public GetReservationsTest(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task GetReservations_ShouldReturnAllReservations()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = Rooms[0],
                GuestEmail = Guests[0],
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(3),
                CheckedIn = false,
                CheckedOut = false
            };

            if (_fixture.DbConnection.State != System.Data.ConnectionState.Open)
                _fixture.DbConnection.Open();
            using var trans = _fixture.DbConnection.BeginTransaction();
            await _reservationRepository.CreateReservation(reservation, trans);

            // Act
            var reservations = await _reservationRepository.GetReservations();

            // Assert
            Assert.Contains(reservations, r => r.Id == reservation.Id);

            await Task.CompletedTask;
        }
    }

    public class GetReservation_WhenReservationExists : SequentialTestBase
    {
        public GetReservation_WhenReservationExists(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task GetReservation_ShouldReturnReservation_WhenReservationExists()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = Rooms[0],
                GuestEmail = Guests[0],
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(2),
                CheckedIn = false,
                CheckedOut = false
            };

            if (_fixture.DbConnection.State != System.Data.ConnectionState.Open)
                _fixture.DbConnection.Open();
            using var trans = _fixture.DbConnection.BeginTransaction();
            await _reservationRepository.CreateReservation(reservation, trans);

            // Act
            var fetchedReservation = await _reservationRepository.GetReservation(Guid.Parse(reservation.Id));

            // Assert
            Assert.Equal(reservation.Id, fetchedReservation.Id);
            Assert.Equal(reservation.RoomNumber, fetchedReservation.RoomNumber);
            Assert.Equal(reservation.GuestEmail, fetchedReservation.GuestEmail);

            await Task.CompletedTask;
        }
    }

    public class GetReservation_WhenReservationDoesNotExist : SequentialTestBase
    {
        public GetReservation_WhenReservationDoesNotExist(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task GetReservation_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            var repository = new ReservationRepository(_fixture.DbConnection);

            // Arrange
            var reservationId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _reservationRepository.GetReservation(reservationId));
        }
    }

    public class CreateReservation : SequentialTestBase
    {
        public CreateReservation(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CreateReservation_ShouldInsertReservationIntoDatabase()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = Rooms[0],
                GuestEmail = Guests[0],
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(4),
                CheckedIn = false,
                CheckedOut = false
            };
            if(_fixture.DbConnection.State != System.Data.ConnectionState.Open)
                _fixture.DbConnection.Open();
            using var trans = _fixture.DbConnection.BeginTransaction();
            // Act
            var createdReservation = await _reservationRepository.CreateReservation(reservation, trans);

            // Assert
            Assert.Equal(reservation.Id, createdReservation.Id);
            Assert.Equal(reservation.RoomNumber, createdReservation.RoomNumber);
            Assert.Equal(reservation.GuestEmail, createdReservation.GuestEmail);

            await Task.CompletedTask;
        }
    }

    public class CheckInReservation : SequentialTestBase
    {
        public CheckInReservation(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CheckInReservation_ShouldSucceed()
        {
            var reservationId = Guid.NewGuid();
            // Arrange
            var reservation = new Reservation
            {
                Id = reservationId.ToString(),
                RoomNumber = Rooms[0],
                GuestEmail = Guests[0],
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(4),
                CheckedIn = false,
                CheckedOut = false
            };
            if (_fixture.DbConnection.State != System.Data.ConnectionState.Open)
                _fixture.DbConnection.Open();
            
            using var trans = _fixture.DbConnection.BeginTransaction();
            var createdReservation = await _reservationRepository.CreateReservation(reservation, trans);

            // Act

            var result = await _reservationRepository.CheckIn(reservationId, reservation.GuestEmail, trans);

            // Assert
            Assert.True(result);

            await Task.CompletedTask;
        }
    }

    public class CheckInReservation_ShouldFail : SequentialTestBase
    {
        public CheckInReservation_ShouldFail(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CreateReservation_ShouldFail()
        {
            if (_fixture.DbConnection.State != System.Data.ConnectionState.Open)
                _fixture.DbConnection.Open();

            using var trans = _fixture.DbConnection.BeginTransaction();
            
            // Act
            var result = await _reservationRepository.CheckIn(Guid.NewGuid(), "Test", trans);

            // Assert
            Assert.False(result);
        }
    }

    public class CreateReservation_WhenGuestEmailDoesntExist : SequentialTestBase
    {
        public CreateReservation_WhenGuestEmailDoesntExist(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CreateReservation_ShouldThrowSqlliteException_WhenGuestEmailDoesntExist()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = Rooms[0],
                GuestEmail = "random@email.com",
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(4),
                CheckedIn = false,
                CheckedOut = false
            };
            if(_fixture.DbConnection.State != System.Data.ConnectionState.Open)
                _fixture.DbConnection.Open();
            using var trans = _fixture.DbConnection.BeginTransaction();
            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _reservationRepository.CreateReservation(reservation, trans));
        }
    }

    public class DeleteReservation : SequentialTestBase
    {
        public DeleteReservation(DatabaseSeedFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task DeleteReservation_ShouldRemoveReservationFromDatabase()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                RoomNumber = Rooms[0],
                GuestEmail = Guests[0],
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddDays(1),
                CheckedIn = false,
                CheckedOut = false
            };

            if (_fixture.DbConnection.State != System.Data.ConnectionState.Open)
                _fixture.DbConnection.Open();
            using var trans = _fixture.DbConnection.BeginTransaction();
            await _reservationRepository.CreateReservation(reservation, trans);

            // Act
            var result = await _reservationRepository.DeleteReservation(Guid.Parse(reservation.Id));

            // Assert
            Assert.True(result);
            await Assert.ThrowsAsync<NotFoundException>(() => _reservationRepository.GetReservation(Guid.Parse(reservation.Id)));
        }
    }
}