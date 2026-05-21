# Task - TASK_001

## Requirement Reference

- **User Story:** US_048
- **Story Location:** .propel/context/tasks/EP-011/us_048/us_048.md
- **Acceptance Criteria:**
  - AC-01: Redis sliding window rate limiting per IP/user
  - AC-02: HTTP 429 response with Retry-After header when exceeded
  - AC-03: Security headers (X-Content-Type-Options, X-Frame-Options, CSP)
- **Edge Cases:**
  - Redis unavailable — fail-open with logging (no blocking)

---

## Applicable Technology Stack

| Layer   | Technology    | Version    | Justification                |
| ------- | ------------- | ---------- | ---------------------------- |
| Backend | ASP.NET Core  | 9.0        | TR-002 — middleware          |
| Cache   | Upstash Redis | Serverless | DR-002 — rate limit counters |

---

## Task Overview

Implement Redis sliding window rate limiting middleware with HTTP 429 responses and security headers.

## Dependent Tasks

- US_009 task (Redis Integration) — Redis connection

## Impacted Components

- API/Middleware/RateLimitingMiddleware.cs — rate limiting
- Infrastructure/Services/RateLimitService.cs — Redis sliding window
- API/Configuration/SecurityHeadersMiddleware.cs — headers

## Implementation Plan

1. Create RateLimitService using Redis sorted sets for sliding window
2. Create RateLimitingMiddleware checking limits per IP/user
3. Return 429 with Retry-After header when exceeded
4. Create SecurityHeadersMiddleware adding X-Content-Type-Options, X-Frame-Options, CSP
5. Handle Redis unavailability with fail-open + logging

## Expected Changes

| Action | File Path                                          | Description    |
| ------ | -------------------------------------------------- | -------------- |
| CREATE | src/API/Middleware/RateLimitingMiddleware.cs       | Rate limiting  |
| CREATE | src/Infrastructure/Services/RateLimitService.cs    | Sliding window |
| CREATE | src/API/Configuration/SecurityHeadersMiddleware.cs | Headers        |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — rate limit logic, header presence

## Implementation Checklist

- [x] Implement Redis sliding window rate limiting (AC-01)
- [x] Return HTTP 429 with Retry-After header (AC-02)
- [x] Add security headers (X-Content-Type-Options, X-Frame-Options, CSP) (AC-03)
- [x] Fail-open with logging on Redis unavailability (Edge Cases)
