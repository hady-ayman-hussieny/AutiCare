using AutiCare.Application.DTOs;
using AutiCare.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace AutiCare.Tests;

public class PhoneValidationTests
{
    private readonly RegisterRequestValidator _registerValidator;
    private readonly UpdateProfileRequestValidator _updateProfileValidator;

    public PhoneValidationTests()
    {
        _registerValidator = new RegisterRequestValidator();
        _updateProfileValidator = new UpdateProfileRequestValidator();
    }

    [Theory]
    [InlineData("01012345678")]
    [InlineData("01112345678")]
    [InlineData("01212345678")]
    [InlineData("01512345678")]
    public void Register_ValidPhone_ShouldNotHaveError(string phone)
    {
        var model = new RegisterRequest("Test User", "test@example.com", "Password123", "Parent", phone, null, null, null);
        var result = _registerValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("01312345678")] // Invalid prefix
    [InlineData("11012345678")] // Doesn't start with 01
    [InlineData("0101234567")]  // 10 digits
    [InlineData("010123456789")] // 12 digits
    [InlineData("010abc45678")] // Non-digits
    [InlineData("")]            // Empty
    [InlineData(null)]          // Null
    public void Register_InvalidPhone_ShouldHaveError(string phone)
    {
        var model = new RegisterRequest("Test User", "test@example.com", "Password123", "Parent", phone, null, null, null);
        var result = _registerValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Phone)
              .WithErrorMessage("Please enter a valid Egyptian mobile number.");
    }

    [Theory]
    [InlineData("01012345678")]
    [InlineData(null)]
    [InlineData("")]
    public void UpdateProfile_ValidOrEmptyPhone_ShouldNotHaveError(string phone)
    {
        var model = new UpdateProfileRequest(null, phone, null, null);
        var result = _updateProfileValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("01312345678")]
    [InlineData("0101234567")]
    public void UpdateProfile_InvalidPhone_ShouldHaveError(string phone)
    {
        var model = new UpdateProfileRequest(null, phone, null, null);
        var result = _updateProfileValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Phone)
              .WithErrorMessage("Please enter a valid Egyptian mobile number.");
    }
}
