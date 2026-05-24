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
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(new GetPatientNotificationsQuery(patientUserId.Value), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
