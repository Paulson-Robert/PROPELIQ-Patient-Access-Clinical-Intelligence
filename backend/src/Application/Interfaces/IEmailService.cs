namespace Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string verificationLink);
    Task SendPasswordResetCodeAsync(string email, string verificationCode, CancellationToken cancellationToken = default);
    Task SendConfirmationEmailAsync(
        string email,
        string subject,
        string body,
        byte[]? pdfAttachment = null,
        string? attachmentFileName = null,
        CancellationToken cancellationToken = default);
}
