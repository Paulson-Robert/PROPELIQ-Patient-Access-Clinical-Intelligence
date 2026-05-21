namespace Application.Interfaces;

public interface ISessionService
{
    Task CreateSessionAsync(
        string tokenId,
        Guid userId,
        string email,
        string role,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task<SessionState?> GetSessionAsync(string tokenId, CancellationToken cancellationToken = default);

    Task<bool> RefreshSessionAsync(
        string tokenId,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task RemoveSessionAsync(string tokenId, CancellationToken cancellationToken = default);
}

public sealed record SessionState(Guid UserId, string Email, string Role);
