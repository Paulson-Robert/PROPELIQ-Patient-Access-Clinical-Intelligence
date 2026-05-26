# Bug Fix Task - datetime_kind_unspecified

## Bug Report Reference

- Bug ID: datetime_kind_unspecified
- Source: Runtime exception log — `System.ArgumentException` thrown by Npgsql when metrics endpoint is called with `YearToDate` range

## Bug Summary

### Issue Classification

- **Priority**: High
- **Severity**: Core analytics feature returns HTTP 500 for the YearToDate filter; no workaround for admin/staff users
- **Affected Version**: HEAD (commit `025c11b`)
- **Environment**: .NET 8, Npgsql 6.0+ strict UTC mode, PostgreSQL 16

### Steps to Reproduce

1. Log in as an admin or staff user with access to the metrics dashboard
2. Navigate to the platform metrics page
3. Select the **Year to Date** date range filter
4. Submit / observe the metrics request
5. **Expected**: Metrics data is returned for Jan 1 of the current year to today
6. **Actual**: `System.ArgumentException` is thrown; API returns HTTP 500

**Error Output**:

```text
System.ArgumentException: 'Cannot write DateTime with Kind=Unspecified to PostgreSQL type
'timestamp with time zone', only UTC is supported.
Note that it's not possible to mix DateTimes with different Kinds in an array,
range, or multirange. (Parameter 'value')'
```

### Root Cause Analysis

- **File**: `backend/src/Infrastructure/Handlers/GetPlatformMetricsQueryHandler.cs:71`
- **Component**: `GetPlatformMetricsQueryHandler` — `ResolveWindow()` private method
- **Function**: `ResolveWindow(MetricsDateRange range)`
- **Cause**: The `YearToDate` switch branch uses the 3-parameter `DateTime` constructor
  (`new DateTime(today.Year, 1, 1)`) which always produces `DateTimeKind.Unspecified`.
  This value is returned as `from` from `ResolveWindow()` and is subsequently passed as the
  `fromUtc` parameter into `BuildSummaryAsync()` and other LINQ query methods. Npgsql 6.0+
  strict mode forbids writing a `DateTime` with `Kind=Unspecified` to a `timestamp with time zone`
  column, throwing `ArgumentException` before the SQL query executes.
  All other date-range branches (`Last7Days`, `Last30Days`, `Last90Days`) derive their `from`
  value via `today.AddDays(-N)`, which preserves the `Kind=Utc` inherited from
  `DateTime.UtcNow.Date`. The `YearToDate` branch alone breaks this invariant.
  The `new DateTime(g.Key.Year, g.Key.Month, 1)` instances at lines 244, 278 and 295 of the
  same file are **not** sent to the database (used for in-memory string formatting only) and
  are therefore safe.

**Immediate trigger**: `new DateTime(today.Year, 1, 1)` at line 71 produces `Kind=Unspecified`
**Underlying cause**: 3-parameter `DateTime(int, int, int)` constructor contract — always `Unspecified`
**Why not caught earlier**: No test covers the `YearToDate` branch against a real Npgsql provider; in-memory/SQLite test providers do not enforce `DateTimeKind`

### Impact Assessment

- **Affected Features**: Platform metrics dashboard — Year to Date range; `GET /api/platform-metrics` endpoint
- **User Impact**: Admin and staff users selecting the Year to Date filter receive HTTP 500; all other date ranges are unaffected
- **Data Integrity Risk**: No — this is a read-only query
- **Security Implications**: None — no PHI exposure; exception is swallowed by the global error handler

## Fix Overview

Change the `new DateTime(today.Year, 1, 1)` expression in `ResolveWindow()` to use the 7-argument constructor that explicitly sets `DateTimeKind.Utc`, making it consistent with every other `DateTime` value in the project that is passed to EF Core LINQ predicates.

## Fix Dependencies

- None — self-contained change in one private method; no external dependencies

## Impacted Components

### Backend — Application / Infrastructure Layer

- `backend/src/Infrastructure/Handlers/GetPlatformMetricsQueryHandler.cs` — **MODIFY** `ResolveWindow()` private method, `YearToDate` switch arm (line 71)

### Backend — Tests Layer

- `backend/tests/Security.Tests/GetPlatformMetricsQueryHandlerTests.cs` — **CREATE** new test file covering `ResolveWindow` UTC-kind invariant

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | `backend/src/Infrastructure/Handlers/GetPlatformMetricsQueryHandler.cs` | Line 71: change `new DateTime(today.Year, 1, 1)` → `new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)` in `ResolveWindow()` |
| CREATE | `backend/tests/Security.Tests/GetPlatformMetricsQueryHandlerTests.cs` | New unit test file: `ResolveWindow_YearToDate_ReturnsUtcKind` and `ResolveWindow_AllRanges_ReturnUtcKind` theory test |

