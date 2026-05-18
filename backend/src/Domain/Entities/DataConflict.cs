using Domain.Enums;

namespace Domain.Entities;

public class DataConflict
{
    public Guid ConflictId { get; set; }
    public Guid PatientProfileId { get; set; }
    public ConflictType ConflictType { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Value1 { get; set; } = string.Empty;
    public Guid SourceDocumentId1 { get; set; }
    public string Value2 { get; set; } = string.Empty;
    public Guid SourceDocumentId2 { get; set; }
    public ResolutionStatus ResolutionStatus { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
    public ClinicalDocument SourceDocument1 { get; set; } = null!;
    public ClinicalDocument SourceDocument2 { get; set; } = null!;
    public User? ResolvedByUser { get; set; }
}
