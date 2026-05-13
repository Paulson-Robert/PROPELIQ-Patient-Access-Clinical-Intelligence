# Task - TASK_001

## Requirement Reference

- **User Story:** US_041
- **Story Location:** .propel/context/tasks/EP-009/us_041/us_041.md
- **Acceptance Criteria:**
  - AC-01: Deterministic algorithm uses 4 input factors (no-show history, appointment gap, chronic conditions, insurance status)
  - AC-02: Score is reproducible given same inputs
  - AC-03: Score recalculated on relevant data changes
- **Edge Cases:**
  - Insufficient data — default to Medium risk with "data pending" flag

---

## Applicable Technology Stack

| Layer   | Technology   | Version | Justification         |
| ------- | ------------ | ------- | --------------------- |
| Backend | ASP.NET Core | 9.0     | TR-002 — risk scoring |

---

## Task Overview

Implement deterministic patient risk scoring algorithm using 4 weighted input factors, producing reproducible numeric scores recalculated on data changes.

## Dependent Tasks

- US_038 task (Backend Aggregation) — patient data inputs

## Impacted Components

- Infrastructure/Services/RiskScoringService.cs — scoring algorithm
- Application/EventHandlers/RecalculateRiskHandler.cs — trigger on data change

## Expected Changes

| Action | File Path                                               | Description   |
| ------ | ------------------------------------------------------- | ------------- |
| CREATE | src/Infrastructure/Services/RiskScoringService.cs       | Algorithm     |
| CREATE | src/Application/EventHandlers/RecalculateRiskHandler.cs | Recalculation |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — deterministic scoring, reproducibility

## Implementation Checklist

- [ ] Implement 4-factor weighted scoring algorithm (AC-01)
- [ ] Ensure reproducible scores given same inputs (AC-02)
- [ ] Trigger recalculation on relevant data changes (AC-03)
- [ ] Default to Medium risk with "data pending" on insufficient data (Edge Cases)
