namespace Domain.Entities;

/// <summary>
/// Immutable audit trail — append-only. No UPDATE or DELETE operations permitted (ADD-8).
/// </summary>
public class AuditLog
{
    public long AuditLogId { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? ActorUserId { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;

    // JSONB column — null for system actions with no additional detail
    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    // Navigation properties
    public User? ActorUser { get; set; }
}
