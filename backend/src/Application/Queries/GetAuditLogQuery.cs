using MediatR;

namespace Application.Queries;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Single audit log entry returned by <see cref="GetAuditLogQuery"/>.</summary>
public sealed record AuditLogEntryDto(
    long AuditLogId,
    DateTime Timestamp,
    /// <summary>Display name of the acting user; null for system-generated entries.</summary>
    string? ActorName,
    Guid? ActorUserId,
    string ActorRole,
    string ActionType,
    string ResourceType,
    string ResourceId,
    string? Details);

/// <summary>Paginated result for the audit log query.</summary>
public sealed record AuditLogPageResult(
    IReadOnlyList<AuditLogEntryDto> Entries,
    int Total,
    int Page,
    int PageSize);

// ---------------------------------------------------------------------------
// Query
// ---------------------------------------------------------------------------

/// <summary>
/// Returns a paginated, server-side-filtered audit log, newest-first (US_045 AC-01).
/// All filter parameters are optional; omitting a parameter returns all records for that dimension.
/// </summary>
/// <param name="Actor">Partial case-insensitive match on the actor's display name (AC-01).</param>
/// <param name="ActionType">Exact-match filter on action type (e.g., "Login", "Update") (AC-01).</param>
/// <param name="ResourceType">Exact-match filter on resource type (e.g., "User", "Appointment").</param>
/// <param name="FromDate">Inclusive UTC start of the date range (AC-01).</param>
/// <param name="ToDate">Inclusive UTC end of the date range; clamped to end-of-day (AC-01).</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Number of entries per page; default 25 (AC-03).</param>
public sealed record GetAuditLogQuery(
    string? Actor,
    string? ActionType,
    string? ResourceType,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 25) : IRequest<AuditLogPageResult>;
