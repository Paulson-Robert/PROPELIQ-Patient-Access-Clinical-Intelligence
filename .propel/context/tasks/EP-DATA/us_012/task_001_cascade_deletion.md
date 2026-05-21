# Task - TASK_001

## Requirement Reference

- **User Story:** US_012
- **Story Location:** .propel/context/tasks/EP-DATA/us_012/us_012.md
- **Acceptance Criteria:**
  - AC-01: Cascade deletion removes all patient data from PostgreSQL
  - AC-02: Cascade deletion clears patient data from Redis cache
  - AC-03: Deletion is irreversible and audit-logged
- **Edge Cases:**
  - Deletion of patient with no data — completes successfully
  - Concurrent deletion and data insertion — wait for transaction completion
  - Audit log references to deleted patient — retain patient ID only

---

## Applicable Technology Stack

| Layer    | Technology            | Version    | Justification                   |
| -------- | --------------------- | ---------- | ------------------------------- |
| Backend  | ASP.NET Core          | 9.0        | TR-002 — API and service layer  |
| Database | PostgreSQL + pgcrypto | 16.x       | DR-007 — HIPAA right-to-delete  |
| Cache    | Upstash Redis         | Serverless | DR-002 — cached data cleanup    |
| ORM      | Entity Framework Core | 9.0        | TR-004 — transaction management |

---

## Task Overview

Implement a complete cascade deletion mechanism that permanently removes all patient data across PostgreSQL (PatientProfile, ClinicalDocument, ExtractedDataRecord, DataConflict, PatientView, MedicalCodeMapping, IntakeRecord, Appointment, Notification, CalendarSync, NoShowRiskFactor, PreferredSlotQueue) and Redis (sessions, cached data) with immutable audit logging.

## Dependent Tasks

- US_008 task (Core Database Schema) — all entity schemas must exist
- US_009 task (Redis Integration) — Redis integration must exist for cache deletion

## Impacted Components

- Application/Commands/DeletePatientDataCommand.cs — CQRS command
- Application/Handlers/DeletePatientDataCommandHandler.cs — deletion orchestration
- Infrastructure/Services/PatientDataDeletionService.cs — cascade deletion logic
- Application/Interfaces/IPatientDataDeletionService.cs — abstraction interface

## Implementation Plan

1. Create IPatientDataDeletionService interface in Application layer
2. Implement PatientDataDeletionService with ordered cascade deletion across all 12 related entities
3. Implement Redis key pattern deletion for patient session and cached data
4. Create DeletePatientDataCommand and handler via MediatR
5. Ensure audit log entry is created BEFORE deletion executes (to capture patient ID)
6. Wrap all deletions in a database transaction for atomicity

## Current Project State

- All entity schemas exist (US_008)
- Redis integration exists (US_009)
- No deletion mechanism implemented

## Expected Changes

| Action | File Path                                                   | Description                     |
| ------ | ----------------------------------------------------------- | ------------------------------- |
| CREATE | src/Application/Interfaces/IPatientDataDeletionService.cs   | Deletion service interface      |
| CREATE | src/Application/Commands/DeletePatientDataCommand.cs        | CQRS command                    |
| CREATE | src/Application/Handlers/DeletePatientDataCommandHandler.cs | Deletion handler                |
| CREATE | src/Infrastructure/Services/PatientDataDeletionService.cs   | Cascade deletion implementation |

## External References

- [EF Core Cascade Delete](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — deletion ordering, Redis cleanup
- [x] Integration tests pass — full cascade deletion leaves zero orphaned records

## Implementation Checklist

- [x] Create deletion service interface and MediatR command/handler (AC-01)
- [x] Implement ordered cascade deletion across all 12 patient-related entities in PostgreSQL (AC-01)
- [x] Implement Redis key pattern deletion for patient sessions and cached data (AC-02)
- [x] Create immutable audit log entry recording deletion actor, timestamp, patient ID, and deleted resource summary (AC-03)
- [x] Wrap all deletions in a database transaction for atomicity (AC-01)
- [x] Handle deletion of patient with no data gracefully (Edge Cases)
