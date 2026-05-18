using Domain.Enums;

namespace Domain.Entities;

public class ExtractedDataRecord
{
    public Guid RecordId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid PatientProfileId { get; set; }
    public ExtractedDataType DataType { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool IsVerified { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? SourceLocation { get; set; }

    // Navigation properties
    public ClinicalDocument Document { get; set; } = null!;
    public PatientProfile PatientProfile { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
    public ICollection<MedicalCodeMapping> MedicalCodeMappings { get; set; } = [];
}
