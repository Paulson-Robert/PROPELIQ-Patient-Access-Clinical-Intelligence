using Application.Commands;
using Application.Queries;
using API.Authorization;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = RoleRequirements.AdminPolicy)]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateUserCommand> _createValidator;
    private readonly IValidator<UpdateUserCommand> _updateValidator;

    public UsersController(
        IMediator mediator,
        IValidator<CreateUserCommand> createValidator,
        IValidator<UpdateUserCommand> updateValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // -------------------------------------------------------------------------
    // GET /api/admin/users?search=&role=&page=1&pageSize=10
    // AC-01: Paginated, filtered list of all user accounts
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<UserListResult>> ListUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 100);

        UserRole? parsedRole = null;
        if (!string.IsNullOrWhiteSpace(role) &&
            Enum.TryParse<UserRole>(role, ignoreCase: true, out var r))
        {
            parsedRole = r;
        }

        var result = await _mediator
            .Send(new ListUsersQuery(search, parsedRole, page, pageSize), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // POST /api/admin/users
    // AC-01: Create user; AC-02: role persisted
    // -------------------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] CreateUserRequestBody body,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserRole>(body.Role, ignoreCase: true, out var parsedRole))
        {
            return BadRequest(new { code = "invalid_role", message = "Role must be Patient, Staff, or Admin." });
        }

        var command = new CreateUserCommand(
            body.Email ?? string.Empty,
            body.Password ?? string.Empty,
            body.FullName,
            parsedRole);

        var validation = await _createValidator
            .ValidateAsync(command, cancellationToken)
            .ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                code = "validation_error",
                message = "Request validation failed.",
                errors = validation.Errors.Select(e => e.ErrorMessage),
            });
        }

        try
        {
            var user = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(ListUsers), new { }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { code = "duplicate_email", message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PATCH /api/admin/users/{userId}
    // AC-01: Update user; AC-02: role change persisted, applied on next login
    // -------------------------------------------------------------------------
    [HttpPatch("{userId:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRequestBody body,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserRole>(body.Role, ignoreCase: true, out var parsedRole))
        {
            return BadRequest(new { code = "invalid_role", message = "Role must be Patient, Staff, or Admin." });
        }

        var command = new UpdateUserCommand(userId, body.Email ?? string.Empty, body.FullName, parsedRole);

        var validation = await _updateValidator
            .ValidateAsync(command, cancellationToken)
            .ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                code = "validation_error",
                message = "Request validation failed.",
                errors = validation.Errors.Select(e => e.ErrorMessage),
            });
        }

        try
        {
            var user = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { code = "update_failed", message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PATCH /api/admin/users/{userId}/deactivate
    // AC-03: IsActive=false; Edge Case: self-deactivation blocked
    // -------------------------------------------------------------------------
    [HttpPatch("{userId:guid}/deactivate")]
    public async Task<ActionResult<UserDto>> DeactivateUser(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var adminId = GetCurrentUserId();
        if (adminId is null)
            return Unauthorized();

        try
        {
            var user = await _mediator
                .Send(new DeactivateUserCommand(userId, adminId.Value), cancellationToken)
                .ConfigureAwait(false);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { code = "deactivation_failed", message = ex.Message });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}

/// <summary>Request body for POST /api/admin/users.</summary>
public sealed record CreateUserRequestBody(string? Email, string? Password, string? FullName, string? Role);

/// <summary>Request body for PATCH /api/admin/users/{userId}.</summary>
public sealed record UpdateUserRequestBody(string? Email, string? FullName, string? Role);
