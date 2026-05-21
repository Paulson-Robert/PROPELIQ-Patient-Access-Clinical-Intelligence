# Task - TASK_001

## Requirement Reference

- **User Story:** US_023
- **Story Location:** .propel/context/tasks/EP-003/us_023/us_023.md
- **Acceptance Criteria:**
  - AC-01: Staff can create walk-in booking by searching existing patient
  - AC-02: Staff can create walk-in for guest (minimal info)
  - AC-03: Walk-in immediately appears in same-day queue
- **Edge Cases:**
  - Duplicate patient search — show match suggestions

---

## Design References

| Reference Type         | Value                                                           |
| ---------------------- | --------------------------------------------------------------- |
| **UI Impact**          | Yes                                                             |
| **Wireframe Status**   | AVAILABLE                                                       |
| **Wireframe Type**     | HTML                                                            |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-018-walk-in.html |
| **Screen Spec**        | SCR-018                                                         |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification          |
| ----------- | ------------ | -------------------- | ---------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — walk-in UI    |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — search, form |

---

## Task Overview

Implement walk-in booking page (SCR-018) for staff with patient search, guest walk-in form, and immediate queue addition.

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/pages/walkin/WalkInBookingPage.tsx — walk-in page
- frontend/src/components/walkin/PatientSearchInput.tsx — patient search

## Implementation Plan

1. Create PatientSearchInput with typeahead patient search
2. Create WalkInBookingPage with search + guest form
3. On submit, call walk-in booking API and show success

## Expected Changes

| Action | File Path                                             | Description    |
| ------ | ----------------------------------------------------- | -------------- |
| CREATE | frontend/src/pages/walkin/WalkInBookingPage.tsx       | Walk-in page   |
| CREATE | frontend/src/components/walkin/PatientSearchInput.tsx | Patient search |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — search, form submission

## Implementation Checklist

- [x] Create PatientSearchInput with typeahead results (AC-01)
- [x] Create WalkInBookingPage with existing patient and guest modes (AC-01, AC-02)
- [x] Submit walk-in booking to API (AC-03)
- [x] Show duplicate patient suggestions (Edge Cases)
