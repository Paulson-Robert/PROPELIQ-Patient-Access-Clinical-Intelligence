using System.Security.Cryptography;
using Application.Commands;
using Application.Interfaces;
using Domain.Enums;
using Hangfire;
using Infrastructure.Auth;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Handlers;

public sealed class RequestPasswordResetCodeCommandHandler : IRequestHandler<RequestPasswordResetCodeCommand, RequestPasswordResetCodeResult>
{
    private const string PasswordResetCachePrefix = "auth:password-reset:";
    private const int ResetCodeAttempts = 5;
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(30);

    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly AuthSettings _authSettings;

    public RequestPasswordResetCodeCommandHandler(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        IBackgroundJobClient backgroundJobs,
        IOptions<AuthSettings> authSettings)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _backgroundJobs = backgroundJobs;
        _authSettings = authSettings.Value;
    }

    public async Task<RequestPasswordResetCodeResult> Handle(RequestPasswordResetCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var cacheKey = BuildResetCacheKey(normalizedEmail);
        var existingState = await _cacheService.GetAsync<PasswordResetCodeState>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (existingState is not null)
        {
            var remainingCooldown = existingState.ResendAvailableAtUtc - DateTime.UtcNow;
            if (remainingCooldown > TimeSpan.Zero)
            {
                return new RequestPasswordResetCodeResult(
                    true,
                    "accepted",
                    "If an account exists, a verification code has been sent to your email.",
                    (int)Math.Ceiling(remainingCooldown.TotalSeconds));
            }
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                u => u.Email == normalizedEmail && u.AuthProvider == AuthProvider.Local,
                cancellationToken)
            .ConfigureAwait(false);

        if (user is not null)
        {
            var verificationCode = GenerateVerificationCode();
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_authSettings.VerificationTokenMinutes);
            var resendAvailableAtUtc = DateTime.UtcNow.Add(ResendCooldown);

            await _cacheService.SetAsync(
                    cacheKey,
                    new PasswordResetCodeState(
                        normalizedEmail,
                        HashCode(verificationCode),
                        expiresAtUtc,
                        ResetCodeAttempts,
                        resendAvailableAtUtc),
                    TimeSpan.FromMinutes(_authSettings.VerificationTokenMinutes),
                    cancellationToken)
                .ConfigureAwait(false);

            _backgroundJobs.Enqueue<IEmailService>(service =>
                service.SendPasswordResetCodeAsync(normalizedEmail, verificationCode, CancellationToken.None));
        }

        return new RequestPasswordResetCodeResult(
            true,
            "accepted",
            "If an account exists, a verification code has been sent to your email.",
            (int)ResendCooldown.TotalSeconds);
    }

    private static string BuildResetCacheKey(string email) =>
        PasswordResetCachePrefix + email;

    private static string GenerateVerificationCode()
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return code.ToString("D6");
    }

    private static string HashCode(string code)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(code);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
