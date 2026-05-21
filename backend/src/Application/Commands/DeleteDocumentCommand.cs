using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Permanently deletes a single clinical document on patient request (US_034).
///
/// The command carries identity and request-context metadata so that the
/// Application layer remains free of infrastructure dependencies.
/// Validation rules applied before the service is invoked:
/// - <see cref="DocumentId"/> must be a non-empty GUID.
/// - <see cref="RequestingPatientUserId"/> must be a non-empty GUID.
///
/// The service layer (<see cref="IDocumentDeletionService"/>) handles:
/// - Authorisation: document must belong to the requesting patient.
/// - In-progress processing cancellation (edge case).
/// - Cascaded removal of ExtractedDataRecords, DataConflicts, MedicalCodeMappings.
/// - Physical file deletion.
/// - Audit log entry (AC-05).
/// - PatientView re-aggregation trigger (AC-02).
/// </summary>
public sealed record DeleteDocumentCommand(
    Guid DocumentId,
    Guid RequestingPatientUserId,
    string? IpAddress = null) : IRequest<DeleteDocumentResult>;

internal sealed class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand, DeleteDocumentResult>
{
    private readonly IDocumentDeletionService _deletion;

    public DeleteDocumentCommandHandler(IDocumentDeletionService deletion)
    {
        _deletion = deletion;
    }

    public Task<DeleteDocumentResult> Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        if (request.DocumentId == Guid.Empty)
        {
            return Task.FromResult(new DeleteDocumentResult(
                Success: false,
                FailureCode: "INVALID_REQUEST",
                FailureReason: "DocumentId is required."));
        }

        if (request.RequestingPatientUserId == Guid.Empty)
        {
            return Task.FromResult(new DeleteDocumentResult(
                Success: false,
                FailureCode: "INVALID_REQUEST",
                FailureReason: "RequestingPatientUserId is required."));
        }

        return _deletion.DeleteAsync(
            new DeleteDocumentRequest(
                request.DocumentId,
                request.RequestingPatientUserId,
                request.IpAddress),
            cancellationToken);
    }
}
