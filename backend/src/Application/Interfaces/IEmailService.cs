namespace Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string verificationLink);

    /// <summary>
    /// Sends a booking confirmation email with an optional PDF attachment (AC-04).
    /// </summary>
    Task SendConfirmationEmailAsync(
        string email,
        string subject,
        string body,
        byte[]? pdfAttachment = null,
        string? attachmentFileName = null,
        CancellationToken cancellationToken = default);
}
