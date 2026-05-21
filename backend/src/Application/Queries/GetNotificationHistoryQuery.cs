using Application.Interfaces;
using MediatR;

namespace Application.Queries;

/// <summary>
/// Returns paginated staff notification history for a given user, newest first (US_028 AC-02).
/// </summary>
public sealed record GetNotificationHistoryQuery(
    Guid StaffUserId,
    int Page = 1,
    int PageSize = 20) : IRequest<NotificationPageDto>;

internal sealed class GetNotificationHistoryQueryHandler
    : IRequestHandler<GetNotificationHistoryQuery, NotificationPageDto>
{
    private readonly IStaffNotificationService _notifications;

    public GetNotificationHistoryQueryHandler(IStaffNotificationService notifications)
    {
        _notifications = notifications;
    }

    public Task<NotificationPageDto> Handle(
        GetNotificationHistoryQuery request,
        CancellationToken cancellationToken)
        => _notifications.GetHistoryAsync(
            request.StaffUserId,
            request.Page,
            request.PageSize,
            cancellationToken);
}