## Implementation Plan

1. **Modify `GetPlatformMetricsQueryHandler.cs`** — `ResolveWindow()` method, `YearToDate` arm:
   ```csharp
   // Before (line 71):
   MetricsDateRange.YearToDate => new DateTime(today.Year, 1, 1),

   // After:
   MetricsDateRange.YearToDate => new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
   ```
   No other lines in the file require changes; lines 244, 278, and 295 use `new DateTime(...)` for
   in-memory string formatting only and are not EF Core query parameters.

2. **Create `GetPlatformMetricsQueryHandlerTests.cs`** — test `ResolveWindow` via a private-method
   test strategy (reflection or `[InternalsVisibleTo]`) or by testing the public handler with a
   Npgsql-backed in-memory substitution. Preferred approach: expose `ResolveWindow` as `internal`
   and reference it via `InternalsVisibleTo`, then assert:
   - For `YearToDate`: `from.Kind == DateTimeKind.Utc` and `from == new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)`
   - Theory test: all `MetricsDateRange` enum values → `from.Kind == DateTimeKind.Utc` and `to.Kind == DateTimeKind.Utc`

## Regression Prevention Strategy

- [x] Unit test `ResolveWindow_YearToDate_ReturnsUtcKind`: asserts `from.Kind == DateTimeKind.Utc` for `MetricsDateRange.YearToDate`
- [x] Theory test `ResolveWindow_AllRanges_ReturnUtcKind`: parameterised over all `MetricsDateRange` enum values; asserts both `from` and `to` have `Kind=Utc` — guards against future enum additions
- [x] Existing metrics integration tests (if any) should pass unchanged

## Rollback Procedure

1. **Immediate rollback**: `git revert <fix-commit> --no-edit` — restores `new DateTime(today.Year, 1, 1)` (unspecified)
2. **Validation**: confirm that the `YearToDate` range returns 500 again (expected pre-fix behaviour)
3. **Workaround while rolled back**: temporarily add `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` to `Program.cs` as a stop-gap — this re-enables Npgsql legacy mode allowing `Unspecified` datetimes, but is NOT a permanent fix and must be removed once the root-cause fix is re-applied
4. **Data recovery**: Not required — no writes involved

## External References

- [Npgsql 6.0 timestamp breaking change](https://www.npgsql.org/doc/release-notes/6.0.html#timestamp-rationalization-and-improvements)
- [DateTime.Kind property — MSDN](https://learn.microsoft.com/en-us/dotnet/api/system.datetime.kind)
- [`new DateTime(int, int, int)` constructor always sets `Kind=Unspecified`](https://learn.microsoft.com/en-us/dotnet/api/system.datetime.-ctor#system-datetime-ctor(system-int32-system-int32-system-int32))

## Build Commands

```bash
# Build backend
cd backend
dotnet build Backend.sln

# Run tests
dotnet test tests/Security.Tests/Security.Tests.csproj --logger "console;verbosity=normal"
```

## Implementation Validation Strategy

- [x] `GET /api/platform-metrics?range=YearToDate` (or equivalent) returns HTTP 200 with data after fix
- [x] All other date range filters (`Last7Days`, `Last30Days`, `Last90Days`) continue to return HTTP 200
- [x] All existing tests pass (`dotnet test`)
- [x] New regression tests `ResolveWindow_YearToDate_ReturnsUtcKind` and `ResolveWindow_AllRanges_ReturnUtcKind` pass
- [x] No `ArgumentException` containing `Kind=Unspecified` in application logs

## Implementation Checklist

- [x] Modify `GetPlatformMetricsQueryHandler.cs` line 71: add `0, 0, 0, DateTimeKind.Utc` to `new DateTime` constructor call
- [x] Create `backend/tests/Security.Tests/GetPlatformMetricsQueryHandlerTests.cs` with `ResolveWindow_YearToDate_ReturnsUtcKind` test
- [x] Add theory test `ResolveWindow_AllRanges_ReturnUtcKind` covering all `MetricsDateRange` enum values
- [x] Run `dotnet build` — confirm zero errors
- [x] Run `dotnet test` — confirm all tests pass including new ones
- [x] Verify no remaining `new DateTime(int, int, int)` (3-arg) calls in files whose `DateTime` values flow into EF Core LINQ predicates
