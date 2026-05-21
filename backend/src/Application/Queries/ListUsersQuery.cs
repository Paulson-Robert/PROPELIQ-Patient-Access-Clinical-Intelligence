using Application.Commands;
using Domain.Enums;
using MediatR;

namespace Application.Queries;

/// <summary>
/// Returns a paged, filtered list of all user accounts (admin-only, US_043 AC-01).
/// </summary>
public sealed record ListUsersQuery(
    string? Search,
    UserRole? Role,
    int Page,
    int PageSize) : IRequest<UserListResult>;

/// <summary>Paged result for the user list.</summary>
public sealed record UserListResult(
    IReadOnlyList<UserDto> Users,
    int Total,
    int Page,
    int PageSize);
