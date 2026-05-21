using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Interfaces;
using Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace API.Middleware;

public sealed class SessionSlidingExpiryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthSettings _authSettings;

    public SessionSlidingExpiryMiddleware(RequestDelegate next, IOptions<AuthSettings> authSettings)
    {
        _next = next;
        _authSettings = authSettings.Value;
    }

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var roleClaims = context.User.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (roleClaims.Length != 1)
            {
                await WriteUnauthorizedAsync(
                        context,
                        "invalid_role_claims",
                        "Exactly one role claim is required.")
                    .ConfigureAwait(false);
                return;
            }

            var tokenId = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                await WriteUnauthorizedAsync(
                        context,
                        "invalid_session",
                        "Token session identifier is missing.")
                    .ConfigureAwait(false);
                return;
            }

            var refreshed = await sessionService
                .RefreshSessionAsync(
                    tokenId,
                    TimeSpan.FromMinutes(_authSettings.AccessTokenMinutes),
                    context.RequestAborted)
                .ConfigureAwait(false);

            if (!refreshed)
            {
                await WriteUnauthorizedAsync(
                        context,
                        "session_expired",
                        "Session expired. Please sign in again.")
                    .ConfigureAwait(false);
                return;
            }
        }

        await _next(context).ConfigureAwait(false);
    }

    private static Task WriteUnauthorizedAsync(HttpContext context, string code, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsJsonAsync(new
        {
            code,
            message,
        });
    }
}

public static class SessionSlidingExpiryMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionSlidingExpiry(this IApplicationBuilder app) =>
        app.UseMiddleware<SessionSlidingExpiryMiddleware>();
}
