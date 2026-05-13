# Task - TASK_001

## Requirement Reference

- **User Story:** US_014
- **Story Location:** .propel/context/tasks/EP-001/us_014/us_014.md
- **Acceptance Criteria:**
  - AC-01: Staff login with MFA verification page (SCR-026)
  - AC-02: Admin login with MFA verification page
  - AC-03: First-login MFA setup flow (SCR-027) with QR code
  - AC-04: Invalid MFA code after 3 attempts terminates session
  - AC-05: Expired MFA code with re-send option
  - AC-06: Deactivated account rejected at login
- **Edge Cases:**
  - MFA device lost — admin reset required
  - Clock skew — ±1 time step TOTP validation

---

## Design References

| Reference Type         | Value                                                                    |
| ---------------------- | ------------------------------------------------------------------------ |
| **UI Impact**          | Yes                                                                      |
| **Wireframe Status**   | AVAILABLE                                                                |
| **Wireframe Type**     | HTML                                                                     |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-026-mfa-verification.html |
| **Screen Spec**        | SCR-001, SCR-026, SCR-027                                                |
| **UXR Requirements**   | UXR-605                                                                  |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification               |
| ----------- | ------------ | -------------------- | --------------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — MFA UI             |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — accessible inputs |

---

## Task Overview

Implement MFA verification page (SCR-026) with TOTP/SMS code input, countdown timer, and retry logic, plus the first-login MFA setup page (SCR-027) with QR code display and manual key entry.

## Dependent Tasks

- US_013 task (Frontend Auth) — login page must exist for MFA flow entry

## Impacted Components

- frontend/src/pages/auth/MfaVerificationPage.tsx — MFA code entry
- frontend/src/pages/auth/MfaSetupPage.tsx — QR code setup
- frontend/src/components/auth/TotpInput.tsx — 6-digit code input

## Implementation Plan

1. Create MfaVerificationPage with 6-digit code input and countdown timer
2. Create MfaSetupPage with QR code display and manual key entry
3. Implement 3-attempt lockout with session termination
4. Implement expired code detection with resend option
5. Connect to backend MFA endpoints

## Current Project State

- Login page exists (US_013)
- No MFA UI exists

## Expected Changes

| Action | File Path                                       | Description                  |
| ------ | ----------------------------------------------- | ---------------------------- |
| CREATE | frontend/src/pages/auth/MfaVerificationPage.tsx | TOTP/SMS code entry          |
| CREATE | frontend/src/pages/auth/MfaSetupPage.tsx        | QR code and manual key       |
| CREATE | frontend/src/components/auth/TotpInput.tsx      | 6-digit code input component |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — code input validation, attempt counter
- [ ] Integration tests pass — MFA flow navigation

## Implementation Checklist

- [ ] Create MfaVerificationPage with 6-digit input and 30-second countdown (AC-01, AC-02)
- [ ] Create MfaSetupPage with QR code rendering and manual key display (AC-03)
- [ ] Implement 3-attempt lockout with session termination and error message (AC-04)
- [ ] Implement expired code handling with "Request new code" button (AC-05)
- [ ] Display deactivated account rejection message (AC-06)
- [ ] Connect MFA pages to backend verification endpoints (AC-01, AC-02)
