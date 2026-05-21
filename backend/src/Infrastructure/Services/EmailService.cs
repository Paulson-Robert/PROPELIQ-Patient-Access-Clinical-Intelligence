using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string email, string verificationLink)
    {
        _logger.LogInformation(
            "Queued verification email dispatch for {Email}. Verification link: {VerificationLink}",
            email,
            verificationLink);

        return Task.CompletedTask;
    }

    public Task SendConfirmationEmailAsync(
        string email,
        string subject,
        string body,
        byte[]? pdfAttachment = null,
        string? attachmentFileName = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Queued confirmation email for {Email}. Subject: {Subject}. PDF attached: {HasPdf}.",
            email, subject, pdfAttachment is { Length: > 0 });

        return Task.CompletedTask;
    }
}
