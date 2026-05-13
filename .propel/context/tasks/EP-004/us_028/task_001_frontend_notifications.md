# Task - TASK_001

## Requirement Reference

- **User Story:** US_028
- **Story Location:** .propel/context/tasks/EP-004/us_028/us_028.md
- **Acceptance Criteria:**
  - AC-01: Toast notifications appear top-right for staff events
  - AC-02: Auto-dismiss after 5 seconds with manual dismiss
  - AC-03: Multiple toasts stack vertically
  - AC-04: Notification history accessible from bell icon
- **Edge Cases:**
  - 10+ simultaneous notifications — show count badge, stack limit

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification             |
| ----------- | ------------ | -------------------- | ------------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — notification UI  |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — toast component |

---

## Task Overview

Implement staff toast notification system with auto-dismiss, stacking, and notification history panel accessible from header bell icon.

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/components/notifications/ToastContainer.tsx — toast manager
- frontend/src/components/notifications/NotificationHistory.tsx — history panel
- frontend/src/hooks/useNotifications.ts — notification state

## Implementation Plan

1. Create useNotifications hook managing notification queue
2. Create ToastContainer with stacking and auto-dismiss
3. Create NotificationHistory panel with bell icon trigger
4. Connect to backend notification events

## Expected Changes

| Action | File Path                                                     | Description       |
| ------ | ------------------------------------------------------------- | ----------------- |
| CREATE | frontend/src/components/notifications/ToastContainer.tsx      | Toast manager     |
| CREATE | frontend/src/components/notifications/NotificationHistory.tsx | History panel     |
| CREATE | frontend/src/hooks/useNotifications.ts                        | Notification hook |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — toast rendering, auto-dismiss, stacking

## Implementation Checklist

- [ ] Create ToastContainer appearing top-right (AC-01)
- [ ] Implement 5-second auto-dismiss with manual dismiss button (AC-02)
- [ ] Implement vertical stacking for multiple toasts (AC-03)
- [ ] Create NotificationHistory panel with bell icon trigger (AC-04)
- [ ] Handle 10+ simultaneous notifications with count badge (Edge Cases)
