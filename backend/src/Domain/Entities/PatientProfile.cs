using Domain.Enums;

namespace Domain.Entities;

public class PatientProfile
{
    public Guid PatientProfileId { get; set; }
    public Guid UserId { get; set; }

    // PHI — encrypted at rest via pgcrypto value converters
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? InsuranceId { get; set; }

    public string? InsuranceName { get; set; }
    public InsuranceValidationStatus? InsuranceValidationStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public PatientView? PatientView { get; set; }
    public NoShowRiskFactor? NoShowRiskFactor { get; set; }
    public ICollection<ClinicalDocument> ClinicalDocuments { get; set; } = [];
    public ICollection<IntakeRecord> IntakeRecords { get; set; } = [];
    public ICollection<ExtractedDataRecord> ExtractedDataRecords { get; set; } = [];
    public ICollection<DataConflict> DataConflicts { get; set; } = [];
    public ICollection<MedicalCodeMapping> MedicalCodeMappings { get; set; } = [];
}
