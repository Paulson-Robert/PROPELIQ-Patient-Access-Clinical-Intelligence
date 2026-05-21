using Infrastructure.Auth;
using Xunit;

namespace Security.Tests;

public sealed class PasswordHashServiceTests
{
    [Fact]
    public void HashAndVerifyRoundTripUsesConfiguredWorkFactor()
    {
        var service = new PasswordHashService();
        const string password = "StrongPass1!";

        var hash = service.HashPassword(password);

        Assert.True(service.VerifyPassword(password, hash));
        Assert.False(service.VerifyPassword("WrongPass1!", hash));
        Assert.False(BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, 12));
    }
}
