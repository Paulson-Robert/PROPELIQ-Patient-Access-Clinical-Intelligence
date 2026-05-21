using Application.Interfaces;

namespace Infrastructure.Auth;

public sealed class SessionService : ISessionService
{
    private const string SessionCachePrefix = "auth:session:";

    private readonly ICacheService _cacheService;

    public SessionService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public Task CreateSessionAsync(
        string tokenId,
        Guid userId,
        string email,
        string role,
        TimeSpan expiry,
        CancellationToken cancellationToken = default) =>
        _cacheService.SetAsync(
            BuildSessionCacheKey(tokenId),
            new SessionState(userId, email, role),
            expiry,
            cancellationToken);

    public Task<SessionState?> GetSessionAsync(
        string tokenId,
        CancellationToken cancellationToken = default) =>
        _cacheService.GetAsync<SessionState>(BuildSessionCacheKey(tokenId), cancellationToken);

    public async Task<bool> RefreshSessionAsync(
        string tokenId,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync(tokenId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return false;
        }

        await _cacheService.SetAsync(
                BuildSessionCacheKey(tokenId),
                session,
                expiry,
                cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    public Task RemoveSessionAsync(
        string tokenId,
        CancellationToken cancellationToken = default) =>
        _cacheService.RemoveAsync(BuildSessionCacheKey(tokenId), cancellationToken);

    private static string BuildSessionCacheKey(string tokenId) =>
        $"{SessionCachePrefix}{tokenId}";
}
