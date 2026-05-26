# Bug Fix Task - BUG_notification_retrycount_null

## Bug Report Reference

- **Bug ID:** BUG_notification_retrycount_null
- **Source:** Runtime error reported during admin dashboard page load

## Bug Summary

### Issue Classification

- **Priority:** Critical
- **Severity:** System-wide notification INSERT failure — every code path that persists a `Notification` entity fails with a PostgreSQL NOT NULL constraint violation; patients cannot receive appointment reminders, and appointment-event notification records are not being created.
- **Affected Version:** Current HEAD (post-migration `20260522082357_AddAllPendingChanges`)
- **Environment:** PostgreSQL backend; .NET 9 / EF Core / Npgsql; affects all environments where the real DB is used (staging, production). Fails silently in tests using in-memory or SQLite providers.

### Steps to Reproduce

1. **Setup:** Run the backend against a PostgreSQL instance with the latest migrations applied (`20260522082357_AddAllPendingChanges`).
2. **Action:** Trigger any workflow that books/modifies an appointment, runs the reminder pipeline, or fires the swap engine (e.g., book an appointment via `POST /api/appointments`, wait for the `ScheduleRemindersJob` to run, or perform a slot swap).
3. **Expected:** A row is inserted into the `Notifications` table with `RetryCount = 0`.
4. **Actual:** `DbUpdateException` is thrown; PostgreSQL reports `23502: null value in column "RetryCount" of relation "Notifications" violates not-null constraint`.

**Error Output:**

```text
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.

Inner Exception:
Npgsql.PostgresException: 23502: null value in column "RetryCount" of relation "Notifications" violates not-null constraint

Stack trace (condensed):
  Npgsql.Internal.NpgsqlConnector.ReadMessageLong(...)
  Npgsql.NpgsqlDataReader.NextResult(...)
  Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(...)
  Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(...)
  Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(...)
```

### Root Cause Analysis

- **File:** `backend/src/Infrastructure/Data/Configurations/NotificationConfiguration.cs:24`
- **Component:** EF Core entity type configuration for `Notification`
- **Function:** `NotificationConfiguration.Configure`
- **Cause:**

  `builder.Property(n => n.RetryCount).IsRequired().HasDefaultValue(0)` calls `HasDefaultValue(0)`, which sets `ValueGeneratedOnAdd` on the `RetryCount` property in the EF Core model (confirmed in `ApplicationDbContextModelSnapshot.cs:718-722` and all Designer files). EF Core's `ValueGeneratedOnAdd` flag causes the ORM to **omit the column from INSERT statements when the in-memory value equals the CLR default** (0 for `int`) — relying on a database-side `DEFAULT` to supply the value.

  However, the original migration `20260518000000_AddDomainEntities.cs:311` created the column as:

  ```csharp
  RetryCount = table.Column<int>(type: "integer", nullable: false),
  ```

  No `defaultValue: 0` was emitted, so the PostgreSQL column is `NOT NULL` with **no DEFAULT clause**. When EF Core omits `RetryCount` from the INSERT, PostgreSQL cannot supply a value and raises the NOT NULL constraint violation.

  All three Notification creation sites (`AppointmentManagementService.cs:308`, `SwapEngineService.cs:289`, `ReminderPipelineService.cs:228`) explicitly set `RetryCount = 0`. Since 0 equals the CLR default, EF Core always omits the column, meaning **every Notification INSERT fails**.

- **Why wasn't this caught earlier:** Tests use SQLite in-memory providers that either (a) enforce EF Core's model-level DEFAULT rather than the DB column definition, or (b) do not replicate `ValueGeneratedOnAdd` omission behaviour. The mismatch between the EF Core model annotation and actual schema is invisible until a real PostgreSQL connection is used.

### Impact Assessment

