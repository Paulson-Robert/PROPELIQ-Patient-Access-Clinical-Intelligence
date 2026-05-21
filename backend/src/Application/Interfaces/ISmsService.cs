namespace Application.Interfaces;

public interface ISmsService
{
    Task SendVerificationCodeAsync(string phoneNumber, string verificationCode, CancellationToken cancellationToken = default);
}