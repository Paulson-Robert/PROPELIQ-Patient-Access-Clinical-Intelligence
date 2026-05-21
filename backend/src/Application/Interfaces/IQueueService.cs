namespace Application.Interfaces;

/// <summary>
/// Result DTO for a single same-day queue entry (US_024 AC-01).
/// </summary>
public sealed record QueueEntryDto(
    Guid AppointmentId,
    int Position,
    Guid PatientId,
    string PatientName,
    string AppointmentTime,
    string ProviderName,
    string Status,
    string BookingType,
    string? ArrivalTimestamp,
    string RiskLevel,
    int EstimatedWaitMinutes);

/// <summary>
/// Summary counts displayed in the queue header bar.
/// </summary>
public sealed record QueueSummaryDto(
    int TotalInQueue,
    int WalkInCount,
    int ArrivedCount,
    int AvgWaitMinutes);

/// <summary>
/// Full queue response returned by <see cref="IQueueService.GetTodayQueueAsync"/>.
/// </summary>
public sealed record QueueResponseDto(
    IReadOnlyList<QueueEntryDto> Entries,
    QueueSummaryDto Summary);

/// <summary>
/// Result returned after marking a patient as arrived (US_024 AC-02).
/// </summary>
public sealed record MarkArrivedResultDto(
    Guid AppointmentId,
    string ArrivalTimestamp);

/// <summary>
/// Result returned after a successful queue reorder (US_024 AC-03).
/// </summary>
public sealed record ReorderQueueResultDto(
    IReadOnlyList<QueueEntryDto> Entries,
    QueueSummaryDto Summary);

public interface IQueueService
{
    /// <summary>
    /// Returns all same-day appointments ordered by queue position then slot start time.
    /// </summary>
    Task<QueueResponseDto> GetTodayQueueAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the appointment status to Arrived and records the arrival timestamp.
    /// </summary>
    Task<MarkArrivedResultDto> MarkArrivedAsync(
        Guid appointmentId,
        Guid staffUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves an appointment to a new queue position and persists the staff reason.
    /// Uses optimistic concurrency — throws <see cref="QueueConcurrencyException"/>
    /// on a concurrent position conflict.
    /// </summary>
    Task<ReorderQueueResultDto> ReorderAsync(
        Guid appointmentId,
        int newPosition,
        string reason,
        Guid staffUserId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Raised when a concurrent reorder is detected via optimistic concurrency check.
/// </summary>
public sealed class QueueConcurrencyException : Exception
{
    public QueueConcurrencyException()
        : base("The queue was modified by another user. Please refresh and try again.") { }
}
