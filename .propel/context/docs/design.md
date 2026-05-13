# Architecture Design

## Project Overview

The **Unified Patient Access & Clinical Intelligence Platform** is a standalone healthcare web application that combines patient-centric appointment scheduling with an AI-powered clinical intelligence engine. The platform targets three user roles — Patients, Staff, and Admins — enabling patients to book appointments, complete intake (AI conversational or manual form), and upload clinical documents; staff to manage walk-ins, queues, and verify AI-extracted clinical data; and admins to manage users and review audit trails. The system automatically extracts, de-duplicates, and consolidates patient data from multi-format documents into a verified 360-Degree Patient View with mapped ICD-10/CPT codes. All operations are 100% HIPAA-compliant with immutable audit logging, AES-256 encryption at rest, and TLS 1.2+ in transit. Hosting is constrained to free/open-source platforms only.

## Architecture Goals

- **AG-1: HIPAA-First Security Posture** — Every architectural decision must satisfy HIPAA Security Rule requirements (access controls, audit controls, integrity controls, transmission security) as a non-negotiable baseline, not an afterthought.
- **AG-2: Trust-First AI Integration** — AI-generated clinical data must always flow through human verification before clinical use, with confidence scoring, conflict highlighting, and full provenance tracking to address the "Black Box" trust deficit.
- **AG-3: Graceful Degradation** — The system must remain functional when any single external dependency (AI service, calendar API, SMS gateway, email service) is unavailable, falling back to manual or queued alternatives without data loss.
- **AG-4: Free-Tier Deployability** — The architecture must be deployable entirely on free/open-source hosting platforms (no paid cloud services), constraining technology choices to solutions with viable free tiers.
- **AG-5: Clinical Prep Efficiency** — Architecture must support the 20+ min → 2 min clinical prep reduction target by enabling parallel document processing, efficient data aggregation, and fast patient view rendering.
- **AG-6: Separation of Concerns** — Clean Architecture with distinct API, Application, Domain, and Infrastructure layers to enable independent testing, HIPAA audit compliance, and long-term maintainability.

## Non-Functional Requirements

- NFR-001: `[SOURCE:INPUT]` System MUST maintain ≥ 99.9% availability measured over rolling 30-day periods
  Basis: Spec Success Criteria SC-006 — "99.9% platform uptime maintained over rolling 30-day periods"

- NFR-002: `[SOURCE:INPUT]` System MUST reduce clinical prep time from 20+ minutes to ≤ 2 minutes per patient through automated extraction, aggregation, and presentation of clinical data
  Basis: Spec Success Criteria SC-002 — "Clinical prep time reduced from 20+ minutes to under 2 minutes per patient"

- NFR-003: `[SOURCE:INPUT]` System MUST achieve > 98% AI-Human Agreement Rate for suggested clinical data extractions and medical code mappings
  Basis: Spec Success Criteria SC-003 — "AI-Human Agreement Rate of >98% for suggested clinical data and medical codes"

- NFR-004: `[SOURCE:INPUT]` System MUST encrypt all patient data (PHI) at rest using AES-256 encryption and in transit using TLS 1.2 or higher
  Basis: Spec FR-040, Constraint CON-001 — "100% HIPAA-compliant data handling"; HIPAA Security Rule 45 CFR §164.312(a)(2)(iv) and §164.312(e)(1)

- NFR-005: `[SOURCE:INPUT]` System MUST maintain immutable, append-only audit logs recording all PHI access events, staff actions, and system state changes with timestamp, actor ID, action type, and affected resource
  Basis: Spec FR-036, FR-037 — "immutable, append-only audit log"; HIPAA Security Rule 45 CFR §164.312(b)

- NFR-006: `[SOURCE:INPUT]` System MUST enforce role-based access control with three roles (Patient, Staff, Admin), each with a defined permission set restricting access to authorized features only, following the principle of least privilege
  Basis: Spec FR-001, FR-003, FR-004; HIPAA Security Rule 45 CFR §164.312(a)(1) — unique user identification and access controls

- NFR-007: `[SOURCE:INPUT]` System MUST issue OAuth 2.0 JWT session tokens with 15-minute sliding expiry and enforce mandatory multi-factor authentication (TOTP or SMS code) for staff and admin roles
  Basis: Spec FR-002, FR-003, FR-005; HIPAA Security Rule 45 CFR §164.312(d) — person or entity authentication

- NFR-008: `[SOURCE:INPUT]` System MUST enforce API rate limiting on all public-facing endpoints, returning HTTP 429 responses when limits are exceeded
  Basis: Spec FR-041; OWASP A04:2021 — Insecure Design (brute-force prevention)

- NFR-009: `[SOURCE:INFERRED]` System MUST comply with WCAG 2.2 Level AA accessibility standards for all patient-facing and staff-facing interfaces
  Basis: Healthcare UI accessibility requirement; WCAG 2.2 standard for inclusive design; aligns with Section 508 compliance expectations for healthcare platforms

- NFR-010: `[SOURCE:INPUT]` System MUST implement graceful degradation: AI intake failures auto-fallback to manual form; calendar sync failures queue for retry; document parsing failures flag documents for manual review — all without data loss
  Basis: Spec elicitation decision — graceful degradation strategy confirmed for all external dependency failures

- NFR-011: `[SOURCE:INPUT]` System MUST deliver all external integration calls (calendar sync, SMS, email) asynchronously with exponential backoff retry on failure, with a maximum of 3 retry attempts per operation
  Basis: Spec elicitation decision — async with exponential backoff retry (max 3 attempts) confirmed for all external calls

