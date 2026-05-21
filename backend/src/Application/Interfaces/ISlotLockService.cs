namespace Application.Interfaces;

/// <summary>
/// Result returned by a successful slot lock acquisition (AC-03).
/// </summary>
public sealed record SlotLockResult(
    Guid SlotId,
    string LockToken,
    DateTimeOffset ExpiresAt,
    int LockDurationSeconds);

public interface ISlotLockService
{
    /// <summary>
    /// Acquires a 30-second Redis lock on the slot.
    /// Returns null when the slot is already locked or unavailable.
    /// </summary>
    Task<SlotLockResult?> AcquireAsync(
        Guid slotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that the given lock token still owns the slot lock.
    /// </summary>
    Task<bool> ValidateAsync(
        Guid slotId,
        string lockToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases the slot lock when the caller owns it.
    /// </summary>
    Task<bool> ReleaseAsync(
        Guid slotId,
        string lockToken,
        CancellationToken cancellationToken = default);
}
