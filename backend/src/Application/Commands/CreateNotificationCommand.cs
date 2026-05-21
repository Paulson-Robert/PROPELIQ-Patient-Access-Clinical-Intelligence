using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Creates staff in-app notifications for one or more staff users (US_028 AC-01).
/// Accepts a list of recipients to support batch creation under high notification volume.
/// </summary>
public sealed record CreateNotificationCommand(
    IReadOnlyList<Guid> StaffUserIds,
    string Variant,
    string Title,
    string? Message) : IRequest<IReadOnlyList<StaffNotificationDto>>;

internal sealed class CreateNotificationCommandHandler
    : IRequestHandler<CreateNotificationCommand, IReadOnlyList<StaffNotificationDto>>
{
    private readonly IStaffNotificationService _notifications;

    public CreateNotificationCommandHandler(IStaffNotificationService notifications)
    {
        _notifications = notifications;
    }

    public Task<IReadOnlyList<StaffNotificationDto>> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
        => _notifications.CreateAsync(
            new CreateStaffNotificationRequest(
                request.StaffUserIds,
                request.Variant,
                request.Title,
                request.Message),
            cancellationToken);
}
