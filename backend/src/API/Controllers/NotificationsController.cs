using Application.Commands;
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // -------------------------------------------------------------------------
    // GET /api/notifications/my
    // Returns the latest notifications for the authenticated patient.
    // -------------------------------------------------------------------------
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<PatientNotificationDto>>> GetMyNotifications(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(new GetPatientNotificationsQuery(userId.Value), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /api/notifications/staff?page=1&pageSize=20
    // Returns paginated staff/admin in-app notification history (US_028).
    // -------------------------------------------------------------------------
    [HttpGet("staff")]
    [Authorize(Roles = "staff,admin")]
    public async Task<ActionResult<NotificationPageDto>> GetStaffNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(new GetNotificationHistoryQuery(userId.Value, page, pageSize), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // POST /api/notifications/staff/{id}/read
    // Marks a single staff notification as read (US_028 AC-03).
    // -------------------------------------------------------------------------
    [HttpPost("staff/{id:guid}/read")]
    [Authorize(Roles = "staff,admin")]
    public async Task<ActionResult<StaffNotificationDto>> MarkStaffNotificationRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            var result = await _mediator
                .Send(new MarkNotificationReadCommand(id, userId.Value), cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
