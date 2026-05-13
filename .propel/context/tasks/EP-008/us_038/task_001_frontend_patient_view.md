# Task - TASK_001

## Requirement Reference

- **User Story:** US_038
- **Story Location:** .propel/context/tasks/EP-008/us_038/us_038.md
- **Acceptance Criteria:**
  - AC-01: 360° patient view displays aggregated data from all sources
  - AC-02: Human verification badges show verified/unverified status
  - AC-03: Data grouped by category (demographics, diagnoses, medications)
- **Edge Cases:**
  - No data available — "No records found" per category

---

## Design References

| Reference Type         | Value                                                               |
| ---------------------- | ------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                 |
| **Wireframe Status**   | AVAILABLE                                                           |
| **Wireframe Type**     | HTML                                                                |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-019-patient-360.html |
| **Screen Spec**        | SCR-019                                                             |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification           |
| ----------- | ------------ | -------------------- | ----------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — patient view   |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — cards, badges |

---

## Task Overview

Implement 360° patient view page (SCR-019) displaying aggregated data grouped by category with verification status badges.

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/pages/clinical/PatientViewPage.tsx — 360° view
- frontend/src/components/clinical/DataCategoryCard.tsx — category groups
- frontend/src/components/clinical/VerificationBadge.tsx — status badge

## Expected Changes

| Action | File Path                                              | Description   |
| ------ | ------------------------------------------------------ | ------------- |
| CREATE | frontend/src/pages/clinical/PatientViewPage.tsx        | Patient view  |
| CREATE | frontend/src/components/clinical/DataCategoryCard.tsx  | Category card |
| CREATE | frontend/src/components/clinical/VerificationBadge.tsx | Badge         |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — rendering, category grouping

## Implementation Checklist

- [ ] Create PatientViewPage with 360° aggregated data display (AC-01)
- [ ] Create VerificationBadge showing verified/unverified status (AC-02)
- [ ] Group data by category (demographics, diagnoses, medications) (AC-03)
- [ ] Handle empty categories with "No records found" (Edge Cases)
