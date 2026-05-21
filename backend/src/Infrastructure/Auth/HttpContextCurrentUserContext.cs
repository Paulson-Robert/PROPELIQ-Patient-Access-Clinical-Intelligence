using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Auth;

/// <summary>
/// Reads the current user's identity context from the ASP.NET Core HTTP pipeline.
/// For background jobs (Hangfire), IHttpContextAccessor returns null — in that case
/// the context resolves to the "SYSTEM" actor (US_044 edge case).
/// </summary>
public sealed class HttpContextCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? ActorUserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User
                .FindFirstValue("sub");

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string ActorRole
    {
        get
        {
            var role = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.Role);

            return string.IsNullOrWhiteSpace(role) ? "System" : role;
        }
    }

    /// <inheritdoc />
    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
