using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

internal sealed class GetPatientDocumentsQueryHandler
    : IRequestHandler<GetPatientDocumentsQuery, IReadOnlyList<PatientDocumentDto>>
{
    private readonly ApplicationDbContext _db;

    public GetPatientDocumentsQueryHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PatientDocumentDto>> Handle(
        GetPatientDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _db.ClinicalDocuments
            .AsNoTracking()
            .Where(d => d.PatientProfile.UserId == request.PatientUserId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new PatientDocumentDto(
                d.DocumentId,
                d.FileName,
                d.FileFormat.ToString(),
                d.FileSizeBytes,
                d.ProcessingStatus.ToString(),
                d.UploadedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.AsReadOnly();
    }
}
