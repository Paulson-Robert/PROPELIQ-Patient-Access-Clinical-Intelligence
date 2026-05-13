# Epic — Unified Patient Access & Clinical Intelligence Platform

## Epic Summary Table

| Epic ID | Epic Title                                       | Mapped Requirement IDs                                                                                     |
| ------- | ------------------------------------------------ | ---------------------------------------------------------------------------------------------------------- |
| EP-TECH | Project Foundation & Infrastructure              | TR-001, TR-002, TR-004, TR-005, TR-007, TR-011, TR-012, NFR-001, NFR-014                                   |
| EP-DATA | Core Data Layer & Persistence                    | DR-001, DR-002, DR-003, DR-004, DR-005, DR-006, DR-007, DR-008, DR-009, DR-010                             |
| EP-001  | User Authentication & Access Control             | FR-001, FR-002, FR-003, FR-004, FR-005, TR-003, TR-013, NFR-006, NFR-007, UXR-004, UXR-605                 |
| EP-002  | Appointment Booking & Scheduling                 | FR-006, FR-007, FR-010, FR-013, FR-014, FR-035, FR-042, UXR-502                                            |
| EP-003  | Staff Operations — Walk-in & Queue Management    | FR-008, FR-009, UXR-106, UXR-506                                                                           |
| EP-004  | Calendar Sync & Notifications                    | FR-011, FR-012, FR-028, FR-029, FR-030, FR-031, FR-032, TR-010, NFR-011, UXR-503                           |
| EP-005  | Patient Intake — AI Conversational & Manual Form | FR-015, FR-016, FR-017, FR-018, AIR-001, TR-009, NFR-010, UXR-105, UXR-504, UXR-505                        |
| EP-006  | Clinical Document Upload & Processing            | FR-019, FR-020, FR-021, FR-026, FR-027, TR-006, NFR-012, NFR-013, UXR-403, UXR-604                         |
| EP-007  | Clinical Intelligence — AI Data Extraction       | FR-022, AIR-002, AIR-003, AIR-005, AIR-008, TR-008, NFR-003                                                |
| EP-008  | 360-Degree Patient View & Medical Code Mapping   | FR-023, FR-024, FR-025, AIR-004, AIR-006, NFR-002, UXR-404                                                 |
| EP-009  | No-Show Risk Assessment                          | FR-033, FR-034, AIR-007, UXR-402                                                                           |
| EP-010  | Administration & Audit                           | FR-036, FR-037, FR-038, FR-039, NFR-005                                                                    |
| EP-011  | Security, Compliance & Encryption                | FR-040, FR-041, NFR-004, NFR-008                                                                           |
| EP-012  | UX Foundation — Design System & Responsiveness   | UXR-001, UXR-002, UXR-003, UXR-101, UXR-102, UXR-103, UXR-104, UXR-301, UXR-302, UXR-303, UXR-401, UXR-501 |
| EP-013  | Accessibility & Error Handling                   | UXR-201, UXR-202, UXR-203, UXR-204, UXR-205, UXR-206, UXR-601, UXR-602, UXR-603, NFR-009                   |

## Epic Description

### EP-TECH: Project Foundation & Infrastructure

**Business Value**: Establishes the technical foundation required for all feature development. Without scaffolding, CI/CD, monitoring, and deployment infrastructure, no feature epic can begin implementation.

**Description**: `[SOURCE:INFERRED]` Bootstrap the green-field project with React 18+/Vite frontend SPA, ASP.NET Core 9 backend in Clean Architecture (API, Application, Domain, Infrastructure layers), MediatR for CQRS-Light, Entity Framework Core 9 with PostgreSQL migrations, Hangfire Community for background job processing, Serilog structured logging, Sentry error tracking, Uptime Robot availability monitoring, xUnit/Vitest/Playwright testing frameworks, and deployment pipelines targeting Vercel (frontend), Railway/Render (backend), and Supabase (PostgreSQL). All tooling must be free/open-source per NFR-014. Platform must target ≥99.9% availability per NFR-001.

