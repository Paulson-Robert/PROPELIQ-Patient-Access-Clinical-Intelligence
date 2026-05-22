using Application.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// No-op swap queue when Redis is unavailable (degraded mode).
/// Swap functionality requires Redis; operations here are silent no-ops.
/// </summary>
internal sealed class NoOpSwapQueueService : ISwapQueueService
{
    public Task EnqueueAsync(
        Guid queueId,
        Guid appointmentId,
        Guid patientUserId,
        Guid preferredSlotId,
        DateTime requestedAt,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<SwapQueueEntry>> GetWaitingEntriesAsync(
        Guid preferredSlotId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SwapQueueEntry>>(Array.Empty<SwapQueueEntry>());

    public Task RemoveAsync(
        Guid queueId,
        Guid preferredSlotId,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
