namespace Application.Interfaces;

/// <summary>
/// Encrypted access and refresh token pair retrieved from the calendar token store.
/// </summary>
public sealed record CalendarTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime TokenExpiry);

/// <summary>
/// Persists and retrieves Google Calendar OAuth tokens encrypted at rest.
/// </summary>
public interface ICalendarTokenStore
{
    /// <summary>
    /// Stores encrypted access and refresh tokens for the user's Google Calendar integration.
    /// Creates a new record or replaces an existing one.
    /// </summary>
    Task StoreTokensAsync(
        Guid userId,
        string accessToken,
        string refreshToken,
        DateTime tokenExpiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the decrypted tokens for an active integration, or <c>null</c> if none exists.
    /// </summary>
    Task<CalendarTokenResult?> GetActiveTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces only the access token (e.g. after a successful refresh).
    /// </summary>
    Task UpdateAccessTokenAsync(
        Guid userId,
        string newAccessToken,
        DateTime newExpiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the integration as disconnected so subsequent syncs are skipped.
    /// Called on unrecoverable token refresh failure.
    /// </summary>
    Task MarkDisconnectedAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