Basis: Green-field project detected — no existing codebase in app, client, backend, or server folders. Infrastructure epic auto-generated.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:

- React 18+ / Vite project scaffold with TailwindCSS and Shadcn UI configured
- ASP.NET Core 9 solution with Clean Architecture layer separation
- MediatR pipeline with command/query separation
- Entity Framework Core 9 configured with PostgreSQL provider and hybrid migration strategy
- Hangfire Community integration with PostgreSQL job storage
- Serilog + Sentry + Uptime Robot monitoring stack
- xUnit, FluentAssertions, Moq (backend); Vitest, React Testing Library (frontend); Playwright (E2E)
- CI/CD pipeline with automated build, test, and deployment
- Vercel deployment for frontend; Railway/Render for backend; Supabase PostgreSQL provisioning

**Dependent EPICs**:

- None

### EP-DATA: Core Data Layer & Persistence

**Business Value**: Provides the foundational data schema and persistence infrastructure that all feature epics depend on for storing and retrieving patient, appointment, clinical, and operational data.

**Description**: `[SOURCE:INFERRED]` Implement the core data layer using PostgreSQL 16 with pgcrypto extension for field-level PHI encryption (DR-001), Upstash Redis for session cache, slot locks, and rate-limiting counters (DR-002), and the full entity relationship model. Includes entity scaffolding for all domain entities (User, PatientProfile, Appointment, AvailabilitySlot, ClinicalDocument, ExtractedData, PatientView, IntakeRecord, Notification, CalendarSync, AuditLog, InsuranceRecord, NoShowRiskAssessment), relationship mapping, database constraints, seed data, and mock data. Covers document format storage configuration (DR-003), data aggregation schema with JSONB support (DR-004), conflict tracking with source provenance (DR-005), ICD-10-CM and CPT-4 reference code tables with version tracking (DR-006), complete patient data deletion capability (DR-007), appointment scheduling data model with status tracking and slot swap queues (DR-008), no-show risk scoring persistence (DR-009), and notification delivery log (DR-010).

Basis: Data layer complexity detected — 10 DR requirements, design.md contains 13+ domain entity definitions, model.md contains ERD and sequence diagrams with database interactions.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:

- PostgreSQL database schema with all entity tables and relationships
- pgcrypto extension configuration for PHI field-level encryption
- Upstash Redis integration for session cache, slot locks, and rate limiting
- EF Core entity configurations, constraints, and indexes
- ICD-10-CM and CPT-4 reference code table seeding with version tracking
- Document storage schema supporting PDF, DOCX, JPG, PNG, DICOM, HL7/FHIR
- Appointment data model with status enum, slot swap queue, and concurrency tokens
- Notification delivery log schema with status, retry count, and failure tracking
- No-show risk assessment data persistence
- Patient data deletion cascade logic across all stores
- Seed and mock data scripts for development and testing

**Dependent EPICs**:

- EP-TECH - Foundational - Requires project scaffold, ORM configuration, and deployment infrastructure

### EP-001: User Authentication & Access Control

**Business Value**: Authentication and authorization gate all user-facing features. Patients cannot book appointments, complete intake, or upload documents without account access. Staff and admin roles cannot perform any operational or administrative tasks without authenticated, role-restricted sessions.

**Description**: Implement the complete identity and access management layer. Patient registration and login via Google/Microsoft social login (OAuth 2.0/OpenID Connect) and email/password (FR-001). Staff and admin authentication with mandatory MFA via TOTP or SMS code (FR-002). OAuth 2.0 JWT session tokens with 15-minute sliding expiry and automatic session termination (FR-003). Role-based access control enforcing Patient, Staff, and Admin permission sets following least privilege (FR-004, NFR-006). Password complexity enforcement (minimum 8 characters, mixed case, digit, special character) with bcrypt hashing (FR-005, TR-013). OpenIddict 5.x as the OAuth 2.0/OIDC server for token issuance, social login integration, and session management (TR-003, NFR-007). Session timeout warning dialog 2 minutes before expiry (UXR-004). Graceful session expiry redirect to login page with return URL preservation (UXR-605).

