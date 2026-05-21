using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.AI;

// ---------------------------------------------------------------------------
// AiIntakeService — OpenAI Chat Completions client (AC-01)
// ---------------------------------------------------------------------------

/// <summary>
/// Typed <see cref="HttpClient"/> wrapper for the OpenAI Chat Completions API (AC-01).
///
/// Retry policy: one automatic retry on <see cref="TaskCanceledException"/> or
/// <see cref="HttpRequestException"/> (edge case: AI provider timeout).
/// Token-limit detection: reads the OpenAI error code "context_length_exceeded" and
/// surfaces <see cref="AiIntakeTurnResult.IsTokenLimitReached"/> so the orchestrator
/// can trim history and retry (edge case: token limit reached).
/// </summary>
public sealed class AiIntakeService : IAiIntakeService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly AiIntakeOptions _options;
    private readonly ILogger<AiIntakeService> _logger;

    public AiIntakeService(
        HttpClient httpClient,
        IOptions<AiIntakeOptions> options,
        ILogger<AiIntakeService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<AiIntakeTurnResult> SendTurnAsync(
        string systemPrompt,
        IReadOnlyList<AiIntakeTurn> history,
        CancellationToken cancellationToken = default)
        => SendWithRetryAsync(systemPrompt, history, attempt: 0, cancellationToken);

    private async Task<AiIntakeTurnResult> SendWithRetryAsync(
        string systemPrompt,
        IReadOnlyList<AiIntakeTurn> history,
        int attempt,
        CancellationToken cancellationToken)
    {
        try
        {
            return await CallCompletionsAsync(systemPrompt, history, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException && attempt == 0)
        {
            _logger.LogWarning(
                "AI intake call failed (attempt {Attempt}); retrying once. Error: {Message}",
                attempt + 1, ex.Message);

            return await SendWithRetryAsync(systemPrompt, history, attempt + 1, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AI intake call failed permanently after {Attempts} attempt(s).",
                attempt + 1);

            return new AiIntakeTurnResult(
                Success: false,
                Content: null,
                FailureReason: "AI provider unavailable.",
                IsTokenLimitReached: false);
        }
    }

    private async Task<AiIntakeTurnResult> CallCompletionsAsync(
        string systemPrompt,
        IReadOnlyList<AiIntakeTurn> history,
        CancellationToken cancellationToken)
    {
        var messages = BuildMessages(systemPrompt, history);

        var body = new
        {
            model = _options.Model,
            messages,
            max_tokens = _options.MaxResponseTokens,
            temperature = 0.3,
        };

        var json = JsonSerializer.Serialize(body, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.CompletionsUrl)
        {
            Content = content,
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        using var response = await _httpClient
            .SendAsync(request, cts.Token)
            .ConfigureAwait(false);

        var responseBody = await response.Content
            .ReadAsStringAsync(CancellationToken.None)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            if (responseBody.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("AI provider token limit reached for current conversation history.");
                return new AiIntakeTurnResult(
                    Success: false,
                    Content: null,
                    FailureReason: "Token limit reached.",
                    IsTokenLimitReached: true);
            }

            _logger.LogWarning(
                "AI provider returned {StatusCode}. Response length: {Length} chars.",
                (int)response.StatusCode, responseBody.Length);

            throw new HttpRequestException(
                $"AI provider returned {(int)response.StatusCode}.");
        }

        var text = ExtractMessageText(responseBody);

        return new AiIntakeTurnResult(
            Success: true,
            Content: text,
            FailureReason: null,
            IsTokenLimitReached: false);
    }

    private static List<object> BuildMessages(string systemPrompt, IReadOnlyList<AiIntakeTurn> history)
    {
        var messages = new List<object>(history.Count + 1)
        {
            new { role = "system", content = systemPrompt },
        };

        foreach (var turn in history)
        {
            messages.Add(new { role = turn.Role, content = turn.Content });
        }

        return messages;
    }

    private static string? ExtractMessageText(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }
}

// ---------------------------------------------------------------------------
// AiIntakePersistenceService — IntakeRecord persistence (AC-04)
// ---------------------------------------------------------------------------

/// <summary>
/// Creates or updates the <see cref="IntakeRecord"/> for the completed AI intake (AC-04).
/// Looks up the <see cref="PatientProfile"/> by <c>UserId</c> to obtain
/// <c>PatientProfileId</c> before writing.
/// </summary>
public sealed class AiIntakePersistenceService : IAiIntakePersistenceService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AiIntakePersistenceService> _logger;

    public AiIntakePersistenceService(
        ApplicationDbContext db,
        ILogger<AiIntakePersistenceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task PersistAsync(
        Guid patientUserId,
        Guid appointmentId,
        AiIntakeSummary summary,
        CancellationToken cancellationToken = default)
    {
        var profile = await _db.PatientProfiles
            .FirstOrDefaultAsync(p => p.UserId == patientUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            _logger.LogError(
                "Cannot persist intake: PatientProfile not found for UserId {UserId}.",
                patientUserId);
            throw new InvalidOperationException(
                $"Patient profile not found for user {patientUserId}.");
        }

        var existing = await _db.IntakeRecords
            .FirstOrDefaultAsync(
                r => r.AppointmentId == appointmentId && r.PatientProfileId == profile.PatientProfileId,
                cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;

        if (existing is not null)
        {
            existing.MedicalHistory = summary.ChronicConditions;
            existing.Medications = summary.CurrentMedications;
            existing.Allergies = summary.Allergies;
            existing.CurrentSymptoms = summary.SurgicalHistory;
            existing.ReasonForVisit = summary.ReasonForVisit;
            existing.CompletedAt = now;
            existing.LastModifiedAt = now;
        }
        else
        {
            _db.IntakeRecords.Add(new IntakeRecord
            {
                IntakeId = Guid.NewGuid(),
                PatientProfileId = profile.PatientProfileId,
                AppointmentId = appointmentId,
                IntakeMode = IntakeMode.AI,
                MedicalHistory = summary.ChronicConditions,
                Medications = summary.CurrentMedications,
                Allergies = summary.Allergies,
                CurrentSymptoms = summary.SurgicalHistory,
                ReasonForVisit = summary.ReasonForVisit,
                CompletedAt = now,
                LastModifiedAt = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "AI intake persisted for AppointmentId {AppointmentId}.",
            appointmentId);
    }
}
