using Domain.Enums;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Updates a user's email, display name, and role (admin-only, AC-01, AC-02).
/// Role changes take effect on the user's next login token issuance.
/// </summary>
public sealed record UpdateUserCommand(
    Guid UserId,
    string Email,
    string? FullName,
    UserRole Role) : IRequest<UserDto>;
