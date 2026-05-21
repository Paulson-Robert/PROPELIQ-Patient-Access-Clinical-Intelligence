# Task - TASK_001

## Requirement Reference

- **User Story:** US_016
- **Story Location:** .propel/context/tasks/EP-001/us_016/us_016.md
- **Acceptance Criteria:**
  - AC-01: Password complexity enforced with inline validation errors
- **Edge Cases:**
  - N/A for frontend-only scope

---

## Design References

| Reference Type         | Value                                                                  |
| ---------------------- | ---------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                    |
| **Wireframe Status**   | AVAILABLE                                                              |
| **Wireframe Type**     | HTML                                                                   |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-029-password-reset.html |
| **Screen Spec**        | SCR-029                                                                |
| **UXR Requirements**   | UXR-004                                                                |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification             |
| ----------- | ------------ | -------------------- | ------------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — password UI      |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — form validation |

---

## Task Overview

Implement password complexity inline validation in the registration and password reset forms, showing specific requirements not met (min 8 chars, uppercase, lowercase, digit, special char).

## Dependent Tasks

- US_013 task (Frontend Auth) — registration form must exist

## Impacted Components

- frontend/src/components/auth/PasswordInput.tsx — password field with complexity indicator
- frontend/src/pages/auth/PasswordResetPage.tsx — password reset flow
- frontend/src/utils/passwordValidation.ts — validation rules

## Implementation Plan

1. Create password validation utility with complexity rules
2. Create PasswordInput component with inline requirement indicators
3. Create PasswordResetPage (SCR-029) with email input and new password form
4. Integrate validation into registration and reset forms

## Current Project State

- Registration form exists (US_013)
- No password complexity UI

## Expected Changes

| Action | File Path                                      | Description                        |
| ------ | ---------------------------------------------- | ---------------------------------- |
| CREATE | frontend/src/components/auth/PasswordInput.tsx | Password with complexity indicator |
| CREATE | frontend/src/pages/auth/PasswordResetPage.tsx  | Password reset page                |
| CREATE | frontend/src/utils/passwordValidation.ts       | Complexity validation rules        |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — validation rules, component rendering

## Implementation Checklist

- [x] Create password validation utility enforcing all complexity requirements (AC-01)
- [x] Create PasswordInput component showing inline requirement indicators (AC-01)
- [x] Create PasswordResetPage matching SCR-029 wireframe (AC-01)
- [x] Integrate complexity validation into registration and reset forms (AC-01)
