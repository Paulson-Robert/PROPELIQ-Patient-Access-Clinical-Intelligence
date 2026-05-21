using Domain.Enums;

namespace Domain.Entities;

public class Appointment
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid SlotId { get; set; }
    public AppointmentStatus Status { get; set; }
    public NoShowRiskTier? NoShowRiskTier { get; set; }
    public decimal? NoShowRiskScore { get; set; }
    public BookingType BookingType { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public Guid? PreferredSlotId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Same-day queue fields (US_024)
    public int? QueuePosition { get; set; }
    public DateTime? ArrivalTimestamp { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public AvailabilitySlot Slot { get; set; } = null!;
    public AvailabilitySlot? PreferredSlot { get; set; }
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<PreferredSlotQueue> PreferredSlotQueues { get; set; } = [];
    public ICollection<IntakeRecord> IntakeRecords { get; set; } = [];
}
