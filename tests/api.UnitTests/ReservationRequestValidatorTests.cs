using Contracts;
using FluentValidation.TestHelper;
using Validators;

namespace api.UnitTests;

public class ReservationRequestValidatorTests
{
    private readonly ReservationRequestValidator _validator = new();

    private static ReservationRequest ValidRequest() => new()
    {
        RoomNumber = "101",
        GuestEmail = "guest@mjail.com",
        Start = DateTime.Today.AddDays(1),
        End = DateTime.Today.AddDays(3)
    };


    [Theory]
    [InlineData("101")]
    [InlineData("201")]
    [InlineData("999")]
    [InlineData("110")]
    public void Valid_room_numbers_pass(string roomNumber)
    {
        // Arrange
        var request = ValidRequest();
        request.RoomNumber = roomNumber;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RoomNumber);
    }

    [Theory]
    [InlineData("000")]
    [InlineData("100")]
    [InlineData("200")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2020")]
    [InlineData("-101")]
    [InlineData("abc")]
    [InlineData("")]
    public void Invalid_room_numbers_fail(string roomNumber)
    {
        var request = ValidRequest();
        request.RoomNumber = roomNumber;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.RoomNumber);
    }


    [Theory]
    [InlineData("someone@test.com")]
    [InlineData("someonelse@hotel.co.uk")]
    public void Valid_emails_pass(string email)
    {
        var request = ValidRequest();
        request.GuestEmail = email;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GuestEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("cecin'estpasunemail")]
    public void Invalid_emails_fail(string email)
    {
        var request = ValidRequest();
        request.GuestEmail = email;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuestEmail);
    }


    [Fact]
    public void Start_date_must_be_before_end_date()
    {
        var request = ValidRequest();
        request.Start = DateTime.Today.AddDays(5);
        request.End = DateTime.Today.AddDays(3);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Start);
    }

    [Fact]
    public void Time_travellers_are_not_allowed()
    {
        var request = ValidRequest();
        request.Start = DateTime.Today.AddDays(-1);
        request.End = DateTime.Today.AddDays(-2);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Start)
            .WithErrorMessage("Time travellers are not welcomed in Mewstel");;
    }

    [Fact]
    public void Start_date_today_is_valid()
    {
        var request = ValidRequest();
        request.Start = DateTime.Today;
        request.End = DateTime.Today.AddDays(2);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Start);
    }


    [Fact]
    public void Minimum_duration_is_1_day()
    {
        var request = ValidRequest();
        request.Start = DateTime.Today.AddDays(1);
        request.End = DateTime.Today.AddDays(1).AddHours(12);

        var result = _validator.TestValidate(request);

        result.ShouldHaveAnyValidationError()
            .WithErrorMessage("Minimum booking duration is 1 day");
    }

    [Fact]
    public void Maximum_duration_is_30_days()
    {
        var request = ValidRequest();
        request.Start = DateTime.Today.AddDays(1);
        request.End = DateTime.Today.AddDays(32);

        var result = _validator.TestValidate(request);

        result.ShouldHaveAnyValidationError()
            .WithErrorMessage("Maximum booking duration is 30 days");
    }

    [Fact]
    public void Exactly_30_days_is_valid()
    {
        var request = ValidRequest();
        request.Start = DateTime.Today.AddDays(1);
        request.End = DateTime.Today.AddDays(31);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Exactly_1_day_is_valid()
    {
        var request = ValidRequest();
        request.Start = DateTime.Today.AddDays(1);
        request.End = DateTime.Today.AddDays(2);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_request_has_no_errors()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }
}
