# Task - TASK_002

## Requirement Reference

- **User Story:** US_018
- **Story Location:** .propel/context/tasks/EP-002/us_018/us_018.md
- **Acceptance Criteria:**
  - AC-01: Slot search returns available slots filtered by provider
  - AC-02: Slot search returns available slots filtered by specialty
  - AC-03: Slot selection acquires 30-second Redis lock
  - AC-04: Booking confirmation saves appointment and generates PDF
  - AC-05: Lock prevents double-booking
  - AC-06: Lock auto-releases after 30 seconds
- **Edge Cases:**
  - Concurrent booking attempts — only first succeeds, others get 409 Conflict
  - Redis failure — falls back to DB-level optimistic concurrency
  - PDF generation failure — booking succeeds, PDF queued for retry

---

## Applicable Technology Stack

| Layer    | Technology            | Version    | Justification                    |
| -------- | --------------------- | ---------- | -------------------------------- |
| Backend  | ASP.NET Core          | 9.0        | TR-002 — booking endpoints       |
| Cache    | Upstash Redis         | Serverless | DR-002 — slot locking            |
| ORM      | Entity Framework Core | 9.0        | TR-003 — appointment persistence |
| Document | iText7                | Latest     | NFR-011 — PDF generation         |

---

## Task Overview

Implement appointment search endpoints (by provider/specialty), 30-second Redis-based slot locking to prevent double-booking, booking confirmation with appointment persistence, and PDF confirmation generation.

## Dependent Tasks

- US_002 task (Backend Scaffold) — API project
- US_009 task (Redis Integration) — Redis connection
- US_003 task (Database ORM) — EF Core context

## Impacted Components

- Application/Queries/SearchSlotsQuery.cs — MediatR slot search
- Application/Commands/LockSlotCommand.cs — Redis slot lock
- Application/Commands/ConfirmBookingCommand.cs — persist booking
- Infrastructure/Services/SlotLockService.cs — Redis locking
- Infrastructure/Services/BookingPdfService.cs — PDF generation

## Implementation Plan

1. Create SearchSlotsQuery with provider/specialty filters and availability check
2. Create SlotLockService using Redis SETNX with 30-second TTL
3. Create LockSlotCommand acquiring lock and returning lock token
4. Create ConfirmBookingCommand validating lock, persisting appointment
5. Create BookingPdfService generating confirmation PDF via iText7
6. Handle concurrent booking with 409 Conflict response
7. Implement Redis fallback with DB-level optimistic concurrency

## Current Project State

- Backend scaffold exists (US_002)
- Redis connected (US_009)
- EF Core configured (US_003)
- No booking logic exists

## Expected Changes

| Action | File Path                                         | Description          |
| ------ | ------------------------------------------------- | -------------------- |
| CREATE | src/Application/Queries/SearchSlotsQuery.cs       | Slot search          |
| CREATE | src/Application/Commands/LockSlotCommand.cs       | Slot locking         |
| CREATE | src/Application/Commands/ConfirmBookingCommand.cs | Booking confirmation |
| CREATE | src/Infrastructure/Services/SlotLockService.cs    | Redis lock service   |
| CREATE | src/Infrastructure/Services/BookingPdfService.cs  | PDF generation       |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — slot search filtering, lock acquisition, booking logic
- [ ] Integration tests pass — concurrent booking prevention

## Implementation Checklist

- [ ] Create SearchSlotsQuery with provider and specialty filters (AC-01, AC-02)
- [ ] Implement Redis SETNX slot lock with 30-second TTL (AC-03, AC-06)
- [ ] Create ConfirmBookingCommand with lock validation and appointment save (AC-04)
- [ ] Implement PDF confirmation generation via iText7 (AC-04)
- [ ] Return 409 Conflict for concurrent booking attempts (AC-05)
- [ ] Implement DB-level fallback for Redis failure (Edge Cases)
- [ ] Queue PDF for retry on generation failure (Edge Cases)
