using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

public sealed class GetPatientIntakesQueryHandler
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
        var query = _db.IntakeRecords.AsNoTracking();

        if (!IsStaffScopedRole(request.ActorRole))
        {
            query = query.Where(i => i.PatientProfile.UserId == request.ActorUserId);
        }

        var results = await query
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

    private static bool IsStaffScopedRole(string actorRole)
        => actorRole.Equals("Staff", StringComparison.OrdinalIgnoreCase)
           || actorRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
}
