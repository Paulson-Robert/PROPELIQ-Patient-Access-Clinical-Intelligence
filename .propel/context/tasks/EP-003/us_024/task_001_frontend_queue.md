# Task - TASK_001

## Requirement Reference

- **User Story:** US_024
- **Story Location:** .propel/context/tasks/EP-003/us_024/us_024.md
- **Acceptance Criteria:**
  - AC-01: Same-day queue displays patients ordered by arrival time
  - AC-02: "Mark Arrived" updates patient status with timestamp
  - AC-03: Drag-and-drop reordering by staff with reason capture
  - AC-04: Queue updates in real-time across all staff views
- **Edge Cases:**
  - Simultaneous reorder — last-write-wins with conflict notification

---

## Design References

| Reference Type         | Value                                                                  |
| ---------------------- | ---------------------------------------------------------------------- |
| **UI Impact**          | Yes                                                                    |
| **Wireframe Status**   | AVAILABLE                                                              |
| **Wireframe Type**     | HTML                                                                   |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-017-same-day-queue.html |
| **Screen Spec**        | SCR-017                                                                |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification           |
| ----------- | ------------ | -------------------- | ----------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — queue UI       |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — drag-and-drop |

---

## Task Overview

Implement same-day queue page (SCR-017) with arrival-ordered patient list, "Mark Arrived" action, drag-and-drop reordering with reason dialog, and real-time updates.

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/pages/queue/SameDayQueuePage.tsx — queue page
- frontend/src/components/queue/QueueItem.tsx — draggable queue item
- frontend/src/components/queue/ReorderReasonDialog.tsx — reason capture

## Implementation Plan

1. Create SameDayQueuePage with ordered patient list
2. Implement QueueItem with "Mark Arrived" button and drag handle
3. Implement drag-and-drop with dnd-kit or similar
4. Create ReorderReasonDialog capturing staff justification
5. Implement real-time updates via polling or SSE

## Expected Changes

| Action | File Path                                             | Description    |
| ------ | ----------------------------------------------------- | -------------- |
| CREATE | frontend/src/pages/queue/SameDayQueuePage.tsx         | Queue page     |
| CREATE | frontend/src/components/queue/QueueItem.tsx           | Queue item     |
| CREATE | frontend/src/components/queue/ReorderReasonDialog.tsx | Reorder reason |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — queue rendering, reorder logic

## Implementation Checklist

- [x] Create SameDayQueuePage with arrival-time ordered list (AC-01)
- [x] Implement "Mark Arrived" action updating status (AC-02)
- [x] Implement drag-and-drop reordering with reason capture (AC-03)
- [x] Implement real-time queue updates across staff views (AC-04)
