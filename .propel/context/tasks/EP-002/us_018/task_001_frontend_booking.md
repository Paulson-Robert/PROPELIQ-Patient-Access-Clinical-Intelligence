# Task - TASK_001

## Requirement Reference

- **User Story:** US_018
- **Story Location:** .propel/context/tasks/EP-002/us_018/us_018.md
- **Acceptance Criteria:**
  - AC-01: Slot search by provider displays available slots sorted by date/time
  - AC-02: Slot search by specialty displays slots across all providers
  - AC-03: Slot selection shows countdown timer (UXR-502) and navigates to SCR-006
  - AC-05: Locked slot shows "temporarily held" message to other users
  - AC-06: Lock timeout shows "Slot hold expired" message
  - AC-07: No results shows guidance with alternative suggestions
- **Edge Cases:**
  - Slot booked between search and selection — refresh with updated message
  - PDF generation failure — booking still succeeds

---

## Design References

| Reference Type         | Value                                                                      |
| ---------------------- | -------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                        |
| **Wireframe Status**   | AVAILABLE                                                                  |
| **Wireframe Type**     | HTML                                                                       |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-005-appointment-search.html |
| **Screen Spec**        | SCR-005, SCR-006, MOD-007                                                  |
| **UXR Requirements**   | UXR-502                                                                    |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification                        |
| ----------- | ------------ | -------------------- | ------------------------------------ |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — booking UI                  |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — search and card components |

---

## Task Overview

Implement the appointment search page (SCR-005) with provider/specialty search, available slot results, the booking confirmation page (SCR-006) with 30-second countdown timer, and slot lock feedback states.

## Dependent Tasks

- US_001 task (Frontend Scaffold) — React project must exist
- US_049 task (Design System) — component library

## Impacted Components

- frontend/src/pages/booking/AppointmentSearchPage.tsx — search page
- frontend/src/pages/booking/BookingConfirmationPage.tsx — confirmation page
- frontend/src/components/booking/SlotCard.tsx — slot display component
- frontend/src/components/booking/CountdownTimer.tsx — 30-second timer
- frontend/src/services/bookingApi.ts — booking API client

## Implementation Plan

1. Create AppointmentSearchPage with provider/specialty search inputs
2. Create SlotCard component displaying provider, specialty, date, time
3. Implement search results with sort and empty state guidance
4. Create BookingConfirmationPage with selected slot details and countdown
5. Create CountdownTimer component with visual 30-second countdown
6. Handle lock expiry and slot unavailability states
7. Connect to backend search and booking endpoints

## Current Project State

- Frontend scaffold exists (US_001)
- No booking pages exist

## Expected Changes

| Action | File Path                                              | Description        |
| ------ | ------------------------------------------------------ | ------------------ |
| CREATE | frontend/src/pages/booking/AppointmentSearchPage.tsx   | Search page        |
| CREATE | frontend/src/pages/booking/BookingConfirmationPage.tsx | Confirmation page  |
| CREATE | frontend/src/components/booking/SlotCard.tsx           | Slot display       |
| CREATE | frontend/src/components/booking/CountdownTimer.tsx     | Lock timer         |
| CREATE | frontend/src/services/bookingApi.ts                    | Booking API client |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — search filtering, countdown logic
- [ ] Integration tests pass — search and booking flow

## Implementation Checklist

- [x] Create AppointmentSearchPage with provider/specialty search (AC-01, AC-02)
- [x] Create SlotCard displaying provider, specialty, date, time with select action (AC-01)
- [x] Implement 30-second CountdownTimer with visual feedback (AC-03, UXR-502)
- [x] Create BookingConfirmationPage with slot details and confirm button (AC-03)
- [x] Display "temporarily held" message for locked slots (AC-05)
- [x] Handle lock timeout with "Slot hold expired" message (AC-06)
- [x] Display "No available slots" with alternative suggestions (AC-07)
- [x] Handle slot booked between search and selection (Edge Cases)
