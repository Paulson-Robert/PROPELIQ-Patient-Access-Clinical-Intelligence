# Task - TASK_001

## Requirement Reference

- **User Story:** US_050
- **Story Location:** .propel/context/tasks/EP-012/us_050/us_050.md
- **Acceptance Criteria:**
  - AC-01: Layout shell with header, sidebar (desktop), content area
  - AC-02: Bottom navigation on mobile
  - AC-03: Role-based menu items (Patient vs Staff vs Admin)
  - AC-04: Active page indicator in navigation
- **Edge Cases:**
  - Route protection — unauthorized nav items hidden

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification          |
| ----------- | ------------ | -------------------- | ---------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — layout        |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — sidebar, nav |

---

## Task Overview

Implement application layout shell with sidebar navigation (desktop), bottom nav (mobile), role-based menu configuration, and active page indication.

## Dependent Tasks

- US_049 task (Design System) — design tokens
- US_015 task (Session RBAC) — role context

## Impacted Components

- frontend/src/components/layout/AppLayout.tsx — main layout
- frontend/src/components/layout/Sidebar.tsx — desktop sidebar
- frontend/src/components/layout/BottomNav.tsx — mobile nav
- frontend/src/config/navigation.ts — role-based menu config

## Expected Changes

| Action | File Path                                    | Description  |
| ------ | -------------------------------------------- | ------------ |
| CREATE | frontend/src/components/layout/AppLayout.tsx | Layout shell |
| CREATE | frontend/src/components/layout/Sidebar.tsx   | Sidebar      |
| CREATE | frontend/src/components/layout/BottomNav.tsx | Mobile nav   |
| CREATE | frontend/src/config/navigation.ts            | Nav config   |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — role-based rendering, responsive behavior

## Implementation Checklist

- [ ] Create AppLayout with header, sidebar, and content area (AC-01)
- [ ] Create BottomNav for mobile breakpoint (AC-02)
- [ ] Configure role-based menu items (AC-03)
- [ ] Implement active page indicator (AC-04)
- [ ] Hide unauthorized nav items (Edge Cases)
