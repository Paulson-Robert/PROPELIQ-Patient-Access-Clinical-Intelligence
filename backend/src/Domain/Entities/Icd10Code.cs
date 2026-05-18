namespace Domain.Entities;

public class Icd10Code
{
    public Guid Icd10CodeId { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CodeSetVersion { get; set; } = string.Empty;
}
