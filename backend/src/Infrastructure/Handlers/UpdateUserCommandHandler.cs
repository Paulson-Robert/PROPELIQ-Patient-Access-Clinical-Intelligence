using Application.Commands;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

/// <summary>
/// Updates a user's email, display name, and role (US_043 AC-01, AC-02).
/// Role persisted; applied on the user's next login.
/// </summary>
public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            throw new InvalidOperationException($"User '{request.UserId}' not found.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _dbContext.Users
            .AnyAsync(
                u => u.Email == normalizedEmail && u.UserId != request.UserId,
                cancellationToken)
            .ConfigureAwait(false);

        if (emailTaken)
            throw new InvalidOperationException($"Email '{normalizedEmail}' is already in use.");

        user.Email = normalizedEmail;
        user.FullName = request.FullName?.Trim();
        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CreateUserCommandHandler.ToDto(user);
    }
}
