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
/// Handles clinical code mapping endpoints for the code mapping workflow (US_040).
///
/// Routes:
/// - POST /api/clinical/patients/{patientId}/code-suggestions  — map extracted entities to codes (AC-01, AC-02)
/// - POST /api/clinical/patients/{patientId}/code-mappings/{mappingId}/verify — staff verify/modify/reject (AC-03, AC-04)
/// </summary>
[ApiController]
[Route("api/clinical")]
[Authorize(Policy = RoleRequirements.StaffPolicy)]
public sealed class ClinicalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICodeMappingService _codeMapping;

    public ClinicalController(IMediator mediator, ICodeMappingService codeMapping)
    {
        _mediator = mediator;
        _codeMapping = codeMapping;
    }

    // -------------------------------------------------------------------------
    // POST /api/clinical/patients/{patientId}/code-suggestions
    // AC-01, AC-02: Map supplied clinical entities to ranked ICD-10/CPT candidates
    // -------------------------------------------------------------------------
    [HttpPost("patients/{patientId}/code-suggestions")]
    public async Task<ActionResult<IReadOnlyList<CodeSuggestionDto>>> GetCodeSuggestions(
        [FromRoute] string patientId,
        [FromBody] CodeSuggestionsRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Patient ID is required." });

        if (request.Entities is null || request.Entities.Count == 0)
            return BadRequest(new { code = "VALIDATION_ERROR", message = "At least one clinical entity is required." });

        var entities = request.Entities.Select(e => new ClinicalEntity(
            e.EntityType,
            e.Text,
            0,
            e.Text.Length > 0 ? e.Text.Length - 1 : 0,
            e.Confidence,
            e.Confidence < 0.85f)).ToList();

        var candidates = await _codeMapping
            .MapAsync(entities, patientId, cancellationToken)
            .ConfigureAwait(false);

        var dtos = candidates.Select(c => new CodeSuggestionDto(
            c.InputText,
            c.CodeType.ToString(),
            c.CodeValue,
            c.CodeDescription,
            (float)c.ConfidenceScore,
            c.Source,
            c.CodeSetVersion)).ToList();

        return Ok(dtos);
    }

    // -------------------------------------------------------------------------
    // POST /api/clinical/patients/{patientId}/code-mappings/{mappingId}/verify
    // AC-03, AC-04: Persist staff verify / modify / reject decision with audit trail
    // -------------------------------------------------------------------------
    [HttpPost("patients/{patientId}/code-mappings/{mappingId:guid}/verify")]
    public async Task<ActionResult<VerifyCodeMappingResult>> VerifyCodeMapping(
        [FromRoute] string patientId,
        [FromRoute] Guid mappingId,
        [FromBody] VerifyCodeMappingRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffUserId = GetCurrentUserId();
        if (staffUserId is null)
            return Unauthorized();

        if (!Enum.TryParse<VerifyCodeAction>(request.Action, ignoreCase: true, out var action))
            return BadRequest(new { code = "INVALID_ACTION", message = $"Action '{request.Action}' is not valid. Use Verify, Modify, or Reject." });

        // AC-04: reason is required for Modify and Reject — validated early to provide clear feedback
        if (action is VerifyCodeAction.Modify or VerifyCodeAction.Reject &&
            string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { code = "REASON_REQUIRED", message = "A reason is required when modifying or rejecting a code mapping." });
        }

        var staffRole = User.FindFirstValue(ClaimTypes.Role) ?? "Staff";

        var result = await _mediator
            .Send(
                new VerifyCodeMappingCommand(
                    mappingId,
                    staffUserId.Value,
                    staffRole,
                    action,
                    request.ModifiedCodeValue,
                    request.Reason),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            return result.FailureCode switch
            {
                "MAPPING_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "REASON_REQUIRED" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                _ => StatusCode(500, new { code = "VERIFY_FAILED", message = result.FailureReason }),
            };
        }

        return Ok(result);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    // -------------------------------------------------------------------------
    // GET /api/clinical/appointments/{appointmentId}/risk-tier
    // AC-02 (US_042): Returns persisted risk tier with contributing factor breakdown
    // -------------------------------------------------------------------------
    [HttpGet("appointments/{appointmentId:guid}/risk-tier")]
    public async Task<ActionResult<RiskTierDto>> GetRiskTier(
        [FromRoute] Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new GetPatientRiskTierQuery(appointmentId), cancellationToken)
            .ConfigureAwait(false);

        return result is null
            ? NotFound(new { code = "APPOINTMENT_NOT_FOUND", message = "Appointment not found or risk score not yet calculated." })
            : Ok(result);
    }

    // -----------------------------------------------------------------------
    // Helpers (continued)
    // -----------------------------------------------------------------------

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    // -----------------------------------------------------------------------
    // DTOs (request/response — scoped to this controller)
    // -----------------------------------------------------------------------

    public sealed record CodeSuggestionEntityDto(
        ClinicalEntityType EntityType,
        string Text,
        float Confidence);

    public sealed record CodeSuggestionsRequestDto(
        IReadOnlyList<CodeSuggestionEntityDto> Entities);

    public sealed record VerifyCodeMappingRequestDto(
        string Action,
        string? ModifiedCodeValue = null,
        string? Reason = null);

    public sealed record CodeSuggestionDto(
        string InputText,
        string CodeType,
        string CodeValue,
        string CodeDescription,
        float ConfidenceScore,
        string Source,
        string CodeSetVersion);
}
