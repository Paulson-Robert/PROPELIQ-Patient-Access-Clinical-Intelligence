using Domain.Enums;
using MediatR;

namespace Application.Commands;

public sealed record SetupMfaCommand(
    string Email,
    MfaMethod Method,
    string? PhoneNumber = null) : IRequest<SetupMfaResult>;

public sealed record SetupMfaResult(
    bool Success,
    string Status,
    string Message,
    Guid UserId,
    string Email,
    UserRole Role,
    MfaMethod Method,
    string? ManualKey,
    string? QrCodeUri,
    string? PhoneNumber,
    DateTime ExpiresAtUtc);