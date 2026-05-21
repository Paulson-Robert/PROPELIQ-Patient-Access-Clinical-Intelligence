# Task - TASK_001

## Requirement Reference

- **User Story:** US_010
- **Story Location:** .propel/context/tasks/EP-DATA/us_010/us_010.md
- **Acceptance Criteria:**
  - AC-01: ClinicalDocument entity supports all required formats (PDF, DOCX, JPG, PNG, DICOM, HL7/FHIR)
  - AC-02: Document processing status tracks pipeline stages with timestamps
  - AC-03: ICD-10-CM reference table seeded with version tracking
  - AC-04: CPT-4 reference table seeded with version tracking
  - AC-05: MedicalCodeMapping links to versioned code sets
- **Edge Cases:**
  - Duplicate code entries across versions — allowed via composite key
  - DICOM metadata-only storage — no full image binary in database
  - Empty code tables — queries return empty results without exceptions

---

## Applicable Technology Stack

| Layer    | Technology            | Version | Justification                                                          |
| -------- | --------------------- | ------- | ---------------------------------------------------------------------- |
| Database | PostgreSQL + pgcrypto | 16.x    | DR-003 — multi-format document storage; DR-006 — versioned code tables |
| ORM      | Entity Framework Core | 9.0     | TR-004 — entity configuration and migrations                           |

---

## Task Overview

Configure the ClinicalDocument entity with format validation and pipeline status tracking, create versioned ICD-10-CM and CPT-4 reference code tables with seed data, and ensure MedicalCodeMapping references link to specific code set versions for traceability.

## Dependent Tasks

- US_008 task (Core Database Schema) — core entity schema must exist before clinical document and code tables

## Impacted Components

- Infrastructure/Data/Configurations/ClinicalDocumentConfiguration.cs — document entity config
- Infrastructure/Data/Configurations/Icd10CodeConfiguration.cs — ICD-10 reference table config
- Infrastructure/Data/Configurations/CptCodeConfiguration.cs — CPT reference table config
- Infrastructure/Data/Seed/CodeTableSeeder.cs — seed data for code tables
- Domain/Entities/Icd10Code.cs — ICD-10 reference entity
- Domain/Entities/CptCode.cs — CPT reference entity
- Domain/Enums/DocumentFormat.cs — supported format enum
- Domain/Enums/ProcessingStatus.cs — pipeline stage enum

## Implementation Plan

1. Create DocumentFormat enum with PDF, DOCX, JPG, PNG, DICOM, HL7_FHIR values
2. Create ProcessingStatus enum with Uploading, Scanning, Processing, Completed, Failed states
3. Configure ClinicalDocument entity with format validation, status tracking, and timestamps per stage transition
4. Create Icd10Code and CptCode domain entities with CodeValue, Description, Category, CodeSetVersion
5. Configure composite unique constraints allowing same code across versions
6. Create seed data script with representative ICD-10-CM and CPT-4 codes
7. Verify MedicalCodeMapping.CodeSetVersion references match code table versions

## Current Project State

- Core entity schema exists (US_008)
- ClinicalDocument entity defined but needs format/status configuration
- No ICD-10/CPT reference tables exist

## Expected Changes

| Action | File Path                                                               | Description                     |
| ------ | ----------------------------------------------------------------------- | ------------------------------- |
| CREATE | src/Domain/Enums/DocumentFormat.cs                                      | Supported document format enum  |
| CREATE | src/Domain/Enums/ProcessingStatus.cs                                    | Pipeline processing status enum |
| CREATE | src/Domain/Entities/Icd10Code.cs                                        | ICD-10-CM reference code entity |
| CREATE | src/Domain/Entities/CptCode.cs                                          | CPT-4 reference code entity     |
| CREATE | src/Infrastructure/Data/Configurations/Icd10CodeConfiguration.cs        | ICD-10 table config             |
| CREATE | src/Infrastructure/Data/Configurations/CptCodeConfiguration.cs          | CPT table config                |
| MODIFY | src/Infrastructure/Data/Configurations/ClinicalDocumentConfiguration.cs | Add format/status config        |
| CREATE | src/Infrastructure/Data/Seed/CodeTableSeeder.cs                         | ICD-10 and CPT seed data        |
| MODIFY | src/Infrastructure/Data/ApplicationDbContext.cs                         | Register code table DbSets      |

## External References

- [ICD-10-CM Code Structure](https://www.cms.gov/medicare/coding-billing/icd-10-codes)
- [CPT Code Overview](https://www.ama-assn.org/practice-management/cpt)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — enum validation, seed data idempotency
- [x] Integration tests pass — migration applies, seed data loads, code lookups work

## Implementation Checklist

- [x] Create DocumentFormat and ProcessingStatus enums with all required values (AC-01, AC-02)
- [x] Configure ClinicalDocument entity with format constraint and status transition timestamps (AC-01, AC-02)
- [x] Create Icd10Code entity with CodeValue, Description, Category, CodeSetVersion columns (AC-03)
- [x] Create CptCode entity with matching structure and independent versioning (AC-04)
- [x] Implement seed data script with representative ICD-10-CM and CPT-4 codes (AC-03, AC-04)
- [x] Configure MedicalCodeMapping to reference CodeSetVersion for traceability (AC-05)
- [x] Add composite unique constraint (CodeValue + CodeSetVersion) on reference tables (Edge Cases)
- [x] Generate migration and validate successful application (AC-01)
