# Task - TASK_001

## Requirement Reference

- **User Story:** US_039
- **Story Location:** .propel/context/tasks/EP-008/us_039/us_039.md
- **Acceptance Criteria:**
  - AC-01: Conflicts highlighted with amber background
  - AC-02: Resolution UI allows selecting preferred value
  - AC-03: Resolution audit trail captured
- **Edge Cases:**
  - Multiple conflicts on same field — show all sources

---

## Design References

| Reference Type         | Value                                                                       |
| ---------------------- | --------------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                         |
| **Wireframe Status**   | AVAILABLE                                                                   |
| **Wireframe Type**     | HTML                                                                        |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-020-conflict-resolution.html |
| **Screen Spec**        | SCR-019, SCR-020, MOD-005                                                   |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification          |
| ----------- | ------------ | -------------------- | ---------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — conflict UI   |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — highlighting |

---

## Task Overview

Implement conflict detection UI with amber highlighting, resolution interface for selecting preferred values, and resolution dialog (MOD-005).

## Dependent Tasks

- US_038 task (Frontend Patient View) — patient view context

## Impacted Components

- frontend/src/components/clinical/ConflictHighlight.tsx — amber highlighting
- frontend/src/components/clinical/ConflictResolutionDialog.tsx — MOD-005

## Expected Changes

| Action | File Path                                                     | Description  |
| ------ | ------------------------------------------------------------- | ------------ |
| CREATE | frontend/src/components/clinical/ConflictHighlight.tsx        | Highlighting |
| CREATE | frontend/src/components/clinical/ConflictResolutionDialog.tsx | Resolution   |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — highlighting, resolution selection

## Implementation Checklist

- [x] Highlight conflicts with amber background (AC-01)
- [x] Create resolution UI for selecting preferred value (AC-02)
- [x] Capture resolution audit trail (AC-03)
- [x] Show all sources for multiple conflicts on same field (Edge Cases)
