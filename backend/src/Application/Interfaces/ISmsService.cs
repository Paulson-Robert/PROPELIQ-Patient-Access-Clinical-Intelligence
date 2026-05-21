namespace Application.Interfaces;

public interface ISmsService
{
    Task SendVerificationCodeAsync(string phoneNumber, string verificationCode, CancellationToken cancellationToken = default);

    Task SendReminderAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}