**UI Impact**: Yes

**Screen References**: SCR-001, SCR-026, SCR-027, SCR-028, SCR-029

**Key Deliverables**:

- OpenIddict 5.x OAuth 2.0/OIDC server configuration
- Google and Microsoft OAuth social login integration
- Email/password registration with email verification flow
- Password complexity validation and bcrypt hashing (work factor ≥12)
- MFA setup and verification (TOTP and SMS code)
- JWT token issuance with 15-minute sliding expiry
- Role-based access control middleware (Patient, Staff, Admin)
- Account lockout after 5 consecutive failed login attempts
- Session timeout warning modal with "Extend Session" and "Log Out" actions
- Session expiry redirect with return URL preservation
- Login, registration, MFA, email verification, and password reset screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires backend scaffold and auth server infrastructure
- EP-DATA - Foundational - Requires User entity schema and session storage

### EP-002: Appointment Booking & Scheduling

**Business Value**: Core revenue-generating feature. Appointment booking is the primary business action that drives patient engagement, provider utilization, and revenue protection through no-show reduction. Preferred slot swap is a key market differentiator.

**Description**: Implement the full appointment lifecycle. Patient search for available slots by provider or specialty and slot booking (FR-006). Dynamic preferred slot swap — patient books an available slot while selecting a preferred unavailable slot; automatic swap when preferred slot opens (FR-007). Double-booking prevention via optimistic concurrency control with Redis distributed locks (FR-010). Appointment confirmation PDF generation and email delivery (FR-013). Appointment cancellation and rescheduling with slot release, calendar sync update, and preferred swap trigger (FR-014). Soft insurance validation against internal dummy records during booking flow without blocking (FR-035). Appointment history display on patient dashboard with status indicators (Scheduled, Completed, Cancelled, No-Show) (FR-042). Visible countdown timer during the 30-second slot lock period (UXR-502).

**UI Impact**: Yes

**Screen References**: SCR-002, SCR-005, SCR-006, SCR-007, SCR-008, SCR-014, SCR-016

**Key Deliverables**:

- Slot search API with provider/specialty filtering
- Slot booking flow with Redis-based distributed lock (30s TTL)
- Optimistic concurrency control with EF Core concurrency tokens
- Preferred slot swap engine with first-come-first-served ordering
- Appointment confirmation PDF generation (iText7 or equivalent)
- Appointment cancellation and rescheduling with slot release
- Insurance soft validation against predefined dummy records
- Appointment history view with status indicators and pagination
- Slot lock countdown timer UI component
- Search & Book, Booking Confirmation, Appointment Detail, Reschedule, Insurance Form, and History screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires backend API scaffold and background job infrastructure
- EP-DATA - Foundational - Requires Appointment, AvailabilitySlot, and InsuranceRecord schemas

### EP-003: Staff Operations — Walk-in & Queue Management

**Business Value**: Enables front desk staff to handle walk-in patients and manage same-day queues from a centralized dashboard, eliminating context switching across disconnected tools and reducing administrative burden per appointment.

**Description**: Implement staff-facing walk-in booking and queue management workflows. Staff create walk-in bookings with optional patient account creation for post-booking use; patients cannot create walk-in bookings (FR-008). Staff manage same-day queues, mark patients as "Arrived" with arrival timestamp recording, and view real-time queue state; patients cannot self-check-in (FR-009). Walk-in appointments display distinct "Walk-in" badges with differentiated background tint to visually separate from scheduled appointments (UXR-106). Queue entries support drag-handle reordering on desktop and long-press move on mobile with immediate persistence (UXR-506).

**UI Impact**: Yes

**Screen References**: SCR-003, SCR-017, SCR-018

**Key Deliverables**:

