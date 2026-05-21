using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="ISlotSearchService"/>.
/// Queries available (non-locked or lock-expired) slots with optional
/// provider-name and specialty filters sorted by start time (AC-01, AC-02).
/// </summary>
public sealed class SlotSearchService : ISlotSearchService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SlotSearchService> _logger;

    public SlotSearchService(ApplicationDbContext db, ILogger<SlotSearchService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SlotDto>> SearchAsync(
        string? provider,
        string? specialty,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var query = _db.AvailabilitySlots
            .AsNoTracking()
            .Where(s =>
                s.IsAvailable &&
                // Treat lock as expired when LockExpiry has passed (AC-06)
                (!s.IsLocked || s.LockExpiry == null || s.LockExpiry < now));

        if (!string.IsNullOrWhiteSpace(provider))
        {
            var term = provider.Trim().ToLower();
            query = query.Where(s => s.ProviderName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(specialty))
            query = query.Where(s => s.Specialty == specialty);

        if (from.HasValue)
        {
            var fromDate = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(s => s.StartTime >= fromDate);
        }

        if (to.HasValue)
        {
            var toDate = to.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(s => s.StartTime <= toDate);
        }

        var slots = await query
            .OrderBy(s => s.StartTime)
            .Select(s => new SlotDto(
                s.SlotId,
                s.ProviderId,
                s.ProviderName,
                s.Specialty,
                s.StartTime,
                s.EndTime,
                (int)(s.EndTime - s.StartTime).TotalMinutes,
                s.IsAvailable,
                s.IsLocked && s.LockExpiry > now))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug(
            "Slot search returned {Count} results. Provider: {Provider}, Specialty: {Specialty}",
            slots.Count, provider, specialty);

        return slots;
    }
}
