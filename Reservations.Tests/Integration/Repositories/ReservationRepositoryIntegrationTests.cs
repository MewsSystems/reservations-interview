using Dapper;
using Microsoft.Data.Sqlite;
using Repositories;

namespace Tests.Integration.Repositories;

public class ReservationRepositoryIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public ReservationRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        CreateSchema();
    }

    private void CreateSchema()
    {
        _connection.Execute(@"
            CREATE TABLE Reservations (
                Id TEXT PRIMARY KEY NOT NULL,
                GuestEmail TEXT NOT NULL,
                RoomNumber INT NOT NULL,
                Start INT NOT NULL,
                End INT NOT NULL,
                CheckedIn INT NOT NULL,
                CheckedOut INT NOT NULL
            );
        ");
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private async Task InsertReservation(int roomNumber, DateTime start, DateTime end)
    {
        await _connection.ExecuteAsync(@"
            INSERT INTO Reservations (Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
            VALUES (@Id, @GuestEmail, @RoomNumber, @Start, @End, 0, 0);
        ",
        new
        {
            Id = Guid.NewGuid().ToString(),
            GuestEmail = "test@test.com",
            RoomNumber = roomNumber,
            Start = start.Date,
            End = end.Date
        });
    }

    private ReservationRepository CreateRepo()
    {
        return new ReservationRepository(_connection);
    }

    [Fact]
    public async Task Should_Return_True_When_Dates_Overlap()
    {
        // Arrange
        await InsertReservation(101, new DateTime(2026, 4, 10), new DateTime(2026, 4, 12));
        var repo = CreateRepo();

        // Act
        var result = await repo.HasConflictingReservation(
            "101",
            new DateTime(2026, 4, 11),
            new DateTime(2026, 4, 13));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_When_No_Overlap_Right_Side()
    {
        // Arrange
        await InsertReservation(101, new DateTime(2026, 4, 10), new DateTime(2026, 4, 12));
        var repo = CreateRepo();

        // Act
        var result = await repo.HasConflictingReservation(
            "101",
            new DateTime(2026, 4, 12),
            new DateTime(2026, 4, 14));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_False_When_No_Overlap_Left_Side()
    {
        // Arrange
        await InsertReservation(101, new DateTime(2026, 4, 10), new DateTime(2026, 4, 12));
        var repo = CreateRepo();

        // Act
        var result = await repo.HasConflictingReservation(
            "101",
            new DateTime(2026, 4, 8),
            new DateTime(2026, 4, 10));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_When_New_Reservation_Fully_Covers_Existing()
    {
        // Arrange
        await InsertReservation(101, new DateTime(2026, 4, 10), new DateTime(2026, 4, 12));
        var repo = CreateRepo();

        // Act
        var result = await repo.HasConflictingReservation(
            "101",
            new DateTime(2026, 4, 9),
            new DateTime(2026, 4, 13));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_For_Different_Room()
    {
        // Arrange
        await InsertReservation(101, new DateTime(2026, 4, 10), new DateTime(2026, 4, 12));
        var repo = CreateRepo();

        // Act
        var result = await repo.HasConflictingReservation(
            "102",
            new DateTime(2026, 4, 11),
            new DateTime(2026, 4, 13));

        // Assert
        Assert.False(result);
    }
}