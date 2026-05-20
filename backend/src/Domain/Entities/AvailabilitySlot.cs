namespace Domain.Entities;

/// <summary>
/// Represents a provider's bookable availability window (ADD-5).
/// </summary>
public class AvailabilitySlot
{
    public Guid SlotId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? RecurrencePattern { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockExpiry { get; set; }
    public uint Version { get; set; }

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Appointment> PreferredAppointments { get; set; } = [];
    public ICollection<PreferredSlotQueue> PreferredSlotQueues { get; set; } = [];
}
