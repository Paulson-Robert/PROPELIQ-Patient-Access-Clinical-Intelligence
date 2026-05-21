using MediatR;

namespace Application.Commands;

public sealed record ResetPasswordCommand(
    string Email,
    string NewPassword,
    string VerificationCode,
    string? ResetToken = null) : IRequest<ResetPasswordResult>;

public sealed record RequestPasswordResetCodeCommand(
    string Email) : IRequest<RequestPasswordResetCodeResult>;

public sealed record RequestPasswordResetCodeResult(
    bool Success,
    string Status,
    string Message,
    int CooldownSeconds);

public sealed record ResetPasswordResult(
    bool Success,
    string Status,
    string Message,
    int? AttemptsRemaining = null);
