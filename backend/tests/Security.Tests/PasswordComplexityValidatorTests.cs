using Application.Commands;
using Application.Validators;
using Xunit;

namespace Security.Tests;

public sealed class PasswordComplexityValidatorTests
{
    private readonly RegisterPatientCommandValidator _validator = new();

    [Theory]
    [InlineData("weak")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoDigits!!")]
    [InlineData("NoSpecial1")]
    public async Task RejectsWeakPassword(string password)
    {
        var result = await _validator.ValidateAsync(new RegisterPatientCommand("user@example.com", password));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AcceptsComplexPassword()
    {
        var result = await _validator.ValidateAsync(new RegisterPatientCommand("user@example.com", "StrongPass1!"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("patient")]
    [InlineData("staff")]
    [InlineData(null)]
    public async Task AcceptsSelfRegistrationRoles(string? role)
    {
        var result = await _validator.ValidateAsync(new RegisterPatientCommand(
            "user@example.com",
            "StrongPass1!",
            Role: role));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("clinician")]
    public async Task RejectsUnsupportedSelfRegistrationRoles(string role)
    {
        var result = await _validator.ValidateAsync(new RegisterPatientCommand(
            "user@example.com",
            "StrongPass1!",
            Role: role));

        Assert.False(result.IsValid);
    }
}
