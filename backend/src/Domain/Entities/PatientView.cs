using Domain.Enums;

namespace Domain.Entities;

public class PatientView
{
    public Guid PatientViewId { get; set; }
    public Guid PatientProfileId { get; set; }

    // JSONB columns — null when no aggregated data exists yet (AC-03)
    public string? AggregatedVitals { get; set; }
    public string? AggregatedMedications { get; set; }
    public string? AggregatedAllergies { get; set; }
    public string? AggregatedDiagnoses { get; set; }
    public string? AggregatedProcedures { get; set; }

    public DateTime? LastAggregatedAt { get; set; }
    public PatientViewVerificationStatus VerificationStatus { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
}