- NFR-012: `[SOURCE:INPUT]` System MUST enforce a maximum file upload size of 50 MB per document and reject files exceeding this limit with a clear error message
  Basis: Spec FR-019, FR-020 — "maximum size of 50MB per file"

- NFR-013: `[SOURCE:INPUT]` System MUST retain uploaded clinical documents indefinitely until the patient explicitly requests deletion, at which point all associated data and extracted records MUST be permanently and irrecoverably removed
  Basis: Spec FR-027, elicitation decision — indefinite retention with right-to-delete; HIPAA Privacy Rule right of access

- NFR-014: `[SOURCE:INPUT]` System MUST be deployable entirely on free/open-source hosting platforms with no paid cloud infrastructure dependencies
  Basis: Spec Constraints CON-003, CON-004 — "free/open-source platforms only (Netlify, Vercel, GitHub Codespaces)"; "No paid cloud infrastructure"

## Data Requirements

- DR-001: `[SOURCE:INPUT]` System MUST use PostgreSQL as the primary relational data store with pgcrypto extension for field-level encryption of PHI columns
  Basis: Spec Constraint CON-007 — PostgreSQL confirmed via elicitation; NFR-004 — AES-256 encryption at rest requires field-level encryption for PHI

- DR-002: `[SOURCE:INPUT]` System MUST use Upstash Redis for session cache, appointment slot locks (optimistic concurrency), and rate-limiting counters
  Basis: Spec Constraint CON-008 — "Upstash Redis caching"; FR-010 — optimistic concurrency for slot locking; NFR-008 — rate limiting

- DR-003: `[SOURCE:INPUT]` System MUST support storage and processing of clinical documents in the following formats: PDF, DOCX, JPG, PNG, DICOM, and HL7/FHIR bundles
  Basis: Spec FR-019 — "upload clinical documents in the following formats: PDF, DOCX, JPG, PNG, DICOM, and HL7/FHIR bundles"

- DR-004: `[SOURCE:INPUT]` System MUST aggregate extracted data from multiple uploaded documents into a single de-duplicated patient record, merging matching entities and preserving source document provenance
  Basis: Spec FR-023 — "aggregate extracted data from multiple uploaded documents into a single de-duplicated 360-Degree Patient View"

- DR-005: `[SOURCE:INPUT]` System MUST track and persist data conflicts across documents (conflicting medications, allergies, diagnoses) with source document references, enabling staff resolution and maintaining resolution audit trail
  Basis: Spec FR-024 — "highlight critical data conflicts across documents"

- DR-006: `[SOURCE:INPUT]` System MUST maintain ICD-10-CM and CPT-4 reference code tables with version tracking to support annual code set updates
  Basis: Spec FR-025 — "map ICD-10 and CPT codes based on aggregated patient data"; ICD-10/CPT standards require versioned code sets

- DR-007: `[SOURCE:INPUT]` System MUST support complete, permanent data deletion on patient request — removing all patient records, uploaded documents, extracted clinical data, audit log references, and cached data across all stores
  Basis: Spec FR-027, FR-040; HIPAA Privacy Rule — patient right of access and deletion; NFR-013

- DR-008: `[SOURCE:INPUT]` System MUST model appointment and scheduling data supporting provider availability slots, recurring schedule patterns, appointment bookings with status tracking (Scheduled, Arrived, Completed, Cancelled, No-Show), preferred slot swap queues, and optimistic concurrency locking
  Basis: Spec FR-006 through FR-014, FR-042 — appointment booking, slot swap, queue management, history display

- DR-009: `[SOURCE:INPUT]` System MUST persist no-show risk scoring inputs (historical no-show count, lead time, time-of-day, new-patient flag) and calculated tier (Low/Medium/High) per appointment for staff visibility and trend analysis
  Basis: Spec FR-033, FR-034 — "calculate a no-show risk score" with four factors and three tiers

- DR-010: `[SOURCE:INFERRED]` System MUST maintain a notification delivery log tracking each notification's status (Queued, Sent, Delivered, Failed), retry count, failure reason, and associated appointment/patient context
  Basis: NFR-011 — retry tracking requires persisted delivery state; FR-032 — staff notification on permanent failure requires failure history; FR-029 — exponential backoff requires retry count persistence

### Domain Entities

- **User**: Represents all platform users. Attributes: UserId (PK), Email, PasswordHash, AuthProvider (Local/Google/Microsoft), Role (Patient/Staff/Admin), MfaEnabled, MfaSecret, IsActive, CreatedAt, UpdatedAt. Relationships: One-to-many with Appointment, AuditLog; one-to-one with PatientProfile (if Patient role).

- **PatientProfile**: Extended patient demographics and clinical context. Attributes: PatientProfileId (PK), UserId (FK), FirstName, LastName, DateOfBirth, Phone, InsuranceName, InsuranceId, InsuranceValidationStatus, CreatedAt. Relationships: One-to-many with ClinicalDocument, IntakeRecord; one-to-one with PatientView.

- **Appointment**: Core scheduling entity. Attributes: AppointmentId (PK), PatientId (FK), ProviderId, SlotId (FK), Status (Scheduled/Arrived/Completed/Cancelled/No-Show), NoShowRiskTier, NoShowRiskScore, BookingType (Online/WalkIn), PreferredSlotId (nullable FK), CreatedByUserId (FK), CreatedAt, UpdatedAt. Relationships: Many-to-one with User, AvailabilitySlot; one-to-many with Notification, CalendarSync.

- **AvailabilitySlot**: Provider time slot for scheduling. Attributes: SlotId (PK), ProviderId, ProviderName, Specialty, StartTime, EndTime, IsAvailable, IsLocked, LockExpiry, RecurrencePattern. Relationships: One-to-many with Appointment.

