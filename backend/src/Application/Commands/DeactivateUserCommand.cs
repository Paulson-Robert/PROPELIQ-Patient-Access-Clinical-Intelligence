using MediatR;

namespace Application.Commands;

/// <summary>
/// Sets a user's IsActive flag to false, preventing authentication (AC-03).
/// Self-deactivation is blocked server-side (Edge Case).
/// </summary>
public sealed record DeactivateUserCommand(
    Guid TargetUserId,
    Guid RequestingAdminId) : IRequest<UserDto>;
