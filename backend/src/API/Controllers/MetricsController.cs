using Application.Queries;
using API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Exposes platform-wide metrics aggregated over a configurable date window (US_046).
///
/// Routes:
///   GET /api/admin/metrics?range=Last30Days&amp;groupBy=Day
/// </summary>
[ApiController]
[Route("api/admin/metrics")]
[Authorize(Policy = RoleRequirements.AdminPolicy)]
public sealed class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetricsController(IMediator mediator) => _mediator = mediator;

    // -------------------------------------------------------------------------
    // GET /api/admin/metrics?range=Last30Days&groupBy=Day
    // AC-01: KPI summary for the selected date range
    // AC-02: Trend data grouped by day, week, or month
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns platform KPIs and trend charts for the specified date window.
    /// </summary>
    /// <param name="range">
    /// Date window: Last7Days | Last30Days | Last90Days | YearToDate.
    /// Defaults to Last30Days.
    /// </param>
    /// <param name="groupBy">
    /// Trend bucket granularity: Day | Week | Month.
    /// Defaults to Day.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<PlatformMetricsResult>> GetMetrics(
        [FromQuery] MetricsDateRange range = MetricsDateRange.Last30Days,
        [FromQuery] MetricsGroupBy groupBy = MetricsGroupBy.Day,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator
            .Send(new GetPlatformMetricsQuery(range, groupBy), cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }
}