- **PreferredSlotQueue**: Tracks patient preferred slot swap requests. Attributes: QueueId (PK), AppointmentId (FK), PreferredSlotId (FK), RequestedAt, Status (Waiting/Swapped/Expired). Relationships: Many-to-one with Appointment, AvailabilitySlot.

- **ClinicalDocument**: Uploaded patient documents. Attributes: DocumentId (PK), PatientProfileId (FK), FileName, FileFormat, FileSizeBytes, StoragePath, MalwareScanStatus, ProcessingStatus (Uploading/Scanning/Processing/Completed/Failed), UploadedAt, ProcessedAt. Relationships: Many-to-one with PatientProfile; one-to-many with ExtractedDataRecord.

- **ExtractedDataRecord**: AI-extracted clinical data from documents. Attributes: RecordId (PK), DocumentId (FK), PatientProfileId (FK), DataType (Vital/Medication/Allergy/Diagnosis/Procedure), FieldName, FieldValue, Confidence, IsVerified, VerifiedByUserId (nullable FK), VerifiedAt, SourceLocation. Relationships: Many-to-one with ClinicalDocument, PatientProfile; many-to-many with DataConflict.

- **DataConflict**: Tracks conflicting clinical data across documents. Attributes: ConflictId (PK), PatientProfileId (FK), ConflictType (Medication/Allergy/Diagnosis), FieldName, Value1, SourceDocumentId1 (FK), Value2, SourceDocumentId2 (FK), ResolutionStatus (Open/Resolved), ResolvedByUserId (nullable FK), ResolvedAt, ResolutionNotes. Relationships: Many-to-one with PatientProfile; references ClinicalDocument.

- **PatientView**: De-duplicated, consolidated 360-Degree Patient View. Attributes: PatientViewId (PK), PatientProfileId (FK), AggregatedVitals (JSONB), AggregatedMedications (JSONB), AggregatedAllergies (JSONB), AggregatedDiagnoses (JSONB), AggregatedProcedures (JSONB), LastAggregatedAt, VerificationStatus. Relationships: One-to-one with PatientProfile.

- **MedicalCodeMapping**: ICD-10/CPT code suggestions. Attributes: MappingId (PK), PatientProfileId (FK), ExtractedRecordId (FK), CodeType (ICD10/CPT), CodeValue, CodeDescription, Confidence, IsVerified, VerifiedByUserId (nullable FK), VerifiedAt, CodeSetVersion. Relationships: Many-to-one with PatientProfile, ExtractedDataRecord.

- **IntakeRecord**: Patient intake data (AI or manual). Attributes: IntakeId (PK), PatientProfileId (FK), AppointmentId (FK), IntakeMode (AI/Manual), MedicalHistory (JSONB), CurrentSymptoms (JSONB), Medications (JSONB), Allergies (JSONB), ReasonForVisit, CompletedAt, LastModifiedAt. Relationships: Many-to-one with PatientProfile, Appointment.

- **Notification**: Tracks all outbound notifications. Attributes: NotificationId (PK), AppointmentId (FK), PatientId (FK), Channel (SMS/Email), NotificationType (Reminder/Confirmation/SlotSwap/Cancellation/FailureAlert), Status (Queued/Sent/Delivered/Failed), RetryCount, LastAttemptAt, FailureReason, CreatedAt. Relationships: Many-to-one with Appointment, User.

- **CalendarSync**: Tracks external calendar sync state. Attributes: SyncId (PK), UserId (FK), Provider (Google/Microsoft), AccessToken (encrypted), RefreshToken (encrypted), TokenExpiry, IsActive, LastSyncAt. Relationships: Many-to-one with User.

- **AuditLog**: Immutable audit trail. Attributes: AuditLogId (PK, auto-increment), Timestamp, ActorUserId (FK), ActorRole, ActionType, ResourceType, ResourceId, Details (JSONB), IpAddress. Constraints: Append-only (no UPDATE or DELETE operations). Relationships: Many-to-one with User.

- **NoShowRiskFactor**: Historical risk scoring inputs. Attributes: FactorId (PK), PatientProfileId (FK), HistoricalNoShowCount, LastNoShowDate, AverageLeadTimeDays, PreferredTimeOfDay, IsNewPatient, LastCalculatedAt. Relationships: One-to-one with PatientProfile.

- **InsuranceRecord**: Internal dummy insurance validation set. Attributes: RecordId (PK), InsuranceName, InsuranceIdPattern, IsActive. Used for soft validation only.

## AI Consideration

**Status:** Applicable

The upstream spec.md contains 6 AI-tagged functional requirements: 2 marked `[AI-CANDIDATE]` (FR-015, FR-022) and 4 marked `[HYBRID]` (FR-023, FR-024, FR-025, FR-033). These requirements span conversational intake, clinical data extraction, data aggregation/conflict detection, medical code mapping, and no-show risk scoring. The AI architecture must comply with HIPAA PHI handling constraints and enforce human-in-the-loop verification for all clinical AI outputs.

## AI Requirements

- AIR-001: `[SOURCE:INPUT]` System MUST provide an NLP-powered conversational intake engine capable of extracting structured patient data (medical history, symptoms, medications, allergies, reason for visit) from natural language chat input
  Basis: Spec FR-015 `[AI-CANDIDATE]` — "AI-assisted conversational intake mode that uses natural language processing"

- AIR-002: `[SOURCE:INPUT]` System MUST implement a Named Entity Recognition (NER) pipeline for extracting clinical data — vitals, medications, allergies, diagnoses, and procedures — from multi-format uploaded documents (PDF, DOCX, JPG/PNG, DICOM, HL7/FHIR)
  Basis: Spec FR-022 `[AI-CANDIDATE]` — "extract clinical data…from uploaded documents using AI-powered natural language processing and named entity recognition"

