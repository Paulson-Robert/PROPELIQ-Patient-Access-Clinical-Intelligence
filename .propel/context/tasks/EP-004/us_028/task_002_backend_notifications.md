# Task - TASK_002

## Requirement Reference

- **User Story:** US_028
- **Story Location:** .propel/context/tasks/EP-004/us_028/us_028.md
- **Acceptance Criteria:**
  - AC-01: Notification endpoint creates staff notifications
  - AC-02: Notification history query with pagination
  - AC-03: Mark-as-read endpoint
- **Edge Cases:**
  - High notification volume — batch insert

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification             |
| ------- | --------------------- | ------- | ------------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — notification API |
| ORM     | Entity Framework Core | 9.0     | TR-003 — persistence      |

---

## Task Overview

Implement notification persistence, paginated history query, and mark-as-read endpoint for staff notifications.

## Dependent Tasks

- US_003 task (Database ORM)

## Impacted Components

- Application/Commands/CreateNotificationCommand.cs — create notification
- Application/Queries/GetNotificationHistoryQuery.cs — paginated history
- Application/Commands/MarkNotificationReadCommand.cs — mark read

## Expected Changes

| Action | File Path                                               | Description |
| ------ | ------------------------------------------------------- | ----------- |
| CREATE | src/Application/Commands/CreateNotificationCommand.cs   | Create      |
| CREATE | src/Application/Queries/GetNotificationHistoryQuery.cs  | History     |
| CREATE | src/Application/Commands/MarkNotificationReadCommand.cs | Mark read   |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — creation, pagination, mark read

## Implementation Checklist

- [x] Create notification persistence command (AC-01)
- [x] Create paginated notification history query (AC-02)
- [x] Create mark-as-read command (AC-03)
