using Domain.Enums;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Creates a new user account with the specified role (admin-only, AC-01).
/// </summary>
public sealed record CreateUserCommand(
    string Email,
    string Password,
    string? FullName,
    UserRole Role) : IRequest<UserDto>;

/// <summary>
/// Shared DTO for user management responses (SCR-023).
/// </summary>
public sealed record UserDto(
    Guid UserId,
    string Email,
    string? FullName,
    UserRole Role,
    string Status,
    DateTime? LastLoginDate);
