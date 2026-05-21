# Task - TASK_002

## Requirement Reference

- **User Story:** US_040
- **Story Location:** .propel/context/tasks/EP-008/us_040/us_040.md
- **Acceptance Criteria:**
  - AC-01: Rule-based + ML.NET mapping of diagnoses to ICD-10/CPT codes
  - AC-02: Confidence score assigned to each mapping
  - AC-03: Verification endpoint persists staff decision (verify/modify/reject)
  - AC-04: Modification logged with reason for audit
- **Edge Cases:**
  - Ambiguous diagnosis — multiple code suggestions ranked by confidence

---

## Applicable Technology Stack

| Layer   | Technology          | Version | Justification            |
| ------- | ------------------- | ------- | ------------------------ |
| Backend | ASP.NET Core        | 9.0     | TR-002 — mapping service |
| AI/ML   | ML.NET + Rule-based | 4.x     | TR-012 — code mapping    |

---

## Task Overview

Implement ICD-10/CPT code mapping using rule-based + ML.NET approach, with confidence scoring, and staff verification endpoint.

## Dependent Tasks

- US_035 task (NER Pipeline) — extracted diagnoses
- US_036 task (Confidence Scoring) — scoring

## Impacted Components

- Infrastructure/ML/CodeMappingService.cs — mapping engine
- Infrastructure/ML/RuleBasedCodeMapper.cs — rule-based lookup
- Application/Commands/VerifyCodeMappingCommand.cs — staff verification

## Expected Changes

| Action | File Path                                            | Description    |
| ------ | ---------------------------------------------------- | -------------- |
| CREATE | src/Infrastructure/ML/CodeMappingService.cs          | Mapping engine |
| CREATE | src/Infrastructure/ML/RuleBasedCodeMapper.cs         | Rule-based     |
| CREATE | src/Application/Commands/VerifyCodeMappingCommand.cs | Verification   |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — mapping accuracy, verification persistence

## Implementation Checklist

- [x] Implement rule-based + ML.NET code mapping (AC-01)
- [x] Assign confidence score to each mapping (AC-02)
- [x] Create verification endpoint for verify/modify/reject (AC-03)
- [x] Log modifications with reason for audit (AC-04)
- [x] Return multiple ranked suggestions for ambiguous diagnoses (Edge Cases)
