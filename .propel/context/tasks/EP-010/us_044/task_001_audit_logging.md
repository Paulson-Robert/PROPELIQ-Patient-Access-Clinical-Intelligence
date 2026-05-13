# Task - TASK_001

## Requirement Reference

- **User Story:** US_044
- **Story Location:** .propel/context/tasks/EP-010/us_044/us_044.md
- **Acceptance Criteria:**
  - AC-01: All state-changing operations logged with actor, action, timestamp, entity
  - AC-02: Audit records immutable — INSERT-only PostgreSQL rules
  - AC-03: MediatR pipeline behavior captures automatically
  - AC-04: PHI access logged
- **Edge Cases:**
  - High-volume operations — batch insert for performance

---

## Applicable Technology Stack

| Layer    | Technology            | Version | Justification            |
| -------- | --------------------- | ------- | ------------------------ |
| Backend  | ASP.NET Core          | 9.0     | TR-002 — audit behavior  |
| ORM      | Entity Framework Core | 9.0     | TR-003 — INSERT-only     |
| Database | PostgreSQL            | 16.x    | TR-004 — immutable rules |

---

## Task Overview

Implement immutable audit logging via MediatR pipeline behavior, capturing all state-changing operations with actor/action/timestamp/entity, using INSERT-only PostgreSQL rules.

## Dependent Tasks

- US_002 task (Backend Scaffold) — MediatR pipeline
- US_008 task (Database Schema) — audit table

## Impacted Components

- Application/Behaviors/AuditLoggingBehavior.cs — MediatR behavior
- Infrastructure/Persistence/AuditLogRepository.cs — INSERT-only persistence
- Domain/Entities/AuditLogEntry.cs — audit entity

## Implementation Plan

1. Create AuditLogEntry entity with actor, action, timestamp, entity fields
2. Create INSERT-only PostgreSQL rule/trigger preventing UPDATE/DELETE
3. Create AuditLoggingBehavior as MediatR pipeline behavior
4. Auto-capture all command handlers via pipeline
5. Add explicit PHI access logging

## Expected Changes

| Action | File Path                                            | Description       |
| ------ | ---------------------------------------------------- | ----------------- |
| CREATE | src/Application/Behaviors/AuditLoggingBehavior.cs    | Pipeline behavior |
| CREATE | src/Infrastructure/Persistence/AuditLogRepository.cs | Repository        |
| CREATE | src/Domain/Entities/AuditLogEntry.cs                 | Entity            |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — behavior captures, immutability

## Implementation Checklist

- [ ] Create AuditLogEntry with actor, action, timestamp, entity (AC-01)
- [ ] Implement INSERT-only PostgreSQL rules (AC-02)
- [ ] Create MediatR pipeline behavior for automatic capture (AC-03)
- [ ] Log PHI access explicitly (AC-04)
- [ ] Implement batch insert for high-volume operations (Edge Cases)
