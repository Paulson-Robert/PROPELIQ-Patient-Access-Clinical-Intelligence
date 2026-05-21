using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

/// <summary>
/// Creates a new user account with admin-assigned role (US_043 AC-01, AC-02).
/// </summary>
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHashService _passwordHashService;

    public CreateUserCommandHandler(
        ApplicationDbContext dbContext,
        IPasswordHashService passwordHashService)
    {
        _dbContext = dbContext;
        _passwordHashService = passwordHashService;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var duplicate = await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
            throw new InvalidOperationException($"An account with email '{normalizedEmail}' already exists.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = normalizedEmail,
            FullName = request.FullName?.Trim(),
            PasswordHash = _passwordHashService.HashPassword(request.Password),
            AuthProvider = AuthProvider.Local,
            Role = request.Role,
            IsActive = true,
            PasswordUpdatedAtUtc = now,
            CreatedAt = now,
            UpdatedAt = now,
            MfaEnabled = false,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ToDto(user);
    }

    internal static UserDto ToDto(User user)
    {
        var status = DetermineStatus(user);
        return new UserDto(
            user.UserId,
            user.Email,
            user.FullName,
            user.Role,
            status,
            user.PasswordUpdatedAtUtc);
    }

    internal static string DetermineStatus(User user)
    {
        if (user.LockedUntilUtc.HasValue && user.LockedUntilUtc.Value > DateTime.UtcNow)
            return "locked";

        return user.IsActive ? "active" : "inactive";
    }
}
