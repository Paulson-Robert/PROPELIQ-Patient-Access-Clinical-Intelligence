# Task - TASK_002

## Requirement Reference

- **User Story:** US_032
- **Story Location:** .propel/context/tasks/EP-006/us_032/us_032.md
- **Acceptance Criteria:**
  - AC-01: Upload endpoint accepts multipart/form-data
  - AC-02: Server-side format and size validation
  - AC-03: Files stored with metadata (patient, upload date, type)
  - AC-04: Upload triggers malware scan pipeline
- **Edge Cases:**
  - Storage failure — return error, no partial state

---

## Applicable Technology Stack

| Layer   | Technology            | Version | Justification                 |
| ------- | --------------------- | ------- | ----------------------------- |
| Backend | ASP.NET Core          | 9.0     | TR-002 — upload endpoint      |
| ORM     | Entity Framework Core | 9.0     | TR-003 — metadata persistence |

---

## Task Overview

Implement document upload endpoint with server-side validation, file storage with metadata, and malware scan pipeline trigger.

## Dependent Tasks

- US_003 task (Database ORM)
- US_033 task (Malware Pipeline) — scan trigger

## Impacted Components

- Application/Commands/UploadDocumentCommand.cs — upload handling
- Infrastructure/Storage/DocumentStorageService.cs — file storage

## Expected Changes

| Action | File Path                                            | Description |
| ------ | ---------------------------------------------------- | ----------- |
| CREATE | src/Application/Commands/UploadDocumentCommand.cs    | Upload      |
| CREATE | src/Infrastructure/Storage/DocumentStorageService.cs | Storage     |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — validation, metadata persistence

## Implementation Checklist

- [ ] Create upload endpoint accepting multipart/form-data (AC-01)
- [ ] Implement server-side format and size validation (AC-02)
- [ ] Store files with metadata (patient, date, type) (AC-03)
- [ ] Trigger malware scan pipeline after upload (AC-04)