- AIR-003: `[SOURCE:EXTERNAL]` System MUST NOT transmit unencrypted or identifiable PHI to cloud-hosted LLM APIs unless the API provider has executed a HIPAA Business Associate Agreement (BAA); for PHI extraction tasks, the system MUST prefer local/on-premise NLP processing
  Basis: HIPAA Security Rule 45 CFR §164.314(a) — Business Associate Contracts; FDA AI/ML guidance on healthcare AI safety; PHI protection requirement from NFR-004

- AIR-004: `[SOURCE:INPUT]` System MUST require human-in-the-loop verification for all AI-generated clinical data (extracted values, de-duplicated records, suggested codes) before the data is persisted as verified or used for clinical decision-making
  Basis: Spec FR-022 "presenting the consolidated data for human verification"; FR-025 "presenting suggested codes…for staff verification"; all `[HYBRID]` tags mandate human verification

- AIR-005: `[SOURCE:INFERRED]` System MUST assign a confidence score (0.0–1.0) to every AI-generated extraction and code mapping, and enforce configurable thresholds: outputs above the high-confidence threshold are auto-suggested for verification; outputs below the low-confidence threshold are flagged for mandatory manual review
  Basis: AIR-004 — human verification requires confidence-based triage to be practical at scale; NFR-003 — >98% agreement rate requires measurable confidence tracking; Spec FR-025 — "confidence indicators"

- AIR-006: `[SOURCE:INPUT]` System MUST map extracted clinical data to ICD-10-CM and CPT-4 codes using an AI/rule-based engine, presenting suggested codes with confidence scores and source data references for staff verification
  Basis: Spec FR-025 `[HYBRID]` — "map ICD-10 and CPT codes based on aggregated patient data, presenting suggested codes with confidence indicators for staff verification"

- AIR-007: `[SOURCE:INPUT]` System MUST calculate a no-show risk score for each appointment using a history-based algorithm with four inputs (historical no-show count, appointment lead time, time-of-day pattern, new-patient flag) and classify results into three tiers (Low, Medium, High)
  Basis: Spec FR-033 `[HYBRID]`, FR-034 — "calculate a no-show risk score…based on: patient's historical no-show count, appointment lead time, time-of-day pattern, and new-patient flag"

- AIR-008: `[SOURCE:INFERRED]` System MUST track AI model versions used for each extraction/mapping operation and monitor accuracy metrics over time, alerting administrators when agreement rate drops below the 98% threshold
  Basis: NFR-003 — >98% agreement rate requires ongoing monitoring; FDA AI/ML guidance recommends model versioning and performance monitoring for healthcare AI systems; AIR-005 — confidence scoring requires calibration tracking

### AI Architecture Pattern

**Selected Pattern:** Hybrid (Local NLP + Cloud LLM with PHI Boundary)

**Rationale:**

- **Local Medical NLP** (for PHI extraction — AIR-002, AIR-003): Clinical documents containing PHI are processed entirely by a local/self-hosted NER pipeline. This avoids transmitting identifiable patient data to cloud LLM APIs, satisfying HIPAA BAA requirements without needing a cloud provider BAA. The local pipeline handles entity extraction from clinical text, structured data parsing from HL7/FHIR, and OCR for image-based documents.
- **Cloud LLM API** (for conversational intake — AIR-001): The conversational intake collects data directly from the patient in a chat interface. Patient-provided free-text responses can be processed by cloud LLM APIs using de-identification or by collecting only non-PHI metadata in the prompt. The patient's structured responses are stored locally.
- **Rule-Based + AI Hybrid** (for code mapping — AIR-006, AIR-007): ICD-10/CPT mapping uses a combination of deterministic rule-based lookup (exact code matching) and AI-assisted fuzzy matching (for ambiguous descriptions). No-show risk scoring uses a deterministic weighted algorithm, not AI inference.
- This pattern satisfies AIR-003 (PHI boundary), AIR-004 (human-in-the-loop for all AI outputs), and NFR-014 (free-tier deployability — local processing avoids cloud API costs for the heaviest workload).

## Architecture and Design Decisions

- **ADD-1: Clean Architecture (4-Layer)** — The system follows Clean Architecture with four layers: API (controllers, middleware, filters), Application (commands, queries, handlers, DTOs, validators), Domain (entities, value objects, domain services, interfaces), and Infrastructure (EF Core repositories, external service clients, file storage, caching). Dependencies point inward — Infrastructure and API depend on Application/Domain, never the reverse. This supports AG-6 (separation of concerns), enables independent unit testing of business logic, and isolates HIPAA-sensitive data access in the Infrastructure layer for targeted audit.

- **ADD-2: CQRS-Light via MediatR** — Commands (state-changing operations) and Queries (read operations) are separated using the Mediator pattern (MediatR library). This is not event-sourcing CQRS — both commands and queries use the same PostgreSQL database. The separation enables independent optimization of read/write paths, clear audit trail insertion points (commands), and testable handler isolation. Justification: NFR-005 (audit logging at command boundaries), AG-5 (query optimization for patient view rendering).

- **ADD-3: Background Job Architecture (Hangfire)** — All asynchronous operations (document processing, calendar sync, notification delivery, preferred slot swap monitoring, no-show risk calculation) execute as background jobs via Hangfire with PostgreSQL storage. Jobs support retry with exponential backoff, dead-letter handling, and dashboard monitoring. Justification: NFR-010 (graceful degradation), NFR-011 (async retry), AG-3 (resilience).

