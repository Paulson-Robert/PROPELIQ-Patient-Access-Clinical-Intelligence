namespace Infrastructure.Calendar;

/// <summary>
/// Configuration for the Microsoft Graph Calendar OAuth 2.0 client.
/// Bind from the "OutlookCalendar" section in appsettings.
/// Client secret must be supplied via environment variable or secrets manager — never hardcoded.
/// </summary>
public sealed class OutlookCalendarSettings
{
    public const string SectionName = "OutlookCalendar";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The redirect URI registered in the Azure App Registration for the calendar OAuth consent screen.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
}