- Walk-in booking API with optional patient account creation
- Same-day queue management API with arrival marking
- Real-time queue dashboard with patient status indicators
- Walk-in badge component with visual differentiation
- Drag-and-drop queue reordering (desktop: drag handle, mobile: long-press)
- Patient search for walk-in booking (name, phone, email)
- Guest walk-in booking with minimal demographic data
- Staff Dashboard, Same-Day Queue, and Walk-in Booking screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires backend API scaffold
- EP-DATA - Foundational - Requires Appointment and User schemas with walk-in booking type

### EP-004: Calendar Sync & Notifications

**Business Value**: Directly targets the 15% no-show rate through multi-channel reminders with risk-escalated sequences. Calendar sync reduces missed appointments by keeping external calendars current. Staff notification on delivery failures ensures operational awareness.

**Description**: Implement bidirectional calendar synchronization and the automated notification pipeline. Google Calendar sync via Google Calendar API with OAuth consent and event CRUD (FR-011). Microsoft Outlook sync via Microsoft Graph API with OAuth consent and event CRUD (FR-012). Automated appointment reminders via SMS and email at configurable intervals (FR-028). Asynchronous delivery with exponential backoff retry, max 3 attempts per notification (FR-029). Additional reminder sequences for Medium and High no-show risk patients with increased frequency (FR-030). Staff notification when preferred slot swap is executed, including patient name, original slot, and new slot (FR-031). Staff notification when notification delivery permanently fails after exhausting retries, including patient name, notification type, and failure reason (FR-032). Calendar API integration with OAuth 2.0 and event CRUD (TR-010). All external integration calls delivered asynchronously (NFR-011). Toast notifications for asynchronous event outcomes with auto-dismiss after 5 seconds (UXR-503).

**UI Impact**: Yes

**Screen References**: SCR-013, SCR-015

**Key Deliverables**:

- Google Calendar API integration (OAuth 2.0 consent, event CRUD, token refresh)
- Microsoft Graph API integration (OAuth 2.0 consent, event CRUD, token refresh)
- Calendar sync settings UI with provider selection and status display
- Hangfire-based reminder scheduling engine with configurable intervals
- Risk-escalated reminder sequences (Low: standard; Medium/High: additional reminders)
- SMS gateway integration for reminder delivery
- Email service integration for reminder and confirmation delivery
- Exponential backoff retry logic with max 3 attempts
- Staff notification on swap execution and delivery failure
- Toast notification component with success/error/info variants
- Calendar Sync Settings and notification management screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires Hangfire background job infrastructure
- EP-DATA - Foundational - Requires Notification, CalendarSync, and Appointment schemas

### EP-005: Patient Intake — AI Conversational & Manual Form

**Business Value**: AI-assisted conversational intake is a core market differentiator. Flexible toggle between AI and manual form ensures zero patient friction. Graceful degradation ensures intake completion even when AI is unavailable, protecting patient data and workflow continuity.

**Description**: Implement dual-mode patient intake with seamless switching. AI-assisted conversational intake using NLP to collect patient information through a chat-style interface (FR-015). Traditional manual form intake with structured fields for medical history, symptoms, medications, allergies, and reason for visit (FR-016). Free toggle between AI and manual modes at any time during intake, preserving all previously entered data across mode switches (FR-017). Automatic fallback from AI to manual form when AI service is unavailable or encounters an error, without data loss (FR-018). NLP-powered conversational engine capable of extracting structured patient data from natural language chat input (AIR-001). Cloud LLM API integration for conversational NLP with de-identified prompts only (TR-009). Graceful degradation ensuring AI failures auto-fallback to manual form without data loss (NFR-010). Data preservation on mode toggle (UXR-105). Inline real-time field validation on blur or keystroke (UXR-504). Typing indicator animation during AI response generation (UXR-505).

**UI Impact**: Yes

**Screen References**: SCR-009, SCR-010

**Key Deliverables**:

