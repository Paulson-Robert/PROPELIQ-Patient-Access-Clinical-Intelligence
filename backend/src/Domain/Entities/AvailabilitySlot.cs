namespace Domain.Entities;

public class AvailabilitySlot
{
    public Guid SlotId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockExpiry { get; set; }
    public string? RecurrencePattern { get; set; }

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Appointment> PreferredAppointments { get; set; } = [];
    public ICollection<PreferredSlotQueue> PreferredSlotQueues { get; set; } = [];
}
