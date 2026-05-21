using Domain.Enums;
using MediatR;

namespace Application.Commands;

public sealed record RequestMfaCodeCommand(
    string Email,
    MfaMethod Method) : IRequest<RequestMfaCodeResult>;

public sealed record RequestMfaCodeResult(
    bool Success,
    string Status,
    string Message,
    Guid UserId,
    string Email,
    UserRole Role,
    MfaMethod Method,
    int AttemptsRemaining,
    DateTime ExpiresAtUtc);