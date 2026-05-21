namespace Application.Interfaces;

/// <summary>
/// Token pair returned by the Microsoft identity platform token endpoint.
/// </summary>
public sealed record MicrosoftTokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresInSeconds);

/// <summary>
/// Microsoft Graph Calendar API operations and OAuth 2.0 token exchange.
/// Mirrors <see cref="ICalendarService"/> for the Microsoft provider.
/// </summary>
public interface IOutlookCalendarService
{
    /// <summary>
    /// Exchanges a Microsoft OAuth 2.0 authorization code for access and refresh tokens.
    /// Returns <c>null</c> if the exchange fails.
    /// </summary>
    Task<MicrosoftTokenResponse?> ExchangeAuthCodeAsync(
        string authCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uses a refresh token to obtain a new access token.
    /// Returns <c>null</c> if the refresh fails (e.g. consent revoked).
    /// </summary>
    Task<MicrosoftTokenResponse?> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Microsoft Graph calendar event for an appointment.
    /// Returns the Graph event ID, or <c>null</c> if creation fails.
    /// </summary>
    Task<string?> CreateEventAsync(
        string accessToken,
        AppointmentEventRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the Graph calendar event associated with the given appointment.
    /// Returns <c>true</c> on success or if no event existed (idempotent).
    /// </summary>
    Task<bool> DeleteEventByAppointmentIdAsync(
        string accessToken,
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Persists and retrieves Microsoft Calendar OAuth tokens encrypted at rest.
/// Mirrors <see cref="ICalendarTokenStore"/> for the Microsoft provider.
/// </summary>
public interface IOutlookCalendarTokenStore
{
    /// <summary>
    /// Stores encrypted access and refresh tokens for the user's Outlook Calendar integration.
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
    /// Marks the Outlook integration as disconnected so subsequent syncs are skipped.
    /// Called on unrecoverable token refresh failure or consent revocation (Edge Case).
    /// </summary>
    Task MarkDisconnectedAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
