# Task - TASK_002

## Requirement Reference

- **User Story:** US_016
- **Story Location:** .propel/context/tasks/EP-001/us_016/us_016.md
- **Acceptance Criteria:**
  - AC-01: Password complexity enforced server-side on registration/change
  - AC-02: Passwords hashed with bcrypt work factor 12
  - AC-03: Account locks after 5 consecutive failed login attempts
  - AC-04: Password reset flow unlocks account and resets counter
  - AC-05: Failed attempt counter resets on successful login
- **Edge Cases:**
  - Staff/admin lockout — admin can unlock via user management
  - Timing attack prevention — consistent response times
  - Password reuse — infrastructure for future history check

---

## Applicable Technology Stack

| Layer    | Technology      | Version | Justification                            |
| -------- | --------------- | ------- | ---------------------------------------- |
| Backend  | ASP.NET Core    | 9.0     | TR-002 — password endpoints              |
| Security | BCrypt.Net-Next | Latest  | NFR-004, FR-005 — HIPAA password hashing |

---

## Task Overview

Implement server-side password complexity validation, bcrypt hashing with work factor 12, account lockout after 5 failed attempts, and password reset flow with account unlock.

## Dependent Tasks

- US_013 task (Backend Auth) — auth endpoints must exist

## Impacted Components

- Application/Commands/ResetPasswordCommand.cs — password reset
- Application/Validators/PasswordComplexityValidator.cs — validation rules
- Infrastructure/Auth/PasswordHashService.cs — bcrypt hashing
- Infrastructure/Auth/AccountLockoutService.cs — lockout logic

## Implementation Plan

1. Create PasswordComplexityValidator with FluentValidation rules
2. Implement PasswordHashService using BCrypt.Net-Next with work factor 12
3. Implement AccountLockoutService tracking failed attempts
4. Create password reset command with email link and account unlock
5. Reset failed counter on successful login
6. Ensure consistent response times to prevent user enumeration

## Current Project State

- Auth endpoints exist (US_013)
- No password security logic

## Expected Changes

| Action | File Path                                                 | Description            |
| ------ | --------------------------------------------------------- | ---------------------- |
| CREATE | src/Application/Validators/PasswordComplexityValidator.cs | FluentValidation rules |
| CREATE | src/Infrastructure/Auth/PasswordHashService.cs            | bcrypt hashing         |
| CREATE | src/Infrastructure/Auth/AccountLockoutService.cs          | Lockout tracking       |
| CREATE | src/Application/Commands/ResetPasswordCommand.cs          | Password reset         |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — complexity validation, lockout logic, hash verification

## Implementation Checklist

- [ ] Create password complexity validator (min 8, upper, lower, digit, special) (AC-01)
- [ ] Implement bcrypt hashing with work factor 12, no plaintext storage (AC-02)
- [ ] Implement 5-attempt lockout with descriptive lockout message (AC-03)
- [ ] Create password reset flow that unlocks account and resets counter (AC-04)
- [ ] Reset failed attempt counter on successful login (AC-05)
- [ ] Ensure consistent response times regardless of email existence (Edge Cases)
