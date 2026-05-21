using Domain.Entities;

namespace Application.Interfaces;

public interface IAccountLockoutService
{
    int MaxFailedAttempts { get; }

    bool IsLocked(User user, DateTime utcNow);

    int GetRemainingAttempts(User user);

    Task RegisterFailedAttemptAsync(User user, CancellationToken cancellationToken);

    Task ResetFailedAttemptsAsync(User user, CancellationToken cancellationToken);

    Task UnlockAsync(User user, CancellationToken cancellationToken);
}
