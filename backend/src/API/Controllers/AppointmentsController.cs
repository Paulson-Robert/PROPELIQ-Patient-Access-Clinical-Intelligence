using Application.Commands;
using Application.Interfaces;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // -------------------------------------------------------------------------
    // GET /api/appointments/slots?provider=&specialty=&from=&to=
    // AC-01, AC-02: Search available slots by provider / specialty
    // -------------------------------------------------------------------------
    [HttpGet("slots")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SlotDto>>> SearchSlots(
        [FromQuery] string? provider,
        [FromQuery] string? specialty,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var slots = await _mediator
            .Send(new SearchSlotsQuery(provider, specialty, from, to), cancellationToken)
            .ConfigureAwait(false);

        return Ok(slots);
    }

    // -------------------------------------------------------------------------
    // POST /api/appointments/slots/{slotId}/lock
    // AC-03: Acquire 30-second Redis slot lock; returns lock token + expiry
    // AC-05: Returns 409 when slot is already locked
    // -------------------------------------------------------------------------
    [HttpPost("slots/{slotId:guid}/lock")]
    public async Task<ActionResult<SlotLockResult>> LockSlot(
        [FromRoute] Guid slotId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new LockSlotCommand(slotId), cancellationToken)
            .ConfigureAwait(false);

        if (result is null)
        {
            return Conflict(new
            {
                code = "SLOT_LOCKED",
                message = "This slot is temporarily held by another user. Please select a different time.",
            });
        }

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // POST /api/appointments
    // AC-04: Validate lock, persist appointment, enqueue PDF
    // AC-05: Returns 409 when lock expired or slot unavailable
    // -------------------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<AppointmentConfirmationDto>> ConfirmBooking(
        [FromBody] ConfirmBookingRequest request,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(
                new ConfirmBookingCommand(
                    request.SlotId,
                    request.LockToken,
                    patientUserId.Value,
                    request.InsuranceProvider,
                    request.InsurancePolicyNumber),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            return result.FailureCode switch
            {
                "LOCK_EXPIRED" => Conflict(new { code = result.FailureCode, message = result.FailureReason }),
                "SLOT_UNAVAILABLE" => Conflict(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "BOOKING_FAILED", message = result.FailureReason }),
            };
        }

        return Ok(result.Appointment);
    }

    // -------------------------------------------------------------------------
    // POST /api/appointments/{appointmentId}/cancel
    // AC-01: Cancel appointment, release slot, trigger swap + calendar sync
    // -------------------------------------------------------------------------
    [HttpPost("{appointmentId:guid}/cancel")]
    public async Task<ActionResult<AppointmentMutationDto>> CancelAppointment(
        [FromRoute] Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(new CancelAppointmentCommand(appointmentId, patientUserId.Value), cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            return result.FailureCode switch
            {
                "NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "CONFLICT" => Conflict(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "CANCEL_FAILED", message = result.FailureReason }),
            };
        }

        return Ok(result.Appointment);
    }

    // -------------------------------------------------------------------------
    // PUT /api/appointments/{appointmentId}/reschedule
    // AC-02: Atomic old-slot release + new-slot booking with rollback on failure
    // -------------------------------------------------------------------------
    [HttpPut("{appointmentId:guid}/reschedule")]
    public async Task<ActionResult<AppointmentMutationDto>> RescheduleAppointment(
        [FromRoute] Guid appointmentId,
        [FromBody] RescheduleAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(
                new RescheduleAppointmentCommand(
                    appointmentId,
                    request.NewSlotId,
                    request.LockToken,
                    patientUserId.Value),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            return result.FailureCode switch
            {
                "NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "LOCK_EXPIRED" => Conflict(new { code = result.FailureCode, message = result.FailureReason }),
                "SLOT_UNAVAILABLE" => Conflict(new { code = result.FailureCode, message = result.FailureReason }),
                "SAME_SLOT" => Conflict(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "RESCHEDULE_FAILED", message = result.FailureReason }),
            };
        }

        return Ok(result.Appointment);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record ConfirmBookingRequest(
    Guid SlotId,
    string LockToken,
    string? InsuranceProvider,
    string? InsurancePolicyNumber);
public sealed record RescheduleAppointmentRequest(Guid NewSlotId, string LockToken);
