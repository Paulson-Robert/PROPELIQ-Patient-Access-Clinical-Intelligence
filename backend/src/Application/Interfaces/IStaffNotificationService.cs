namespace Application.Interfaces;

/// <summary>
/// Single staff notification returned from creation or mark-read operations.
/// </summary>
public sealed record StaffNotificationDto(
    Guid StaffNotificationId,
    Guid StaffUserId,
    string Variant,
    string Title,
    string? Message,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);

/// <summary>
/// Paginated page of staff notifications.
/// </summary>
public sealed record NotificationPageDto(
    IReadOnlyList<StaffNotificationDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// Payload for batch staff notification creation (US_028 AC-01, edge-case batch insert).
/// </summary>
public sealed record CreateStaffNotificationRequest(
    IReadOnlyList<Guid> StaffUserIds,
    string Variant,
    string Title,
    string? Message);

public interface IStaffNotificationService
{
    /// <summary>
    /// Persists one notification per entry in <see cref="CreateStaffNotificationRequest.StaffUserIds"/>.
    /// Handles batch creation in a single SaveChanges call.
    /// </summary>
    Task<IReadOnlyList<StaffNotificationDto>> CreateAsync(
        CreateStaffNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns notifications for a staff user ordered by newest first with offset pagination.
    /// </summary>
    Task<NotificationPageDto> GetHistoryAsync(
        Guid staffUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a single notification as read. Idempotent — no-ops if already read.
    /// Throws <see cref="KeyNotFoundException"/> when the notification does not belong to the user.
    /// </summary>
    Task<StaffNotificationDto> MarkReadAsync(
        Guid staffNotificationId,
        Guid staffUserId,
        CancellationToken cancellationToken = default);
}