- **Affected Features:** Appointment confirmation notifications, appointment reminder pipeline (24 h / 2 h reminders), slot-swap notifications
- **User Impact:** 100% of `Notification` INSERT operations fail. Patients receive no appointment reminders. Notification records are never persisted, breaking the notification delivery service's retry mechanism.
- **Data Integrity Risk:** Yes — notification records are silently dropped on exception; downstream `NotificationDeliveryService` has no rows to process
- **Security Implications:** None

## Fix Overview

Remove `.HasDefaultValue(0)` from the `RetryCount` property configuration in `NotificationConfiguration`. This removes `ValueGeneratedOnAdd` from the EF Core model, ensuring EF Core always includes `RetryCount` in INSERT statements regardless of value. A companion migration updates the model snapshot so future `dotnet ef migrations` commands start from the correct baseline. No database schema change is required because the column was never given a DEFAULT clause in the first place.

## Fix Dependencies

- .NET 9 SDK with `dotnet ef` CLI tools installed
- Access to the PostgreSQL connection string to verify the migration applies cleanly (or at minimum to run the app and confirm the error is resolved)

## Impacted Components

### Backend — Infrastructure / Data / Configurations

- `backend/src/Infrastructure/Data/Configurations/NotificationConfiguration.cs` — **MODIFY**: remove `.HasDefaultValue(0)` from `RetryCount`

### Backend — Infrastructure / Migrations

- New migration file `backend/src/Infrastructure/Migrations/<timestamp>_FixNotificationRetryCountDefault.cs` — **CREATE**: snapshot update migration; `Up()` contains an `AlterColumn` call that removes the (phantom) default from the EF Core model baseline
- `backend/src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` — **MODIFY** (auto-generated by EF): `RetryCount` entry loses `ValueGeneratedOnAdd()` and `HasDefaultValue(0)` annotations

### Backend — Tests

- New test class or additional test method in `backend/tests/Security.Tests/` — **CREATE**: regression test asserting that `RetryCount` is included in the INSERT SQL when a `Notification` entity with `RetryCount = 0` is saved

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | `backend/src/Infrastructure/Data/Configurations/NotificationConfiguration.cs` | Remove `.HasDefaultValue(0)` from the `RetryCount` property fluent configuration |
| CREATE | `backend/src/Infrastructure/Migrations/<timestamp>_FixNotificationRetryCountDefault.cs` | New EF Core migration; `Up()` alters `RetryCount` column to remove the model-assumed default; `Down()` restores it |
| MODIFY | `backend/src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` | Auto-updated by EF tooling; `RetryCount` loses `ValueGeneratedOnAdd()` and `HasDefaultValue(0)` |
| CREATE | `backend/tests/Security.Tests/NotificationRetryCountInsertTests.cs` | Regression test confirming `RetryCount = 0` is persisted without constraint violation |

> Only concrete, verifiable file operations are listed above.

## Implementation Plan

1. **Edit `NotificationConfiguration.cs`** — In `Configure()`, change:

   ```csharp
   // Before
   builder.Property(n => n.RetryCount)
       .IsRequired()
       .HasDefaultValue(0);

   // After
   builder.Property(n => n.RetryCount)
       .IsRequired();
   ```

2. **Generate migration** — From the `backend/` directory run:

   ```bash
   dotnet ef migrations add FixNotificationRetryCountDefault \
     --project src/Infrastructure \
     --startup-project src/API
   ```

   Review the generated `Up()` body. EF Core (Npgsql provider) may emit an `AlterColumn` statement that drops the database default. Since the DB column never had a DEFAULT clause, this SQL is a no-op on PostgreSQL (`ALTER TABLE "Notifications" ALTER COLUMN "RetryCount" DROP DEFAULT;` succeeds even when no default exists). Verify and keep as-is.

3. **Apply migration** — Run:

   ```bash
   dotnet ef database update \
     --project src/Infrastructure \
     --startup-project src/API
   ```

