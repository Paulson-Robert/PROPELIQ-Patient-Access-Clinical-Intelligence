using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

internal sealed class GetPatientIntakesQueryHandler
    : IRequestHandler<GetPatientIntakesQuery, IReadOnlyList<PatientIntakeDto>>
{
    private readonly ApplicationDbContext _db;

    public GetPatientIntakesQueryHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PatientIntakeDto>> Handle(
        GetPatientIntakesQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _db.IntakeRecords
            .AsNoTracking()
            .Where(i => i.PatientProfile.UserId == request.PatientUserId)
            .OrderByDescending(i => i.LastModifiedAt)
            .Select(i => new PatientIntakeDto(
                i.IntakeId,
                i.AppointmentId,
                i.IntakeMode.ToString(),
                i.ReasonForVisit,
                i.CompletedAt,
                i.LastModifiedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.AsReadOnly();
    }
}
