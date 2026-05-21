namespace Application.Interfaces;

/// <summary>
/// Token pair returned by Google OAuth token endpoint.
/// </summary>
public sealed record GoogleTokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresInSeconds);

/// <summary>
/// Details needed to create a Google Calendar event for an appointment.
/// </summary>
public sealed record AppointmentEventRequest(
    Guid AppointmentId,
    string ProviderName,
    string Specialty,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc);

/// <summary>
/// Google Calendar API operations and OAuth 2.0 token exchange.
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// Exchanges an OAuth 2.0 authorization code for access and refresh tokens.
    /// Returns <c>null</c> if the exchange fails.
    /// </summary>
    Task<GoogleTokenResponse?> ExchangeAuthCodeAsync(
        string authCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uses a refresh token to obtain a new access token.
    /// Returns <c>null</c> if the refresh fails (e.g. token revoked).
    /// </summary>
    Task<GoogleTokenResponse?> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Google Calendar event for an appointment and returns the event ID,
    /// or <c>null</c> if creation fails.
    /// </summary>
    Task<string?> CreateEventAsync(
        string accessToken,
        AppointmentEventRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds and deletes the Google Calendar event associated with the given appointment.
    /// Returns <c>true</c> if the event was removed or was not found (idempotent).
    /// </summary>
    Task<bool> DeleteEventByAppointmentIdAsync(
        string accessToken,
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}