- **ADD-4: PHI Boundary Enforcement** — A strict architectural boundary separates PHI-processing components from external API integrations. Clinical document processing (NER extraction, data aggregation) occurs entirely within the local application boundary. Cloud LLM API calls for conversational intake use de-identified prompts only. All PHI fields in PostgreSQL use pgcrypto field-level encryption. Redis caches use encrypted connections and store only non-PHI identifiers (slot IDs, session tokens). Justification: NFR-004 (encryption), AIR-003 (no PHI to cloud without BAA), AG-1 (HIPAA-first).

- **ADD-5: Optimistic Concurrency with Redis Distributed Locks** — Appointment slot booking uses Redis-based distributed locks with short TTL (30 seconds) to prevent double-booking. The lock is acquired when a patient selects a slot, held during confirmation, and released on booking completion or timeout. EF Core concurrency tokens on the AvailabilitySlot entity provide a secondary safeguard at the database level. Justification: FR-010 (prevent double-booking), DR-002 (Redis for slot locks).

- **ADD-6: Event-Driven Notification Pipeline** — Notification dispatch follows a fire-and-forget pattern from the booking/scheduling flow. Booking commands publish domain events (AppointmentBooked, AppointmentCancelled, SlotSwapped) to in-process MediatR notifications, which trigger Hangfire background jobs for actual delivery. This decouples the booking flow from notification delivery latency. Justification: NFR-011 (async delivery), AG-3 (degradation — booking succeeds even if notification fails).

- **ADD-7: Document Processing Pipeline** — Uploaded documents flow through a staged pipeline: (1) Upload & validation, (2) Malware scan (ClamAV), (3) Format-specific parsing (PDF text extraction, DOCX parsing, OCR for images, FHIR bundle deserialization), (4) NER extraction, (5) Data aggregation & conflict detection, (6) Patient View update. Each stage updates the document's processing status (DR-003, FR-026) and failures at any stage flag the document for manual review (NFR-010). The pipeline executes as chained Hangfire jobs.

- **ADD-8: Immutable Audit Log Design** — The AuditLog table uses database-level constraints to prevent UPDATE and DELETE operations (PostgreSQL rules/triggers). All audit entries are INSERT-only with auto-generated timestamps. A cross-cutting MediatR pipeline behavior automatically captures audit records for all command handlers, eliminating the risk of missed audit points. Justification: NFR-005 (immutable audit), AG-1 (HIPAA compliance).

## Technology Stack

| Layer                     | Technology                                                       | Version              | Justification                                                                      |
| ------------------------- | ---------------------------------------------------------------- | -------------------- | ---------------------------------------------------------------------------------- |
| Frontend                  | React + Vite                                                     | React 18.x, Vite 5.x | BRD mandate; NFR-014 (free, open-source); NFR-009 (ecosystem a11y support)         |
| Frontend UI               | Material UI (MUI)                                                | 5.x                  | NFR-009 (WCAG 2.2 AA built-in); healthcare UX accessibility requirements           |
| Frontend State            | React Context + useReducer                                       | Built-in             | Sufficient for auth state, patient context; avoids unnecessary dependency          |
| Backend                   | ASP.NET Core                                                     | 9.0                  | BRD mandate; NFR-004, NFR-005, NFR-006 (built-in security, auth, audit middleware) |
| Backend Pattern           | MediatR (CQRS-Light)                                             | 12.x                 | ADD-2; NFR-005 (audit at command boundaries)                                       |
| Database                  | PostgreSQL + pgcrypto                                            | 16.x                 | DR-001; NFR-004 (field-level PHI encryption); NFR-014 (free)                       |
| ORM                       | Entity Framework Core                                            | 9.0                  | DR-001; TR-004 (hybrid migration strategy)                                         |
| Cache                     | Upstash Redis                                                    | Serverless           | DR-002; FR-010 (slot locks); NFR-008 (rate limiting); NFR-014 (free tier)          |
| Background Jobs           | Hangfire Community                                               | 1.8.x                | ADD-3; NFR-010, NFR-011 (async retry, degradation); NFR-014 (free, MIT)            |
| Auth Server               | OpenIddict                                                       | 5.x                  | NFR-007 (OAuth 2.0/OIDC, JWT, MFA); NFR-014 (free, open-source)                    |
| AI/ML — Local NLP         | ML.NET + custom NER model                                        | 4.x                  | AIR-002, AIR-003 (local PHI extraction); NFR-014 (free, .NET-native)               |
| AI/ML — Cloud LLM         | OpenAI API / Google Gemini                                       | Latest               | AIR-001 (conversational intake, de-identified only); NFR-014 (free tier)           |
| AI/ML — Code Mapping      | Rule-based engine + ML.NET                                       | Custom               | AIR-006 (ICD-10/CPT mapping); deterministic + ML hybrid                            |
| Document Processing       | iText7 (PDF), DocumentFormat.OpenXml (DOCX), Tesseract.NET (OCR) | Latest stable        | DR-003 (multi-format support); NFR-014 (free/open-source)                          |
| Malware Scanning          | ClamAV (via nClam)                                               | Latest               | FR-021 (malware scanning); NFR-014 (free, open-source)                             |
| Testing — Backend         | xUnit + FluentAssertions + Moq                                   | Latest stable        | Industry standard .NET testing stack                                               |
| Testing — Frontend        | React Testing Library + Vitest                                   | Latest stable        | Component testing aligned with React best practices                                |
| Testing — E2E             | Playwright                                                       | Latest stable        | E2E testing per project standards                                                  |
| Monitoring — Errors       | Sentry                                                           | Free tier            | NFR-001 (uptime monitoring); NFR-014 (free tier: 10K events/month)                 |
| Monitoring — Uptime       | Uptime Robot                                                     | Free tier            | NFR-001 (99.9% availability monitoring); NFR-014 (free: 50 monitors)               |
| Logging                   | Serilog                                                          | Latest stable        | Structured logging to console/file; integrates with .NET and Sentry                |
| Deployment — Frontend     | Vercel                                                           | Free tier            | NFR-014 (free static hosting); React SPA deployment                                |
| Deployment — Backend      | Railway / Render                                                 | Free tier            | NFR-014 (free PaaS for .NET); container deployment support                         |
| Deployment — Database     | Supabase (PostgreSQL)                                            | Free tier            | NFR-014 (free managed PostgreSQL); DR-001                                          |
| Security — Passwords      | bcrypt (BCrypt.Net-Next)                                         | Latest               | NFR-004, FR-005 (HIPAA password hashing); OWASP recommendation                     |
| Security — PHI Encryption | ASP.NET Core Data Protection + pgcrypto                          | Built-in             | NFR-004 (AES-256 at rest); ADD-4 (PHI boundary)                                    |

