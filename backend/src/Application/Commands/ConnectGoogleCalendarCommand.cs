using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Result of the Google Calendar OAuth connect flow.
/// </summary>
public sealed record ConnectGoogleCalendarResult(bool Success, string? FailureReason);

/// <summary>
/// Exchanges the OAuth 2.0 authorization code returned by Google's consent screen
/// for access and refresh tokens, then stores them encrypted (AC-01, AC-02).
/// </summary>
public sealed record ConnectGoogleCalendarCommand(
    Guid UserId,
    string AuthorizationCode) : IRequest<ConnectGoogleCalendarResult>;

internal sealed class ConnectGoogleCalendarCommandHandler
    : IRequestHandler<ConnectGoogleCalendarCommand, ConnectGoogleCalendarResult>
{
    private readonly ICalendarService _calendarService;
    private readonly ICalendarTokenStore _tokenStore;

    public ConnectGoogleCalendarCommandHandler(
        ICalendarService calendarService,
        ICalendarTokenStore tokenStore)
    {
        _calendarService = calendarService;
        _tokenStore = tokenStore;
    }

    public async Task<ConnectGoogleCalendarResult> Handle(
        ConnectGoogleCalendarCommand request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _calendarService
            .ExchangeAuthCodeAsync(request.AuthorizationCode, cancellationToken)
            .ConfigureAwait(false);

        if (tokenResponse is null)
        {
            return new ConnectGoogleCalendarResult(
                Success: false,
                FailureReason: "Failed to exchange authorization code with Google.");
        }

        // Google only issues a refresh token on the first authorization or after consent revocation.
        // access_type=offline must be requested in the consent screen URL.
        if (string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
        {
            return new ConnectGoogleCalendarResult(
                Success: false,
                FailureReason: "Google did not return a refresh token. " +
                               "Ensure access_type=offline and prompt=consent are set in the OAuth URL.");
        }

        var expiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds);

        await _tokenStore
            .StoreTokensAsync(
                request.UserId,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                expiry,
                cancellationToken)
            .ConfigureAwait(false);

        return new ConnectGoogleCalendarResult(Success: true, FailureReason: null);
    }
}
