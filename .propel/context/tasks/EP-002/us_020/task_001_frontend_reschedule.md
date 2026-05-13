# Task - TASK_001

## Requirement Reference

- **User Story:** US_020
- **Story Location:** .propel/context/tasks/EP-002/us_020/us_020.md
- **Acceptance Criteria:**
  - AC-01: Cancel button shows confirmation dialog (MOD-003)
  - AC-02: Reschedule navigates to slot search with pre-filled context
  - AC-03: Calendar displays status update in real-time after cancellation
- **Edge Cases:**
  - Cancel within lock window — slot still locked by another user

---

## Design References

| Reference Type         | Value                                                                      |
| ---------------------- | -------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                        |
| **Wireframe Status**   | AVAILABLE                                                                  |
| **Wireframe Type**     | HTML                                                                       |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-007-appointment-detail.html |
| **Screen Spec**        | SCR-007, SCR-008, MOD-003                                                  |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification              |
| ----------- | ------------ | -------------------- | -------------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — reschedule UI     |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — dialog component |

---

## Task Overview

Implement appointment detail page (SCR-007) with cancel/reschedule actions, cancellation confirmation dialog (MOD-003), and reschedule flow navigating to slot search with pre-filled provider/specialty context.

## Dependent Tasks

- US_018 task (Frontend Booking) — search page for reschedule target

## Impacted Components

- frontend/src/pages/booking/AppointmentDetailPage.tsx — detail view
- frontend/src/components/booking/CancelConfirmDialog.tsx — MOD-003

## Implementation Plan

1. Create AppointmentDetailPage showing appointment info with actions
2. Create CancelConfirmDialog (MOD-003) with confirm/dismiss
3. Implement cancel action calling API and updating calendar status
4. Implement reschedule navigating to search with pre-filled context

## Expected Changes

| Action | File Path                                               | Description   |
| ------ | ------------------------------------------------------- | ------------- |
| CREATE | frontend/src/pages/booking/AppointmentDetailPage.tsx    | Detail page   |
| CREATE | frontend/src/components/booking/CancelConfirmDialog.tsx | Cancel dialog |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — dialog interaction, navigation

## Implementation Checklist

- [ ] Create AppointmentDetailPage with cancel and reschedule actions (AC-01, AC-02)
- [ ] Create CancelConfirmDialog matching MOD-003 wireframe (AC-01)
- [ ] Implement reschedule navigating to search with pre-filled context (AC-02)
- [ ] Update calendar status display after cancellation (AC-03)
