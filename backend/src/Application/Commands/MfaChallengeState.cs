using Domain.Enums;

namespace Application.Commands;

public sealed record MfaChallengeState(
    string Email,
    UserRole Role,
    MfaMethod Method,
    string? PhoneNumber,
    string? SmsCode,
    DateTime ExpiresAtUtc,
    int AttemptsRemaining,
    long? LastTimeStep = null);