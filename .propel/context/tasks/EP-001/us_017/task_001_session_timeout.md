# Task - TASK_001

## Requirement Reference

- **User Story:** US_017
- **Story Location:** .propel/context/tasks/EP-001/us_017/us_017.md
- **Acceptance Criteria:**
  - AC-01: Timeout warning dialog appears at 2-minute mark with countdown
  - AC-02: "Extend Session" resets sliding expiry by 15 minutes
  - AC-03: Session expiry redirects to login with toast message
  - AC-04: Post-login redirect returns user to previous page
  - AC-05: "Log Out" button ends session cleanly without expired toast
- **Edge Cases:**
  - Multiple browser tabs — warning appears in all tabs
  - Background tab — dialog appears on tab focus
  - Network disconnection during extend — error message shown

---

## Design References

| Reference Type         | Value                                                                   |
| ---------------------- | ----------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                     |
| **Wireframe Status**   | AVAILABLE                                                               |
| **Wireframe Type**     | HTML                                                                    |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-MOD-001-session-timeout.html |
| **Screen Spec**        | MOD-001                                                                 |
| **UXR Requirements**   | UXR-004, UXR-605                                                        |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification              |
| ----------- | ------------ | -------------------- | -------------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — session UI        |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — dialog component |

---

## Task Overview

Implement the session timeout warning dialog (MOD-001) with countdown timer, extend/logout actions, automatic redirect on expiry with return URL preservation, and multi-tab synchronization.

## Dependent Tasks

- US_015 task (Session RBAC) — sliding expiry backend must exist

## Impacted Components

- frontend/src/components/session/SessionTimeoutDialog.tsx — warning modal
- frontend/src/hooks/useSessionTimer.ts — session countdown logic
- frontend/src/context/AuthContext.tsx — session state management

## Implementation Plan

1. Create useSessionTimer hook tracking remaining session time
2. Create SessionTimeoutDialog with countdown, extend, and logout buttons
3. Implement session extension API call on "Extend Session" click
4. Implement redirect to login with "session expired" toast on timeout
5. Preserve return URL in login redirect for post-login navigation
6. Handle multi-tab synchronization via BroadcastChannel API

## Current Project State

- Auth context exists (US_013)
- Session RBAC exists (US_015)
- No timeout warning exists

## Expected Changes

| Action | File Path                                                | Description                   |
| ------ | -------------------------------------------------------- | ----------------------------- |
| CREATE | frontend/src/components/session/SessionTimeoutDialog.tsx | Timeout warning modal         |
| CREATE | frontend/src/hooks/useSessionTimer.ts                    | Session countdown hook        |
| MODIFY | frontend/src/context/AuthContext.tsx                     | Add session timer integration |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — timer logic, dialog rendering
- [ ] Integration tests pass — extend resets timer, expiry redirects

## Implementation Checklist

- [ ] Create useSessionTimer hook with 2-minute warning threshold (AC-01)
- [ ] Create SessionTimeoutDialog with countdown timer and action buttons (AC-01)
- [ ] Implement "Extend Session" calling backend to reset 15-minute expiry (AC-02)
- [ ] Implement automatic redirect to login with toast on session expiry (AC-03)
- [ ] Preserve return URL for post-login redirect navigation (AC-04)
- [ ] Implement clean logout clearing session without expired toast (AC-05)
