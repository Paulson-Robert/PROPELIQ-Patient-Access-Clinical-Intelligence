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
/// Microsoft Graph Calendar REST API client with OAuth 2.0 token exchange and exponential backoff
/// on rate-limit (429) and service-unavailable (503) responses (AC-01, AC-03, AC-04, AC-05).
/// appointmentId is stored on Graph events via singleValueExtendedProperties to enable
/// deletion by appointmentId without a local event-ID mapping (AC-04).
/// </summary>
public sealed class OutlookCalendarService : IOutlookCalendarService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // Stable PropelIQ GUID namespace for Graph singleValueExtendedProperties.
    // Decision: chosen once and never changed — property set GUID is part of the stored data contract.
    private const string AppointmentIdPropertyId =
        "String {66f5a359-4659-4830-9070-00047ec6ac6e} Name appointmentId";

    private const string TokenEndpoint =
        "https://login.microsoftonline.com/common/oauth2/v2.0/token";

    private const string GraphEventsBaseUrl =
        "https://graph.microsoft.com/v1.0/me/calendar/events";

    // 4 retries gives 1s + 2s + 4s + 8s = 15s total backoff before giving up
    private const int MaxRetries = 4;

    private readonly HttpClient _httpClient;
    private readonly OutlookCalendarSettings _settings;
    private readonly ILogger<OutlookCalendarService> _logger;

    public OutlookCalendarService(
        HttpClient httpClient,
        IOptions<OutlookCalendarSettings> settings,
        ILogger<OutlookCalendarService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<MicrosoftTokenResponse?> ExchangeAuthCodeAsync(
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
            ["scope"] = "Calendars.ReadWrite offline_access",
        };

        return await PostTokenRequestAsync(body, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MicrosoftTokenResponse?> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["grant_type"] = "refresh_token",
            ["scope"] = "Calendars.ReadWrite offline_access",
        };

        return await PostTokenRequestAsync(body, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> CreateEventAsync(
        string accessToken,
        AppointmentEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventBody = new GraphEventPayload(
            Subject: $"Appointment: {request.Specialty} with {request.ProviderName}",
            Start: new GraphEventTime(request.StartTimeUtc.ToString("O"), "UTC"),
            End: new GraphEventTime(request.EndTimeUtc.ToString("O"), "UTC"),
            SingleValueExtendedProperties:
            [
                new GraphExtendedProperty(AppointmentIdPropertyId, request.AppointmentId.ToString()),
            ]);

        var json = JsonSerializer.Serialize(eventBody, JsonOptions);

        using var response = await ExecuteWithRetryAsync(
            () =>
            {
                var req = new HttpRequestMessage(HttpMethod.Post, GraphEventsBaseUrl)
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
                "Graph Calendar CreateEvent failed. AppointmentId={AppointmentId}, StatusCode={StatusCode}.",
                request.AppointmentId,
                response.StatusCode);
            return null;
        }

        using var stream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await JsonSerializer
            .DeserializeAsync<GraphEventResponse>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result?.Id;
    }

    public async Task<bool> DeleteEventByAppointmentIdAsync(
        string accessToken,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        // Encode the property filter; appointmentId value is a GUID — safe for URL embedding.
        var encodedPropertyId = Uri.EscapeDataString(AppointmentIdPropertyId);
        var searchUrl =
            $"{GraphEventsBaseUrl}" +
            $"?$filter=singleValueExtendedProperties/any(ep:ep/id eq '{encodedPropertyId}'" +
            $" and ep/value eq '{appointmentId}')" +
            $"&$select=id";

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
                "Graph Calendar event search failed. AppointmentId={AppointmentId}, StatusCode={StatusCode}.",
                appointmentId,
                searchResponse.StatusCode);
            return false;
        }

        using var stream = await searchResponse.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var events = await JsonSerializer
            .DeserializeAsync<GraphEventsListResponse>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        var eventId = events?.Value?.FirstOrDefault()?.Id;
        if (eventId is null)
            return true; // already removed or never created — idempotent success

        var deleteUrl = $"{GraphEventsBaseUrl}/{Uri.EscapeDataString(eventId)}";

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
                "Graph Calendar DeleteEvent failed. AppointmentId={AppointmentId}, EventId={EventId}, StatusCode={StatusCode}.",
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
                delay *= 2;
            }

            last = await _httpClient.SendAsync(requestFactory(), cancellationToken)
                .ConfigureAwait(false);

            if (last.StatusCode is not HttpStatusCode.TooManyRequests
                                and not HttpStatusCode.ServiceUnavailable)
            {
                return last;
            }

            _logger.LogWarning(
                "Graph Calendar API rate-limited. Attempt={Attempt}/{Total}, StatusCode={StatusCode}.",
                attempt + 1,
                MaxRetries + 1,
                last.StatusCode);
        }

        return last!;
    }

    private async Task<MicrosoftTokenResponse?> PostTokenRequestAsync(
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
            _logger.LogWarning(ex, "Microsoft identity token endpoint unreachable.");
            return null;
        }

        using var _ = response;

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Microsoft OAuth token exchange failed. StatusCode={StatusCode}.",
                response.StatusCode);
            return null;
        }

        using var stream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var raw = await JsonSerializer
            .DeserializeAsync<RawMicrosoftTokenResponse>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (raw is null)
            return null;

        return new MicrosoftTokenResponse(raw.AccessToken, raw.RefreshToken, raw.ExpiresIn);
    }

    // -------------------------------------------------------------------------
    // Internal DTOs — used only for (de)serialization against Graph/identity APIs
    // -------------------------------------------------------------------------

    private sealed record GraphEventPayload(
        string Subject,
        GraphEventTime Start,
        GraphEventTime End,
        IReadOnlyList<GraphExtendedProperty> SingleValueExtendedProperties);

    private sealed record GraphEventTime(string DateTime, string TimeZone);

    private sealed record GraphExtendedProperty(string Id, string Value);

    private sealed class RawMicrosoftTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }

    private sealed class GraphEventResponse
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class GraphEventsListResponse
    {
        public IReadOnlyList<GraphEventResponse>? Value { get; set; }
    }
}
