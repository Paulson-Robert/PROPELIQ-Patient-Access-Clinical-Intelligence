using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

internal sealed class GetPatientNotificationsQueryHandler
    : IRequestHandler<GetPatientNotificationsQuery, IReadOnlyList<PatientNotificationDto>>
{
    private readonly ApplicationDbContext _db;

    public GetPatientNotificationsQueryHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PatientNotificationDto>> Handle(
        GetPatientNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.PatientId == request.PatientUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new PatientNotificationDto(
                n.NotificationId,
                n.AppointmentId,
                n.Channel.ToString(),
                n.NotificationType.ToString(),
                n.Status.ToString(),
                n.CreatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.AsReadOnly();
    }
}