- AI conversational intake chat interface with multi-turn NLP
- Cloud LLM API integration (OpenAI/Gemini) with de-identified prompts
- Manual form intake with structured fields and real-time validation
- Bidirectional mode toggle with full data preservation
- AI-to-manual automatic fallback on service failure
- Structured data extraction from natural language responses
- Patient review and confirmation of AI-extracted data
- Typing indicator component with screen reader announcement
- Inline field validation with debounced keystroke and blur triggers
- AI Conversational Intake and Manual Form Intake screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires frontend scaffold and backend API infrastructure
- EP-DATA - Foundational - Requires IntakeRecord schema linked to Appointment

### EP-006: Clinical Document Upload & Processing

**Business Value**: Document upload is the entry point for the clinical intelligence pipeline. Without reliable upload, validation, malware scanning, and processing status feedback, the platform cannot ingest the clinical data needed for the 360-Degree Patient View and ICD-10/CPT code mapping.

**Description**: Implement the patient-facing clinical document upload and processing pipeline. Support uploads in PDF, DOCX, JPG, PNG, DICOM, and HL7/FHIR bundle formats (FR-019). Validate uploaded files against 50MB max size and supported format whitelist with clear error messages on rejection (FR-020). Malware scanning of all uploaded documents via ClamAV before processing, quarantining and rejecting infected files with patient notification (FR-021). Real-time processing status display showing pipeline stages (Uploading, Scanning, Processing, Completed, Failed) with progress indicators (FR-026). Indefinite document retention until patient explicitly requests deletion, with permanent removal of all associated data and extracted records (FR-027). ClamAV integration via nClam .NET client (TR-006). Maximum 50MB file upload enforcement (NFR-012). Indefinite retention with permanent deletion capability (NFR-013). Horizontal step indicator for processing pipeline stages (UXR-403). Specific rejection reasons with accepted format list on upload failure (UXR-604).

**UI Impact**: Yes

**Screen References**: SCR-011, SCR-012

**Key Deliverables**:

- File upload API with format whitelist and 50MB size validation
- ClamAV malware scanning integration via nClam
- Document processing pipeline as chained Hangfire background jobs
- Real-time processing status tracking (Uploading → Scanning → Processing → Completed/Failed)
- Document list view with status indicators and management actions
- Patient document deletion with cascade removal across all stores
- Processing status step indicator component
- Upload rejection error display with accepted formats and size limit
- Document Upload and Document List & Status screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires Hangfire background jobs and file storage infrastructure
- EP-DATA - Foundational - Requires ClinicalDocument schema and file storage configuration

### EP-007: Clinical Intelligence — AI Data Extraction

**Business Value**: Core clinical differentiator that transforms 20-minute manual clinical prep into automated extraction. The NER pipeline is the engine that powers the 360-Degree Patient View and ICD-10/CPT code mapping, directly delivering the platform's primary value proposition.

**Description**: Implement the AI-powered clinical data extraction pipeline. Extract clinical data — vitals, medical history, medications, allergies, and diagnoses — from uploaded documents using NLP and NER (FR-022). NER pipeline for multi-format clinical document extraction using ML.NET with custom-trained models (AIR-002, TR-008). PHI boundary enforcement — all PHI extraction processed locally without transmitting data to external services (AIR-003). Confidence scoring (0.0–1.0) for every extraction with configurable thresholds for auto-suggest vs. mandatory manual review (AIR-005). AI model version tracking and accuracy monitoring with administrator alerts when agreement rate drops below 98% (AIR-008). Target >98% AI-Human Agreement Rate for clinical data extractions (NFR-003).

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:

- ML.NET custom NER model for clinical entity extraction
- Format-specific document parsers (iText7 for PDF, DocumentFormat.OpenXml for DOCX, Tesseract.NET for OCR, FHIR bundle deserializer)
- Structured data output with confidence scores per extracted field
- Configurable confidence thresholds for auto-suggest and manual review triage
- PHI boundary enforcement — zero PHI transmission to cloud APIs
- Model version tracking with extraction operation linkage
- Accuracy monitoring dashboard with sub-98% alert threshold
- Extraction pipeline integration with Hangfire background jobs

**Dependent EPICs**:

