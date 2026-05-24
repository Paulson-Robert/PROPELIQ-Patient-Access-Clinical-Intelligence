using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

internal sealed class GetPatientAppointmentsQueryHandler
    : IRequestHandler<GetPatientAppointmentsQuery, IReadOnlyList<PatientAppointmentDto>>
{
    private readonly ApplicationDbContext _db;

    public GetPatientAppointmentsQueryHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PatientAppointmentDto>> Handle(
        GetPatientAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == request.PatientUserId);

        if (!string.IsNullOrWhiteSpace(request.StatusFilter))
        {
            if (Enum.TryParse<Domain.Enums.AppointmentStatus>(
                    request.StatusFilter, ignoreCase: true, out var status))
            {
                query = query.Where(a => a.Status == status);
            }
        }

        var results = await query
            .OrderByDescending(a => a.Slot.StartTime)
            .Select(a => new PatientAppointmentDto(
                a.AppointmentId,
                a.SlotId,
                a.Slot.ProviderName,
                a.Slot.Specialty,
                a.Slot.StartTime,
                a.Slot.EndTime,
                (int)(a.Slot.EndTime - a.Slot.StartTime).TotalMinutes,
                a.Status.ToString(),
                a.InsuranceProvider))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.AsReadOnly();
    }
}
