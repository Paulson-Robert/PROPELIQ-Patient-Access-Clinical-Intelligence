using System.Security.Cryptography;
using Application.Commands;
using Application.Interfaces;
using Domain.Enums;
using Hangfire;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

public sealed class SetupMfaCommandHandler : IRequestHandler<SetupMfaCommand, SetupMfaResult>
{
    private const string Issuer = "PropelIQ";
    private const string MfaChallengeCachePrefix = "auth:mfa:challenge:";

    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ITotpService _totpService;
    private readonly IBackgroundJobClient _backgroundJobs;

    public SetupMfaCommandHandler(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        ITotpService totpService,
        IBackgroundJobClient backgroundJobs)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _totpService = totpService;
        _backgroundJobs = backgroundJobs;
    }

    public async Task<SetupMfaResult> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return new SetupMfaResult(false, "not_found", "Account not found.", Guid.Empty, normalizedEmail, UserRole.Patient, request.Method, null, null, request.PhoneNumber, DateTime.UtcNow);
        }

        if (!user.IsActive)
        {
            return new SetupMfaResult(false, "account_disabled", "Your account has been disabled. Contact your administrator.", user.UserId, user.Email, user.Role, request.Method, null, null, request.PhoneNumber, DateTime.UtcNow);
        }

        if (user.Role is not (UserRole.Staff or UserRole.Admin))
        {
            return new SetupMfaResult(false, "not_required", "MFA setup is only required for staff and admin accounts.", user.UserId, user.Email, user.Role, request.Method, null, null, request.PhoneNumber, DateTime.UtcNow);
        }

        var expiresAtUtc = DateTime.UtcNow.AddSeconds(30);

        if (request.Method == MfaMethod.Totp)
        {
            var setup = _totpService.CreateSetup(Issuer, user.Email);
            user.MfaEnabled = true;
            user.MfaMethod = MfaMethod.Totp;
            user.MfaSecret = setup.Secret;
            user.MfaPhoneNumber = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await _cacheService.SetAsync(
                    BuildChallengeCacheKey(user.Email),
                    new MfaChallengeState(user.Email, user.Role, MfaMethod.Totp, null, null, expiresAtUtc, 3),
                    TimeSpan.FromSeconds(30),
                    cancellationToken)
                .ConfigureAwait(false);

            return new SetupMfaResult(true, "totp_ready", "Scan the QR code with your authenticator app.", user.UserId, user.Email, user.Role, MfaMethod.Totp, setup.ManualKey, setup.OtpAuthUri, null, expiresAtUtc);
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return new SetupMfaResult(false, "phone_required", "A phone number is required for SMS MFA.", user.UserId, user.Email, user.Role, MfaMethod.Sms, null, null, null, DateTime.UtcNow);
        }

        var verificationCode = GenerateVerificationCode();
        user.MfaEnabled = true;
        user.MfaMethod = MfaMethod.Sms;
        user.MfaSecret = null;
        user.MfaPhoneNumber = request.PhoneNumber.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _cacheService.SetAsync(
                BuildChallengeCacheKey(user.Email),
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

        return new SetupMfaResult(true, "sms_sent", "A verification code was sent to your phone.", user.UserId, user.Email, user.Role, MfaMethod.Sms, null, null, user.MfaPhoneNumber, expiresAtUtc);
    }

    private static string BuildChallengeCacheKey(string email) =>
        MfaChallengeCachePrefix + email.Trim().ToLowerInvariant();

    private static string GenerateVerificationCode()
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return code.ToString("D6");
    }
}