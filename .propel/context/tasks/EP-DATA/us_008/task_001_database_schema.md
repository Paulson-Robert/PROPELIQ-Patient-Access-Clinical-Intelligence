# Task - TASK_001

## Requirement Reference

- **User Story:** US_008
- **Story Location:** .propel/context/tasks/EP-DATA/us_008/us_008.md
- **Acceptance Criteria:**
  - AC-01: EF Core DbContext registers all domain entities and generates migration successfully
  - AC-02: PHI columns use pgcrypto field-level encryption (AES-256)
  - AC-03: JSONB columns configured for PatientView aggregated data
  - AC-04: Entity relationships and foreign keys are enforced
  - AC-05: Data conflict tracking schema supports multi-document conflicts
- **Edge Cases:**
  - pgcrypto extension not installed — migration fails with clear error
  - JSONB null handling — null values accepted without serialization errors
  - Concurrent migration execution — EF Core migration lock prevents duplicates

---

## Applicable Technology Stack

| Layer    | Technology            | Version | Justification                                                     |
| -------- | --------------------- | ------- | ----------------------------------------------------------------- |
| Database | PostgreSQL + pgcrypto | 16.x    | DR-001 — primary relational store with field-level PHI encryption |
| ORM      | Entity Framework Core | 9.0     | DR-001, TR-004 — hybrid migration strategy                        |
| Backend  | ASP.NET Core          | 9.0     | TR-002 — required for EF Core integration                         |

---

## Task Overview

Define the complete EF Core entity configurations for all 16 domain entities (User, PatientProfile, Appointment, AvailabilitySlot, PreferredSlotQueue, ClinicalDocument, ExtractedDataRecord, DataConflict, PatientView, MedicalCodeMapping, IntakeRecord, Notification, CalendarSync, AuditLog, NoShowRiskFactor, InsuranceRecord), configure pgcrypto symmetric encryption on PHI columns, set up JSONB column mappings, and establish all foreign key relationships.

## Dependent Tasks

- US_003 task (Database & ORM Config) — EF Core and PostgreSQL connection must be configured before schema can be defined

## Impacted Components

- Infrastructure/Data/ApplicationDbContext.cs — DbContext with DbSet registrations
- Infrastructure/Data/Configurations/ — entity configuration classes (IEntityTypeConfiguration<T>)
- Infrastructure/Migrations/ — generated migration files
- Infrastructure/Data/Extensions/PgCryptoExtensions.cs — pgcrypto encryption helpers

## Implementation Plan

1. Create domain entity classes in Domain/Entities/ for all 16 entities with properties matching the design document
2. Create IEntityTypeConfiguration<T> classes for each entity in Infrastructure/Data/Configurations/
3. Configure PHI column encryption using pgcrypto value converters for PatientProfile (FirstName, LastName, DateOfBirth, Phone)
4. Configure JSONB column mappings for PatientView aggregated fields (AggregatedVitals, AggregatedMedications, AggregatedAllergies, AggregatedDiagnoses, AggregatedProcedures)
5. Define foreign key relationships, indexes, and constraints across all entities
6. Configure DataConflict entity to support multi-document conflict tracking with dual source document references
7. Generate and validate the initial schema migration
8. Ensure pgcrypto extension is created in migration via `migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto")`

## Current Project State

- Backend solution structure exists (US_002)
- EF Core configured with PostgreSQL connection (US_003)
- No entity configurations exist yet

## Expected Changes

| Action | File Path                                                | Description                               |
| ------ | -------------------------------------------------------- | ----------------------------------------- |
| CREATE | src/Domain/Entities/User.cs                              | User entity with role, status, MFA fields |
| CREATE | src/Domain/Entities/PatientProfile.cs                    | Patient profile with PHI fields           |
| CREATE | src/Domain/Entities/Appointment.cs                       | Appointment with status lifecycle         |
| CREATE | src/Domain/Entities/AvailabilitySlot.cs                  | Provider availability slots               |
| CREATE | src/Domain/Entities/PreferredSlotQueue.cs                | Preferred slot swap queue                 |
| CREATE | src/Domain/Entities/ClinicalDocument.cs                  | Clinical document metadata                |
| CREATE | src/Domain/Entities/ExtractedDataRecord.cs               | NER extraction results                    |
| CREATE | src/Domain/Entities/DataConflict.cs                      | Multi-document conflict tracking          |
| CREATE | src/Domain/Entities/PatientView.cs                       | 360-degree aggregated view                |
| CREATE | src/Domain/Entities/MedicalCodeMapping.cs                | ICD-10/CPT code mappings                  |
| CREATE | src/Domain/Entities/IntakeRecord.cs                      | Patient intake data                       |
| CREATE | src/Domain/Entities/Notification.cs                      | Notification delivery log                 |
| CREATE | src/Domain/Entities/CalendarSync.cs                      | Calendar sync tokens                      |
| CREATE | src/Domain/Entities/AuditLog.cs                          | Immutable audit log                       |
| CREATE | src/Domain/Entities/NoShowRiskFactor.cs                  | No-show risk scoring inputs               |
| CREATE | src/Domain/Entities/InsuranceRecord.cs                   | Insurance validation records              |
| CREATE | src/Infrastructure/Data/Configurations/                  | 16 entity configuration files             |
| MODIFY | src/Infrastructure/Data/ApplicationDbContext.cs          | Register all DbSets                       |
| CREATE | src/Infrastructure/Data/Extensions/PgCryptoExtensions.cs | pgcrypto value converters                 |

## External References

- [EF Core Entity Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [PostgreSQL pgcrypto](https://www.postgresql.org/docs/16/pgcrypto.html)
- [Npgsql JSONB Support](https://www.npgsql.org/efcore/mapping/json.html)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — entity configuration tests verify relationships, constraints, and column types
- [ ] Integration tests pass — migration applies cleanly to PostgreSQL 16 with pgcrypto

## Implementation Checklist

- [ ] Create all 16 domain entity classes with correct properties and data types (AC-01)
- [ ] Configure pgcrypto AES-256 encryption on PatientProfile PHI columns via EF Core value converters (AC-02)
- [ ] Configure JSONB column mappings for PatientView aggregated data fields (AC-03)
- [ ] Define all foreign key relationships with cascade behaviors and concurrency tokens (AC-04)
- [ ] Configure DataConflict entity with dual source document ID references and resolution status (AC-05)
- [ ] Create migration with pgcrypto extension enablement and validate successful application (AC-01)
- [ ] Add appropriate indexes on frequently queried columns (PatientId, Status, CreatedAt) (AC-04)
- [ ] Handle JSONB null values and pgcrypto extension-not-installed error scenarios (Edge Cases)
