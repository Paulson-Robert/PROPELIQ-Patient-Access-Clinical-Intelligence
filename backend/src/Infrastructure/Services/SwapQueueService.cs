using Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Redis sorted set implementation of the swap queue (AC-01, AC-02, AC-03).
/// Each slot has its own sorted set keyed as <c>swapqueue:{preferredSlotId}</c>.
/// Member = JSON-serialised SwapQueueEntry; score = RequestedAt ticks for FCFS order.
/// On Redis failure operations are logged and no-oped to avoid blocking booking flow.
/// </summary>
public sealed class SwapQueueService : ISwapQueueService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SwapQueueService> _logger;

    public SwapQueueService(
        IConnectionMultiplexer redis,
        ILogger<SwapQueueService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private static string QueueKey(Guid preferredSlotId) =>
        $"swapqueue:{preferredSlotId:N}";

    private static string MemberKey(Guid queueId) =>
        $"entry:{queueId:N}";

    /// <inheritdoc/>
    public async Task EnqueueAsync(
        Guid queueId,
        Guid appointmentId,
        Guid patientUserId,
        Guid preferredSlotId,
        DateTime requestedAt,
        CancellationToken cancellationToken = default)
    {
        var key = QueueKey(preferredSlotId);
        var memberKey = MemberKey(queueId);
        var score = (double)requestedAt.Ticks;
        var payload = JsonSerializer.Serialize(new
        {
            QueueId = queueId,
            AppointmentId = appointmentId,
            PatientUserId = patientUserId,
            PreferredSlotId = preferredSlotId,
            RequestedAt = requestedAt
        });

        try
        {
            var db = _redis.GetDatabase();

            // Store payload in a hash keyed by member key for retrieval; use sorted set for ordering.
            // Use a pipeline to keep both operations atomic.
            var batch = db.CreateBatch();
            var addTask = batch.SortedSetAddAsync(key, memberKey, score);
            var hashTask = batch.HashSetAsync($"swapqueue:data:{preferredSlotId:N}", memberKey, payload);
            batch.Execute();

            await Task.WhenAll(addTask, hashTask).ConfigureAwait(false);

            _logger.LogInformation(
                "Enqueued swap preference. QueueId={QueueId}, PatientUserId={PatientUserId}, PreferredSlotId={PreferredSlotId}, Score={Score}.",
                queueId, patientUserId, preferredSlotId, score);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogError(ex,
                "Redis swap queue enqueue failed for QueueId={QueueId}, PreferredSlotId={PreferredSlotId}. Entry not queued.",
                queueId, preferredSlotId);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SwapQueueEntry>> GetWaitingEntriesAsync(
        Guid preferredSlotId,
        CancellationToken cancellationToken = default)
    {
        var key = QueueKey(preferredSlotId);
        var dataKey = $"swapqueue:data:{preferredSlotId:N}";

        try
        {
            var db = _redis.GetDatabase();

            // Retrieve all members sorted by score ascending (FCFS).
            var members = await db
                .SortedSetRangeByScoreAsync(key, order: Order.Ascending)
                .ConfigureAwait(false);

            if (members.Length == 0)
                return [];

            var hashEntries = await db
                .HashGetAsync(dataKey, members.Select(m => (RedisValue)m.ToString()).ToArray())
                .ConfigureAwait(false);

            var results = new List<SwapQueueEntry>(hashEntries.Length);
            foreach (var entry in hashEntries)
            {
                if (entry.IsNullOrEmpty)
                    continue;

                try
                {
                    using var doc = JsonDocument.Parse(entry.ToString());
                    var root = doc.RootElement;
                    results.Add(new SwapQueueEntry(
                        AppointmentId: root.GetProperty("AppointmentId").GetGuid(),
                        PatientUserId: root.GetProperty("PatientUserId").GetGuid(),
                        PreferredSlotId: root.GetProperty("PreferredSlotId").GetGuid(),
                        RequestedAt: root.GetProperty("RequestedAt").GetDateTime()));
                }
                catch (Exception parseEx)
                {
                    _logger.LogWarning(parseEx,
                        "Failed to parse swap queue entry for PreferredSlotId={PreferredSlotId}. Skipping.",
                        preferredSlotId);
                }
            }

            return results;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogError(ex,
                "Redis swap queue read failed for PreferredSlotId={PreferredSlotId}. Returning empty list.",
                preferredSlotId);
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(
        Guid queueId,
        Guid preferredSlotId,
        CancellationToken cancellationToken = default)
    {
        var key = QueueKey(preferredSlotId);
        var memberKey = MemberKey(queueId);
        var dataKey = $"swapqueue:data:{preferredSlotId:N}";

        try
        {
            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            var removeTask = batch.SortedSetRemoveAsync(key, memberKey);
            var hashDeleteTask = batch.HashDeleteAsync(dataKey, memberKey);
            batch.Execute();

            await Task.WhenAll(removeTask, hashDeleteTask).ConfigureAwait(false);

            _logger.LogInformation(
                "Removed swap queue entry. QueueId={QueueId}, PreferredSlotId={PreferredSlotId}.",
                queueId, preferredSlotId);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogError(ex,
                "Redis swap queue remove failed for QueueId={QueueId}, PreferredSlotId={PreferredSlotId}.",
                queueId, preferredSlotId);
        }
    }
}
