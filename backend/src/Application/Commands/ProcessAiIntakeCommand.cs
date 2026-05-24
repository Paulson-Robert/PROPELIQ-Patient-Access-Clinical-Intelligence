using Application.Interfaces;
using MediatR;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Processes one turn of the AI-assisted pre-visit intake conversation (AC-01).
///
/// Each call represents a single round-trip: the caller supplies the full
/// conversation history (all prior turns) and the latest patient message.
/// The handler de-identifies the patient message (AC-02), forwards the history
/// to the AI provider, parses the response for structured data (AC-03), and
/// persists the intake record when all fields are captured (AC-04).
///
/// Edge cases handled:
/// - AI provider timeout: service retries once, then returns <see cref="AiIntakeResult.SuggestManualFallback"/>.
/// - Token limit reached: oldest user turns are trimmed and the call is retried once.
/// </summary>
public sealed record ProcessAiIntakeCommand(
    Guid ActorUserId,
    string ActorRole,
    Guid AppointmentId,
    IReadOnlyList<AiIntakeTurn> ConversationHistory) : IRequest<AiIntakeResult>;

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

internal sealed class ProcessAiIntakeCommandHandler
    : IRequestHandler<ProcessAiIntakeCommand, AiIntakeResult>
{
    // Regex that locates a JSON block inside the AI response text.
    private static readonly Regex JsonBlockPattern = new(
        @"\{[^{}]*""complete""\s*:\s*true[^{}]*\}",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    // System prompt sent on every call — never contains patient names or identifiers (AC-02).
    private const string SystemPrompt = """
        You are a medical intake assistant for a healthcare portal.
        Your role is to collect the following information from the patient, asking ONE question at a time:
          1. Chronic conditions (e.g. diabetes, hypertension)
          2. Current medications and dosages
          3. Known allergies (medications, food, environmental)
          4. Prior surgical or procedure history
          5. Primary reason for today's visit

        Guidelines:
        - Keep responses brief and conversational.
        - Briefly acknowledge and summarise what the patient just shared before asking the next question.
        - Do NOT ask for or reference the patient's full name, date of birth, address, or any identification numbers.
        - If a response seems unclear, ask a single clarifying question.

        When you have collected all five items, respond ONLY with the following JSON (no other text):
        {"complete":true,"chronicConditions":"...","currentMedications":"...","allergies":"...","surgicalHistory":"...","reasonForVisit":"..."}
        Use null (not the string "null") for any field the patient explicitly said does not apply.
        """;

    // Minimum user turns to retain after trimming (keeps last 2 exchanges for context).
    private const int MinHistoryTurnsAfterTrim = 4;

    private readonly IAiIntakeService _aiService;
    private readonly IDeIdentificationService _deId;
    private readonly IAiIntakePersistenceService _persistence;

    public ProcessAiIntakeCommandHandler(
        IAiIntakeService aiService,
        IDeIdentificationService deId,
        IAiIntakePersistenceService persistence)
    {
        _aiService = aiService;
        _deId = deId;
        _persistence = persistence;
    }

    public async Task<AiIntakeResult> Handle(
        ProcessAiIntakeCommand request,
        CancellationToken cancellationToken)
    {
        var history = BuildDeIdentifiedHistory(request.ConversationHistory);

        var result = await _aiService.SendTurnAsync(SystemPrompt, history, cancellationToken)
            .ConfigureAwait(false);

        // Edge case: token limit — trim oldest turns and retry once
        if (result.IsTokenLimitReached)
        {
            var trimmed = TrimOldestTurns(history);
            result = await _aiService.SendTurnAsync(SystemPrompt, trimmed, cancellationToken)
                .ConfigureAwait(false);
        }

        if (!result.Success)
        {
            return new AiIntakeResult(
                Success: false,
                NextMessage: null,
                IsComplete: false,
                Summary: null,
                FailureReason: result.FailureReason,
                SuggestManualFallback: true);
        }

        var responseText = result.Content ?? string.Empty;

        var summary = TryExtractSummary(responseText);
        if (summary is not null)
        {
            var persistenceResult = await _persistence.PersistAsync(
                request.ActorUserId,
                request.ActorRole,
                request.AppointmentId,
                summary,
                cancellationToken).ConfigureAwait(false);

            if (!persistenceResult.Success)
            {
                return new AiIntakeResult(
                    Success: false,
                    NextMessage: null,
                    IsComplete: false,
                    Summary: null,
                    FailureReason: persistenceResult.FailureReason,
                    SuggestManualFallback: true);
            }

            return new AiIntakeResult(
                Success: true,
                NextMessage: null,
                IsComplete: true,
                Summary: summary,
                FailureReason: null,
                SuggestManualFallback: false);
        }

        return new AiIntakeResult(
            Success: true,
            NextMessage: responseText,
            IsComplete: false,
            Summary: null,
            FailureReason: null,
            SuggestManualFallback: false);
    }

    /// <summary>
    /// Returns a new history list with all "user" turn content scrubbed of PHI (AC-02).
    /// "assistant" turns from the AI provider are passed through unchanged.
    /// User message content is also capped at 500 characters to limit prompt-injection surface.
    /// </summary>
    private IReadOnlyList<AiIntakeTurn> BuildDeIdentifiedHistory(
        IReadOnlyList<AiIntakeTurn> source)
    {
        var result = new List<AiIntakeTurn>(source.Count);

        foreach (var turn in source)
        {
            if (turn.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                // Truncate, then scrub PHI (AC-02).
                var truncated = turn.Content.Length > 500
                    ? turn.Content[..500]
                    : turn.Content;

                result.Add(turn with { Content = _deId.Scrub(truncated) });
            }
            else
            {
                result.Add(turn);
            }
        }

        return result;
    }

    /// <summary>
    /// Trims the oldest user+assistant turn pair, keeping at least
    /// <see cref="MinHistoryTurnsAfterTrim"/> turns for coherent context.
    /// </summary>
    private static IReadOnlyList<AiIntakeTurn> TrimOldestTurns(
        IReadOnlyList<AiIntakeTurn> history)
    {
        if (history.Count <= MinHistoryTurnsAfterTrim)
            return history;

        // Drop the first 2 entries (oldest user + assistant exchange).
        return history.Skip(2).ToList();
    }

    /// <summary>
    /// Attempts to extract a completed intake summary from the AI response.
    /// Returns <c>null</c> when the response is a conversational message, not the final JSON.
    /// </summary>
    private AiIntakeSummary? TryExtractSummary(string responseText)
    {
        var match = JsonBlockPattern.Match(responseText);
        if (!match.Success)
            return null;

        try
        {
            using var doc = JsonDocument.Parse(match.Value);
            var root = doc.RootElement;

            // AC-03: extract structured fields from the AI response JSON.
            return new AiIntakeSummary(
                ChronicConditions: GetNullableString(root, "chronicConditions"),
                CurrentMedications: GetNullableString(root, "currentMedications"),
                Allergies: GetNullableString(root, "allergies"),
                SurgicalHistory: GetNullableString(root, "surgicalHistory"),
                ReasonForVisit: root.GetProperty("reasonForVisit").GetString() ?? string.Empty);
        }
        catch (Exception ex)
        {
            // Parsing failure — treat as a non-terminal conversational response.
            _ = ex;
            return null;
        }
    }

    private static string? GetNullableString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return null;

        return prop.ValueKind == JsonValueKind.Null ? null : prop.GetString();
    }
}
