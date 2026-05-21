using Application.Commands;
using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

/// <summary>
/// Returns a paged, filtered list of all user accounts (US_043 AC-01).
/// </summary>
public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, UserListResult>
{
    private readonly ApplicationDbContext _dbContext;

    public ListUsersQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserListResult> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                (u.FullName != null && u.FullName.ToLower().Contains(term)));
        }

        if (request.Role.HasValue)
            query = query.Where(u => u.Role == request.Role.Value);

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var rows = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FullName,
                u.Role,
                u.IsActive,
                u.LockedUntilUtc,
                u.PasswordUpdatedAtUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.Select(u =>
        {
            var status = (u.LockedUntilUtc.HasValue && u.LockedUntilUtc.Value > DateTime.UtcNow)
                ? "locked"
                : (u.IsActive ? "active" : "inactive");

            return new UserDto(u.UserId, u.Email, u.FullName, u.Role, status, u.PasswordUpdatedAtUtc);
        }).ToList();

        return new UserListResult(dtos, total, request.Page, request.PageSize);
    }
}
