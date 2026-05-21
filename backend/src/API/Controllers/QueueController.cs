using Application.Commands;
using Application.Interfaces;
using Application.Queries;
using API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/queue")]
[Authorize(Policy = RoleRequirements.StaffPolicy)]
public sealed class QueueController : ControllerBase
{
    private readonly IMediator _mediator;

    public QueueController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // -------------------------------------------------------------------------
    // GET /api/queue/today
    // AC-01: Returns today's patients ordered by queue position then slot time
    // -------------------------------------------------------------------------
    [HttpGet("today")]
    public async Task<ActionResult<QueueResponseDto>> GetTodayQueue(
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .Send(new GetSameDayQueueQuery(), cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    // -------------------------------------------------------------------------
    // POST /api/queue/{appointmentId}/arrived
    // AC-02: Mark patient as arrived with timestamp
    // -------------------------------------------------------------------------
    [HttpPost("{appointmentId:guid}/arrived")]
    public async Task<ActionResult<MarkArrivedResultDto>> MarkArrived(
        [FromRoute] Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var staffUserId = GetCurrentUserId();
        if (staffUserId is null)
            return Unauthorized();

        try
        {
            var result = await _mediator
                .Send(new MarkArrivedCommand(appointmentId, staffUserId.Value), cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { code = "MARK_ARRIVED_FAILED", message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // POST /api/queue/{appointmentId}/reorder
    // AC-03: Persist new queue position with staff reason
    // Edge Case: 409 on optimistic concurrency conflict
    // -------------------------------------------------------------------------
    [HttpPost("{appointmentId:guid}/reorder")]
    public async Task<ActionResult<ReorderQueueResultDto>> ReorderQueue(
        [FromRoute] Guid appointmentId,
        [FromBody] ReorderQueueRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffUserId = GetCurrentUserId();
        if (staffUserId is null)
            return Unauthorized();

        if (request.NewPosition < 1)
            return BadRequest(new { code = "INVALID_POSITION", message = "Position must be 1 or greater." });

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { code = "REASON_REQUIRED", message = "A reason is required for queue reordering." });

        try
        {
            var result = await _mediator
                .Send(
                    new ReorderQueueCommand(
                        appointmentId,
                        request.NewPosition,
                        request.Reason.Trim(),
                        staffUserId.Value),
                    cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
        catch (QueueConcurrencyException ex)
        {
            return Conflict(new { code = "CONCURRENT_REORDER", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { code = "REORDER_FAILED", message = ex.Message });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}

/// <summary>
/// Request body for the queue reorder endpoint.
/// </summary>
public sealed record ReorderQueueRequestDto(int NewPosition, string Reason);
