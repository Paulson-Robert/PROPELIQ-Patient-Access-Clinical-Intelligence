using Domain.Enums;
using MediatR;

namespace Application.Commands;

public sealed record VerifyMfaCommand(
    string Email,
    string Code,
    MfaMethod Method) : IRequest<VerifyMfaResult>;

public sealed record VerifyMfaResult(
    bool Success,
    string Status,
    string Message,
    Guid UserId,
    string Email,
    UserRole Role,
    MfaMethod Method,
    int AttemptsRemaining,
    DateTime? ExpiresAtUtc,
    bool RequiresResend,
    long? TimeStepMatched = null);