4. **Write regression test** — Add `NotificationRetryCountInsertTests.cs` (see Regression Prevention Strategy below).

5. **Run all tests** — `dotnet test backend/Backend.sln` — ensure no regressions.

6. **Smoke test** — Start the API, book an appointment, and confirm that a row is inserted into `Notifications` with `RetryCount = 0`.

## Regression Prevention Strategy

- [ ] Unit test: Verify that a `Notification` entity with `RetryCount = 0` can be saved via `ApplicationDbContext` without throwing `DbUpdateException` (use SQLite in-memory but with `EnsureCreated()` so the schema reflects the updated model)
- [ ] Configuration guard test: Assert that the EF Core model for `Notification.RetryCount` does NOT have `ValueGenerated.OnAdd` set (`metadata.GetValueGeneratedConfigurationSource()` or model metadata inspection)
- [ ] Integration test: Verify the `AppointmentManagementService` notification creation path succeeds end-to-end against a real (or containerised) PostgreSQL test instance
- [ ] Edge case test: Verify that `NotificationDeliveryService` incrementing `RetryCount` (lines 86, 114) and then calling `SaveChanges` still works correctly after the fix

## Rollback Procedure

1. **Detection:** Monitor `DbUpdateException` log entries containing `23502` after deployment. If they continue, the migration was not applied or the config change was not deployed.
2. **Revert steps:**
   a. Revert `NotificationConfiguration.cs` to restore `.HasDefaultValue(0)`.
   b. Drop the new migration from the database: `dotnet ef database update <previous-migration-name>`.
   c. Delete the generated migration files.
3. **Data recovery:** No data was lost at the row level (the INSERT was rejected, no partial rows exist). The notification queue simply has a gap for the duration of the outage — no manual data recovery needed, but the operations team should be aware that any appointment notifications during the failure window were not queued.

## External References

- [EF Core `HasDefaultValue` and `ValueGeneratedOnAdd` documentation](https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties) — covers the INSERT-omission behaviour
- PostgreSQL error code `23502`: `not_null_violation`
- Related codebase findings: `F001`–`F003` (findings-registry) describe delivery behaviour of the `NotificationDeliveryService`; none previously flagged the configuration mismatch

## Build Commands

```bash
# From backend/ directory

# 1. Restore
dotnet restore Backend.sln

# 2. Build
dotnet build Backend.sln

# 3. Generate migration (after editing NotificationConfiguration.cs)
dotnet ef migrations add FixNotificationRetryCountDefault \
  --project src/Infrastructure \
  --startup-project src/API

# 4. Apply migration
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/API

# 5. Run tests
dotnet test Backend.sln
```

## Implementation Validation Strategy

- [ ] `DbUpdateException` with `23502` no longer thrown when any notification creation path is exercised
- [ ] All existing tests in `Backend.sln` pass without modification
- [ ] New regression test `NotificationRetryCountInsertTests` passes
- [ ] `ApplicationDbContextModelSnapshot.cs` no longer contains `ValueGeneratedOnAdd()` or `HasDefaultValue(0)` for `RetryCount`
- [ ] A `Notification` row is observable in the `Notifications` table with `RetryCount = 0` after booking an appointment

## Implementation Checklist

- [x] Remove `.HasDefaultValue(0)` from `NotificationConfiguration.cs` (line 24)
- [x] Run `dotnet ef migrations add FixNotificationRetryCountDefault`
- [x] Review generated migration `Up()` body — ensure it is safe against a DB that never had `DEFAULT 0` on `RetryCount`
- [x] Run `dotnet ef database update` on a local/staging PostgreSQL instance
- [x] Create `NotificationRetryCountInsertTests.cs` with minimum one INSERT test case
- [x] Run `dotnet test Backend.sln` — all tests green
- [x] Smoke test: book appointment → confirm `Notifications` row inserted successfully
- [ ] Deploy to staging and re-verify no `DbUpdateException 23502` in logs
