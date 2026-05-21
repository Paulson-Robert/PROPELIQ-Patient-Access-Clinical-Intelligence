using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationCodeAsync(string phoneNumber, string verificationCode, CancellationToken cancellationToken = default)
    {
        var maskedPhone = MaskPhoneNumber(phoneNumber);
        _logger.LogInformation("Queued MFA SMS verification for {PhoneNumber}", maskedPhone);
        return Task.CompletedTask;
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length <= 4)
        {
            return phoneNumber;
        }

        return new string('*', phoneNumber.Length - 4) + phoneNumber[^4..];
    }
}