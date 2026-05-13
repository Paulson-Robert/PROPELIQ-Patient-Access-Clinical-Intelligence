# Task - TASK_002

## Requirement Reference

- **User Story:** US_022
- **Story Location:** .propel/context/tasks/EP-002/us_022/us_022.md
- **Acceptance Criteria:**
  - AC-01: Paginated appointment history endpoint with cursor-based pagination
  - AC-02: Filter by status and date range
  - AC-03: Returns appointment with provider, specialty, status, date/time
- **Edge Cases:**
  - Large history — efficient query with indexed columns

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification            |
| ------- | --------------------- | ------- | ------------------------ |
| Backend | ASP.NET Core          | 9.0     | TR-002 — history API     |
| ORM     | Entity Framework Core | 9.0     | TR-003 — paginated query |

---

## Task Overview

Implement paginated appointment history query endpoint with status/date filters, returning appointment details with provider info.

## Dependent Tasks

- US_003 task (Database ORM) — EF Core context
- US_008 task (Database Schema) — appointment table

## Impacted Components

- Application/Queries/GetAppointmentHistoryQuery.cs — paginated query

## Implementation Plan

1. Create GetAppointmentHistoryQuery with cursor pagination, status/date filters
2. Return projected appointment DTOs with provider info
3. Ensure indexed query on (PatientId, AppointmentDate)

## Expected Changes

| Action | File Path                                             | Description   |
| ------ | ----------------------------------------------------- | ------------- |
| CREATE | src/Application/Queries/GetAppointmentHistoryQuery.cs | History query |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — pagination, filtering

## Implementation Checklist

- [ ] Create paginated history query with cursor-based pagination (AC-01)
- [ ] Implement status and date range filters (AC-02)
- [ ] Return appointment with provider, specialty, status, date/time (AC-03)
- [ ] Ensure efficient indexed query for large histories (Edge Cases)
