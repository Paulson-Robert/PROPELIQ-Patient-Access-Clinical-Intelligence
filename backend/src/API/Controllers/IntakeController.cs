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
    // GET /api/intake/my
    // Returns all intake records for the authenticated patient.
    // -------------------------------------------------------------------------
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<PatientIntakeDto>>> GetMyIntakes(
        CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        var actorRole = GetCurrentRole();
        if (actorUserId is null || actorRole is null)
            return Unauthorized();

        var result = await _mediator
            .Send(new GetPatientIntakesQuery(actorUserId.Value, actorRole), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
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
        var actorUserId = GetCurrentUserId();
        var actorRole = GetCurrentRole();
        if (actorUserId is null || actorRole is null)
            return Unauthorized();

        if (request is null || string.IsNullOrWhiteSpace(request.ReasonForVisit))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Reason for visit is required." });

        var result = await _mediator
            .Send(
                new SubmitManualIntakeCommand(
                    actorUserId.Value,
                    actorRole,
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
                "APPOINTMENT_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "INVALID_REQUEST" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                "VALIDATION_ERROR" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "INTAKE_FAILED", message = result.FailureReason }),
            };
        }

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // POST /api/intake/{appointmentId}/ai/submit
    // Persist the completed AI-assisted intake summary
    // -------------------------------------------------------------------------
    [HttpPost("{appointmentId:guid}/ai/submit")]
    public async Task<ActionResult<AiIntakeSubmissionResult>> SubmitAi(
        [FromRoute] Guid appointmentId,
        [FromBody] SubmitAiIntakeRequestDto request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        var actorRole = GetCurrentRole();
        if (actorUserId is null || actorRole is null)
            return Unauthorized();

        if (request is null || string.IsNullOrWhiteSpace(request.ReasonForVisit))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Reason for visit is required." });

        var result = await _mediator
            .Send(
                new SubmitAiIntakeCommand(
                    actorUserId.Value,
                    actorRole,
                    appointmentId,
                    request.ChronicConditions,
                    request.CurrentMedications,
                    request.Allergies,
                    request.SurgicalHistory,
                    request.ReasonForVisit),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            return result.FailureCode switch
            {
                "PATIENT_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "APPOINTMENT_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "INVALID_REQUEST" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                "VALIDATION_ERROR" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "INTAKE_FAILED", message = result.FailureReason }),
            };
        }

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
        var actorUserId = GetCurrentUserId();
        var actorRole = GetCurrentRole();
        if (actorUserId is null || actorRole is null)
            return Unauthorized();

        var result = await _mediator
            .Send(
                new SaveIntakeDraftCommand(
                    actorUserId.Value,
                    actorRole,
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
                "APPOINTMENT_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "PATIENT_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "INVALID_REQUEST" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "DRAFT_SAVE_FAILED", message = result.FailureReason }),
            };
        }

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
        var actorUserId = GetCurrentUserId();
        var actorRole = GetCurrentRole();
        if (actorUserId is null || actorRole is null)
            return Unauthorized();

        var draft = await _mediator
            .Send(new GetIntakeDraftQuery(actorUserId.Value, actorRole, appointmentId), cancellationToken)
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

    private string? GetCurrentRole()
        => User.FindFirstValue(ClaimTypes.Role);
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

public sealed record SubmitAiIntakeRequestDto(
    string? ChronicConditions,
    string? CurrentMedications,
    string? Allergies,
    string? SurgicalHistory,
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
