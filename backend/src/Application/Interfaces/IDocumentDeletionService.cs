namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// Request / result DTOs
// ---------------------------------------------------------------------------

/// <summary>Payload passed to the document deletion service.</summary>
public sealed record DeleteDocumentRequest(
    Guid DocumentId,
    Guid RequestingPatientUserId,
    string? IpAddress = null);

/// <summary>Result returned by <see cref="IDocumentDeletionService.DeleteAsync"/>.</summary>
public sealed record DeleteDocumentResult(
    bool Success,
    string? FailureCode,
    string? FailureReason);

// ---------------------------------------------------------------------------
// Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Permanently removes a single clinical document, all associated extracted data,
/// conflicts, and code mappings, then triggers PatientView re-aggregation (US_034).
///
/// Responsibilities:
/// - Authorisation check: document must belong to the requesting patient.
/// - Cancel any in-progress NER processing before deleting (edge case).
/// - Physical file removal and DB record deletion within a transaction.
/// - Audit log entry for every deletion (AC-05).
/// - Enqueue PatientView re-aggregation after a successful deletion (AC-02).
/// </summary>
public interface IDocumentDeletionService
{
    Task<DeleteDocumentResult> DeleteAsync(
        DeleteDocumentRequest request,
        CancellationToken cancellationToken = default);
}
