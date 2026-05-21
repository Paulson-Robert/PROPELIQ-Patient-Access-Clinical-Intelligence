using System.Security.Cryptography;
using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using Infrastructure.Auth;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private const int BcryptWorkFactor = 12;
    private const string VerificationCachePrefix = "auth:verify:";

    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly IJwtTokenService _tokenService;
    private readonly GoogleOAuthService _googleOAuthService;
    private readonly MicrosoftOAuthService _microsoftOAuthService;
    private readonly AuthSettings _authSettings;

    public AuthController(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        IEmailService emailService,
        IBackgroundJobClient backgroundJobs,
        IJwtTokenService tokenService,
        GoogleOAuthService googleOAuthService,
        MicrosoftOAuthService microsoftOAuthService,
        IOptions<AuthSettings> authSettings)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _emailService = emailService;
        _backgroundJobs = backgroundJobs;
        _tokenService = tokenService;
        _googleOAuthService = googleOAuthService;
        _microsoftOAuthService = microsoftOAuthService;
        _authSettings = authSettings.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPatientCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var duplicateEmail = await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        if (duplicateEmail)
        {
            return Conflict(new
            {
                code = "duplicate_email",
                message = "An account with this email already exists. Please sign in or reset your password.",
            });
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, BcryptWorkFactor),
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now,
            MfaEnabled = false,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var verificationToken = GenerateOpaqueToken();
        var verificationCacheKey = BuildVerificationCacheKey(verificationToken);
        await _cacheService.SetAsync(
                verificationCacheKey,
                new EmailVerificationPayload(user.UserId, user.Email),
                TimeSpan.FromMinutes(_authSettings.VerificationTokenMinutes),
                cancellationToken)
            .ConfigureAwait(false);

        var verificationLink =
            $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";
        _backgroundJobs.Enqueue<IEmailService>(service =>
            service.SendVerificationEmailAsync(user.Email, verificationLink));

        return Accepted(new
        {
            status = "pending_verification",
            message = "Registration created. Check your email for the verification link.",
            email = user.Email,
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                u => u.Email == normalizedEmail && u.AuthProvider == AuthProvider.Local,
                cancellationToken)
            .ConfigureAwait(false);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Unauthorized(new
            {
                code = "invalid_credentials",
                message = "Email or password is incorrect.",
            });
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return Unauthorized(new
            {
                code = "invalid_credentials",
                message = "Email or password is incorrect.",
            });
        }

        if (!user.IsActive)
        {
            var accountDisabledMessage = user.Role is UserRole.Staff or UserRole.Admin
                ? "Your account has been disabled. Contact your administrator."
                : "Please verify your email before logging in.";

            return StatusCode(StatusCodes.Status423Locked, new
            {
                code = user.Role is UserRole.Staff or UserRole.Admin ? "account_disabled" : "account_not_verified",
                message = accountDisabledMessage,
            });
        }

        if (user.Role is UserRole.Staff or UserRole.Admin)
        {
            var challengeState = user.MfaEnabled ? "verify" : "setup";
            var challengeMethod = user.MfaMethod.ToString().ToLowerInvariant();

            return Ok(new
            {
                mfaChallenge = new
                {
                    state = challengeState,
                    method = challengeMethod,
                    attemptsRemaining = 3,
                    expiresAtUtc = DateTime.UtcNow.AddSeconds(30),
                },
                user = new
                {
                    email = user.Email,
                    role = user.Role.ToString().ToLowerInvariant(),
                },
            });
        }

        var tokenPayload = new AuthTokenPayload(
            user.UserId,
            user.Email,
            user.Role.ToString().ToLowerInvariant());
        var token = await _tokenService.IssueTokenAsync(tokenPayload, cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            accessToken = token.AccessToken,
            token.ExpiresAtUtc,
            token.TokenType,
            user = new
            {
                email = user.Email,
                role = user.Role.ToString().ToLowerInvariant(),
            },
        });
    }

    [HttpPost("social/start")]
    [AllowAnonymous]
    public async Task<IActionResult> StartSocialLogin(
        [FromBody] SocialStartRequest request,
        CancellationToken cancellationToken)
    {
        var provider = request.Provider.Trim().ToLowerInvariant();

        var callbackResult = provider switch
        {
            "google" => await _googleOAuthService.ExchangeCodeAsync(
                code: "dev-google-code",
                emailHint: request.EmailHint,
                overrideEmail: request.Email,
                cancellationToken: cancellationToken).ConfigureAwait(false),
            "microsoft" => await _microsoftOAuthService.ExchangeCodeAsync(
                code: "dev-microsoft-code",
                emailHint: request.EmailHint,
                overrideEmail: request.Email,
                cancellationToken: cancellationToken).ConfigureAwait(false),
            _ => SocialAuthResult.Failure(
                AuthProvider.Local,
                OAuthErrorCode.InvalidCode,
                "Unsupported social provider."),
        };

        if (!callbackResult.IsSuccess)
        {
            return callbackResult.ErrorCode switch
            {
                OAuthErrorCode.ConsentDenied => BadRequest(new
                {
                    code = "consent_denied",
                    message = callbackResult.ErrorDescription,
                    redirectPath = BuildLoginRedirect("oauthError=consent_denied"),
                }),
                OAuthErrorCode.ProviderUnavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    code = "provider_unavailable",
                    message = callbackResult.ErrorDescription,
                    redirectPath = BuildLoginRedirect($"providerOutage={provider}"),
                }),
                OAuthErrorCode.EmailMismatch => Conflict(new
                {
                    code = "email_mismatch",
                    message = callbackResult.ErrorDescription,
                    redirectPath = BuildLoginRedirect("oauthError=email_mismatch"),
                }),
                _ => BadRequest(new
                {
                    code = "invalid_social_request",
                    message = callbackResult.ErrorDescription,
                }),
            };
        }

        var normalizedEmail = callbackResult.Email!.Trim().ToLowerInvariant();
        var existingUser = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;
        if (existingUser is null)
        {
            existingUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = normalizedEmail,
                AuthProvider = callbackResult.Provider,
                Role = UserRole.Patient,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                MfaEnabled = false,
            };
            _dbContext.Users.Add(existingUser);
        }
        else
        {
            existingUser.AuthProvider = callbackResult.Provider;
            existingUser.IsActive = true;
            existingUser.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            redirectUrl = $"/dashboard/{existingUser.Role.ToString().ToLowerInvariant()}?provider={provider}",
        });
    }

    [HttpGet("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(token);
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return BadRequest(new
            {
                code = "invalid_verification_token",
                message = "Verification token is required.",
            });
        }

        var verificationCacheKey = BuildVerificationCacheKey(command.Token);
        var payload = await _cacheService
            .GetAsync<EmailVerificationPayload>(verificationCacheKey, cancellationToken)
            .ConfigureAwait(false);
        if (payload is null)
        {
            return StatusCode(StatusCodes.Status410Gone, new
            {
                code = "verification_expired",
                message = "Verification link expired. Request a new link.",
            });
        }

        await _cacheService.RemoveAsync(verificationCacheKey, cancellationToken).ConfigureAwait(false);

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.UserId == payload.UserId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return NotFound(new
            {
                code = "account_not_found",
                message = "Associated account was not found.",
            });
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            status = "verified",
            email = user.Email,
        });
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                u => u.Email == normalizedEmail && u.AuthProvider == AuthProvider.Local,
                cancellationToken)
            .ConfigureAwait(false);

        if (user is not null && !user.IsActive)
        {
            var verificationToken = GenerateOpaqueToken();
            var verificationCacheKey = BuildVerificationCacheKey(verificationToken);

            await _cacheService.SetAsync(
                    verificationCacheKey,
                    new EmailVerificationPayload(user.UserId, user.Email),
                    TimeSpan.FromMinutes(_authSettings.VerificationTokenMinutes),
                    cancellationToken)
                .ConfigureAwait(false);

            var verificationLink =
                $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";
            _backgroundJobs.Enqueue<IEmailService>(service =>
                service.SendVerificationEmailAsync(user.Email, verificationLink));
        }

        return Ok(new
        {
            status = "verification_resent",
            message = "If an inactive account exists, a new verification email was sent.",
        });
    }

    [HttpPost("social/google/callback")]
    [AllowAnonymous]
    public Task<IActionResult> GoogleCallback(
        [FromBody] SocialOAuthCallbackRequest request,
        CancellationToken cancellationToken) =>
        AuthenticateSocialAsync(
            request,
            _googleOAuthService.ExchangeCodeAsync,
            AuthProvider.Google,
            cancellationToken);

    [HttpPost("social/microsoft/callback")]
    [AllowAnonymous]
    public Task<IActionResult> MicrosoftCallback(
        [FromBody] SocialOAuthCallbackRequest request,
        CancellationToken cancellationToken) =>
        AuthenticateSocialAsync(
            request,
            _microsoftOAuthService.ExchangeCodeAsync,
            AuthProvider.Microsoft,
            cancellationToken);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return Unauthorized(new
            {
                code = "missing_authorization",
                message = "Bearer token is required.",
            });
        }

        var headerValue = authorizationHeader.ToString();
        var token = headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? headerValue[7..].Trim()
            : headerValue;

        var refreshedToken = await _tokenService
            .RefreshTokenAsync(token, cancellationToken)
            .ConfigureAwait(false);
        if (refreshedToken is null)
        {
            return Unauthorized(new
            {
                code = "invalid_session",
                message = "Session is invalid or expired.",
            });
        }

        return Ok(refreshedToken);
    }

    private async Task<IActionResult> AuthenticateSocialAsync(
        SocialOAuthCallbackRequest request,
        Func<string?, string?, string?, CancellationToken, Task<SocialAuthResult>> exchangeCode,
        AuthProvider provider,
        CancellationToken cancellationToken)
    {
        var authResult = await exchangeCode(
                request.Code,
                request.EmailHint,
                request.Email,
                cancellationToken)
            .ConfigureAwait(false);

        if (!authResult.IsSuccess)
        {
            return authResult.ErrorCode switch
            {
                OAuthErrorCode.ConsentDenied => BadRequest(new
                {
                    code = "consent_denied",
                    message = authResult.ErrorDescription,
                    redirectPath = BuildLoginRedirect("oauthError=consent_denied"),
                }),
                OAuthErrorCode.ProviderUnavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    code = "provider_unavailable",
                    provider = provider.ToString().ToLowerInvariant(),
                    message = authResult.ErrorDescription,
                }),
                OAuthErrorCode.EmailMismatch => Conflict(new
                {
                    code = "email_mismatch",
                    message = authResult.ErrorDescription,
                    redirectPath = BuildLoginRedirect("oauthError=email_mismatch"),
                }),
                _ => BadRequest(new
                {
                    code = "invalid_oauth_callback",
                    message = authResult.ErrorDescription,
                }),
            };
        }

        var normalizedEmail = authResult.Email!.Trim().ToLowerInvariant();
        var existingUser = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;
        var linkedExistingAccount = false;

        if (existingUser is null)
        {
            existingUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = normalizedEmail,
                AuthProvider = provider,
                Role = UserRole.Patient,
                PasswordHash = null,
                IsActive = true,
                MfaEnabled = false,
                CreatedAt = now,
                UpdatedAt = now,
            };

            _dbContext.Users.Add(existingUser);
        }
        else
        {
            if (existingUser.AuthProvider == AuthProvider.Local)
            {
                existingUser.AuthProvider = provider;
                existingUser.IsActive = true;
                existingUser.UpdatedAt = now;
                linkedExistingAccount = true;
            }
            else if (existingUser.AuthProvider != provider)
            {
                return Conflict(new
                {
                    code = "email_mismatch",
                    message = "This email is already linked to another social provider.",
                    redirectPath = BuildLoginRedirect("oauthError=email_mismatch"),
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tokenPayload = new AuthTokenPayload(
            existingUser.UserId,
            existingUser.Email,
            existingUser.Role.ToString().ToLowerInvariant());
        var token = await _tokenService.IssueTokenAsync(tokenPayload, cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            token.AccessToken,
            token.ExpiresAtUtc,
            token.TokenType,
            user = new
            {
                userId = existingUser.UserId,
                email = existingUser.Email,
                role = existingUser.Role.ToString().ToLowerInvariant(),
                linkedAccount = linkedExistingAccount,
            },
        });
    }

    private static string GenerateOpaqueToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string BuildVerificationCacheKey(string token) =>
        $"{VerificationCachePrefix}{token}";

    private string BuildLoginRedirect(string query) =>
        $"{_authSettings.LoginRedirectPath}?{query}";

    private sealed record EmailVerificationPayload(Guid UserId, string Email);

    public sealed record LoginRequest(string Email, string Password);
    public sealed record SocialStartRequest(string Provider, string? Email = null, string? EmailHint = null);
    public sealed record SocialOAuthCallbackRequest(string? Code, string? Email = null, string? EmailHint = null);
    public sealed record ResendVerificationRequest(string Email);
}
