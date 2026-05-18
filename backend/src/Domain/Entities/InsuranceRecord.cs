namespace Domain.Entities;

/// <summary>
/// Internal reference table for soft insurance validation.
/// Not linked to patient PHI — reference data only.
/// </summary>
public class InsuranceRecord
{
    public Guid RecordId { get; set; }
    public string InsuranceName { get; set; } = string.Empty;
    public string InsuranceIdPattern { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
