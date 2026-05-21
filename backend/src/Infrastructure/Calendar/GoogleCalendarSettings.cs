namespace Infrastructure.Calendar;

/// <summary>
/// Configuration for the Google Calendar OAuth 2.0 client.
/// Bind from the "GoogleCalendar" section in appsettings.
/// Client secret must be supplied via environment variable or secrets manager — never hardcoded.
/// </summary>
public sealed class GoogleCalendarSettings
{
    public const string SectionName = "GoogleCalendar";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The redirect URI registered in the Google Cloud Console for the calendar OAuth consent screen.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
}
