# Task - TASK_001

## Requirement Reference

- **User Story:** US_013
- **Story Location:** .propel/context/tasks/EP-001/us_013/us_013.md
- **Acceptance Criteria:**
  - AC-01: Google social login OAuth flow renders and completes
  - AC-02: Microsoft social login OAuth flow renders and completes
  - AC-03: Email/password registration form with validation
  - AC-04: Email verification activation page (SCR-028)
  - AC-05: Social login denied consent shows alternative
  - AC-06: Duplicate email prevents registration with guidance
- **Edge Cases:**
  - Expired verification link — "Resend verification" option
  - Social login email mismatch — account linking
  - OAuth provider outage — disabled button with tooltip

---

## Design References

| Reference Type         | Value                                                                      |
| ---------------------- | -------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                        |
| **Wireframe Status**   | AVAILABLE                                                                  |
| **Wireframe Type**     | HTML                                                                       |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-001-login-registration.html |
| **Screen Spec**        | SCR-001, SCR-028                                                           |
| **UXR Requirements**   | UXR-605                                                                    |

---

## Applicable Technology Stack

| Layer          | Technology                 | Version              | Justification                        |
| -------------- | -------------------------- | -------------------- | ------------------------------------ |
| Frontend       | React + Vite               | React 18.x, Vite 5.x | TR-001 — UI rendering                |
| Frontend UI    | Shadcn UI                  | Latest               | NFR-009 — accessible form components |
| Frontend State | React Context + useReducer | Built-in             | Auth state management                |

---

## Task Overview

Implement the login/registration page (SCR-001) with Google and Microsoft social login buttons, email/password registration form with inline validation, and the email verification page (SCR-028) with success/failure/expired states.

## Dependent Tasks

- US_001 task (Frontend Scaffold) — React project must exist
- US_049 task (Design System) — component library for form styling

## Impacted Components

- frontend/src/pages/auth/LoginPage.tsx — login/registration page
- frontend/src/pages/auth/EmailVerificationPage.tsx — verification page
- frontend/src/components/auth/SocialLoginButtons.tsx — Google/Microsoft OAuth buttons
- frontend/src/components/auth/RegistrationForm.tsx — email/password form
- frontend/src/hooks/useAuth.ts — authentication hook

## Implementation Plan

1. Create LoginPage with toggle between login and register modes
2. Implement SocialLoginButtons initiating OAuth redirect flows
3. Implement RegistrationForm with password complexity validation
4. Create EmailVerificationPage with success/failure/expired states
5. Implement duplicate email detection with guidance message
6. Handle OAuth consent denial with fallback UI
7. Implement session redirect after successful login
8. Connect to backend auth API endpoints

## Current Project State

- Frontend scaffold exists (US_001)
- No auth pages exist

## Expected Changes

| Action | File Path                                           | Description             |
| ------ | --------------------------------------------------- | ----------------------- |
| CREATE | frontend/src/pages/auth/LoginPage.tsx               | Login/registration page |
| CREATE | frontend/src/pages/auth/EmailVerificationPage.tsx   | Verification states     |
| CREATE | frontend/src/components/auth/SocialLoginButtons.tsx | OAuth buttons           |
| CREATE | frontend/src/components/auth/RegistrationForm.tsx   | Registration form       |
| CREATE | frontend/src/hooks/useAuth.ts                       | Auth state hook         |
| CREATE | frontend/src/services/authApi.ts                    | Auth API client         |

## External References

- [React OAuth2 Google](https://github.com/MomenSher662/react-oauth)
- [MSAL React](https://github.com/AzureAD/microsoft-authentication-library-for-js)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — form validation, OAuth button rendering
- [x] Integration tests pass — login/register flow navigates correctly

## Implementation Checklist

- [x] Create LoginPage with login/register mode toggle matching SCR-001 wireframe (AC-01, AC-02, AC-03)
- [x] Implement Google and Microsoft social login buttons with OAuth redirect (AC-01, AC-02)
- [x] Implement registration form with inline password complexity validation (AC-03)
- [x] Create EmailVerificationPage with success/failure/expired states (AC-04)
- [x] Handle OAuth consent denial with fallback to email registration (AC-05)
- [x] Implement duplicate email detection with login/reset guidance (AC-06)
- [x] Implement post-login redirect to role-specific dashboard (AC-01, AC-02)
- [x] Handle OAuth provider outage with disabled buttons and tooltip (Edge Cases)
