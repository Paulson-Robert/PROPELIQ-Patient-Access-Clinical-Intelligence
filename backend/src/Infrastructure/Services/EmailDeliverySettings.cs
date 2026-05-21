namespace Infrastructure.Services;

public sealed class EmailDeliverySettings
{
    public const string SectionName = "EmailDelivery";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "smtp";
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "PropelIQ";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool UseStartTls { get; set; } = true;
}
