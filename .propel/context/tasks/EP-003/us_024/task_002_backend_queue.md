# Task - TASK_002

## Requirement Reference

- **User Story:** US_024
- **Story Location:** .propel/context/tasks/EP-003/us_024/us_024.md
- **Acceptance Criteria:**
  - AC-01: Queue endpoint returns today's patients ordered by arrival
  - AC-02: Mark arrived endpoint updates status and timestamp
  - AC-03: Reorder endpoint persists new order with staff reason
  - AC-04: Queue changes broadcast to connected clients
- **Edge Cases:**
  - Concurrent reorder — optimistic concurrency with version check

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification        |
| ------- | --------------------- | ------- | -------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — queue API   |
| ORM     | Entity Framework Core | 9.0     | TR-003 — persistence |

---

## Task Overview

Implement same-day queue backend: query today's patients by arrival order, mark arrived with timestamp, persist drag-and-drop reorder with reason, and broadcast updates.

## Dependent Tasks

- US_003 task (Database ORM) — EF Core

## Impacted Components

- Application/Queries/GetSameDayQueueQuery.cs — queue query
- Application/Commands/MarkArrivedCommand.cs — mark arrived
- Application/Commands/ReorderQueueCommand.cs — reorder with reason

## Implementation Plan

1. Create GetSameDayQueueQuery filtering today's appointments by arrival
2. Create MarkArrivedCommand setting arrival timestamp
3. Create ReorderQueueCommand with position update and reason logging
4. Implement optimistic concurrency for concurrent reorders

## Expected Changes

| Action | File Path                                       | Description  |
| ------ | ----------------------------------------------- | ------------ |
| CREATE | src/Application/Queries/GetSameDayQueueQuery.cs | Queue query  |
| CREATE | src/Application/Commands/MarkArrivedCommand.cs  | Mark arrived |
| CREATE | src/Application/Commands/ReorderQueueCommand.cs | Reorder      |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — queue ordering, arrival marking, reorder

## Implementation Checklist

- [ ] Create queue query returning today's patients by arrival order (AC-01)
- [ ] Create mark arrived command with timestamp (AC-02)
- [ ] Create reorder command persisting new order with reason (AC-03)
- [ ] Implement change notification for connected clients (AC-04)
- [ ] Handle concurrent reorder with optimistic concurrency (Edge Cases)
