# Task - TASK_002

## Requirement Reference

- **User Story:** US_034
- **Story Location:** .propel/context/tasks/EP-006/us_034/us_034.md
- **Acceptance Criteria:**
  - AC-01: Document deletion removes file and metadata
  - AC-02: Deletion triggers re-aggregation of patient data
  - AC-03: Retention policy enforced via scheduled cleanup job
- **Edge Cases:**
  - Deletion during NER processing — cancel processing first

---

## Applicable Technology Stack

| Layer      | Technology   | Version | Justification              |
| ---------- | ------------ | ------- | -------------------------- |
| Backend    | ASP.NET Core | 9.0     | TR-002 — deletion API      |
| Background | Hangfire     | 1.8.x   | TR-005 — retention cleanup |

---

## Task Overview

Implement document deletion removing files and metadata, triggering patient data re-aggregation, and scheduled retention policy enforcement.

## Dependent Tasks

- US_032 task (Backend Upload) — document storage
- US_038 task (Backend Aggregation) — re-aggregation trigger

## Impacted Components

- Application/Commands/DeleteDocumentCommand.cs — deletion
- Infrastructure/Jobs/RetentionCleanupJob.cs — scheduled cleanup

## Expected Changes

| Action | File Path                                         | Description   |
| ------ | ------------------------------------------------- | ------------- |
| CREATE | src/Application/Commands/DeleteDocumentCommand.cs | Deletion      |
| CREATE | src/Infrastructure/Jobs/RetentionCleanupJob.cs    | Retention job |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — deletion, re-aggregation trigger

## Implementation Checklist

- [ ] Delete file and metadata on patient request (AC-01)
- [ ] Trigger re-aggregation after document deletion (AC-02)
- [ ] Create Hangfire retention cleanup job (AC-03)
- [ ] Cancel active NER processing before deletion (Edge Cases)
