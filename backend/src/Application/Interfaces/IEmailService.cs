namespace Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string verificationLink);
    Task SendPasswordResetCodeAsync(string email, string verificationCode, CancellationToken cancellationToken = default);
}
