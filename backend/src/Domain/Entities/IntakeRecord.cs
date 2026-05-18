using Domain.Enums;

namespace Domain.Entities;

public class IntakeRecord
{
    public Guid IntakeId { get; set; }
    public Guid PatientProfileId { get; set; }
    public Guid AppointmentId { get; set; }
    public IntakeMode IntakeMode { get; set; }

    // JSONB columns — null when not yet provided
    public string? MedicalHistory { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? Medications { get; set; }
    public string? Allergies { get; set; }

    public string ReasonForVisit { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
}
