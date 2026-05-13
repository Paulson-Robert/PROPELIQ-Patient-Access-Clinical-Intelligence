# Task - TASK_001

## Requirement Reference

- **User Story:** US_030
- **Story Location:** .propel/context/tasks/EP-005/us_030/us_030.md
- **Acceptance Criteria:**
  - AC-01: Multi-step form with inline validation
  - AC-02: Progress steps visible showing current position
  - AC-03: Save draft between steps
- **Edge Cases:**
  - Browser close during intake — draft preserved

---

## Design References

| Reference Type         | Value                                                                 |
| ---------------------- | --------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                   |
| **Wireframe Status**   | AVAILABLE                                                             |
| **Wireframe Type**     | HTML                                                                  |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-010-manual-intake.html |
| **Screen Spec**        | SCR-010                                                               |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification           |
| ----------- | ------------ | -------------------- | ----------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — form UI        |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — stepper, form |

---

## Task Overview

Implement manual intake multi-step form (SCR-010) with inline validation, step progress indicator, and draft saving.

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/pages/intake/ManualIntakePage.tsx — multi-step form
- frontend/src/components/intake/IntakeStepper.tsx — step indicator

## Implementation Plan

1. Create IntakeStepper showing step progress
2. Create ManualIntakePage with multi-step form and inline validation
3. Implement draft saving to localStorage/API between steps

## Expected Changes

| Action | File Path                                        | Description     |
| ------ | ------------------------------------------------ | --------------- |
| CREATE | frontend/src/pages/intake/ManualIntakePage.tsx   | Multi-step form |
| CREATE | frontend/src/components/intake/IntakeStepper.tsx | Step indicator  |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — form validation, step navigation

## Implementation Checklist

- [ ] Create multi-step form with inline validation (AC-01)
- [ ] Create IntakeStepper showing current position (AC-02)
- [ ] Implement draft saving between steps (AC-03)
- [ ] Preserve draft on browser close via localStorage (Edge Cases)
