namespace Domain.Entities;

public class CptCode
{
    public Guid CptCodeId { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CodeSetVersion { get; set; } = string.Empty;
}
