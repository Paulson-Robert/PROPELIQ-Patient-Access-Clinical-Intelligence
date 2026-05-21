using Domain.Entities;
using Domain.Enums;
using Infrastructure.Auth;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Security.Tests;

public sealed class AccountLockoutServiceTests
{
    [Fact]
    public async Task LocksUserAfterFiveFailedAttemptsAndResetsOnUnlock()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lockout-{Guid.NewGuid()}")
            .Options;

        await using var context = new ApplicationDbContext(options, null);
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "lockout@example.com",
            PasswordHash = "hash",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AccountLockoutService(context);

        for (var i = 0; i < 5; i++)
            await service.RegisterFailedAttemptAsync(user, CancellationToken.None);

        Assert.True(service.IsLocked(user, DateTime.UtcNow));
        Assert.Equal(0, service.GetRemainingAttempts(user));

        await service.UnlockAsync(user, CancellationToken.None);

        Assert.False(service.IsLocked(user, DateTime.UtcNow));
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntilUtc);
    }
}
