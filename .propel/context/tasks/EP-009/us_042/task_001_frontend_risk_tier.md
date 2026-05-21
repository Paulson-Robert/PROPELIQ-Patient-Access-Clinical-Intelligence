# Task - TASK_001

## Requirement Reference

- **User Story:** US_042
- **Story Location:** .propel/context/tasks/EP-009/us_042/us_042.md
- **Acceptance Criteria:**
  - AC-01: Risk tier badge shows Low (green), Medium (amber), High (red)
  - AC-02: Badge includes icon + text + color
  - AC-03: Displayed in queue view (SCR-017) and patient view (SCR-022)
- **Edge Cases:**
  - Score pending — grey badge with "Calculating..."

---

## Applicable Technology Stack

| Layer       | Technology   | Version              | Justification            |
| ----------- | ------------ | -------------------- | ------------------------ |
| Frontend    | React + Vite | React 18.x, Vite 5.x | TR-001 — badge component |
| Frontend UI | Shadcn UI    | Latest               | NFR-009 — badge variants |

---

## Task Overview

Implement risk tier badge component with color-coded Low/Medium/High display including icon and text, used in queue and patient views.

## Dependent Tasks

- US_024 task (Frontend Queue) — queue view integration
- US_038 task (Frontend Patient View) — patient view integration

## Impacted Components

- frontend/src/components/clinical/RiskTierBadge.tsx — badge component

## Expected Changes

| Action | File Path                                          | Description |
| ------ | -------------------------------------------------- | ----------- |
| CREATE | frontend/src/components/clinical/RiskTierBadge.tsx | Risk badge  |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — badge variants rendering

## Implementation Checklist

- [x] Create RiskTierBadge with Low/Medium/High color variants (AC-01)
- [x] Include icon + text + color per tier (AC-02)
- [x] Integrate in queue and patient views (AC-03)
- [x] Handle pending score with grey "Calculating..." badge (Edge Cases)
