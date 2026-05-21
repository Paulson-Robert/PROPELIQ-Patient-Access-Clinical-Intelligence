namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// Request / result DTOs
// ---------------------------------------------------------------------------

/// <summary>Payload passed to the document storage service on upload.</summary>
public sealed record UploadDocumentRequest(
    Guid PatientUserId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream);

/// <summary>Result returned from <see cref="IDocumentStorageService.StoreAsync"/>.</summary>
public sealed record UploadDocumentResult(
    bool Success,
    Guid? DocumentId,
    string? FailureCode,
    string? FailureReason);

// ---------------------------------------------------------------------------
// Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Validates, stores, and registers document metadata for an uploaded file.
///
/// Responsibilities:
/// - Server-side format and size validation (AC-02).
/// - Persists file to configured storage and creates a <c>ClinicalDocument</c>
///   record with patient, upload date, and file type metadata (AC-03).
/// - Sets <see cref="Domain.Enums.MalwareScanStatus.Pending"/> so the upstream
///   malware scan pipeline (US_033) can pick up the record (AC-04).
/// </summary>
public interface IDocumentStorageService
{
    Task<UploadDocumentResult> StoreAsync(
        UploadDocumentRequest request,
        CancellationToken cancellationToken = default);
}