### AI Component Stack

| Component              | Technology                            | Purpose                                                                                                    |
| ---------------------- | ------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Model Provider — Local | ML.NET (custom NER model)             | PHI-safe clinical entity extraction from documents (vitals, medications, allergies, diagnoses, procedures) |
| Model Provider — Cloud | OpenAI GPT-4o / Google Gemini         | Conversational intake NLP (de-identified patient input only)                                               |
| Code Mapping Engine    | Rule-based lookup + ML.NET classifier | ICD-10-CM/CPT-4 code suggestion with confidence scoring                                                    |
| Risk Scoring Engine    | Weighted deterministic algorithm      | No-show risk calculation (not AI inference — history-based scoring)                                        |
| Guardrails             | Custom middleware + FluentValidation  | Input/output schema validation, confidence threshold enforcement, PHI boundary checks                      |

### Alternative Technology Options

- **Auth: Duende IdentityServer** — Considered for OAuth 2.0/OIDC. Rejected: requires a paid license for production use ($1,500+/year), violating NFR-014 (free-tier constraint). OpenIddict provides equivalent functionality with a fully free Apache 2.0 license.

- **Auth: ASP.NET Core Identity (standalone)** — Considered for basic auth. Rejected: does not provide a full OAuth 2.0/OIDC server required for social login flows (FR-001) and JWT token management (NFR-007). OpenIddict builds on top of ASP.NET Core Identity, adding the OIDC server layer.

- **AI/ML — Local NLP: SciSpacy / MedSpaCy (Python)** — Considered for medical NER. Rejected: requires a separate Python runtime alongside the .NET backend, increasing deployment complexity and violating architectural simplicity on free-tier hosting. ML.NET keeps the entire stack in .NET, simplifying deployment (single runtime). If ML.NET NER accuracy proves insufficient, a Python microservice can be introduced in a later phase.

- **AI/ML — Cloud LLM: Anthropic Claude API** — Considered for conversational intake. Rejected for primary selection: free-tier availability is more limited than OpenAI/Gemini. Retained as a fallback option if primary providers deprecate free tiers.

- **Database: SQL Server (Express)** — BRD initially mentioned SQL Server. Rejected: SQL Server Express has a 10GB database size limit and limited free-tier hosting availability. PostgreSQL has no size limit, broader free-tier hosting support (Supabase, ElephantSQL, Railway), and pgcrypto for native field-level encryption. PostgreSQL was confirmed via elicitation.

- **Background Jobs: .NET BackgroundService** — Considered for async processing. Rejected: lacks persistent job storage (jobs lost on restart), no built-in retry with backoff, no dashboard for monitoring. Hangfire provides persistent PostgreSQL-backed job storage, exponential backoff retry, a monitoring dashboard, and scheduled/recurring job support — all required by NFR-010 and NFR-011.

- **Frontend Framework: Angular** — Considered as an alternative to React. Rejected: BRD explicitly mandates React. Angular is a viable alternative but not evaluated further due to this constraint.

- **Cache: Self-hosted Redis** — Considered vs. Upstash. Rejected: requires dedicated server hosting (violates NFR-014 free-tier constraint for infrastructure). Upstash provides serverless Redis with a generous free tier (10K commands/day).

### Technology Decision

#### Frontend Framework

| Metric                         | React + Vite                | Angular + CLI             | Rationale                                          |
| ------------------------------ | --------------------------- | ------------------------- | -------------------------------------------------- |
| BRD Mandate                    | Yes                         | No                        | React explicitly mandated in BRD                   |
| Bundle Size (NFR-009, NFR-014) | ~45KB gzipped               | ~65KB gzipped             | Smaller bundle benefits free-tier bandwidth limits |
| a11y Ecosystem                 | Strong (MUI, react-aria)    | Strong (Angular Material) | Both adequate; React selected per mandate          |
| Free Hosting Compatibility     | Excellent (Vercel, Netlify) | Good                      | React SPAs have first-class Vercel support         |

#### Backend Framework

| Metric                  | ASP.NET Core 9                           | Node.js (Express)           | Rationale                                                                           |
| ----------------------- | ---------------------------------------- | --------------------------- | ----------------------------------------------------------------------------------- |
| BRD Mandate             | Yes (.NET)                               | No                          | .NET explicitly mandated in BRD                                                     |
| HIPAA Security Features | Built-in Data Protection, Identity, RBAC | Requires third-party        | Native security stack reduces HIPAA compliance effort                               |
| Background Job Support  | Hangfire (mature, .NET-native)           | Bull/BullMQ (Redis-based)   | Hangfire integrates with PostgreSQL, no additional Redis dependency for job storage |
| Type Safety             | C# (compile-time)                        | TypeScript (transpile-time) | Stronger type safety for healthcare data handling                                   |

