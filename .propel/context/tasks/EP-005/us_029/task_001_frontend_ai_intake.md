# Task - TASK_001

## Requirement Reference

- **User Story:** US_029
- **Story Location:** .propel/context/tasks/EP-005/us_029/us_029.md
- **Acceptance Criteria:**
  - AC-01: Chat UI displays AI conversation with message bubbles
  - AC-02: Patient responses captured and validated
  - AC-03: Progress indicator shows intake completion percentage
  - AC-04: Summary review screen before submission
- **Edge Cases:**
  - AI unavailable — graceful fallback to manual form

---

## Design References

| Reference Type         | Value                                                             |
| ---------------------- | ----------------------------------------------------------------- |
| **UI Impact**          | Yes                                                               |
| **Wireframe Status**   | AVAILABLE                                                         |
| **Wireframe Type**     | HTML                                                              |
| **Wireframe Path/URL** | .propel/context/wireframes/Hi-Fi/wireframe-SCR-009-ai-intake.html |
| **Screen Spec**        | SCR-009                                                           |

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification             |
| ----------- | ------------ | -------------------- | ------------------------- |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — chat UI          |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — chat components |

---

## Task Overview

Implement AI conversational intake chat UI (SCR-009) with message bubbles, progress indicator, response capture, and summary review screen.

## Dependent Tasks

- US_001 task (Frontend Scaffold)

## Impacted Components

- frontend/src/pages/intake/AiIntakePage.tsx — chat page
- frontend/src/components/intake/ChatBubble.tsx — message bubbles
- frontend/src/components/intake/IntakeProgress.tsx — progress indicator
- frontend/src/components/intake/IntakeSummary.tsx — review screen

## Implementation Plan

1. Create ChatBubble component for AI and patient messages
2. Create AiIntakePage with streaming chat interface
3. Create IntakeProgress showing completion percentage
4. Create IntakeSummary for review before submission
5. Handle AI unavailability with fallback to manual form

## Expected Changes

| Action | File Path                                         | Description     |
| ------ | ------------------------------------------------- | --------------- |
| CREATE | frontend/src/pages/intake/AiIntakePage.tsx        | Chat page       |
| CREATE | frontend/src/components/intake/ChatBubble.tsx     | Message bubbles |
| CREATE | frontend/src/components/intake/IntakeProgress.tsx | Progress        |
| CREATE | frontend/src/components/intake/IntakeSummary.tsx  | Summary review  |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — chat rendering, progress calculation

## Implementation Checklist

- [x] Create ChatBubble with AI/patient message variants (AC-01)
- [x] Create AiIntakePage with streaming message display (AC-01)
- [x] Capture and validate patient responses (AC-02)
- [x] Create IntakeProgress showing completion percentage (AC-03)
- [x] Create IntakeSummary review screen before submission (AC-04)
- [x] Handle AI unavailability with graceful fallback (Edge Cases)
