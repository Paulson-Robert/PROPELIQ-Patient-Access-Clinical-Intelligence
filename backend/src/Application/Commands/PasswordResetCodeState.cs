namespace Application.Commands;

public sealed record PasswordResetCodeState(
    string Email,
    string HashedCode,
    DateTime ExpiresAtUtc,
    int AttemptsRemaining,
    DateTime ResendAvailableAtUtc);
