using MediatR;

namespace Application.Queries;

/// <summary>
/// DTO representing a notification in the patient's notification list.
/// </summary>
public sealed record PatientNotificationDto(
    Guid NotificationId,
    Guid AppointmentId,
    string Channel,
    string NotificationType,
    string Status,
    DateTime CreatedAt);

/// <summary>
/// Returns notifications for the authenticated patient, ordered by creation date descending.
/// </summary>
public sealed record GetPatientNotificationsQuery(
    Guid PatientUserId) : IRequest<IReadOnlyList<PatientNotificationDto>>;
