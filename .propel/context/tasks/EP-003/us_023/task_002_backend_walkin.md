# Task - TASK_002

## Requirement Reference

- **User Story:** US_023
- **Story Location:** .propel/context/tasks/EP-003/us_023/us_023.md
- **Acceptance Criteria:**
  - AC-01: Walk-in endpoint creates appointment with "walk-in" type
  - AC-02: Patient search by name/DOB returns matches
  - AC-03: Guest walk-in creates temporary patient record
  - AC-04: Walk-in auto-added to same-day queue
- **Edge Cases:**
  - Guest later registers — merge records

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification        |
| ------- | --------------------- | ------- | -------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — walk-in API |
| ORM     | Entity Framework Core | 9.0     | TR-003 — persistence |

---

## Task Overview

Implement walk-in booking endpoint creating walk-in type appointments, patient search by name/DOB, guest patient record creation, and automatic same-day queue insertion.

## Dependent Tasks

- US_018 task (Backend Booking) — appointment entity
- US_024 task (Backend Queue) — queue insertion

## Impacted Components

- Application/Commands/CreateWalkInCommand.cs — walk-in booking
- Application/Queries/SearchPatientsQuery.cs — patient search

## Implementation Plan

1. Create SearchPatientsQuery with name/DOB filter
2. Create CreateWalkInCommand persisting walk-in appointment
3. Create temporary patient record for guest walk-ins
4. Auto-insert into same-day queue on creation

## Expected Changes

| Action | File Path                                       | Description      |
| ------ | ----------------------------------------------- | ---------------- |
| CREATE | src/Application/Commands/CreateWalkInCommand.cs | Walk-in creation |
| CREATE | src/Application/Queries/SearchPatientsQuery.cs  | Patient search   |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — walk-in creation, patient search

## Implementation Checklist

- [ ] Create patient search by name and DOB (AC-02)
- [ ] Create walk-in appointment with "walk-in" type (AC-01)
- [ ] Create temporary patient record for guests (AC-03)
- [ ] Auto-add walk-in to same-day queue (AC-04)
