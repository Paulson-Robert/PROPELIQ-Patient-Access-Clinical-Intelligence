using Application.Commands;
using Application.Queries;
using API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

/// <summary>
/// Handles clinical document upload for authenticated patients (US_032).
///
/// Routes:
/// - POST /api/documents/upload — upload a single document (AC-01)
/// </summary>
[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    // 25 MiB limit enforced at the framework level before the action body runs (AC-02).
    private const long MaxUploadBytes = 26_214_400;

    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // -------------------------------------------------------------------------
    // GET /api/documents/my
    // Returns clinical documents for the authenticated patient.
    // -------------------------------------------------------------------------
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<PatientDocumentDto>>> GetMyDocuments(
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var result = await _mediator
            .Send(new GetPatientDocumentsQuery(patientUserId.Value), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // POST /api/documents/upload
    // AC-01: Accept multipart/form-data with a single file field named "file"
    // -------------------------------------------------------------------------
    [HttpPost("upload")]
    [Authorize(Policy = RoleRequirements.PatientPolicy)]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new { code = "NO_FILE", message = "A non-empty file is required." });

        await using var stream = file.OpenReadStream();

        var result = await _mediator.Send(
            new UploadDocumentCommand(
                patientUserId.Value,
                file.FileName,
                file.ContentType,
                file.Length,
                stream),
            cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            return result.FailureCode switch
            {
                "UNSUPPORTED_FORMAT" => UnprocessableEntity(new { code = result.FailureCode, message = result.FailureReason }),
                "FILE_TOO_LARGE"     => UnprocessableEntity(new { code = result.FailureCode, message = result.FailureReason }),
                "PATIENT_NOT_FOUND"  => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
                "INVALID_REQUEST"    => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
                _                    => StatusCode(StatusCodes.Status500InternalServerError,
                                            new { code = "UPLOAD_FAILED", message = result.FailureReason }),
            };
        }

        return Ok(new
        {
            documentId = result.DocumentId,
            message = "File uploaded and queued for malware scan.",
        });
    }

    // -------------------------------------------------------------------------
    // DELETE /api/documents/{documentId}
    // Permanently deletes one document owned by the authenticated patient.
    // -------------------------------------------------------------------------
    [HttpDelete("{documentId:guid}")]
    [Authorize(Policy = RoleRequirements.PatientPolicy)]
    public async Task<IActionResult> Delete(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var result = await DeleteDocumentForCurrentPatient(
            documentId,
            patientUserId.Value,
            cancellationToken)
            .ConfigureAwait(false);

        return ToDeleteActionResult(result);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/documents/my
    // Permanently deletes every document owned by the authenticated patient.
    // -------------------------------------------------------------------------
    [HttpDelete("my")]
    [Authorize(Policy = RoleRequirements.PatientPolicy)]
    public async Task<IActionResult> DeleteMyDocuments(CancellationToken cancellationToken)
    {
        var patientUserId = GetCurrentUserId();
        if (patientUserId is null)
            return Unauthorized();

        var documents = await _mediator
            .Send(new GetPatientDocumentsQuery(patientUserId.Value), cancellationToken)
            .ConfigureAwait(false);

        foreach (var document in documents)
        {
            var result = await DeleteDocumentForCurrentPatient(
                document.DocumentId,
                patientUserId.Value,
                cancellationToken)
                .ConfigureAwait(false);

            if (!result.Success)
                return ToDeleteActionResult(result);
        }

        return NoContent();
    }

    private Task<Application.Interfaces.DeleteDocumentResult> DeleteDocumentForCurrentPatient(
        Guid documentId,
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        return _mediator.Send(
            new DeleteDocumentCommand(
                documentId,
                patientUserId,
                HttpContext.Connection.RemoteIpAddress?.ToString()),
            cancellationToken);
    }

    private IActionResult ToDeleteActionResult(Application.Interfaces.DeleteDocumentResult result)
    {
        if (result.Success)
            return NoContent();

        return result.FailureCode switch
        {
            "NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
            "PATIENT_NOT_FOUND" => NotFound(new { code = result.FailureCode, message = result.FailureReason }),
            "INVALID_REQUEST" => BadRequest(new { code = result.FailureCode, message = result.FailureReason }),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new { code = "DELETE_FAILED", message = result.FailureReason }),
        };
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
