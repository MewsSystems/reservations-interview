using Extensions;
using Models;

namespace api.tests;

public class RoomValidationTests
{
    [Theory]
    [InlineData("101")]
    [InlineData("999")]
    [InlineData("010")]
    [InlineData("001")]
    public void ValidRoomNumbers_PassValidation(string number)
    {
        var room = new Room { Number = number };
        var errors = room.Validate();
        Assert.Empty(errors);
    }

    [Fact]
    public void NullRoomNumber_ReturnsRequired()
    {
        var room = new Room { Number = null! };
        var errors = room.Validate();
        Assert.Single(errors);
        Assert.Contains("required", errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyOrWhitespace_ReturnsRequired(string number)
    {
        var room = new Room { Number = number };
        var errors = room.Validate();
        Assert.Single(errors);
        Assert.Contains("required", errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("1234")]
    public void WrongLength_ReturnsLengthError(string number)
    {
        var room = new Room { Number = number };
        var errors = room.Validate();
        Assert.Single(errors);
        Assert.Contains("exactly 3 digits", errors[0]);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1a2")]
    [InlineData("!!1")]
    public void NonDigits_ReturnsDigitError(string number)
    {
        var room = new Room { Number = number };
        var errors = room.Validate();
        Assert.Single(errors);
        Assert.Contains("only digits", errors[0]);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("200")]
    [InlineData("900")]
    [InlineData("000")]
    public void DoorZeroZero_ReturnsDoorError(string number)
    {
        var room = new Room { Number = number };
        var errors = room.Validate();
        Assert.Single(errors);
        Assert.Contains("Door number cannot be \"00\"", errors[0]);
    }
}
