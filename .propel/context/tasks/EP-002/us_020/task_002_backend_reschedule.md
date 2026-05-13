# Task - TASK_002

## Requirement Reference

- **User Story:** US_020
- **Story Location:** .propel/context/tasks/EP-002/us_020/us_020.md
- **Acceptance Criteria:**
  - AC-01: Cancel endpoint releases slot and triggers swap engine
  - AC-02: Reschedule cancels old + books new in single transaction
  - AC-03: Calendar sync triggered on cancellation/reschedule
- **Edge Cases:**
  - Reschedule fails on new slot — old appointment preserved

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification                   |
| ------- | --------------------- | ------- | ------------------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — cancellation endpoints |
| ORM     | Entity Framework Core | 9.0     | TR-003 — transactional updates  |

---

## Task Overview

Implement cancellation endpoint releasing the slot and triggering the swap engine, reschedule as atomic cancel+book transaction, and calendar sync notification on state change.

## Dependent Tasks

- US_018 task (Backend Booking) — booking persistence
- US_019 task (Swap Engine) — swap trigger on cancel

## Impacted Components

- Application/Commands/CancelAppointmentCommand.cs — cancel logic
- Application/Commands/RescheduleAppointmentCommand.cs — atomic reschedule

## Implementation Plan

1. Create CancelAppointmentCommand releasing slot and publishing cancellation event
2. Create RescheduleAppointmentCommand as atomic transaction (cancel old + book new)
3. Publish domain event triggering swap engine and calendar sync
4. Rollback reschedule if new booking fails

## Expected Changes

| Action | File Path                                                | Description  |
| ------ | -------------------------------------------------------- | ------------ |
| CREATE | src/Application/Commands/CancelAppointmentCommand.cs     | Cancellation |
| CREATE | src/Application/Commands/RescheduleAppointmentCommand.cs | Reschedule   |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — cancel releases slot, reschedule atomicity

## Implementation Checklist

- [ ] Create CancelAppointmentCommand releasing slot and publishing event (AC-01)
- [ ] Trigger swap engine on cancellation via domain event (AC-01)
- [ ] Create RescheduleAppointmentCommand as atomic cancel+book (AC-02)
- [ ] Publish calendar sync event on state change (AC-03)
- [ ] Rollback reschedule preserving old appointment on failure (Edge Cases)
