using Application.Commands;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

/// <summary>
/// Deactivates a user account (IsActive=false) preventing future authentication (US_043 AC-03).
/// Blocks self-deactivation to protect the requesting admin (Edge Case).
/// </summary>
public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, UserDto>
{
    private readonly ApplicationDbContext _dbContext;

    public DeactivateUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDto> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        // Edge case: block self-deactivation server-side
        if (request.TargetUserId == request.RequestingAdminId)
            throw new InvalidOperationException("An admin cannot deactivate their own account.");

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.UserId == request.TargetUserId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            throw new InvalidOperationException($"User '{request.TargetUserId}' not found.");

        if (!user.IsActive)
            throw new InvalidOperationException("User account is already inactive.");

        // AC-03: set IsActive=false; data preserved for audit trail (soft-delete pattern)
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CreateUserCommandHandler.ToDto(user);
    }
}