- EP-TECH - Foundational - Requires ML.NET runtime, Hangfire jobs, and monitoring infrastructure
- EP-DATA - Foundational - Requires ExtractedData schema with confidence scores and model version tracking

### EP-008: 360-Degree Patient View & Medical Code Mapping

**Business Value**: Delivers the platform's primary clinical value — reducing clinical prep from 20+ minutes to ≤2 minutes per patient. The verified patient view with conflict resolution and ICD-10/CPT mapping eliminates manual data aggregation and prevents safety risks from undetected data conflicts. Critical Conflicts Identified metric tracks prevented safety risks and claim denials.

**Description**: Implement data aggregation, conflict resolution, and medical code mapping. Aggregate extracted data from multiple uploaded documents into a single de-duplicated 360-Degree Patient View, presenting consolidated data for human verification before clinical use (FR-023). Explicitly highlight critical data conflicts across documents — conflicting medications, allergies, or diagnoses — displaying both conflicting values with source document references for staff resolution (FR-024). Map ICD-10 and CPT codes based on aggregated patient data, presenting suggested codes with confidence indicators for staff verification before downstream use (FR-025). Human-in-the-loop verification for all AI-generated clinical data before persistence as verified (AIR-004). AI/rule-based engine for ICD-10-CM and CPT-4 code mapping with confidence scores and source references (AIR-006). Clinical prep time reduction to ≤2 minutes per patient (NFR-002). Conflict highlighting using amber border, tinted background, and AlertTriangle icon with source document references (UXR-404).

**UI Impact**: Yes

**Screen References**: SCR-019, SCR-020, SCR-021

**Key Deliverables**:

- Data aggregation engine with de-duplication across multiple documents
- Conflict detection algorithm for medications, allergies, and diagnoses
- 360-Degree Patient View with tabbed sections (Vitals, Medications, Allergies, Diagnoses, Procedures)
- Conflict resolution panel with both values, source references, and staff resolution actions
- ICD-10-CM and CPT-4 code mapping engine (rule-based + AI fuzzy matching)
- Code suggestion interface with confidence indicators and source data references
- Staff verification workflow for AI-generated data (accept, modify, reject)
- Verified/Pending Review status tracking per section
- Conflict indicator components with amber styling and AlertTriangle icon
- 360-Degree Patient View, Conflict Resolution Panel, and Code Mapping Interface screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires backend API scaffold and frontend component infrastructure
- EP-DATA - Foundational - Requires PatientView, ExtractedData, and ICD-10/CPT code table schemas

### EP-009: No-Show Risk Assessment

**Business Value**: Directly supports the primary business objective of reducing the 15% no-show rate. Risk scoring enables targeted intervention through escalated reminders for high-risk patients and provides staff with actionable visibility into appointment risk levels for proactive queue management.

**Description**: Implement history-based no-show risk scoring and tier classification. Calculate a no-show risk score for each appointment using four factors: patient's historical no-show count, appointment lead time (bookings >7 days out receive higher risk), time-of-day pattern (early AM and late PM receive higher risk), and new-patient flag (FR-033). Classify each appointment's risk into Low, Medium, or High tier based on the calculated score, displaying the tier on staff-facing schedule and queue views (FR-034). Deterministic weighted scoring algorithm with conservative tier boundary classification (AIR-007). Risk tier visual indicators using combined color, icon, and text label: Low = green/Shield/"Low", Medium = amber/AlertCircle/"Medium", High = red/AlertTriangle/"High" (UXR-402).

**UI Impact**: Yes

**Screen References**: SCR-017, SCR-022

**Key Deliverables**:

- Weighted risk scoring algorithm with four input factors
- Tier classification engine (Low/Medium/High) with conservative boundary handling
- Risk score calculation triggered on appointment creation
- Risk tier display on staff queue dashboard and schedule view
- Risk tier badge component with color + icon + text label (non-color-only encoding)
- Risk score persistence linked to appointment records
- New-patient detection for elevated baseline risk

**Dependent EPICs**:

