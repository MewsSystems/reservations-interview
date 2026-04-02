using Validators;

namespace Reservations.Tests.Unit.Validators;

public class EmailValidatorTests
{
    //Validate tests
    [Fact]
    public void Validate_ShouldThrow_WhenEmailIsNull()
    {
        Assert.Throws<ArgumentException>(() =>
            EmailValidator.Validate(null));
    }

    [Fact]
    public void Validate_ShouldThrow_WhenEmailIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            EmailValidator.Validate(""));
    }

    [Fact]
    public void Validate_ShouldThrow_WhenEmailIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() =>
            EmailValidator.Validate("   "));
    }

    [Fact]
    public void Validate_ShouldThrow_WhenEmailIsInvalid()
    {
        Assert.Throws<ArgumentException>(() =>
            EmailValidator.Validate("invalid-email"));
    }

    [Fact]
    public void Validate_ShouldPass_WhenEmailIsValid()
    {
        EmailValidator.Validate("test@example.com");
    }

    // IsValid tests
    [Fact]
    public void IsValid_ShouldReturnTrue_ForValidEmail()
    {
        var result = EmailValidator.IsValid("test@example.com");

        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenMissingAt()
    {
        var result = EmailValidator.IsValid("testexample.com");

        Assert.False(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenMissingDomainDot()
    {
        var result = EmailValidator.IsValid("test@example");

        Assert.False(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_ForInvalidFormat()
    {
        var result = EmailValidator.IsValid("test@.com");

        Assert.False(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenNull()
    {
        var result = EmailValidator.IsValid(null);

        Assert.False(result);
    }
}