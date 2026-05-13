# Task - TASK_001

## Requirement Reference

- **User Story:** US_035
- **Story Location:** .propel/context/tasks/EP-007/us_035/us_035.md
- **Acceptance Criteria:**
  - AC-01: ML.NET NER model extracts entities (names, dates, diagnoses, medications)
  - AC-02: Extraction accuracy ≥ 95% on test dataset
  - AC-03: Model retrainable with new labeled data
- **Edge Cases:**
  - Low-confidence extraction — flagged for human review

---

## Applicable Technology Stack

| Layer   | Technology   | Version | Justification         |
| ------- | ------------ | ------- | --------------------- |
| Backend | ASP.NET Core | 9.0     | TR-002 — NER pipeline |
| AI/ML   | ML.NET       | 4.x     | TR-010 — custom NER   |

---

## Task Overview

Implement ML.NET custom NER model for clinical entity extraction (names, dates, diagnoses, medications, procedures) with training pipeline and prediction service.

## Dependent Tasks

- US_033 task (Malware Pipeline) — clean files feed NER

## Impacted Components

- Infrastructure/ML/NerModelService.cs — prediction service
- Infrastructure/ML/NerTrainingPipeline.cs — model training
- Infrastructure/ML/Models/ClinicalEntity.cs — entity model

## Implementation Plan

1. Define ClinicalEntity model with entity types and spans
2. Create NerTrainingPipeline for model training with labeled data
3. Create NerModelService loading trained model and running predictions
4. Flag low-confidence extractions for human review
5. Implement model versioning for retraining

## Expected Changes

| Action | File Path                                      | Description  |
| ------ | ---------------------------------------------- | ------------ |
| CREATE | src/Infrastructure/ML/NerModelService.cs       | Prediction   |
| CREATE | src/Infrastructure/ML/NerTrainingPipeline.cs   | Training     |
| CREATE | src/Infrastructure/ML/Models/ClinicalEntity.cs | Entity model |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — entity extraction accuracy ≥ 95%

## Implementation Checklist

- [ ] Create ClinicalEntity model with entity types (AC-01)
- [ ] Implement NER prediction service with ML.NET (AC-01)
- [ ] Achieve ≥ 95% accuracy on test dataset (AC-02)
- [ ] Create retraining pipeline accepting new labeled data (AC-03)
- [ ] Flag low-confidence extractions for review (Edge Cases)
