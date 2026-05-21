using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Calendar;

/// <summary>
/// Google Calendar REST API client with OAuth 2.0 token exchange and exponential backoff
/// on rate-limit (429) and service-unavailable (503) responses.
/// </summary>
public sealed class GoogleCalendarService : ICalendarService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string CalendarEventsBaseUrl =
        "https://www.googleapis.com/calendar/v3/calendars/primary/events";

    // 4 retries gives 1s + 2s + 4s + 8s = 15s total backoff before giving up
    private const int MaxRetries = 4;

    private readonly HttpClient _httpClient;
    private readonly GoogleCalendarSettings _settings;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(
        HttpClient httpClient,
        IOptions<GoogleCalendarSettings> settings,
        ILogger<GoogleCalendarService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<GoogleTokenResponse?> ExchangeAuthCodeAsync(
        string authCode,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, string>
        {
            ["code"] = authCode,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["redirect_uri"] = _settings.RedirectUri,
            ["grant_type"] = "authorization_code",
        };

        return await PostTokenRequestAsync(body, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GoogleTokenResponse?> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["grant_type"] = "refresh_token",
        };

        return await PostTokenRequestAsync(body, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> CreateEventAsync(
        string accessToken,
        AppointmentEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventBody = new GoogleEventPayload(
            Summary: $"Appointment: {request.Specialty} with {request.ProviderName}",
            Start: new GoogleEventTime(request.StartTimeUtc.ToString("O"), "UTC"),
            End: new GoogleEventTime(request.EndTimeUtc.ToString("O"), "UTC"),
            ExtendedProperties: new GoogleExtendedProperties(
                Private: new Dictionary<string, string>
                {
                    ["appointmentId"] = request.AppointmentId.ToString(),
                }));

        var json = JsonSerializer.Serialize(eventBody, JsonOptions);

        using var response = await ExecuteWithRetryAsync(
            () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, CalendarEventsBaseUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return req;
            },
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Google Calendar CreateEvent failed. AppointmentId={AppointmentId}, StatusCode={StatusCode}.",
                request.AppointmentId,
                response.StatusCode);
            return null;
        }

        using var stream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await JsonSerializer
            .DeserializeAsync<GoogleEventResponse>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result?.Id;
    }

    public async Task<bool> DeleteEventByAppointmentIdAsync(
        string accessToken,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var searchUrl =
            $"{CalendarEventsBaseUrl}?privateExtendedProperty=appointmentId%3D{appointmentId}";

        using var searchResponse = await ExecuteWithRetryAsync(
            () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Get, searchUrl);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return req;
            },
            cancellationToken).ConfigureAwait(false);

        if (!searchResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Google Calendar event search failed. AppointmentId={AppointmentId}, StatusCode={StatusCode}.",
                appointmentId,
                searchResponse.StatusCode);
            return false;
        }

        using var stream = await searchResponse.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var events = await JsonSerializer
            .DeserializeAsync<GoogleEventsListResponse>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        var eventId = events?.Items?.FirstOrDefault()?.Id;
        if (eventId is null)
            return true; // already removed or never created — idempotent success

        var deleteUrl = $"{CalendarEventsBaseUrl}/{Uri.EscapeDataString(eventId)}";

        using var deleteResponse = await ExecuteWithRetryAsync(
            () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return req;
            },
            cancellationToken).ConfigureAwait(false);

        if (deleteResponse.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            return true;

        if (!deleteResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Google Calendar DeleteEvent failed. AppointmentId={AppointmentId}, EventId={EventId}, StatusCode={StatusCode}.",
                appointmentId,
                eventId,
                deleteResponse.StatusCode);
            return false;
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(1);
        HttpResponseMessage? last = null;

        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 32));
            }

            last?.Dispose();
            last = await _httpClient.SendAsync(requestFactory(), cancellationToken)
                .ConfigureAwait(false);

            if (last.StatusCode is not HttpStatusCode.TooManyRequests
                                and not HttpStatusCode.ServiceUnavailable)
            {
                return last;
            }

            _logger.LogWarning(
                "Google Calendar API rate-limited. Attempt={Attempt}/{Total}, StatusCode={StatusCode}.",
                attempt + 1,
                MaxRetries + 1,
                last.StatusCode);
        }

        return last!;
    }

    private async Task<GoogleTokenResponse?> PostTokenRequestAsync(
        Dictionary<string, string> body,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(body);
        HttpResponseMessage response;

        try
        {
            response = await _httpClient
                .PostAsync(TokenEndpoint, content, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Google OAuth token endpoint unreachable.");
            return null;
        }

        using var _ = response;

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Google OAuth token exchange failed. StatusCode={StatusCode}.",
                response.StatusCode);
            return null;
        }

        using var stream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var raw = await JsonSerializer
            .DeserializeAsync<RawGoogleTokenResponse>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (raw is null)
            return null;

        return new GoogleTokenResponse(raw.AccessToken, raw.RefreshToken, raw.ExpiresIn);
    }

    // -------------------------------------------------------------------------
    // Internal DTOs — used only for (de)serialization against Google's API
    // -------------------------------------------------------------------------

    private sealed record GoogleEventPayload(
        string Summary,
        GoogleEventTime Start,
        GoogleEventTime End,
        GoogleExtendedProperties ExtendedProperties);

    private sealed record GoogleEventTime(string DateTime, string TimeZone);

    private sealed record GoogleExtendedProperties(
        [property: JsonPropertyName("private")]
        Dictionary<string, string> Private);

    private sealed class RawGoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }

    private sealed class GoogleEventResponse
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class GoogleEventsListResponse
    {
        public IReadOnlyList<GoogleEventResponse>? Items { get; set; }
    }
}
