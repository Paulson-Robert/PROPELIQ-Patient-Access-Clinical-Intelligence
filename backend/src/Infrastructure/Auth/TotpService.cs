using Application.Interfaces;
using Microsoft.Extensions.Options;
using OtpNet;

namespace Infrastructure.Auth;

public sealed class TotpService : ITotpService
{
    private readonly AuthSettings _settings;

    public TotpService(IOptions<AuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public TotpSetupResult CreateSetup(string issuer, string email)
    {
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);
        var otpUri = new OtpUri(
            OtpType.Totp,
            base32Secret,
            email,
            issuer: string.IsNullOrWhiteSpace(issuer) ? _settings.JwtIssuer : issuer,
            algorithm: OtpHashMode.Sha1,
            digits: 6,
            period: 30);

        return new TotpSetupResult(base32Secret, base32Secret, otpUri.ToString());
    }

    public bool Validate(string base32Secret, string code, out long timeStepMatched)
    {
        var secretKey = Base32Encoding.ToBytes(base32Secret);
        var totp = new Totp(secretKey);
        return totp.VerifyTotp(code.Trim(), out timeStepMatched, new VerificationWindow(previous: 1, future: 1));
    }
}