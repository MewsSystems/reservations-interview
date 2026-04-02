using Extensions;
using Models;
using Models.Errors;

namespace api.tests;

public class ReservationValidationTests
{
    private static Reservation MakeValid() => new()
    {
        RoomNumber = "101",
        GuestEmail = "guest@example.com",
        Start = DateTime.Today,
        End = DateTime.Today.AddDays(3),
    };

    [Fact]
    public void ValidReservation_DoesNotThrow()
    {
        var r = MakeValid();
        var ex = Record.Exception(() => r.Validate());
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingEmail_Throws(string? email)
    {
        var r = MakeValid();
        r.GuestEmail = email!;
        var ex = Assert.Throws<ValidationException>(() => r.Validate());
        Assert.Contains(ex.Errors, e => e.Contains("Email", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("noatsign")]
    [InlineData("trailing@")]
    public void BadEmailFormat_Throws(string email)
    {
        var r = MakeValid();
        r.GuestEmail = email;
        var ex = Assert.Throws<ValidationException>(() => r.Validate());
        Assert.Contains(ex.Errors, e => e.Contains("domain", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void StartInPast_Throws()
    {
        var r = MakeValid();
        r.Start = DateTime.Today.AddDays(-1);
        var ex = Assert.Throws<ValidationException>(() => r.Validate());
        Assert.Contains(ex.Errors, e => e.Contains("past", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void StartAfterEnd_Throws()
    {
        var r = MakeValid();
        r.End = r.Start.AddHours(-1);
        var ex = Assert.Throws<ValidationException>(() => r.Validate());
        Assert.Contains(ex.Errors, e => e.Contains("before the end", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DurationOver30Days_Throws()
    {
        var r = MakeValid();
        r.End = r.Start.AddDays(31);
        var ex = Assert.Throws<ValidationException>(() => r.Validate());
        Assert.Contains(ex.Errors, e => e.Contains("30 days", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void InvalidRoomNumber_Throws()
    {
        var r = MakeValid();
        r.RoomNumber = "abc";
        var ex = Assert.Throws<ValidationException>(() => r.Validate());
        Assert.Contains(ex.Errors, e => e.Contains("digits", StringComparison.OrdinalIgnoreCase));
    }
}