- EP-TECH - Foundational - Requires backend API scaffold
- EP-DATA - Foundational - Requires NoShowRiskAssessment and Appointment schemas

### EP-010: Administration & Audit

**Business Value**: Provides admin operational control for user lifecycle management and HIPAA-mandated immutable audit logging. Audit trail is a regulatory requirement for healthcare operations. Platform adoption metrics dashboard enables data-driven decisions on platform effectiveness.

**Description**: Implement admin user management and audit logging capabilities. Allow admins to create, update, and deactivate user accounts and assign roles (Patient, Staff, Admin) through an admin interface (FR-036). Maintain an immutable, append-only audit log recording all patient data access events, staff actions, and system state changes with timestamp, actor ID, action type, and affected resource (FR-037). Provide admins with a searchable, filterable audit log viewer supporting filtering by date range, actor, action type, and affected resource (FR-038). Display an admin dashboard showing platform adoption metrics: total patient dashboards created, total appointments booked, current no-show rate, and trend data (FR-039). Immutable audit log design with database-level UPDATE/DELETE prevention and cross-cutting MediatR pipeline behavior for automatic audit capture (NFR-005).

**UI Impact**: Yes

**Screen References**: SCR-004, SCR-023, SCR-024, SCR-025

**Key Deliverables**:

- User management CRUD API with role assignment
- Account activation email flow with retry
- Account deactivation with active session invalidation
- Last-admin protection guard
- Immutable audit log table with PostgreSQL rules/triggers preventing UPDATE/DELETE
- MediatR pipeline behavior for automatic audit record capture on all commands
- Searchable, filterable audit log viewer with pagination
- Audit log filtering by date range, actor, action type, and affected resource
- Admin dashboard with adoption metrics and trend charts
- User Management, Audit Log Viewer, and Platform Metrics Dashboard screens

**Dependent EPICs**:

- EP-TECH - Foundational - Requires backend API scaffold, MediatR pipeline, and monitoring infrastructure
- EP-DATA - Foundational - Requires AuditLog and User schemas

### EP-011: Security, Compliance & Encryption

**Business Value**: Establishes the HIPAA compliance security baseline required for healthcare operations. PHI encryption at rest and in transit, API rate limiting, and security hardening protect patient data and prevent unauthorized access — regulatory failures would block deployment entirely.

**Description**: Implement cross-cutting security and compliance controls. Encrypt all patient data (PHI) at rest using AES-256 encryption and in transit using TLS 1.2 or higher (FR-040, NFR-004). Enforce API rate limiting on all public-facing endpoints to prevent brute-force attacks and abuse, returning HTTP 429 responses when limits are exceeded (FR-041, NFR-008). Rate-limiting implementation using Upstash Redis counters with sliding window algorithm. PHI field-level encryption using ASP.NET Core Data Protection and pgcrypto. TLS enforcement across all API endpoints and client connections.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:

- AES-256 field-level PHI encryption via pgcrypto for database columns
- ASP.NET Core Data Protection configuration for application-layer encryption
- TLS 1.2+ enforcement on all endpoints
- API rate limiting middleware with Redis-backed sliding window counters
- HTTP 429 response handling with Retry-After headers
- Rate limit configuration per endpoint category (auth, public API, file upload)
- Security headers configuration (HSTS, CSP, X-Content-Type-Options)
- HIPAA compliance self-assessment documentation

**Dependent EPICs**:

- EP-TECH - Foundational - Requires backend API scaffold and Redis infrastructure

### EP-012: UX Foundation — Design System & Responsiveness

**Business Value**: Establishes the visual design system and responsive layout foundation that all feature screens build upon. Consistent design language, component library, responsive breakpoints, and interaction patterns reduce development time across all feature epics and ensure a cohesive patient and staff experience.

