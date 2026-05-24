using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

internal sealed class GetAppointmentQueryHandler
    : IRequestHandler<GetAppointmentQuery, AppointmentDetailDto?>
{
    private readonly ApplicationDbContext _db;

    public GetAppointmentQueryHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AppointmentDetailDto?> Handle(
        GetAppointmentQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _db.Appointments
            .AsNoTracking()
            .Where(a =>
                a.AppointmentId == request.AppointmentId &&
                a.PatientId == request.PatientUserId)
            .Select(a => new AppointmentDetailDto(
                a.AppointmentId,
                a.SlotId,
                a.Slot.ProviderName,
                a.Slot.Specialty,
                a.Slot.StartTime,
                a.Slot.EndTime,
                (int)(a.Slot.EndTime - a.Slot.StartTime).TotalMinutes,
                a.Status.ToString(),
                a.Patient.Email,
                a.InsuranceProvider,
                a.InsurancePolicyNumber))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}
