namespace Application.Interfaces;

/// <summary>
/// A single conversational turn sent to or received from the AI provider.
/// </summary>
/// <param name="Role">"system", "user", or "assistant".</param>
/// <param name="Content">The message text. User turns must be de-identified before sending to the AI provider (AC-02).</param>
public sealed record AiIntakeTurn(string Role, string Content);

/// <summary>
/// Structured clinical data extracted from a completed AI intake conversation.
/// All values are de-identified summaries — no PHI.
/// </summary>
public sealed record AiIntakeSummary(
    string? ChronicConditions,
    string? CurrentMedications,
    string? Allergies,
    string? SurgicalHistory,
    string ReasonForVisit);

/// <summary>
/// Result of a single AI provider API call.
/// </summary>
public sealed record AiIntakeTurnResult(
    bool Success,
    string? Content,
    string? FailureReason,
    bool IsTokenLimitReached);

/// <summary>
/// Result returned to the caller of <see cref="ProcessAiIntakeCommand"/>.
/// </summary>
public sealed record AiIntakeResult(
    bool Success,
    string? NextMessage,
    bool IsComplete,
    AiIntakeSummary? Summary,
    string? FailureReason,
    bool SuggestManualFallback);

/// <summary>
/// Result returned after a completed AI-assisted intake summary is persisted.
/// </summary>
public sealed record AiIntakeSubmissionResult(
    bool Success,
    Guid? IntakeId,
    string? FailureReason,
    string? FailureCode);

/// <summary>
/// Sends a conversational turn to the configured AI provider (GPT-4o or compatible).
/// Retries once on transient failure before returning a failure result.
/// </summary>
public interface IAiIntakeService
{
    Task<AiIntakeTurnResult> SendTurnAsync(
        string systemPrompt,
        IReadOnlyList<AiIntakeTurn> history,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Strips HIPAA Safe Harbor PHI categories from free-text before it is transmitted to an external AI provider (AC-02).
/// </summary>
public interface IDeIdentificationService
{
    /// <summary>
    /// Returns a copy of <paramref name="input"/> with recognised PHI patterns replaced by
    /// placeholder tokens (e.g. [NAME], [DATE], [PHONE]).
    /// </summary>
    string Scrub(string input);
}

/// <summary>
/// Persists a completed intake summary as an <c>IntakeRecord</c> entity.
/// </summary>
public interface IAiIntakePersistenceService
{
    /// <summary>
    /// Creates or updates the <c>IntakeRecord</c> for the given appointment, marking it complete (AC-04).
    /// </summary>
    Task<AiIntakeSubmissionResult> PersistAsync(
        Guid actorUserId,
        string actorRole,
        Guid appointmentId,
        AiIntakeSummary summary,
        CancellationToken cancellationToken = default);
}
