using Domain.Enums;

namespace Domain.Entities;

public class CalendarSync
{
    public Guid SyncId { get; set; }
    public Guid UserId { get; set; }
    public CalendarProvider Provider { get; set; }

    // Sensitive tokens — encrypt at rest (infrastructure concern)
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime TokenExpiry { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
