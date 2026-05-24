using Application.Queries;
using Infrastructure.Data;
using Infrastructure.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Handlers;

internal sealed class GetPatientDocumentsQueryHandler
    : IRequestHandler<GetPatientDocumentsQuery, IReadOnlyList<PatientDocumentDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly DocumentStorageOptions _storageOptions;

    public GetPatientDocumentsQueryHandler(
        ApplicationDbContext db,
        IOptions<DocumentStorageOptions> storageOptions)
    {
        _db = db;
        _storageOptions = storageOptions.Value;
    }

    public async Task<IReadOnlyList<PatientDocumentDto>> Handle(
        GetPatientDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var completeUploadsImmediately = _storageOptions.CompleteUploadsImmediately;

        var results = await _db.ClinicalDocuments
            .AsNoTracking()
            .Where(d => d.PatientProfile.UserId == request.PatientUserId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new PatientDocumentDto(
                d.DocumentId,
                d.FileName,
                d.FileFormat.ToString(),
                d.FileSizeBytes,
                completeUploadsImmediately && d.ProcessingStatus != Domain.Enums.DocumentProcessingStatus.Failed
                    ? Domain.Enums.DocumentProcessingStatus.Completed.ToString()
                    : d.ProcessingStatus.ToString(),
                d.UploadedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.AsReadOnly();
    }
}
