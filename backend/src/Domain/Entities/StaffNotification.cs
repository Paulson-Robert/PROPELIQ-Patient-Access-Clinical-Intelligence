using Domain.Enums;

namespace Domain.Entities;

public class StaffNotification
{
    public Guid StaffNotificationId { get; set; }
    public Guid StaffUserId { get; set; }
    public StaffNotificationVariant Variant { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public User Staff { get; set; } = null!;
}
