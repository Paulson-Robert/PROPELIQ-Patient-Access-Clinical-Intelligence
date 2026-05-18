using Domain.Enums;

namespace Domain.Entities;

public class ClinicalDocument
{
    public Guid DocumentId { get; set; }
    public Guid PatientProfileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public MalwareScanStatus MalwareScanStatus { get; set; }
    public DocumentProcessingStatus ProcessingStatus { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
    public ICollection<ExtractedDataRecord> ExtractedDataRecords { get; set; } = [];
}
