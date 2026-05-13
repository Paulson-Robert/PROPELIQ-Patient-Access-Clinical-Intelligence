# Task - TASK_001

## Requirement Reference

- **User Story:** US_034
- **Story Location:** .propel/context/tasks/EP-006/us_034/us_034.md
- **Acceptance Criteria:**
  - AC-01: Document list shows retention period per document
  - AC-02: Patient can request document deletion
  - AC-03: Deletion confirmation with consequences warning
- **Edge Cases:**
  - Deletion of document used in aggregation — re-aggregate after removal

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification           |
| ----------- | ------------ | -------------------- | ----------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — document list  |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — list, dialogs |

---

## Task Overview

Implement document list showing retention info and patient-initiated deletion with confirmation dialog warning of consequences.

## Dependent Tasks

- US_032 task (Frontend Upload) — document context

## Impacted Components

- frontend/src/pages/documents/DocumentListPage.tsx — document list
- frontend/src/components/documents/DeleteConfirmDialog.tsx — deletion dialog

## Expected Changes

| Action | File Path                                                 | Description   |
| ------ | --------------------------------------------------------- | ------------- |
| CREATE | frontend/src/pages/documents/DocumentListPage.tsx         | Document list |
| CREATE | frontend/src/components/documents/DeleteConfirmDialog.tsx | Delete dialog |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — list rendering, dialog interaction

## Implementation Checklist

- [ ] Create document list with retention period display (AC-01)
- [ ] Implement patient deletion request action (AC-02)
- [ ] Create deletion confirmation dialog with consequences warning (AC-03)
