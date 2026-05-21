using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Marks a staff notification as read (US_028 AC-03).
/// Idempotent — repeated calls on an already-read notification are safe.
/// Throws <see cref="KeyNotFoundException"/> if the notification does not belong to the staff user.
/// </summary>
public sealed record MarkNotificationReadCommand(
    Guid StaffNotificationId,
    Guid StaffUserId) : IRequest<StaffNotificationDto>;

internal sealed class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, StaffNotificationDto>
{
    private readonly IStaffNotificationService _notifications;

    public MarkNotificationReadCommandHandler(IStaffNotificationService notifications)
    {
        _notifications = notifications;
    }

    public Task<StaffNotificationDto> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
        => _notifications.MarkReadAsync(
            request.StaffNotificationId,
            request.StaffUserId,
            cancellationToken);
}
