namespace Application.Interfaces;

public sealed record TotpSetupResult(string Secret, string ManualKey, string OtpAuthUri);

public interface ITotpService
{
    TotpSetupResult CreateSetup(string issuer, string email);

    bool Validate(string base32Secret, string code, out long timeStepMatched);
}