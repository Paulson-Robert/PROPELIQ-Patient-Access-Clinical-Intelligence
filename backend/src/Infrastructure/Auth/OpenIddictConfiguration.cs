using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Infrastructure.Services;

namespace Infrastructure.Auth;

public sealed class AuthSettings
{
    public const string SectionName = "Authentication";

    public string JwtIssuer { get; set; } = "PropelIQ";
    public string JwtAudience { get; set; } = "PropelIQ.Client";
    public string JwtSigningKey { get; set; } = "replace-with-a-strong-32-byte-min-secret";
    public int AccessTokenMinutes { get; set; } = 15;
    public int VerificationTokenMinutes { get; set; } = 30;
    public string LoginRedirectPath { get; set; } = "/auth/login";
}

public sealed class AuthSettingsValidator : IValidateOptions<AuthSettings>
{
    public ValidateOptionsResult Validate(string? name, AuthSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.JwtIssuer) ||
            string.IsNullOrWhiteSpace(options.JwtAudience) ||
            string.IsNullOrWhiteSpace(options.JwtSigningKey))
        {
            return ValidateOptionsResult.Fail("Authentication settings are incomplete.");
        }

        if (options.JwtSigningKey.Length < 32)
        {
            return ValidateOptionsResult.Fail("Authentication:JwtSigningKey must be at least 32 characters.");
        }

        if (options.AccessTokenMinutes <= 0)
        {
            return ValidateOptionsResult.Fail("Authentication:AccessTokenMinutes must be greater than zero.");
        }

        if (options.VerificationTokenMinutes <= 0)
        {
            return ValidateOptionsResult.Fail("Authentication:VerificationTokenMinutes must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}

public sealed record AuthTokenPayload(Guid UserId, string Email, string Role);
public sealed record AuthTokenResult(string AccessToken, DateTime ExpiresAtUtc, string TokenType = "Bearer");

public interface IJwtTokenService
{
    Task<AuthTokenResult> IssueTokenAsync(AuthTokenPayload payload, CancellationToken cancellationToken = default);
    Task<AuthTokenResult?> RefreshTokenAsync(string? currentAccessToken, CancellationToken cancellationToken = default);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly ISessionService _sessionService;
    private readonly AuthSettings _settings;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _validationParameters;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(ISessionService sessionService, IOptions<AuthSettings> settings)
    {
        _sessionService = sessionService;
        _settings = settings.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSigningKey));
        _signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        _validationParameters = BuildValidationParameters(_settings);
    }

    public async Task<AuthTokenResult> IssueTokenAsync(
        AuthTokenPayload payload,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_settings.AccessTokenMinutes);
        var tokenId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, payload.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new(JwtRegisteredClaimNames.Email, payload.Email),
            new(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
            new(ClaimTypes.Email, payload.Email),
            new(ClaimTypes.Role, payload.Role),
        };

        var jwtToken = new JwtSecurityToken(
            issuer: _settings.JwtIssuer,
            audience: _settings.JwtAudience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        var serializedToken = _tokenHandler.WriteToken(jwtToken);
        await _sessionService.CreateSessionAsync(
            tokenId,
            payload.UserId,
            payload.Email,
            payload.Role,
            TimeSpan.FromMinutes(_settings.AccessTokenMinutes),
            cancellationToken)
            .ConfigureAwait(false);

        return new AuthTokenResult(serializedToken, expiresAtUtc);
    }

    public async Task<AuthTokenResult?> RefreshTokenAsync(
        string? currentAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentAccessToken))
        {
            return null;
        }

        ClaimsPrincipal principal;
        try
        {
            principal = _tokenHandler.ValidateToken(
                currentAccessToken,
                _validationParameters,
                out _);
        }
        catch
        {
            return null;
        }

        var tokenId = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return null;
        }

        var existingSession = await _sessionService
            .GetSessionAsync(tokenId, cancellationToken)
            .ConfigureAwait(false);
        if (existingSession is null)
        {
            return null;
        }

        await _sessionService.RemoveSessionAsync(tokenId, cancellationToken).ConfigureAwait(false);
        return await IssueTokenAsync(
                new AuthTokenPayload(
                    existingSession.UserId,
                    existingSession.Email,
                    existingSession.Role),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static TokenValidationParameters BuildValidationParameters(AuthSettings settings)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSigningKey));

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = settings.JwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    }
}

public static class OpenIddictConfiguration
{
    public static IServiceCollection AddOpenIddictAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        services.AddSingleton<IValidateOptions<AuthSettings>, AuthSettingsValidator>();
        services.AddOptions<AuthSettings>()
            .Bind(configuration.GetSection(AuthSettings.SectionName))
            .ValidateOnStart();

        var settings = configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>() ?? new AuthSettings();
        var tokenValidationParameters = BuildValidationParameters(settings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var roleClaims = context.Principal?.FindAll(ClaimTypes.Role)
                            .Select(claim => claim.Value)
                            .Where(value => !string.IsNullOrWhiteSpace(value))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray() ?? [];

                        if (roleClaims.Length != 1)
                        {
                            context.Fail("Exactly one role claim is required.");
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            code = "session_expired",
                            message = "Session expired. Please sign in again.",
                        }).ConfigureAwait(false);
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            code = "forbidden",
                            message = "You do not have permission to access this resource.",
                        }).ConfigureAwait(false);
                    },
                };
            });

        services.AddAuthorization();

        services.AddOpenIddict()
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("connect/token");
                options.AllowPasswordFlow();
                options.AcceptAnonymousClients();
                options.DisableAccessTokenEncryption();
                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(settings.AccessTokenMinutes));

                if (environment?.IsDevelopment() == true)
                {
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                }
                else
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }

                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<GoogleOAuthService>();
        services.AddScoped<MicrosoftOAuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<ITotpService, TotpService>();

        return services;
    }

    private static TokenValidationParameters BuildValidationParameters(AuthSettings settings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = settings.JwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    }
}