#### Database

| Metric                 | PostgreSQL 16                  | SQL Server Express                 | Rationale                                                                                    |
| ---------------------- | ------------------------------ | ---------------------------------- | -------------------------------------------------------------------------------------------- |
| Size Limit             | None                           | 10GB                               | PostgreSQL has no database size limit — critical for indefinite document retention (NFR-013) |
| Free Hosting           | Supabase, ElephantSQL, Railway | Limited                            | Broader free-tier hosting availability (NFR-014)                                             |
| Field-Level Encryption | pgcrypto (native)              | Always Encrypted (Enterprise only) | pgcrypto is free; SQL Server field-level encryption requires Enterprise edition              |
| JSONB Support          | Native                         | JSON (no indexing)                 | JSONB enables efficient storage/querying of clinical data aggregations (DR-004)              |

#### Authentication Server

| Metric               | OpenIddict 5.x                 | Duende IdentityServer     | Rationale                                 |
| -------------------- | ------------------------------ | ------------------------- | ----------------------------------------- |
| License Cost         | Free (Apache 2.0)              | $1,500+/year (production) | NFR-014 mandates free tooling             |
| OAuth 2.0 / OIDC     | Full support                   | Full support              | Feature parity                            |
| .NET Integration     | Native (ASP.NET Core Identity) | Native                    | Both integrate with ASP.NET Core Identity |
| Social Login Support | Via external providers         | Via external providers    | Both support Google/Microsoft OIDC        |

#### AI — Local NLP Engine

| Metric                | ML.NET (Custom NER)              | SciSpacy/MedSpaCy (Python)             | Rationale                                                                                  |
| --------------------- | -------------------------------- | -------------------------------------- | ------------------------------------------------------------------------------------------ |
| Runtime Homogeneity   | .NET (same as backend)           | Requires Python runtime                | Single runtime simplifies free-tier deployment (NFR-014)                                   |
| Medical NER Quality   | Good (trainable, custom models)  | Excellent (pre-trained medical models) | SciSpacy has superior pre-trained medical models, but ML.NET can be trained on domain data |
| Deployment Complexity | Low (single .NET app)            | High (Python sidecar/microservice)     | Free-tier hosting limits make multi-runtime deployment challenging                         |
| HIPAA Compliance      | Local processing, no data egress | Local processing, no data egress       | Both satisfy AIR-003                                                                       |

#### AI — Cloud LLM Provider (Conversational Intake)

| Metric                 | OpenAI GPT-4o           | Google Gemini                   | Anthropic Claude | Rationale                                                    |
| ---------------------- | ----------------------- | ------------------------------- | ---------------- | ------------------------------------------------------------ |
| Free Tier Availability | Limited (trial credits) | Generous (Gemini API free tier) | Limited          | Gemini offers the most generous free tier (NFR-014)          |
| Conversational Quality | Excellent               | Very Good                       | Excellent        | All adequate for structured intake                           |
| Function Calling       | Yes                     | Yes                             | Yes (tool use)   | All support structured output extraction                     |
| HIPAA BAA Available    | Yes (paid tier)         | Yes (paid tier)                 | Yes (paid tier)  | For free-tier use, only de-identified data is sent (AIR-003) |

## Technical Requirements

- TR-001: `[SOURCE:INPUT]` System MUST implement the frontend using React 18+ with Vite as the build tool, producing a single-page application deployable as static assets
  Basis: BRD technology mandate — React frontend; NFR-014 — Vite is free/open-source

- TR-002: `[SOURCE:INPUT]` System MUST implement the backend using ASP.NET Core 9 with Clean Architecture (API, Application, Domain, Infrastructure layers) and MediatR for command/query separation
  Basis: BRD technology mandate — .NET backend; ADD-1 (Clean Architecture), ADD-2 (CQRS-Light); NFR-005 (audit at command boundaries)

- TR-003: `[SOURCE:INFERRED]` System MUST use OpenIddict 5.x as the OAuth 2.0 / OpenID Connect server for JWT token issuance, social login integration (Google, Microsoft), and session management
  Basis: NFR-007 (OAuth 2.0 JWT, MFA); NFR-014 (free — Apache 2.0 license); FR-001 (social login); FR-003 (session tokens)

- TR-004: `[SOURCE:INPUT]` System MUST use Entity Framework Core 9 as the ORM with a hybrid migration strategy: EF Core migrations during development and reviewed SQL scripts applied in production environments
  Basis: DR-001 (PostgreSQL ORM); elicitation decision — hybrid migration strategy confirmed

- TR-005: `[SOURCE:INPUT]` System MUST use Hangfire Community (MIT license) with PostgreSQL storage for all background job processing including document processing, notification delivery, calendar sync, slot swap monitoring, and no-show risk calculation
  Basis: ADD-3 (background job architecture); NFR-010 (graceful degradation via retry); NFR-011 (async with exponential backoff); NFR-014 (free — MIT license); elicitation decision — Hangfire confirmed

- TR-006: `[SOURCE:INFERRED]` System MUST integrate ClamAV (via nClam .NET client) for malware scanning of all uploaded clinical documents before processing
  Basis: FR-021 (malware scanning); NFR-004 (HIPAA security safeguards for ePHI); NFR-014 (free — ClamAV is GPL)

- TR-007: `[SOURCE:INPUT]` System MUST use Sentry (free tier) for application error tracking, Uptime Robot (free tier) for availability monitoring, and Serilog for structured logging throughout the .NET backend
  Basis: NFR-001 (99.9% uptime monitoring requires tooling); NFR-014 (free tier); elicitation decision — monitoring stack confirmed

