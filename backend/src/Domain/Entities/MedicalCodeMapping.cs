using Domain.Enums;

namespace Domain.Entities;

public class MedicalCodeMapping
{
    public Guid MappingId { get; set; }
    public Guid PatientProfileId { get; set; }
    public Guid ExtractedRecordId { get; set; }
    public MedicalCodeType CodeType { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool IsVerified { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string CodeSetVersion { get; set; } = string.Empty;

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
    public ExtractedDataRecord ExtractedRecord { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
