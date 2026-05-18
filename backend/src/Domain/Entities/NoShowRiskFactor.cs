namespace Domain.Entities;

public class NoShowRiskFactor
{
    public Guid FactorId { get; set; }
    public Guid PatientProfileId { get; set; }
    public int HistoricalNoShowCount { get; set; }
    public DateTime? LastNoShowDate { get; set; }
    public decimal AverageLeadTimeDays { get; set; }
    public string? PreferredTimeOfDay { get; set; }
    public bool IsNewPatient { get; set; }
    public DateTime LastCalculatedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
}
