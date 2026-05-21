# Task - TASK_001

## Requirement Reference

- **User Story:** US_021
- **Story Location:** .propel/context/tasks/EP-002/us_021/us_021.md
- **Acceptance Criteria:**
  - AC-01: Insurance provider and policy number captured during booking
  - AC-02: Soft validation shows warning (not blocking) for invalid format
  - AC-03: Booking confirmation shows insurance info
- **Edge Cases:**
  - Patient without insurance — optional field, booking proceeds

---

## Design References

| Reference Type         | Value                                                                  |
| ---------------------- | ---------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                    |
| **Wireframe Status**   | AVAILABLE                                                              |
| **Wireframe Type**     | HTML                                                                   |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-014-insurance-info.html |
| **Screen Spec**        | SCR-014, SCR-006                                                       |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification             |
| ----------- | ------------ | -------------------- | ------------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — insurance form   |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — form validation |

---

## Task Overview

Implement insurance information capture form (SCR-014) with provider/policy fields, soft validation warnings for format issues, and display of insurance info on booking confirmation (SCR-006).

## Dependent Tasks

- US_018 task (Frontend Booking) — booking confirmation page

## Impacted Components

- frontend/src/components/booking/InsuranceForm.tsx — insurance input
- frontend/src/pages/booking/BookingConfirmationPage.tsx — show insurance

## Implementation Plan

1. Create InsuranceForm with provider dropdown and policy number input
2. Implement soft validation showing non-blocking format warnings
3. Integrate into booking flow between slot selection and confirmation
4. Display insurance info on confirmation page

## Expected Changes

| Action | File Path                                              | Description    |
| ------ | ------------------------------------------------------ | -------------- |
| CREATE | frontend/src/components/booking/InsuranceForm.tsx      | Insurance form |
| MODIFY | frontend/src/pages/booking/BookingConfirmationPage.tsx | Show insurance |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — form rendering, soft validation warnings

## Implementation Checklist

- [x] Create InsuranceForm with provider and policy number fields (AC-01)
- [x] Implement soft validation showing non-blocking warnings (AC-02)
- [x] Display insurance information on booking confirmation page (AC-03)
- [x] Handle optional insurance — booking proceeds without it (Edge Cases)
