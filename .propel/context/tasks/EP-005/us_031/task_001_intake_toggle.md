# Task - TASK_001

## Requirement Reference

- **User Story:** US_031
- **Story Location:** .propel/context/tasks/EP-005/us_031/us_031.md
- **Acceptance Criteria:**
  - AC-01: Toggle switch between AI and manual intake modes
  - AC-02: Data preserved when switching modes
  - AC-03: Auto-fallback to manual when AI unavailable
- **Edge Cases:**
  - Partial AI data mapped to manual form fields on switch

---

## Applicable Technology Stack

| Layer    | Technology   | Version              | Justification      |
| -------- | ------------ | -------------------- | ------------------ |
| Frontend | React + Vite | React 18.x, Vite 5.x | TR-001 — toggle UI |

---

## Task Overview

Implement intake mode toggle between AI and manual, preserving data across switches, with automatic fallback to manual on AI unavailability.

## Dependent Tasks

- US_029 task (Frontend AI Intake)
- US_030 task (Frontend Manual Intake)

## Impacted Components

- frontend/src/pages/intake/IntakePage.tsx — parent with toggle
- frontend/src/hooks/useIntakeMode.ts — mode state and data mapping

## Implementation Plan

1. Create IntakePage with mode toggle controlling which child renders
2. Create useIntakeMode hook managing mode state and data preservation
3. Map AI-collected data to manual form fields on switch
4. Detect AI unavailability and auto-switch to manual

## Expected Changes

| Action | File Path                                | Description   |
| ------ | ---------------------------------------- | ------------- |
| CREATE | frontend/src/pages/intake/IntakePage.tsx | Toggle parent |
| CREATE | frontend/src/hooks/useIntakeMode.ts      | Mode hook     |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — toggle logic, data preservation

## Implementation Checklist

- [x] Create toggle switch between AI and manual modes (AC-01)
- [x] Preserve collected data when switching modes (AC-02)
- [x] Auto-fallback to manual when AI unavailable (AC-03)
- [x] Map partial AI data to manual form fields (Edge Cases)
