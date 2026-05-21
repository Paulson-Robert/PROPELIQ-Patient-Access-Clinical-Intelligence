using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Uploads a clinical document for a patient (US_032).
///
/// The command carries the raw stream and metadata extracted from the
/// HTTP request so that the Application layer remains free of ASP.NET
/// dependencies. Validation is split:
/// - Structural validation (empty GUID, missing name) is performed here.
/// - Format/size validation is performed in <see cref="IDocumentStorageService"/>
///   so both layers enforce the same rules independently (AC-02).
///
/// On success the handler returns the new <c>DocumentId</c>.
/// </summary>
public sealed record UploadDocumentCommand(
    Guid PatientUserId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream) : IRequest<UploadDocumentResult>;

internal sealed class UploadDocumentCommandHandler
    : IRequestHandler<UploadDocumentCommand, UploadDocumentResult>
{
    private readonly IDocumentStorageService _storage;

    public UploadDocumentCommandHandler(IDocumentStorageService storage)
    {
        _storage = storage;
    }

    public Task<UploadDocumentResult> Handle(
        UploadDocumentCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PatientUserId == Guid.Empty)
        {
            return Task.FromResult(new UploadDocumentResult(
                Success: false,
                DocumentId: null,
                FailureCode: "INVALID_REQUEST",
                FailureReason: "PatientUserId is required."));
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return Task.FromResult(new UploadDocumentResult(
                Success: false,
                DocumentId: null,
                FailureCode: "INVALID_REQUEST",
                FailureReason: "FileName is required."));
        }

        return _storage.StoreAsync(
            new UploadDocumentRequest(
                request.PatientUserId,
                request.FileName,
                request.ContentType,
                request.FileSizeBytes,
                request.FileStream),
            cancellationToken);
    }
}
