using Application.Queries;
using Infrastructure.Handlers;
using Xunit;

namespace Security.Tests;

// ---------------------------------------------------------------------------
// GetPlatformMetricsQueryHandlerTests
// Regression guard for: bug_datetime_kind_unspecified
// AC: ResolveWindow() must always return DateTime values with Kind=Utc so that
//     Npgsql 6.0+ strict mode does not throw ArgumentException when the values
//     are used as timestamptz parameters in EF Core LINQ predicates.
// ---------------------------------------------------------------------------

public sealed class GetPlatformMetricsQueryHandlerTests
{
    // -----------------------------------------------------------------------
    // ResolveWindow_YearToDate_ReturnsUtcKind
    // -----------------------------------------------------------------------

    [Fact]
    public void ResolveWindow_YearToDate_ReturnsUtcKind()
    {
        var (from, to) = GetPlatformMetricsQueryHandler.ResolveWindow(MetricsDateRange.YearToDate);

        Assert.Equal(DateTimeKind.Utc, from.Kind);
        Assert.Equal(DateTimeKind.Utc, to.Kind);

        var expectedYearStart = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(expectedYearStart, from);
    }

    // -----------------------------------------------------------------------
    // ResolveWindow_AllRanges_ReturnUtcKind
    // Parameterised theory: every current and future MetricsDateRange value must
    // produce DateTimeKind.Utc for both bounds, guarding against regressions
    // when new enum members are added.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(MetricsDateRange.Last7Days)]
    [InlineData(MetricsDateRange.Last30Days)]
    [InlineData(MetricsDateRange.Last90Days)]
    [InlineData(MetricsDateRange.YearToDate)]
    public void ResolveWindow_AllRanges_ReturnUtcKind(MetricsDateRange range)
    {
        var (from, to) = GetPlatformMetricsQueryHandler.ResolveWindow(range);

        Assert.Equal(DateTimeKind.Utc, from.Kind);
        Assert.Equal(DateTimeKind.Utc, to.Kind);
    }
}
