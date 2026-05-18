using Domain.Enums;

namespace Domain.Entities;

public class Notification
{
    public Guid NotificationId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationType NotificationType { get; set; }
    public NotificationStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
    public User Patient { get; set; } = null!;
}
