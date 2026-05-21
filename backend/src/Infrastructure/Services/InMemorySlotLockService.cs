using Application.Interfaces;
using System.Collections.Concurrent;

namespace Infrastructure.Services;

/// <summary>
/// In-process slot lock fallback used when Redis is not configured (development / degraded mode).
/// Uses a ConcurrentDictionary with expiry timestamps. Not distributed — single-node only.
/// </summary>
internal sealed class InMemorySlotLockService : ISlotLockService
{
    private sealed record LockEntry(string Token, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<Guid, LockEntry> _locks = new();
    private const int LockDurationSeconds = 30;

    public Task<SlotLockResult?> AcquireAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Remove expired entry if present
        if (_locks.TryGetValue(slotId, out var existing) && existing.ExpiresAt <= now)
            _locks.TryRemove(slotId, out _);

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = now.AddSeconds(LockDurationSeconds);
        var entry = new LockEntry(token, expiresAt);

        if (!_locks.TryAdd(slotId, entry))
            return Task.FromResult<SlotLockResult?>(null); // already locked

        return Task.FromResult<SlotLockResult?>(
            new SlotLockResult(slotId, token, expiresAt, LockDurationSeconds));
    }

    public Task<bool> ValidateAsync(Guid slotId, string lockToken, CancellationToken cancellationToken = default)
    {
        if (_locks.TryGetValue(slotId, out var entry))
        {
            var valid = entry.Token == lockToken && entry.ExpiresAt > DateTimeOffset.UtcNow;
            return Task.FromResult(valid);
        }

        return Task.FromResult(false);
    }

    public Task<bool> ReleaseAsync(Guid slotId, string lockToken, CancellationToken cancellationToken = default)
    {
        if (_locks.TryGetValue(slotId, out var entry) && entry.Token == lockToken)
        {
            _locks.TryRemove(slotId, out _);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
