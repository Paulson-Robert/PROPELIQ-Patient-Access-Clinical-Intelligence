using Application.Commands;
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

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
