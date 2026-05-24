using Application.Queries;
using API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace API.Controllers;

/// <summary>
/// Exposes audit log entries to admin users (US_045).
///
/// Routes:
///   GET  /api/admin/audit-log  — paginated, filtered list
///   GET  /api/admin/audit-log/export  — CSV download
/// </summary>
[ApiController]
[Route("api/admin/audit-log")]
[Authorize(Policy = RoleRequirements.AdminPolicy)]
public sealed class AuditLogController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogController(IMediator mediator) => _mediator = mediator;

    // -------------------------------------------------------------------------
    // GET /api/admin/audit-log?actor=&action=&resource=&fromDate=&toDate=&page=1&pageSize=25
    // AC-01: Paginated, filtered audit log — newest first
    // AC-02: Index-backed actor / action / resource / date filters
    // AC-03: Server-side pagination
    // -------------------------------------------------------------------------

    [HttpGet]
    public async Task<ActionResult<AuditLogPageResult>> ListEntries(
        [FromQuery] string? actor,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await _mediator
            .Send(
                new GetAuditLogQuery(actor, action, resource, fromDate, toDate, page, pageSize),
                cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /api/admin/audit-log/export?actor=&action=&resource=&fromDate=&toDate=
    // AC-04: Export all matching entries as a CSV download
    // -------------------------------------------------------------------------

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] string? actor,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        // Fetch all matching entries in a single page (capped at 10 000 rows)
        var result = await _mediator
            .Send(
                new GetAuditLogQuery(actor, action, resource, fromDate, toDate, Page: 1, PageSize: 10_000),
                cancellationToken)
            .ConfigureAwait(false);

        var csv = BuildCsv(result.Entries);
        var bytes = Encoding.UTF8.GetBytes(csv);
        var fileName = $"audit-log-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string BuildCsv(IReadOnlyList<AuditLogEntryDto> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Timestamp,Actor,Role,Action,Resource,ResourceId,Details");

        foreach (var e in entries)
        {
            var details = string.IsNullOrWhiteSpace(e.Details)
                ? string.Empty
                : e.Details.Replace("\"", "\"\"");

            sb.AppendLine(
                $"{e.AuditLogId}," +
                $"{e.Timestamp:O}," +
                $"\"{EscapeCsv(e.ActorName ?? "System")}\"," +
                $"\"{EscapeCsv(e.ActorRole)}\"," +
                $"\"{EscapeCsv(e.ActionType)}\"," +
                $"\"{EscapeCsv(e.ResourceType)}\"," +
                $"\"{EscapeCsv(e.ResourceId)}\"," +
                $"\"{details}\"");
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value) =>
        (value ?? string.Empty).Replace("\"", "\"\"");
}
