using Validators;
using Models;

namespace Reservations.Tests;

public class ReservationValidatorTests
{
    [Fact]
    public void ValidateForCreate_ShouldThrow_WhenEndDateIsBeforeStartDate()
    {
        var reservation = CreateReservation(
            start: new DateTime(2026, 4, 10),
            end: new DateTime(2026, 4, 9));

        var ex = Assert.Throws<ArgumentException>(() =>
            ReservationValidator.ValidateForCreate(reservation));

        Assert.Equal("End date must be after start date.", ex.Message);
    }

    [Fact]
    public void ValidateForCreate_ShouldThrow_WhenReservationIsLessThanOneDay()
    {
        var reservation = CreateReservation(
            start: new DateTime(2026, 4, 10),
            end: new DateTime(2026, 4, 10));

        var ex = Assert.Throws<ArgumentException>(() =>
            ReservationValidator.ValidateForCreate(reservation));

        Assert.Equal("Reservation must be at least 1 day long.", ex.Message);
    }

    [Fact]
    public void ValidateForCreate_ShouldThrow_WhenReservationExceedsThirtyDays()
    {
        var reservation = CreateReservation(
            start: new DateTime(2026, 4, 1),
            end: new DateTime(2026, 5, 2)); // 31 days

        var ex = Assert.Throws<ArgumentException>(() =>
            ReservationValidator.ValidateForCreate(reservation));

        Assert.Equal("Reservation cannot exceed 30 days.", ex.Message);
    }

    [Fact]
    public void ValidateForCreate_ShouldPass_WhenReservationIsOneDayLong()
    {
        var reservation = CreateReservation(
            start: new DateTime(2026, 4, 10),
            end: new DateTime(2026, 4, 11));

        ReservationValidator.ValidateForCreate(reservation);
    }

    [Fact]
    public void ValidateForCreate_ShouldPass_WhenReservationIsThirtyDaysLong()
    {
        var reservation = CreateReservation(
            start: new DateTime(2026, 4, 1),
            end: new DateTime(2026, 5, 1)); // 30 days

        ReservationValidator.ValidateForCreate(reservation);
    }

    [Fact]
    public void ValidateForCreate_ShouldIgnoreTimePart_WhenComparingDates()
    {
        var reservation = CreateReservation(
            start: new DateTime(2026, 4, 10, 23, 0, 0),
            end: new DateTime(2026, 4, 11, 1, 0, 0));

        ReservationValidator.ValidateForCreate(reservation);
    }

    private static Reservation CreateReservation(DateTime start, DateTime end)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RoomNumber = "101",
            GuestEmail = "test@example.com",
            Start = start,
            End = end,
            CheckedIn = false,
            CheckedOut = false
        };
    }
}