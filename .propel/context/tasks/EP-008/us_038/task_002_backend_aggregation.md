# Task - TASK_002

## Requirement Reference

- **User Story:** US_038
- **Story Location:** .propel/context/tasks/EP-008/us_038/us_038.md
- **Acceptance Criteria:**
  - AC-01: Aggregation service merges data from NER, intake, and external sources
  - AC-02: De-duplication identifies and merges duplicate records
  - AC-03: Human verification status tracked per data point
  - AC-04: Re-aggregation triggered on new document or deletion
- **Edge Cases:**
  - Conflicting data from multiple sources — flagged for resolution

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification        |
| ------- | --------------------- | ------- | -------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — aggregation |
| ORM     | Entity Framework Core | 9.0     | TR-003 — persistence |

---

## Task Overview

Implement patient data aggregation service merging NER extractions, intake data, and external sources with de-duplication and verification tracking.

## Dependent Tasks

- US_035 task (NER Pipeline) — extracted entities
- US_029/030 tasks (Intake) — intake data

## Impacted Components

- Application/Commands/AggregatePatientDataCommand.cs — aggregation
- Infrastructure/Services/DataDeduplicationService.cs — dedup
- Infrastructure/Services/VerificationTrackingService.cs — verification

## Expected Changes

| Action | File Path                                                  | Description  |
| ------ | ---------------------------------------------------------- | ------------ |
| CREATE | src/Application/Commands/AggregatePatientDataCommand.cs    | Aggregation  |
| CREATE | src/Infrastructure/Services/DataDeduplicationService.cs    | Dedup        |
| CREATE | src/Infrastructure/Services/VerificationTrackingService.cs | Verification |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — merging, deduplication, verification tracking

## Implementation Checklist

- [x] Merge data from NER, intake, and external sources (AC-01)
- [x] Implement de-duplication identifying and merging duplicates (AC-02)
- [x] Track human verification status per data point (AC-03)
- [x] Trigger re-aggregation on new document or deletion (AC-04)
- [x] Flag conflicting data for resolution (Edge Cases)