**Description**: Implement the cross-cutting UX foundation layer. Mobile-first responsive layout with fluid scaling across all breakpoints (UXR-001). Shadcn UI component library with TailwindCSS for all UI elements (UXR-002). Lucide icon pack integration with consistent 16/20/24px sizing and 1.5px stroke weight (UXR-003). Primary task completion within 3 clicks from role-specific dashboards (UXR-101). 200ms perceived response time using skeleton loading for search results (UXR-102). Consistent navigation patterns across all dashboards using shared layout shell — header, sidebar (desktop), bottom nav (mobile) with role-determined menu items (UXR-103). Real-time status feedback for all asynchronous operations with progress indicators (UXR-104). Responsive breakpoints at 375px, 768px, 1024px, and 1440px (UXR-301). Sidebar-to-bottom-nav collapse on mobile viewports below 768px (UXR-302). Data table to stacked card layout transformation on mobile (UXR-303). Consistent spacing using TailwindCSS 4px base unit scale (UXR-401). Loading spinner on submit buttons during form submission with disabled state to prevent double-click (UXR-501).

**UI Impact**: Yes

**Screen References**: All screens (SCR-001 through SCR-029, MOD-001 through MOD-007)

**Key Deliverables**:

- Shadcn UI component library integration with TailwindCSS configuration
- Lucide icon pack setup with size and stroke consistency
- Shared layout shell component (header, sidebar, bottom nav) with role-based menu
- Responsive breakpoint configuration (375px / 768px / 1024px / 1440px)
- Sidebar-to-bottom-nav responsive collapse component
- Data table to card layout responsive transformation component
- Skeleton loading component for perceived performance
- Submit button loading spinner with disabled state
- TailwindCSS spacing scale tokens (4px base unit)
- Design token configuration (colors, typography, spacing, radius, shadows)
- Patient, Staff, and Admin dashboard layout shells

**Dependent EPICs**:

- EP-TECH - Foundational - Requires React/Vite frontend scaffold with TailwindCSS

### EP-013: Accessibility & Error Handling

**Business Value**: WCAG 2.2 Level AA compliance is both a regulatory expectation for healthcare applications and a usability necessity for diverse patient populations including those with disabilities. Robust error handling ensures no dead-end states in clinical workflows where blocked users could impact patient safety.

**Description**: Implement cross-cutting accessibility compliance and error handling patterns. Color contrast ratios ≥4.5:1 for normal text and ≥3:1 for large text and UI components (UXR-201). Full keyboard navigation for all interactive elements with logical tab order and no keyboard traps (UXR-202). Visible focus indicators on all interactive components meeting 3:1 contrast (UXR-203). ARIA labels and roles for all complex widgets including data tables, modals, chat interface, and step indicators (UXR-204). Minimum 44x44px touch targets on mobile viewports with ≥8px spacing between adjacent targets (UXR-205). Non-color-only information encoding — all color-coded indicators include redundant icon or text label (UXR-206). Actionable recovery options on all error states with retry, navigate back, or contact support (UXR-601). Inline form validation errors below invalid fields with destructive-colored border and AlertCircle icon (UXR-602). Persistent top banner on network connectivity failure with WifiOff icon, retry action, and auto-reconnect (UXR-603). WCAG 2.2 Level AA compliance across all interfaces (NFR-009).

**UI Impact**: Yes

**Screen References**: All screens (SCR-001 through SCR-029, MOD-001 through MOD-007)

**Key Deliverables**:

- Accessibility audit and contrast ratio verification across all color combinations
- Keyboard navigation implementation with logical tab order for all interactive elements
- Focus ring component with 3:1 contrast against adjacent colors
- ARIA label and role implementation for complex widgets (tables, modals, chat, steppers)
- Touch target sizing enforcement (44x44px minimum) on mobile viewports
- Color-independent indicator system (icon + text + color for all status indicators)
- Error state components with actionable recovery options (retry, back, support)
- Inline validation error component with destructive styling
- Network connectivity banner component with auto-retry and auto-dismiss
- Skip-to-content links on all pages
- Screen reader live regions for toast notifications and chat messages
- Automated accessibility testing integration (axe-core)

**Dependent EPICs**:

- EP-TECH - Foundational - Requires React/Vite frontend scaffold with component library
