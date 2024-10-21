using api.Services;

namespace Api.UnitTests;

public class ReservationValidationTests
{
    private readonly ReservationValidation _sut = new();

    [Theory]
    [InlineData("101")]
    [InlineData("102")]
    [InlineData("103")]
    [InlineData("104")]
    [InlineData("105")]
    [InlineData("201")]
    [InlineData("202")]
    [InlineData("203")]
    public void Valid_Rooms(string roomNumber)
    {
        Assert.Empty(_sut.ValidateRoomNumber(roomNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("-101")]
    [InlineData("100")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2020")]
    [InlineData("000")]
    public void Invalid_Rooms(string roomNumber)
    {
        Assert.NotEmpty(_sut.ValidateRoomNumber(roomNumber));
    }
}