- TR-008: `[SOURCE:INFERRED]` System MUST use ML.NET with a custom-trained NER model for clinical data extraction from uploaded documents, processing all PHI locally without transmitting data to external services
  Basis: AIR-002 (NER pipeline for clinical extraction); AIR-003 (no PHI to cloud without BAA); NFR-014 (free — ML.NET is MIT, .NET-native); ADD-4 (PHI boundary enforcement)

- TR-009: `[SOURCE:INPUT]` System MUST integrate with a cloud LLM API (OpenAI GPT-4o or Google Gemini) for conversational intake NLP, transmitting only de-identified or patient-provided free-text input — never extracted PHI
  Basis: AIR-001 (NLP conversational intake); AIR-003 (PHI boundary — de-identified only); NFR-014 (free tier API); elicitation decision — cloud LLM APIs confirmed

- TR-010: `[SOURCE:INPUT]` System MUST integrate with Google Calendar API (OAuth 2.0, Calendar event CRUD) and Microsoft Graph API (OAuth 2.0, Calendar event CRUD) for bidirectional appointment synchronization
  Basis: FR-011 (Google Calendar sync); FR-012 (Microsoft Outlook sync); DR-008 (scheduling data model); NFR-011 (async retry for sync failures)

- TR-011: `[SOURCE:INFERRED]` System MUST be deployable on Vercel (frontend static hosting), Railway or Render (backend container hosting), and Supabase (managed PostgreSQL) — all using free-tier plans
  Basis: NFR-014 (free/open-source platforms only); Spec Constraints CON-003, CON-004; AG-4 (free-tier deployability)

- TR-012: `[SOURCE:INFERRED]` System MUST use xUnit with FluentAssertions and Moq for backend unit/integration testing, React Testing Library with Vitest for frontend component testing, and Playwright for end-to-end testing
  Basis: NFR-003 (>98% AI accuracy verification requires testable architecture); industry standard .NET and React testing stacks; project Playwright standards

- TR-013: `[SOURCE:INPUT]` System MUST hash all user passwords using bcrypt (BCrypt.Net-Next) with a minimum work factor of 12, never storing plaintext or reversibly encrypted passwords
  Basis: FR-005 (bcrypt or Argon2 hashing); NFR-004 (HIPAA password storage); OWASP A02:2021 — Cryptographic Failures

- TR-014: `[SOURCE:INFERRED]` System MUST use ASP.NET Core Data Protection API for application-level encryption and PostgreSQL pgcrypto for database field-level encryption of all PHI columns (names, dates of birth, medical data, contact information)
  Basis: NFR-004 (AES-256 encryption at rest); DR-001 (pgcrypto); ADD-4 (PHI boundary enforcement); HIPAA Security Rule §164.312(a)(2)(iv)

## Technical Constraints & Assumptions

### Constraints

- **CON-001:** All hosting infrastructure must use free-tier plans — no paid cloud services (AWS, Azure, GCP). This limits compute resources, database size, and API call volumes.
- **CON-002:** The system is a Phase 1 release — no provider logins, payment integration, family profiles, patient self-check-in, direct EHR integration, or full claims submission.
- **CON-003:** Single-runtime deployment (.NET) is preferred for backend and AI processing to minimize free-tier hosting complexity. A Python sidecar for medical NLP may be introduced in Phase 2 if ML.NET NER accuracy is insufficient.
- **CON-004:** Cloud LLM API usage is limited to free-tier quotas (e.g., Gemini API free tier). The system must function without cloud LLM availability for core clinical workflows (document extraction, code mapping) — only conversational intake depends on cloud LLM.
- **CON-005:** DICOM file processing is limited to metadata extraction and embedded report text. Full DICOM image rendering/analysis is out of scope for Phase 1.
- **CON-006:** HL7/FHIR support is limited to parsing uploaded FHIR bundles (read-only). Bidirectional FHIR API integration with external EHR systems is out of scope.
- **CON-007:** Insurance validation is a soft check against an internal dummy dataset — no integration with real insurance payers or clearinghouses.
- **CON-008:** Free-tier database hosting (Supabase free tier) imposes storage limits (~500MB). Document binary storage should use file system or object storage rather than database BLOBs.
- **CON-009:** Upstash Redis free tier limits to 10K commands/day. Rate limiting and session management must be designed within this budget, with fallback to in-memory caching if Redis quota is exhausted.

### Assumptions

- **ASM-001:** The development team has .NET and React development expertise. No additional training is budgeted for technology onboarding.
- **ASM-002:** ML.NET custom NER models can achieve acceptable accuracy (>95%) for clinical entity extraction with sufficient training data. If accuracy falls below threshold, Phase 2 will introduce a Python-based medical NLP sidecar (SciSpacy/MedSpaCy).
- **ASM-003:** Free-tier hosting platforms (Vercel, Railway/Render, Supabase) will remain available with current free-tier limits throughout the project lifecycle.
- **ASM-004:** Google Calendar API and Microsoft Graph API free tiers provide sufficient quota for the expected user volume in Phase 1.
- **ASM-005:** ICD-10-CM and CPT-4 code sets are available as downloadable reference data from CMS (Centers for Medicare & Medicaid Services) at no cost.
- **ASM-006:** ClamAV can be deployed as a sidecar service or installed on the hosting platform. If not feasible on free-tier hosting, malware scanning may be deferred to a file upload proxy or client-side validation with server-side format checks.
- **ASM-007:** Patients will upload English-language clinical documents. Multi-language NER support is not planned for Phase 1.
- **ASM-008:** The system will operate in a single timezone initially. Multi-timezone scheduling support may be added in a future phase.
