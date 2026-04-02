using Models.Errors;
using Validators;

namespace Reservations.Tests;

public class RoomValidatorTests
{
    [Fact]
    public void ValidateRoomNumber_ShouldThrow_WhenRoomNumberIsNull()
    {
        Assert.Throws<InvalidRoomNumber>(() =>
            RoomValidator.ValidateRoomNumber(null));
    }

    [Fact]
    public void ValidateRoomNumber_ShouldThrow_WhenRoomNumberIsEmpty()
    {
        Assert.Throws<InvalidRoomNumber>(() =>
            RoomValidator.ValidateRoomNumber(""));
    }

    [Fact]
    public void ValidateRoomNumber_ShouldThrow_WhenRoomNumberIsWhitespace()
    {
        Assert.Throws<InvalidRoomNumber>(() =>
            RoomValidator.ValidateRoomNumber("   "));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("2020")]
    public void ValidateRoomNumber_ShouldThrow_WhenLengthIsNotThree(string roomNumber)
    {
        Assert.Throws<InvalidRoomNumber>(() =>
            RoomValidator.ValidateRoomNumber(roomNumber));
    }

    [Theory]
    [InlineData("-101")]
    [InlineData("10a")]
    [InlineData("A01")]
    [InlineData("1 1")]
    [InlineData("1-1")]
    public void ValidateRoomNumber_ShouldThrow_WhenContainsNonDigits(string roomNumber)
    {
        Assert.Throws<InvalidRoomNumber>(() =>
            RoomValidator.ValidateRoomNumber(roomNumber));
    }

    [Theory]
    [InlineData("000")]
    [InlineData("100")]
    [InlineData("200")]
    [InlineData("900")]
    public void ValidateRoomNumber_ShouldThrow_WhenDoorNumberIs00(string roomNumber)
    {
        Assert.Throws<InvalidRoomNumber>(() =>
            RoomValidator.ValidateRoomNumber(roomNumber));
    }

    [Theory]
    [InlineData("001")]
    [InlineData("010")]
    [InlineData("101")]
    [InlineData("105")]
    [InlineData("201")]
    [InlineData("203")]
    [InlineData("999")]
    public void ValidateRoomNumber_ShouldPass_ForValidRoomNumbers(string roomNumber)
    {
        RoomValidator.ValidateRoomNumber(roomNumber);
    }
}