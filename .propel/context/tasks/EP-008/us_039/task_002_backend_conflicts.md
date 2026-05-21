# Task - TASK_002

## Requirement Reference

- **User Story:** US_039
- **Story Location:** .propel/context/tasks/EP-008/us_039/us_039.md
- **Acceptance Criteria:**
  - AC-01: Conflict detection identifies mismatches across sources
  - AC-02: Resolution endpoint persists selected value
  - AC-03: Resolution logged in audit trail
- **Edge Cases:**
  - New data re-opens resolved conflict — staff notified

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification             |
| ------- | --------------------- | ------- | ------------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — conflict service |
| ORM     | Entity Framework Core | 9.0     | TR-003 — persistence      |

---

## Task Overview

Implement conflict detection comparing data across sources, resolution endpoint persisting selections, and audit trail logging.

## Dependent Tasks

- US_038 task (Backend Aggregation) — aggregated data

## Impacted Components

- Infrastructure/Services/ConflictDetectionService.cs — detection
- Application/Commands/ResolveConflictCommand.cs — resolution

## Expected Changes

| Action | File Path                                               | Description |
| ------ | ------------------------------------------------------- | ----------- |
| CREATE | src/Infrastructure/Services/ConflictDetectionService.cs | Detection   |
| CREATE | src/Application/Commands/ResolveConflictCommand.cs      | Resolution  |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — detection logic, resolution persistence

## Implementation Checklist

- [x] Detect mismatches across data sources (AC-01)
- [x] Persist resolution selection (AC-02)
- [x] Log resolution in audit trail (AC-03)
- [x] Re-open resolved conflict on new conflicting data (Edge Cases)
