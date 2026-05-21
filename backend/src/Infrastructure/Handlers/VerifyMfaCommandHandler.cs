using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

public sealed class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, VerifyMfaResult>
{
    private const string MfaChallengeCachePrefix = "auth:mfa:challenge:";
    private const string LastStepCachePrefix = "auth:mfa:last-step:";

    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ITotpService _totpService;

    public VerifyMfaCommandHandler(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        ITotpService totpService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _totpService = totpService;
    }

    public async Task<VerifyMfaResult> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return new VerifyMfaResult(false, "not_found", "Account not found.", Guid.Empty, normalizedEmail, UserRole.Patient, request.Method, 0, null, false);
        }

        if (!user.IsActive)
        {
            return new VerifyMfaResult(false, "account_disabled", "Your account has been disabled. Contact your administrator.", user.UserId, user.Email, user.Role, request.Method, 0, null, false);
        }

        if (user.Role is not (UserRole.Staff or UserRole.Admin))
        {
            return new VerifyMfaResult(false, "not_required", "MFA verification is only required for staff and admin accounts.", user.UserId, user.Email, user.Role, request.Method, 0, null, false);
        }

        var cacheKey = BuildChallengeCacheKey(user.Email);
        var challenge = await _cacheService.GetAsync<MfaChallengeState>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (challenge is null)
        {
            return new VerifyMfaResult(false, "expired", "Code expired. Request a new code to continue.", user.UserId, user.Email, user.Role, request.Method, 0, null, true);
        }

        if (challenge.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await _cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            return new VerifyMfaResult(false, "expired", "Code expired. Request a new code to continue.", user.UserId, user.Email, user.Role, request.Method, 0, challenge.ExpiresAtUtc, true);
        }

        if (user.MfaMethod != request.Method)
        {
            return new VerifyMfaResult(false, "method_mismatch", "Use the MFA method configured for this account.", user.UserId, user.Email, user.Role, request.Method, challenge.AttemptsRemaining, challenge.ExpiresAtUtc, false);
        }

        if (request.Method == MfaMethod.Totp)
        {
            if (string.IsNullOrWhiteSpace(user.MfaSecret))
            {
                return new VerifyMfaResult(false, "setup_required", "MFA setup is required before verification.", user.UserId, user.Email, user.Role, request.Method, challenge.AttemptsRemaining, challenge.ExpiresAtUtc, false);
            }

            var lastTimeStep = await _cacheService.GetAsync<long?>(BuildLastStepCacheKey(user.Email), cancellationToken).ConfigureAwait(false);
            var isValid = _totpService.Validate(user.MfaSecret, request.Code.Trim(), out var timeStepMatched);
            if (!isValid)
            {
                return await RegisterFailureAsync(user, challenge, request.Method, cancellationToken).ConfigureAwait(false);
            }

            if (lastTimeStep.HasValue && timeStepMatched <= lastTimeStep.Value)
            {
                return await RegisterFailureAsync(user, challenge, request.Method, cancellationToken).ConfigureAwait(false);
            }

            await _cacheService.SetAsync(BuildLastStepCacheKey(user.Email), timeStepMatched, TimeSpan.FromMinutes(15), cancellationToken).ConfigureAwait(false);
            await _cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);

            return new VerifyMfaResult(true, "success", "MFA verification completed.", user.UserId, user.Email, user.Role, request.Method, 3, null, false, timeStepMatched);
        }

        if (!string.Equals(challenge.SmsCode, request.Code.Trim(), StringComparison.Ordinal))
        {
            return await RegisterFailureAsync(user, challenge, request.Method, cancellationToken).ConfigureAwait(false);
        }

        await _cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        return new VerifyMfaResult(true, "success", "MFA verification completed.", user.UserId, user.Email, user.Role, request.Method, challenge.AttemptsRemaining, null, false);
    }

    private async Task<VerifyMfaResult> RegisterFailureAsync(
        User user,
        MfaChallengeState challenge,
        MfaMethod method,
        CancellationToken cancellationToken)
    {
        var nextAttempts = challenge.AttemptsRemaining - 1;
        if (nextAttempts <= 0)
        {
            await _cacheService.RemoveAsync(BuildChallengeCacheKey(user.Email), cancellationToken).ConfigureAwait(false);
            await _cacheService.RemoveAsync(BuildLastStepCacheKey(user.Email), cancellationToken).ConfigureAwait(false);

            return new VerifyMfaResult(false, "locked", "Too many failed attempts — please log in again.", user.UserId, user.Email, user.Role, method, 0, challenge.ExpiresAtUtc, false);
        }

        await _cacheService.SetAsync(
                BuildChallengeCacheKey(user.Email),
                challenge with { AttemptsRemaining = nextAttempts },
                TimeSpan.FromMinutes(5),
                cancellationToken)
            .ConfigureAwait(false);

        return new VerifyMfaResult(false, "invalid", "Invalid MFA code.", user.UserId, user.Email, user.Role, method, nextAttempts, challenge.ExpiresAtUtc, false);
    }

    private static string BuildChallengeCacheKey(string email) =>
        MfaChallengeCachePrefix + email.Trim().ToLowerInvariant();

    private static string BuildLastStepCacheKey(string email) =>
        LastStepCachePrefix + email.Trim().ToLowerInvariant();
}