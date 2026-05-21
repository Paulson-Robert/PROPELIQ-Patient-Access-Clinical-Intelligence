namespace Infrastructure.Notifications;

public sealed class NotificationSettings
{
    public const string SectionName = "Notifications";

    /// <summary>
    /// Email address that receives staff alerts when all reminder delivery retries are exhausted.
    /// Leave empty to disable email alerts (warnings are always logged).
    /// </summary>
    public string StaffAlertEmail { get; set; } = string.Empty;
}
