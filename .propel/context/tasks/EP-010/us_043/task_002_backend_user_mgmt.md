# Task - TASK_002

## Requirement Reference

- **User Story:** US_043
- **Story Location:** .propel/context/tasks/EP-010/us_043/us_043.md
- **Acceptance Criteria:**
  - AC-01: CRUD endpoints for user management (admin-only)
  - AC-02: Role assignment persisted and applied on next login
  - AC-03: Deactivation sets IsActive=false, prevents auth
- **Edge Cases:**
  - Self-deactivation blocked server-side

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification                |
| ------- | --------------------- | ------- | ---------------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — user management API |
| ORM     | Entity Framework Core | 9.0     | TR-003 — persistence         |

---

## Task Overview

Implement admin user management CRUD endpoints with role assignment and account deactivation.

## Dependent Tasks

- US_015 task (Session RBAC) — admin policy

## Impacted Components

- Application/Commands/CreateUserCommand.cs — create
- Application/Commands/UpdateUserCommand.cs — update/role
- Application/Commands/DeactivateUserCommand.cs — deactivate

## Expected Changes

| Action | File Path                                         | Description |
| ------ | ------------------------------------------------- | ----------- |
| CREATE | src/Application/Commands/CreateUserCommand.cs     | Create user |
| CREATE | src/Application/Commands/UpdateUserCommand.cs     | Update user |
| CREATE | src/Application/Commands/DeactivateUserCommand.cs | Deactivate  |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — CRUD, deactivation, self-deactivation block

## Implementation Checklist

- [x] Create user management CRUD endpoints (admin-only) (AC-01)
- [x] Persist role assignment (AC-02)
- [x] Implement deactivation preventing auth (AC-03)
- [x] Block self-deactivation server-side (Edge Cases)
