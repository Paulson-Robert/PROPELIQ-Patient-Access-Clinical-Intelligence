using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Result of the Microsoft Outlook Calendar OAuth connect flow.
/// </summary>
public sealed record ConnectOutlookCalendarResult(bool Success, string? FailureReason);

/// <summary>
/// Exchanges the OAuth 2.0 authorization code returned by Microsoft's consent screen
/// for access and refresh tokens, then stores them encrypted (AC-01, AC-02).
/// </summary>
public sealed record ConnectOutlookCalendarCommand(
    Guid UserId,
    string AuthorizationCode) : IRequest<ConnectOutlookCalendarResult>;

internal sealed class ConnectOutlookCalendarCommandHandler
    : IRequestHandler<ConnectOutlookCalendarCommand, ConnectOutlookCalendarResult>
{
    private readonly IOutlookCalendarService _outlookCalendarService;
    private readonly IOutlookCalendarTokenStore _tokenStore;

    public ConnectOutlookCalendarCommandHandler(
        IOutlookCalendarService outlookCalendarService,
        IOutlookCalendarTokenStore tokenStore)
    {
        _outlookCalendarService = outlookCalendarService;
        _tokenStore = tokenStore;
    }

    public async Task<ConnectOutlookCalendarResult> Handle(
        ConnectOutlookCalendarCommand request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _outlookCalendarService
            .ExchangeAuthCodeAsync(request.AuthorizationCode, cancellationToken)
            .ConfigureAwait(false);

        if (tokenResponse is null)
        {
            return new ConnectOutlookCalendarResult(
                Success: false,
                FailureReason: "Failed to exchange authorization code with Microsoft.");
        }

        // Microsoft issues a refresh token only when offline_access scope is requested.
        // The scope must be requested in the consent screen OAuth URL.
        if (string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
        {
            return new ConnectOutlookCalendarResult(
                Success: false,
                FailureReason: "Microsoft did not return a refresh token. " +
                               "Ensure offline_access scope is included in the OAuth URL.");
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

        return new ConnectOutlookCalendarResult(Success: true, FailureReason: null);
    }
}
