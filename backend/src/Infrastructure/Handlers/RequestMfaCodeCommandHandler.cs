using System.Security.Cryptography;
using Application.Commands;
using Application.Interfaces;
using Domain.Enums;
using Hangfire;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

public sealed class RequestMfaCodeCommandHandler : IRequestHandler<RequestMfaCodeCommand, RequestMfaCodeResult>
{
    private const string MfaChallengeCachePrefix = "auth:mfa:challenge:";

    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ISmsService _smsService;
    private readonly IBackgroundJobClient _backgroundJobs;

    public RequestMfaCodeCommandHandler(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        ISmsService smsService,
        IBackgroundJobClient backgroundJobs)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _smsService = smsService;
        _backgroundJobs = backgroundJobs;
    }

    public async Task<RequestMfaCodeResult> Handle(RequestMfaCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return new RequestMfaCodeResult(false, "not_found", "Account not found.", Guid.Empty, normalizedEmail, UserRole.Patient, request.Method, 0, DateTime.UtcNow);
        }

        var cacheKey = BuildChallengeCacheKey(user.Email);
        var challenge = await _cacheService.GetAsync<MfaChallengeState>(cacheKey, cancellationToken).ConfigureAwait(false);

        if (request.Method == MfaMethod.Totp)
        {
            if (challenge is null)
            {
                return new RequestMfaCodeResult(false, "expired", "Code expired. Generate a new code in your authenticator app.", user.UserId, user.Email, user.Role, request.Method, 3, DateTime.UtcNow.AddSeconds(30));
            }

            var refreshedChallenge = challenge with { ExpiresAtUtc = DateTime.UtcNow.AddSeconds(30), AttemptsRemaining = 3 };
            await _cacheService.SetAsync(cacheKey, refreshedChallenge, TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

            return new RequestMfaCodeResult(true, "totp_refreshed", "Generate a fresh code in your authenticator app.", user.UserId, user.Email, user.Role, request.Method, refreshedChallenge.AttemptsRemaining, refreshedChallenge.ExpiresAtUtc);
        }

        if (!user.MfaEnabled || user.MfaMethod != MfaMethod.Sms || string.IsNullOrWhiteSpace(user.MfaPhoneNumber))
        {
            return new RequestMfaCodeResult(false, "sms_unavailable", "SMS MFA is not configured for this account.", user.UserId, user.Email, user.Role, request.Method, 0, DateTime.UtcNow);
        }

        var verificationCode = GenerateVerificationCode();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(5);

        await _cacheService.SetAsync(
                cacheKey,
                new MfaChallengeState(
                    user.Email,
                    user.Role,
                    MfaMethod.Sms,
                    user.MfaPhoneNumber,
                    verificationCode,
                    expiresAtUtc,
                    3),
                TimeSpan.FromMinutes(5),
                cancellationToken)
            .ConfigureAwait(false);

        _backgroundJobs.Enqueue<ISmsService>(service =>
            service.SendVerificationCodeAsync(user.MfaPhoneNumber!, verificationCode, CancellationToken.None));

        return new RequestMfaCodeResult(true, "sms_sent", "A fresh verification code was sent to your phone.", user.UserId, user.Email, user.Role, request.Method, 3, expiresAtUtc);
    }

    private static string BuildChallengeCacheKey(string email) =>
        MfaChallengeCachePrefix + email.Trim().ToLowerInvariant();

    private static string GenerateVerificationCode()
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return code.ToString("D6");
    }
}