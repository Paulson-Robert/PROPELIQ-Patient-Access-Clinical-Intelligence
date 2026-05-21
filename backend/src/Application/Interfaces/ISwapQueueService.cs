namespace Application.Interfaces;

/// <summary>
/// Entry stored in the Redis swap queue for FCFS ordering (AC-01, AC-02, AC-03).
/// Score is the UTC registration timestamp in ticks for natural FCFS ordering.
/// </summary>
public sealed record SwapQueueEntry(
    Guid AppointmentId,
    Guid PatientUserId,
    Guid PreferredSlotId,
    DateTime RequestedAt);

public interface ISwapQueueService
{
    /// <summary>
    /// Adds a patient's preferred slot preference to the Redis sorted set queue.
    /// Score = RequestedAt ticks; members deduplicated by QueueId (AC-01).
    /// </summary>
    Task EnqueueAsync(
        Guid queueId,
        Guid appointmentId,
        Guid patientUserId,
        Guid preferredSlotId,
        DateTime requestedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all waiting queue entries for a specific preferred slot, ordered by
    /// registration timestamp ascending (FCFS) (AC-02, AC-03).
    /// </summary>
    Task<IReadOnlyList<SwapQueueEntry>> GetWaitingEntriesAsync(
        Guid preferredSlotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific queue entry (patient cancels swap request) (Edge Case).
    /// </summary>
    Task RemoveAsync(
        Guid queueId,
        Guid preferredSlotId,
        CancellationToken cancellationToken = default);
}
