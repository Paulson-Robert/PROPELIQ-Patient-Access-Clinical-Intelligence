# Task - TASK_001

## Requirement Reference

- **User Story:** US_040
- **Story Location:** .propel/context/tasks/EP-008/us_040/us_040.md
- **Acceptance Criteria:**
  - AC-01: ICD-10/CPT codes displayed with confidence indicators
  - AC-02: Staff can verify, modify, or reject suggested codes
  - AC-03: Modified codes logged with reason
- **Edge Cases:**
  - No code suggestion — "Unable to map" shown with manual entry

---

## Design References

| Reference Type         | Value                                                                |
| ---------------------- | -------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                  |
| **Wireframe Status**   | AVAILABLE                                                            |
| **Wireframe Type**     | HTML                                                                 |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-021-code-mapping.html |
| **Screen Spec**        | SCR-021                                                              |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification            |
| ----------- | ------------ | -------------------- | ------------------------ |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — code mapping UI |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — data tables    |

---

## Task Overview

Implement code mapping UI (SCR-021) displaying ICD-10/CPT suggestions with confidence indicators and verify/modify/reject actions.

## Dependent Tasks

- US_038 task (Frontend Patient View) — patient context

## Impacted Components

- frontend/src/pages/clinical/CodeMappingPage.tsx — code mapping
- frontend/src/components/clinical/CodeSuggestionRow.tsx — suggestion row
- frontend/src/components/clinical/ConfidenceIndicator.tsx — confidence display

## Expected Changes

| Action | File Path                                                | Description  |
| ------ | -------------------------------------------------------- | ------------ |
| CREATE | frontend/src/pages/clinical/CodeMappingPage.tsx          | Code mapping |
| CREATE | frontend/src/components/clinical/CodeSuggestionRow.tsx   | Row          |
| CREATE | frontend/src/components/clinical/ConfidenceIndicator.tsx | Indicator    |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — rendering, action handling

## Implementation Checklist

- [x] Display ICD-10/CPT codes with confidence indicators (AC-01)
- [x] Implement verify/modify/reject actions per suggestion (AC-02)
- [x] Capture modification reason (AC-03)
- [x] Show "Unable to map" with manual entry option (Edge Cases)
