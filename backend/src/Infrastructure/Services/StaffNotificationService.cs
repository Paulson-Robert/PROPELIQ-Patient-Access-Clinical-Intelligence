using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Persists and retrieves staff in-app notifications using EF Core (US_028).
/// </summary>
public sealed class StaffNotificationService : IStaffNotificationService
{
    private readonly ApplicationDbContext _db;

    public StaffNotificationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<StaffNotificationDto>> CreateAsync(
        CreateStaffNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<StaffNotificationVariant>(
                request.Variant, ignoreCase: true, out var variant))
        {
            variant = StaffNotificationVariant.Info;
        }

        var now = DateTime.UtcNow;

        var notifications = request.StaffUserIds
            .Select(userId => new StaffNotification
            {
                StaffNotificationId = Guid.NewGuid(),
                StaffUserId = userId,
                Variant = variant,
                Title = request.Title,
                Message = request.Message,
                IsRead = false,
                CreatedAt = now,
            })
            .ToList();

        _db.StaffNotifications.AddRange(notifications);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return notifications.ConvertAll(ToDto).AsReadOnly();
    }

    public async Task<NotificationPageDto> GetHistoryAsync(
        Guid staffUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var safePage = Math.Max(1, page);

        var baseQuery = _db.StaffNotifications
            .Where(n => n.StaffUserId == staffUserId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await baseQuery
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = await baseQuery
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new NotificationPageDto(
            items.ConvertAll(ToDto).AsReadOnly(),
            totalCount,
            safePage,
            safePageSize);
    }

    public async Task<StaffNotificationDto> MarkReadAsync(
        Guid staffNotificationId,
        Guid staffUserId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _db.StaffNotifications
            .FirstOrDefaultAsync(
                n => n.StaffNotificationId == staffNotificationId
                  && n.StaffUserId == staffUserId,
                cancellationToken)
            .ConfigureAwait(false);

        if (notification is null)
        {
            throw new KeyNotFoundException(
                $"Staff notification {staffNotificationId} not found for user {staffUserId}.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return ToDto(notification);
    }

    private static StaffNotificationDto ToDto(StaffNotification n) =>
        new(
            n.StaffNotificationId,
            n.StaffUserId,
            n.Variant.ToString().ToLowerInvariant(),
            n.Title,
            n.Message,
            n.IsRead,
            n.ReadAt,
            n.CreatedAt);
}
