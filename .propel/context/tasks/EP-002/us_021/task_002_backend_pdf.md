# Task - TASK_002

## Requirement Reference

- **User Story:** US_021
- **Story Location:** .propel/context/tasks/EP-002/us_021/us_021.md
- **Acceptance Criteria:**
  - AC-01: Insurance info persisted with appointment record
  - AC-02: PDF confirmation includes insurance details
  - AC-03: Soft validation regex check on policy number format
- **Edge Cases:**
  - Insurance field null — allowed, stored as null

---

## Applicable Technology Stack

| Layer    | Technology   | Version | Justification                |
| -------- | ------------ | ------- | ---------------------------- |
| Backend  | ASP.NET Core | 9.0     | TR-002 — insurance API       |
| Document | iText7       | Latest  | NFR-011 — PDF with insurance |

---

## Task Overview

Persist insurance information with appointment records, include insurance details in PDF confirmation generation, and implement server-side soft validation for policy number format.

## Dependent Tasks

- US_018 task (Backend Booking) — BookingPdfService

## Impacted Components

- Domain/Entities/Appointment.cs — insurance fields
- Infrastructure/Services/BookingPdfService.cs — add insurance to PDF

## Implementation Plan

1. Add insurance provider and policy number fields to Appointment entity
2. Update ConfirmBookingCommand to persist insurance data
3. Update BookingPdfService to include insurance in confirmation PDF
4. Add server-side soft validation for policy number format

## Expected Changes

| Action | File Path                                        | Description          |
| ------ | ------------------------------------------------ | -------------------- |
| MODIFY | src/Domain/Entities/Appointment.cs               | Add insurance fields |
| MODIFY | src/Infrastructure/Services/BookingPdfService.cs | Insurance in PDF     |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — insurance persistence, PDF contains insurance

## Implementation Checklist

- [ ] Add insurance provider and policy number to Appointment entity (AC-01)
- [ ] Persist insurance info with booking confirmation (AC-01)
- [ ] Include insurance details in PDF confirmation (AC-02)
- [ ] Implement server-side soft validation for policy format (AC-03)
