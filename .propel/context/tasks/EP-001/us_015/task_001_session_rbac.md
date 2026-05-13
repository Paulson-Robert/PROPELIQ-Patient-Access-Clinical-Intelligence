# Task - TASK_001

## Requirement Reference

- **User Story:** US_015
- **Story Location:** .propel/context/tasks/EP-001/us_015/us_015.md
- **Acceptance Criteria:**
  - AC-01: JWT token issued with userId, email, role, 15-minute expiry
  - AC-02: Sliding expiry extends on authenticated requests
  - AC-03: Patient role restricted to patient-only endpoints (403)
  - AC-04: Staff role restricted to staff-authorized endpoints (403)
  - AC-05: Admin role has full access to all endpoints
  - AC-06: Expired token returns 401, frontend redirects to login
- **Edge Cases:**
  - Token tampering — signature validation fails → 401
  - Role change mid-session — takes effect on next token refresh
  - Simultaneous role claims — rejected

---

## Applicable Technology Stack

| Layer       | Technology    | Version    | Justification                      |
| ----------- | ------------- | ---------- | ---------------------------------- |
| Backend     | ASP.NET Core  | 9.0        | NFR-006 — authorization middleware |
| Auth Server | OpenIddict    | 5.x        | NFR-007 — JWT token management     |
| Cache       | Upstash Redis | Serverless | DR-002 — sliding expiry tracking   |

---

## Task Overview

Implement JWT session management with 15-minute sliding expiry via Redis, and role-based access control middleware restricting Patient, Staff, and Admin roles to their authorized endpoints.

## Dependent Tasks

- US_002 task (Backend Scaffold) — middleware pipeline
- US_009 task (Redis Integration) — session cache for sliding expiry
- US_013 task (Backend Auth) — JWT issuance via OpenIddict

## Impacted Components

- API/Middleware/SessionSlidingExpiryMiddleware.cs — sliding expiry refresh
- API/Authorization/RoleRequirements.cs — role-based policies
- Application/Interfaces/ISessionService.cs — session abstraction
- Infrastructure/Auth/SessionService.cs — Redis-backed session tracking

## Implementation Plan

1. Create session service interface and Redis-backed implementation
2. Implement sliding expiry middleware refreshing Redis TTL on each authenticated request
3. Configure role-based authorization policies (Patient, Staff, Admin)
4. Apply [Authorize(Policy = "...")] attributes to endpoint groups
5. Implement 401 response for expired tokens with consistent response format
6. Implement 403 response for unauthorized role access

## Current Project State

- OpenIddict configured (US_013)
- Redis integrated (US_009)
- No RBAC middleware exists

## Expected Changes

| Action | File Path                                            | Description                           |
| ------ | ---------------------------------------------------- | ------------------------------------- |
| CREATE | src/API/Middleware/SessionSlidingExpiryMiddleware.cs | Sliding expiry                        |
| CREATE | src/API/Authorization/RoleRequirements.cs            | RBAC policies                         |
| CREATE | src/Application/Interfaces/ISessionService.cs        | Session interface                     |
| CREATE | src/Infrastructure/Auth/SessionService.cs            | Redis session service                 |
| MODIFY | src/API/Program.cs                                   | Register auth policies and middleware |

## External References

- [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — policy evaluation, sliding expiry logic
- [ ] Integration tests pass — role-based access control enforcement

## Implementation Checklist

- [ ] Create session service with Redis-backed 15-minute TTL management (AC-01, AC-02)
- [ ] Implement sliding expiry middleware refreshing session on authenticated requests (AC-02)
- [ ] Configure role-based authorization policies for Patient, Staff, Admin (AC-03, AC-04, AC-05)
- [ ] Apply authorization attributes restricting endpoints by role (AC-03, AC-04)
- [ ] Return 401 with consistent body for expired tokens (AC-06)
- [ ] Return 403 for unauthorized role access attempts (AC-03, AC-04)
- [ ] Validate JWT signature and reject tampered tokens (Edge Cases)
- [ ] Reject tokens with multiple role claims (Edge Cases)
