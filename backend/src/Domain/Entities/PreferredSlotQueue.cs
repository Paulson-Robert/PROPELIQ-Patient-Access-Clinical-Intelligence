using Domain.Enums;

namespace Domain.Entities;

public class PreferredSlotQueue
{
    public Guid QueueId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PreferredSlotId { get; set; }
    public DateTime RequestedAt { get; set; }
    public QueueStatus Status { get; set; }

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
    public AvailabilitySlot PreferredSlot { get; set; } = null!;
}
