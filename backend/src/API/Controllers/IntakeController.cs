using Application.Commands;
using Application.Interfaces;
using Application.Queries;
using API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

/// <summary>
/// Handles manual pre-visit intake form endpoints (US_030).
///
/// Routes:
/// - POST /api/intake/{appointmentId}/submit  — submit complete intake (AC-01)
/// - PUT  /api/intake/{appointmentId}/draft   — auto-save partial draft (AC-02)
/// - GET  /api/intake/{appointmentId}/draft   — load existing draft    (AC-02)
/// </summary>
[ApiController]
[Route("api/intake")]
[Authorize]
public sealed class IntakeController : ControllerBase
{
    private readonly IMediator _mediator;

    public IntakeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // -------------------------------------------------------------------------
    // POST /api/intake/{appointmentId}/submit
    // AC-01: Validate and persist completed manual intake
    // -------------------------------------------------------------------------
    [HttpPost("{appointmentId:guid}/submit")]
    public async Task<ActionResult<ManualIntakeResult>> Submit(
        [FromRoute] Guid appointmentId,
        [FromBody] SubmitManualIntakeRequestDto request,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.ReasonForVisit))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Reason for visit is required." });

        var result = await _mediator
            .Send(
                new SubmitManualIntakeCommand(
                    patientUserId.Value,
                    appointmentId,
                    request.ChronicConditions,
                    request.PastSurgeries,
                    request.FamilyHistory,
                    request.SymptomsDescription,
                    request.SymptomOnset,
                    request.SymptomSeverity,
                    request.CurrentMedications,
                    request.KnownAllergies,
                    request.ReasonForVisit),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            return result.FailureCode switch
            {
                "PATIENT_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "VALIDATION_ERROR" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "INTAKE_FAILED", message = result.FailureReason }),
            };
        }

        // ALREADY_SUBMITTED is a success — return 200 so the client can handle idempotency gracefully.
        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // PUT /api/intake/{appointmentId}/draft
    // AC-02: Save partial draft between steps
    // -------------------------------------------------------------------------
    [HttpPut("{appointmentId:guid}/draft")]
    public async Task<ActionResult<IntakeDraftResult>> SaveDraft(
        [FromRoute] Guid appointmentId,
        [FromBody] SaveIntakeDraftRequestDto request,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(
                new SaveIntakeDraftCommand(
                    patientUserId.Value,
                    appointmentId,
                    request.ChronicConditions,
                    request.PastSurgeries,
                    request.FamilyHistory,
                    request.SymptomsDescription,
                    request.SymptomOnset,
                    request.SymptomSeverity,
                    request.CurrentMedications,
                    request.KnownAllergies,
                    request.ReasonForVisit),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
            return StatusCode(500, new { code = "DRAFT_SAVE_FAILED", message = result.FailureReason });

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /api/intake/{appointmentId}/draft
    // AC-02: Load existing draft for form restoration
    // -------------------------------------------------------------------------
    [HttpGet("{appointmentId:guid}/draft")]
    public async Task<ActionResult<IntakeDraftDto>> GetDraft(
        [FromRoute] Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var draft = await _mediator
            .Send(new GetIntakeDraftQuery(patientUserId.Value, appointmentId), cancellationToken)
            .ConfigureAwait(false);

        if (draft is null)
            return NoContent();

        return Ok(draft);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

// ---------------------------------------------------------------------------
// Request DTOs (controller input contracts — separate from Application DTOs)
// ---------------------------------------------------------------------------

public sealed record SubmitManualIntakeRequestDto(
    string? ChronicConditions,
    string? PastSurgeries,
    string? FamilyHistory,
    string? SymptomsDescription,
    string? SymptomOnset,
    string? SymptomSeverity,
    string? CurrentMedications,
    string? KnownAllergies,
    string ReasonForVisit);

public sealed record SaveIntakeDraftRequestDto(
    string? ChronicConditions,
    string? PastSurgeries,
    string? FamilyHistory,
    string? SymptomsDescription,
    string? SymptomOnset,
    string? SymptomSeverity,
    string? CurrentMedications,
    string? KnownAllergies,
    string? ReasonForVisit);
