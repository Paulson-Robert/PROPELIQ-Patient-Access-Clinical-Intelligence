# Task - TASK_001

## Requirement Reference

- **User Story:** US_036
- **Story Location:** .propel/context/tasks/EP-007/us_036/us_036.md
- **Acceptance Criteria:**
  - AC-01: Each extracted entity assigned confidence score 0.0-1.0
  - AC-02: Configurable thresholds (auto-accept ≥ 0.9, review 0.7-0.9, reject < 0.7)
  - AC-03: PHI boundary enforcement — extraction never crosses patient records
- **Edge Cases:**
  - Threshold configuration change — applies to new extractions only

---

## Applicable Technology Stack

| Layer   | Technology   | Version | Justification              |
| ------- | ------------ | ------- | -------------------------- |
| Backend | ASP.NET Core | 9.0     | TR-002 — scoring service   |
| AI/ML   | ML.NET       | 4.x     | TR-010 — confidence output |

---

## Task Overview

Implement confidence scoring for NER extractions with configurable thresholds for auto-accept/review/reject decisions and PHI boundary enforcement.

## Dependent Tasks

- US_035 task (NER Pipeline) — entity extraction

## Impacted Components

- Infrastructure/ML/ConfidenceScoringService.cs — scoring logic
- Application/Configuration/ExtractionThresholdConfig.cs — configurable thresholds

## Implementation Plan

1. Extract confidence scores from ML.NET prediction output
2. Create configurable threshold configuration
3. Apply threshold logic: auto-accept, queue for review, reject
4. Enforce PHI boundaries — no cross-patient entity extraction

## Expected Changes

| Action | File Path                                                  | Description |
| ------ | ---------------------------------------------------------- | ----------- |
| CREATE | src/Infrastructure/ML/ConfidenceScoringService.cs          | Scoring     |
| CREATE | src/Application/Configuration/ExtractionThresholdConfig.cs | Config      |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — scoring, threshold logic, PHI boundaries

## Implementation Checklist

- [ ] Assign confidence score 0.0-1.0 to each extraction (AC-01)
- [ ] Implement configurable thresholds with auto-accept/review/reject (AC-02)
- [ ] Enforce PHI boundary — no cross-patient extraction (AC-03)
