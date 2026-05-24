using MediatR;

namespace Application.Queries;

/// <summary>
/// DTO representing a document in the patient's document list.
/// </summary>
public sealed record PatientDocumentDto(
    Guid DocumentId,
    string FileName,
    string FileFormat,
    long FileSizeBytes,
    string ProcessingStatus,
    DateTime UploadedAt);

/// <summary>
/// Returns clinical documents for the authenticated patient, ordered by upload date descending.
/// </summary>
public sealed record GetPatientDocumentsQuery(
    Guid PatientUserId) : IRequest<IReadOnlyList<PatientDocumentDto>>;
