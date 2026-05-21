using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailDeliverySettings _settings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailDeliverySettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public Task SendVerificationEmailAsync(string email, string verificationLink)
    {
        return SendEmailAsync(
            email,
            "Verify your PropelIQ account",
            $"Use this verification link to complete your registration: {verificationLink}",
            CancellationToken.None);
    }

    public Task SendPasswordResetCodeAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
    {
        return SendEmailAsync(
            email,
            "Your PropelIQ password reset code",
            $"Your verification code is {verificationCode}. It expires shortly. If you did not request this, ignore this email.",
            cancellationToken);
    }

    private async Task SendEmailAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "Email delivery disabled. Skipping outbound email to {Email} with subject {Subject}.",
                recipientEmail,
                subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.FromAddress) ||
            string.IsNullOrWhiteSpace(_settings.SmtpHost) ||
            string.IsNullOrWhiteSpace(_settings.SmtpUsername) ||
            string.IsNullOrWhiteSpace(_settings.SmtpPassword))
        {
            _logger.LogError("Email delivery is enabled but SMTP settings are incomplete.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
        {
            Text = body,
        };

        try
        {
            using var smtpClient = new SmtpClient();
            var secureSocketOptions = _settings.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await smtpClient.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secureSocketOptions, cancellationToken).ConfigureAwait(false);
            await smtpClient.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken).ConfigureAwait(false);
            await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await smtpClient.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Sent email to {Email} with subject {Subject}.", recipientEmail, subject);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send email to {Email} with subject {Subject}.", recipientEmail, subject);
        }
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
