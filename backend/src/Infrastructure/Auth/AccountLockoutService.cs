using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Auth;

public sealed class AccountLockoutService : IAccountLockoutService
{
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly ApplicationDbContext _dbContext;

    public AccountLockoutService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int MaxFailedAttempts => 5;

    public bool IsLocked(User user, DateTime utcNow) =>
        user.LockedUntilUtc.HasValue && user.LockedUntilUtc.Value > utcNow;

    public int GetRemainingAttempts(User user)
    {
        var remaining = MaxFailedAttempts - user.FailedLoginAttempts;
        return remaining < 0 ? 0 : remaining;
    }

    public async Task RegisterFailedAttemptAsync(User user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        if (!IsLocked(user, now) && user.LockedUntilUtc.HasValue && user.LockedUntilUtc <= now)
        {
            user.FailedLoginAttempts = 0;
            user.LockedUntilUtc = null;
        }

        user.FailedLoginAttempts += 1;
        user.LastFailedLoginAtUtc = now;
        user.UpdatedAt = now;

        if (user.FailedLoginAttempts >= MaxFailedAttempts)
            user.LockedUntilUtc = now.Add(LockoutDuration);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ResetFailedAttemptsAsync(User user, CancellationToken cancellationToken)
    {
        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;
        user.LastFailedLoginAtUtc = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task UnlockAsync(User user, CancellationToken cancellationToken) =>
        ResetFailedAttemptsAsync(user, cancellationToken);
}
