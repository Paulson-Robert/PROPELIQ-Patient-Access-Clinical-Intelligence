# Task - TASK_002

## Requirement Reference

- **User Story:** US_030
- **Story Location:** .propel/context/tasks/EP-005/us_030/us_030.md
- **Acceptance Criteria:**
  - AC-01: Manual intake submission endpoint validates and persists
  - AC-02: Draft save/load endpoints for partial intake
- **Edge Cases:**
  - Duplicate submission — idempotency check

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification        |
| ------- | --------------------- | ------- | -------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — intake API  |
| ORM     | Entity Framework Core | 9.0     | TR-003 — persistence |

---

## Task Overview

Implement manual intake submission with server-side validation, draft save/load endpoints for partial completion.

## Dependent Tasks

- US_003 task (Database ORM)

## Impacted Components

- Application/Commands/SubmitManualIntakeCommand.cs — submission
- Application/Commands/SaveIntakeDraftCommand.cs — draft save
- Application/Queries/GetIntakeDraftQuery.cs — draft load

## Expected Changes

| Action | File Path                                             | Description |
| ------ | ----------------------------------------------------- | ----------- |
| CREATE | src/Application/Commands/SubmitManualIntakeCommand.cs | Submission  |
| CREATE | src/Application/Commands/SaveIntakeDraftCommand.cs    | Draft save  |
| CREATE | src/Application/Queries/GetIntakeDraftQuery.cs        | Draft load  |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — validation, draft CRUD

## Implementation Checklist

- [x] Create intake submission with server-side validation (AC-01)
- [x] Create draft save endpoint for partial intake (AC-02)
- [x] Create draft load endpoint (AC-02)
- [x] Implement idempotency check preventing duplicate submissions (Edge Cases)
