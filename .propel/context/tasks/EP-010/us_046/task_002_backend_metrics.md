# Task - TASK_002

## Requirement Reference

- **User Story:** US_046
- **Story Location:** .propel/context/tasks/EP-010/us_046/us_046.md
- **Acceptance Criteria:**
  - AC-01: Metrics aggregation endpoint returns KPIs for date range
  - AC-02: Trend data grouped by day/week/month
- **Edge Cases:**
  - Large date range — pre-aggregated materialized data

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification        |
| ------- | --------------------- | ------- | -------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — metrics API |
| ORM     | Entity Framework Core | 9.0     | TR-003 — aggregation |

---

## Task Overview

Implement metrics aggregation endpoint returning KPIs and trend data grouped by time period.

## Dependent Tasks

- US_003 task (Database ORM) — appointment data

## Impacted Components

- Application/Queries/GetPlatformMetricsQuery.cs — metrics query

## Expected Changes

| Action | File Path                                          | Description   |
| ------ | -------------------------------------------------- | ------------- |
| CREATE | src/Application/Queries/GetPlatformMetricsQuery.cs | Metrics query |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — aggregation accuracy

## Implementation Checklist

- [ ] Create metrics aggregation endpoint with date range (AC-01)
- [ ] Group trend data by day/week/month (AC-02)
