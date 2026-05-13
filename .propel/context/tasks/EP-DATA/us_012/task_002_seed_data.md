# Task - TASK_002

## Requirement Reference

- **User Story:** US_012
- **Story Location:** .propel/context/tasks/EP-DATA/us_012/us_012.md
- **Acceptance Criteria:**
  - AC-04: Seed data script populates representative test data (3 users, 5 slots, 2 appointments, 1 document)
  - AC-05: Seed data is idempotent — no duplicates on re-run
- **Edge Cases:**
  - Empty code tables — seed data includes ICD-10/CPT reference data
  - Re-execution — upsert logic prevents duplicate records

---

## Applicable Technology Stack

| Layer    | Technology            | Version | Justification                                   |
| -------- | --------------------- | ------- | ----------------------------------------------- |
| Database | PostgreSQL + pgcrypto | 16.x    | DR-001 — seed data target                       |
| ORM      | Entity Framework Core | 9.0     | TR-004 — seed data via HasData or custom seeder |

---

## Task Overview

Create idempotent seed data scripts that populate the database with representative test data including users (1 patient, 1 staff, 1 admin), availability slots, appointments, clinical documents with extracted records, and sample ICD-10/CPT reference data.

## Dependent Tasks

- US_008 task (Core Database Schema) — all entity schemas must exist
- US_010 task (Clinical Document Schema) — code reference tables must exist

## Impacted Components

- Infrastructure/Data/Seed/DatabaseSeeder.cs — main seed orchestration
- Infrastructure/Data/Seed/UserSeeder.cs — user seed data
- Infrastructure/Data/Seed/AppointmentSeeder.cs — appointment and slot seed data
- Infrastructure/Data/Seed/ClinicalDataSeeder.cs — document and extraction seed data

## Implementation Plan

1. Create DatabaseSeeder class that orchestrates all sub-seeders
2. Create UserSeeder with 3 users (patient, staff, admin) with bcrypt-hashed passwords
3. Create AppointmentSeeder with 5 availability slots and 2 appointments
4. Create ClinicalDataSeeder with 1 clinical document and associated extracted records
5. Implement upsert logic using existence checks to ensure idempotency
6. Register seeder in application startup for Development environment only

## Current Project State

- All entity schemas exist (US_008, US_010, US_011)
- Code reference tables exist (US_010)
- No seed data exists

## Expected Changes

| Action | File Path                                          | Description                                 |
| ------ | -------------------------------------------------- | ------------------------------------------- |
| CREATE | src/Infrastructure/Data/Seed/DatabaseSeeder.cs     | Seed orchestration                          |
| CREATE | src/Infrastructure/Data/Seed/UserSeeder.cs         | 3 test users with roles                     |
| CREATE | src/Infrastructure/Data/Seed/AppointmentSeeder.cs  | Slots and appointments                      |
| CREATE | src/Infrastructure/Data/Seed/ClinicalDataSeeder.cs | Document and extraction data                |
| MODIFY | src/API/Program.cs                                 | Register seeder for Development environment |

## External References

- [EF Core Data Seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — seeder idempotency verification
- [ ] Integration tests pass — seed runs twice without duplicate errors

## Implementation Checklist

- [ ] Create DatabaseSeeder orchestrating all sub-seeders in dependency order (AC-04)
- [ ] Implement UserSeeder with 1 patient, 1 staff, 1 admin with bcrypt-hashed passwords (AC-04)
- [ ] Implement AppointmentSeeder with 5 availability slots and 2 appointments (AC-04)
- [ ] Implement ClinicalDataSeeder with 1 document and extracted records (AC-04)
- [ ] Implement upsert/existence-check logic for idempotent re-execution (AC-05)
- [ ] Register seeder execution in Development environment startup (AC-04)
