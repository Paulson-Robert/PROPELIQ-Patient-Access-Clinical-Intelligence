# Task - TASK_001

## Requirement Reference

- **User Story:** US_037
- **Story Location:** .propel/context/tasks/EP-007/us_037/us_037.md
- **Acceptance Criteria:**
  - AC-01: Model versions tracked with deployment history
  - AC-02: Accuracy monitored against human-verified extractions
  - AC-03: Alert triggered when agreement rate drops below 98%
- **Edge Cases:**
  - Insufficient verification data — monitoring paused with warning

---

## Applicable Technology Stack

| Layer      | Technology   | Version | Justification               |
| ---------- | ------------ | ------- | --------------------------- |
| Backend    | ASP.NET Core | 9.0     | TR-002 — monitoring service |
| Background | Hangfire     | 1.8.x   | TR-005 — periodic checks    |

---

## Task Overview

Implement NER model versioning, accuracy monitoring comparing predictions to human verifications, and alerting when agreement rate drops below 98%.

## Dependent Tasks

- US_035 task (NER Pipeline) — model to monitor
- US_036 task (Confidence Scoring) — verified extractions

## Impacted Components

- Infrastructure/ML/ModelVersioningService.cs — version tracking
- Infrastructure/ML/ModelMonitoringService.cs — accuracy monitoring
- Infrastructure/Jobs/ModelAccuracyCheckJob.cs — periodic check

## Implementation Plan

1. Create ModelVersioningService tracking model versions and deployments
2. Create ModelMonitoringService comparing predictions to verifications
3. Create Hangfire job for periodic accuracy checks
4. Trigger alert when agreement < 98%

## Expected Changes

| Action | File Path                                        | Description    |
| ------ | ------------------------------------------------ | -------------- |
| CREATE | src/Infrastructure/ML/ModelVersioningService.cs  | Versioning     |
| CREATE | src/Infrastructure/ML/ModelMonitoringService.cs  | Monitoring     |
| CREATE | src/Infrastructure/Jobs/ModelAccuracyCheckJob.cs | Periodic check |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — accuracy calculation, alert threshold

## Implementation Checklist

- [x] Track model versions with deployment history (AC-01)
- [x] Monitor accuracy against human-verified extractions (AC-02)
- [x] Alert when agreement rate drops below 98% (AC-03)
- [x] Handle insufficient verification data gracefully (Edge Cases)
