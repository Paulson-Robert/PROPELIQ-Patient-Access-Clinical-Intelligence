<!-- Schema: ./findings-registry-schema.md -->

# Findings Registry

## Index

| File                                                            | Finding IDs      |
| --------------------------------------------------------------- | ---------------- |
| src/Infrastructure/Notifications/NotificationDeliveryService.cs | F001, F002, F003 |
| src/Infrastructure/Jobs/ScheduleRemindersJob.cs                 | F004             |

## Entries

```yaml
- id: F001
  file: src/Infrastructure/Notifications/NotificationDeliveryService.cs
  cat: implementation-decision
  type: decision
  severity: HIGH
  issue: Exponential backoff delays set to 30s base (30s, 60s)
  cause: Task specifies max 3 attempts but no delay values; 30 s base chosen to balance timeliness vs server load
  date: 2026-05-21
  workflow: implement-tasks

- id: F002
  file: src/Infrastructure/Notifications/NotificationDeliveryService.cs
  cat: implementation-decision
  type: decision
  severity: HIGH
  issue: Staff alert email sourced from NotificationSettings.StaffAlertEmail
  cause: Task requires staff alert on exhausted retries but specifies no recipient; config-driven approach chosen
  date: 2026-05-21
  workflow: implement-tasks

- id: F003
  file: src/Infrastructure/Notifications/NotificationDeliveryService.cs
  cat: implementation-decision
  type: decision
  severity: HIGH
  issue: Reminder email body uses appointment time in UTC formatted as local-friendly string
  cause: No locale or timezone requirement specified; UTC formatting used pending tz-aware implementation
  date: 2026-05-21
  workflow: implement-tasks

- id: F004
  file: src/Infrastructure/Jobs/ScheduleRemindersJob.cs
  cat: implementation-decision
  type: decision
  severity: HIGH
  issue: ScheduleRemindersJob recurrence set to every 15 minutes
  cause: Task specifies no scan frequency; 15-min cadence balances latency vs DB load for a 26-hour scan window
  date: 2026-05-21
  workflow: implement-tasks
```
