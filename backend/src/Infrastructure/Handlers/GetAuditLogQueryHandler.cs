using Application.Queries;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

/// <summary>
/// Executes a paginated, filtered audit log query using indexed columns (US_045 AC-01, AC-02).
///
/// Index usage (AC-02 — performant queries on large datasets):
///   • <c>ActionType</c>   — ix_audit_logs_action_type (exact-match; evaluated at SQL level)
///   • <c>Timestamp</c>    — ix_audit_logs_timestamp (range predicate; evaluated at SQL level)
///   • <c>ResourceType</c> — composite ix_audit_logs_resource_type_resource_id
///   • <c>ActorUserId</c>  — ix_audit_logs_actor_user_id (used by the LEFT JOIN when Actor filter active)
///
/// The actor-name filter joins to the Users table via the <c>ActorUser</c> navigation property.
/// <c>Include(ActorUser)</c> is applied unconditionally because the DTO always projects ActorName.
/// </summary>
public sealed class GetAuditLogQueryHandler
    : IRequestHandler<GetAuditLogQuery, AuditLogPageResult>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAuditLogQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditLogPageResult> Handle(
        GetAuditLogQuery request,
        CancellationToken cancellationToken)
    {
        // AC-02: AsNoTracking avoids change-tracker overhead on read-only query.
        // Include(ActorUser) is needed for both the actor-name filter and the DTO projection.
        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .Include(l => l.ActorUser)
            .AsQueryable();

        // AC-01: action type filter — uses ix_audit_logs_action_type
        if (!string.IsNullOrWhiteSpace(request.ActionType))
            query = query.Where(l => l.ActionType == request.ActionType);

        // AC-01: resource type filter — uses composite ix_audit_logs_resource_type_resource_id
        if (!string.IsNullOrWhiteSpace(request.ResourceType))
            query = query.Where(l => l.ResourceType == request.ResourceType);

        // AC-01: date range filter — uses ix_audit_logs_timestamp
        if (request.FromDate.HasValue)
            query = query.Where(l => l.Timestamp >= request.FromDate.Value);

        if (request.ToDate.HasValue)
        {
            // Include the full ToDate day by clamping to end-of-day
            var endOfDay = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(l => l.Timestamp <= endOfDay);
        }

        // AC-01: actor name filter — LEFT JOIN on Users via ix_audit_logs_actor_user_id
        if (!string.IsNullOrWhiteSpace(request.Actor))
        {
            var term = request.Actor.Trim().ToLowerInvariant();
            query = query.Where(l =>
                l.ActorUser != null &&
                l.ActorUser.FullName != null &&
                l.ActorUser.FullName.ToLower().Contains(term));
        }

        var total = await query
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var rows = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new AuditLogEntryDto(
                l.AuditLogId,
                l.Timestamp,
                l.ActorUser != null ? l.ActorUser.FullName : null,
                l.ActorUserId,
                l.ActorRole,
                l.ActionType,
                l.ResourceType,
                l.ResourceId,
                l.Details))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AuditLogPageResult(rows, total, request.Page, request.PageSize);
    }
}
