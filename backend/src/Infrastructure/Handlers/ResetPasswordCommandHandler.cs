using Application.Commands;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Handlers;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private const string PasswordResetCachePrefix = "auth:password-reset:";

    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAccountLockoutService _accountLockoutService;

    public ResetPasswordCommandHandler(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        IPasswordHashService passwordHashService,
        IAccountLockoutService accountLockoutService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _passwordHashService = passwordHashService;
        _accountLockoutService = accountLockoutService;
    }

    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                u => u.Email == normalizedEmail && u.AuthProvider == AuthProvider.Local,
                cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return new ResetPasswordResult(
                true,
                "accepted",
                "If an account exists and code is valid, the password has been reset.");
        }

        var cacheKey = BuildResetCacheKey(normalizedEmail);
        var codeState = await _cacheService.GetAsync<PasswordResetCodeState>(cacheKey, cancellationToken).ConfigureAwait(false);

        if (codeState is null || codeState.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await _cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            return new ResetPasswordResult(false, "invalid_code", "Verification code is invalid or expired.");
        }

        if (!IsCodeMatch(request.VerificationCode.Trim(), codeState.HashedCode))
        {
            var attemptsRemaining = codeState.AttemptsRemaining - 1;
            if (attemptsRemaining <= 0)
            {
                await _cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                return new ResetPasswordResult(false, "invalid_code", "Verification code is invalid or expired.", 0);
            }

            await _cacheService.SetAsync(
                    cacheKey,
                    codeState with { AttemptsRemaining = attemptsRemaining },
                    codeState.ExpiresAtUtc > DateTime.UtcNow
                        ? codeState.ExpiresAtUtc - DateTime.UtcNow
                        : TimeSpan.FromSeconds(1),
                    cancellationToken)
                .ConfigureAwait(false);

            return new ResetPasswordResult(false, "invalid_code", "Verification code is invalid or expired.", attemptsRemaining);
        }

        user.PasswordHash = _passwordHashService.HashPassword(request.NewPassword);
        user.PasswordUpdatedAtUtc = DateTime.UtcNow;

        await _cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);

        await _accountLockoutService.UnlockAsync(user, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ResetPasswordResult(
            true,
            "accepted",
            "If an account exists and code is valid, the password has been reset.");
    }

    private static string BuildResetCacheKey(string email) =>
        PasswordResetCachePrefix + email;

    private static bool IsCodeMatch(string verificationCode, string hashedCode)
    {
        var candidateHash = HashCode(verificationCode);
        var left = Encoding.UTF8.GetBytes(candidateHash);
        var right = Encoding.UTF8.GetBytes(hashedCode);
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private static string HashCode(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(code);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
