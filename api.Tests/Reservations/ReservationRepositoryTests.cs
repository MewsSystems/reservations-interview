using Dapper;
using Models;
using Models.Errors;
using Xunit;
using api.Tests.Common;

namespace api.Tests;

//TODO3HRS: better to have test containers instead of inmemorydb
public sealed class ReservationRepositoryTests
{
    [Fact]
    public async Task CreateReservation_PersistsValidReservation()
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation();

        var created = await fixture.ReservationRepository.CreateReservation(reservation);
        var reservationCount = await fixture.Connection.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM Reservations WHERE Id = @Id;",
            new { Id = reservation.Id.ToString() }
        );

        Assert.Equal(reservation.Id, created.Id);
        Assert.Equal(reservation.RoomNumber, created.RoomNumber);
        Assert.Equal(reservation.GuestEmail, created.GuestEmail);
        Assert.Equal(1, reservationCount);
        Assert.Equal("guest@example.com", await fixture.Connection.QuerySingleAsync<string>(
            "SELECT Email FROM Guests WHERE Email = @Email;",
            new { Email = reservation.GuestEmail }
        ));
    }

    [Fact]
    public async Task CreateReservation_RejectsDateRangeWhereStartIsAfterOrEqualToEnd()
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation(startOffsetDays: 0, endOffsetDays: 0);

        var ex = await Assert.ThrowsAsync<ReservationValidationException>(
            () => fixture.ReservationRepository.CreateReservation(reservation)
        );

        Assert.Equal("Start date must be before end date.", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_RejectsStayShorterThanOneDay()
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation(
            start: new DateTime(2026, 4, 12, 12, 0, 0, DateTimeKind.Utc),
            end: new DateTime(2026, 4, 13, 11, 0, 0, DateTimeKind.Utc)
        );

        var ex = await Assert.ThrowsAsync<ReservationValidationException>(
            () => fixture.ReservationRepository.CreateReservation(reservation)
        );

        Assert.Equal("Minimum stay is 1 day.", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_RejectsStayLongerThanThirtyDays()
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation(startOffsetDays: 0, endOffsetDays: 31);

        var ex = await Assert.ThrowsAsync<ReservationValidationException>(
            () => fixture.ReservationRepository.CreateReservation(reservation)
        );

        Assert.Equal("Maximum stay is 30 days.", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_RejectsInvalidGuestEmail()
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation(email: "guest@localhost");

        var ex = await Assert.ThrowsAsync<ReservationValidationException>(
            () => fixture.ReservationRepository.CreateReservation(reservation)
        );

        Assert.Equal("Guest email must include a valid domain.", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_RejectsUnknownRoom()
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation(roomNumber: "202");

        var ex = await Assert.ThrowsAsync<ReservationValidationException>(
            () => fixture.ReservationRepository.CreateReservation(reservation)
        );

        Assert.Equal("Room 202 does not exist.", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_AllowsValidThreeDigitRoomNumberFormat()
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation(roomNumber: "101");

        var created = await fixture.ReservationRepository.CreateReservation(reservation);

        Assert.Equal("101", created.RoomNumber);
    }

    [Theory]
    [InlineData("-101")]
    [InlineData("100")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2020")]
    [InlineData("000")]
    public async Task CreateReservation_RejectsInvalidRoomNumberFormats(string roomNumber)
    {
        await using var fixture = await TestDb.CreateAsync();
        var reservation = TestReservation(roomNumber: roomNumber);

        await Assert.ThrowsAsync<InvalidRoomNumber>(
            () => fixture.ReservationRepository.CreateReservation(reservation)
        );
    }

    private static Reservation TestReservation(
        string roomNumber = "101",
        string email = "guest@example.com",
        int startOffsetDays = 0,
        int endOffsetDays = 1,
        DateTime? start = null,
        DateTime? end = null
    )
    {
        var baseDate = new DateTime(2026, 4, 12, 0, 0, 0, DateTimeKind.Utc);

        return new Reservation
        {
            Id = Guid.NewGuid(),
            RoomNumber = roomNumber,
            GuestEmail = email,
            Start = start ?? baseDate.AddDays(startOffsetDays),
            End = end ?? baseDate.AddDays(endOffsetDays),
            CheckedIn = false,
            CheckedOut = false
        };
    }

}
