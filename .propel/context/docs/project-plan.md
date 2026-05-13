# Unified Patient Access & Clinical Intelligence Platform

## Executive Summary

**Project Type**: Green-field
**Business Context**: Healthcare organizations operate fragmented systems — booking tools lack clinical context, and clinical staff manually read multi-format PDF reports (20+ minutes per patient) to gather vitals, history, and medications. No-show rates reach 15% due to complex booking flows and lack of intelligent reminders. This project builds a unified platform that combines patient-centric appointment scheduling with a "Trust-First" clinical intelligence engine.
**Solution Overview**: A single platform where patients book appointments through an intuitive interface with smart slot management, complete intake via AI conversation or manual forms, and upload clinical documents. The system automatically extracts, de-duplicates, and consolidates patient data into a verified 360-Degree Patient View with mapped ICD-10/CPT codes — transforming a 20-minute manual search into a 2-minute verification action. Staff manage walk-ins, queues, and arrivals from a centralized dashboard. All data handling is 100% HIPAA-compliant with immutable audit trails.
**Key Stakeholders**: Product Owner, Development Team (3 FE, 1 BE, 1 QA, 1 PM), Clinical Staff (end-users), Patients (end-users), Compliance/Security Officer
**AI-Paired Development**: Yes — AI reduction factors applied to estimates (20–30% reduction on automatable tasks)

## Project Scope

### In Scope

- Patient registration, authentication (social + email/password), and session management — mapped to EP-001
- Appointment search, booking, cancellation, rescheduling, and preferred slot swap — mapped to EP-002
- Staff walk-in booking, same-day queue management, and arrival marking — mapped to EP-003
- Calendar sync (Google Calendar, Microsoft Outlook) and multi-channel notification pipeline — mapped to EP-004
- Dual-mode patient intake (AI conversational + manual form) with seamless toggle — mapped to EP-005
- Clinical document upload, validation, malware scanning, and processing pipeline — mapped to EP-006
- AI-powered clinical data extraction via ML.NET NER pipeline — mapped to EP-007
- 360-Degree Patient View, conflict resolution, and ICD-10/CPT code mapping — mapped to EP-008
- No-show risk scoring (history-based, three-tier classification) — mapped to EP-009
- Admin user management, immutable audit logging, and platform metrics dashboard — mapped to EP-010
- PHI encryption (AES-256 at rest, TLS 1.2+ in transit) and API rate limiting — mapped to EP-011
- UX design system (Shadcn UI, TailwindCSS, Lucide icons) and responsive layout foundation — mapped to EP-012
- WCAG 2.2 Level AA accessibility compliance and error handling patterns — mapped to EP-013
- Project scaffolding, CI/CD, deployment infrastructure, monitoring, and testing frameworks — mapped to EP-TECH
- Core data layer: PostgreSQL + pgcrypto, Upstash Redis, entity schema, seed data — mapped to EP-DATA

### Out of Scope

- Provider logins or provider-facing actions
- Payment gateway integration (provisioning for future reservation fees only)
- Family member profile features
- Patient self-check-in (mobile, web portal, or QR code)
- Direct, bi-directional EHR integration or full claims submission
- Use of paid cloud infrastructure (AWS, Azure, GCP)
- Multi-language support (English-only in Phase 1)
- Mobile native applications (web responsive only)

## Objectives and Goals

### Business Objectives

| Objective ID | Objective                     | Success Metric                           | Target                                    | Signal Source  | Basis                                                                                                   |
| ------------ | ----------------------------- | ---------------------------------------- | ----------------------------------------- | -------------- | ------------------------------------------------------------------------------------------------------- |
| OBJ-001      | Reduce patient no-show rate   | No-show rate reduction from 15% baseline | Demonstrable reduction over first 90 days | [SOURCE:INPUT] | Spec success criteria — "Demonstrable reduction in baseline no-show rate (15%)"                         |
| OBJ-002      | Reduce clinical prep time     | Staff time per patient                   | From 20+ minutes to ≤2 minutes            | [SOURCE:INPUT] | Spec success criteria — "Clinical prep time reduced from 20+ minutes to under 2 minutes per patient"    |
| OBJ-003      | Achieve AI clinical accuracy  | AI-Human Agreement Rate                  | >98% for clinical data and medical codes  | [SOURCE:INPUT] | Spec success criteria — "AI-Human Agreement Rate of >98% for suggested clinical data and medical codes" |
| OBJ-004      | Maintain platform reliability | Platform uptime                          | ≥99.9% over rolling 30-day periods        | [SOURCE:INPUT] | Spec success criteria — "99.9% platform uptime maintained over rolling 30-day periods"                  |
| OBJ-005      | Ensure regulatory compliance  | HIPAA compliance violations              | Zero violations during initial deployment | [SOURCE:INPUT] | Spec success criteria — "Zero HIPAA compliance violations during initial deployment phase"              |
| OBJ-006      | Drive platform adoption       | Patient dashboards and appointments      | Measurable volume within first quarter    | [SOURCE:INPUT] | Spec success criteria — "measurable volume of patient dashboards created and appointments booked"       |

### Project Goals

- Deliver a production-ready healthcare platform within 20 sprints (~40 weeks) with full requirement coverage of 121 requirements across 15 epics
- Establish a secure, HIPAA-compliant architecture with immutable audit trails and encrypted PHI handling from Sprint 1
- Achieve MVP (authentication + appointment booking) by Sprint 8 for early stakeholder validation
- Complete the clinical intelligence pipeline (document processing + NER + 360-Degree Patient View) by Sprint 16
- Deploy on free-tier infrastructure (Vercel, Railway/Render, Supabase) with zero paid cloud dependencies

### Alignment to Requirements

| Objective | Mapped Requirements                                                |
| --------- | ------------------------------------------------------------------ |
| OBJ-001   | FR-028, FR-029, FR-030, FR-033, FR-034, AIR-007, DR-009            |
| OBJ-002   | FR-022, FR-023, FR-024, FR-025, NFR-002, AIR-002, AIR-004, AIR-006 |
| OBJ-003   | FR-022, FR-025, NFR-003, AIR-002, AIR-005, AIR-008                 |
| OBJ-004   | NFR-001, TR-007, TR-011                                            |
| OBJ-005   | FR-037, FR-040, FR-041, NFR-004, NFR-005, NFR-006, NFR-007, DR-007 |
| OBJ-006   | FR-006, FR-039, FR-042                                             |

## Project Timeline / Milestones

### High-Level Timeline

| Milestone ID | Milestone Title                        | Target Date | Deliverables                                                                                                   | Dependencies   | Signal Source     | Basis                                                                                                                                   |
| ------------ | -------------------------------------- | ----------- | -------------------------------------------------------------------------------------------------------------- | -------------- | ----------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| MS-001       | Project Kickoff                        | 2026-05-19  | Team onboarding, environment setup, backlog grooming                                                           | None           | [SOURCE:INFERRED] | Start date derived from elicitation (no fixed deadline); next business day after planning                                               |
| MS-002       | Foundation Complete                    | 2026-07-10  | Project scaffold (FE+BE), database schema, design system, accessibility framework deployed                     | MS-001         | [SOURCE:INFERRED] | EP-TECH (13 SP) + EP-DATA (21 SP) + EP-012 (21 SP) + EP-013 (13 SP) = 68 SP across 4 sprints; foundation epics unblock all feature work |
| MS-003       | MVP — Auth + Booking                   | 2026-09-04  | Patient/staff login, appointment search/booking, slot swap, insurance pre-check, appointment history           | MS-002         | [SOURCE:INFERRED] | EP-001 (21 SP) + EP-002 (34 SP) + EP-011 (8 SP) = 63 SP across 4 sprints; delivers core user-facing value for early validation          |
| MS-004       | Clinical Pipeline Complete             | 2026-11-13  | Document upload, malware scanning, processing pipeline, NER extraction, staff operations, no-show risk scoring | MS-002         | [SOURCE:INFERRED] | EP-003 (13 SP) + EP-006 (21 SP) + EP-007 (55 SP) + EP-009 (8 SP) = 97 SP across 5 sprints; clinical data ingestion fully operational    |
| MS-005       | AI Intelligence & Integration Complete | 2027-01-16  | 360-Degree Patient View, conflict resolution, ICD-10/CPT mapping, calendar sync, notifications                 | MS-004         | [SOURCE:INFERRED] | EP-004 (21 SP) + EP-005 (34 SP) + EP-008 (34 SP) = 89 SP across 4 sprints; clinical intelligence and integration layer complete         |
| MS-006       | Feature Complete + Admin               | 2027-02-13  | Admin dashboard, user management, audit log viewer, platform metrics; all features code-complete               | MS-003, MS-005 | [SOURCE:INFERRED] | EP-010 (21 SP) = 21 SP across 2 sprints; all 15 epics delivered                                                                         |
| MS-007       | UAT & Production Release               | 2027-02-27  | User acceptance testing, bug fixes, performance tuning, production deployment                                  | MS-006         | [SOURCE:INFERRED] | 2 sprints for integration testing, UAT, hardening, and production release preparation                                                   |

### Key Dates

| Event                   | Date       | Notes                                                |
| ----------------------- | ---------- | ---------------------------------------------------- |
| Project Kickoff         | 2026-05-19 | Sprint 1 begins                                      |
| MVP Delivery            | 2026-09-04 | End of Sprint 8 — stakeholder demo and feedback      |
| Clinical Pipeline Demo  | 2026-11-13 | End of Sprint 13 — clinical intelligence walkthrough |
| Feature Freeze          | 2027-02-13 | End of Sprint 19 — no new features after this date   |
| Final Delivery          | 2027-02-27 | End of Sprint 20 — production release                |
| Buffered Delivery (20%) | 2027-04-11 | Contingency target if risks materialize              |

## Team Composition

### Recommended Team Structure

| Role                                  | Count | Responsibility                                                                                      | Allocation | Notes                                                                           |
| ------------------------------------- | ----- | --------------------------------------------------------------------------------------------------- | ---------- | ------------------------------------------------------------------------------- |
| Project Manager / Scrum Master        | 1     | Sprint planning, impediment removal, stakeholder communication, reporting                           | 100%       | Dual PM/SM role given team size                                                 |
| Frontend Developer (React/TypeScript) | 3     | UI implementation (Shadcn/Tailwind), patient flows, staff dashboards, accessibility                 | 100%       | 29 screens + 7 modals; WCAG 2.2 AA compliance                                   |
| Backend Developer (ASP.NET Core / C#) | 1     | API development, Clean Architecture layers, MediatR handlers, database integration, ML.NET pipeline | 100%       | **Critical path risk** — single point of failure for 60% of effort (see RK-001) |
| QA Engineer                           | 1     | Test planning, manual + automated testing, accessibility audits, HIPAA compliance validation        | 100%       | 1:4 developer ratio (below recommended 1:3; see RK-003)                         |

### RACI Matrix

| Activity                    | PM  | FE Dev | BE Dev | QA  |
| --------------------------- | --- | ------ | ------ | --- |
| Sprint Planning             | A/R | C      | C      | C   |
| UI Implementation           | I   | R      | I      | C   |
| API Development             | I   | C      | R      | I   |
| Database Schema Design      | I   | I      | R      | I   |
| ML.NET / NER Pipeline       | I   | I      | R      | C   |
| Component Testing           | I   | R      | R      | A/R |
| Integration Testing         | C   | C      | C      | R   |
| Accessibility Testing       | I   | C      | I      | R   |
| HIPAA Compliance Validation | A   | C      | C      | R   |
| Sprint Review & Demo        | R   | R      | R      | C   |
| Backlog Refinement          | R   | C      | C      | C   |
| Release Management          | A/R | C      | C      | R   |

_R = Responsible, A = Accountable, C = Consulted, I = Informed_

## Cost Baseline

### Effort Estimate

| Epic ID   | Epic Title                                   | Base SP | AI Factor | Adjusted SP | Category       | Signal Source     |
| --------- | -------------------------------------------- | ------- | --------- | ----------- | -------------- | ----------------- |
| EP-TECH   | Project Scaffolding & Infrastructure         | 18      | 0.70      | 13          | Foundation     | [SOURCE:INFERRED] |
| EP-DATA   | Core Data Layer                              | 28      | 0.75      | 21          | Foundation     | [SOURCE:INFERRED] |
| EP-001    | Patient Authentication & Profile Management  | 30      | 0.75      | 21          | Feature        | [SOURCE:INFERRED] |
| EP-002    | Appointment Search, Booking & Management     | 42      | 0.80      | 34          | Feature        | [SOURCE:INFERRED] |
| EP-003    | Staff Walk-In & Queue Management             | 18      | 0.75      | 13          | Feature        | [SOURCE:INFERRED] |
| EP-004    | Calendar Integration & Notification Pipeline | 28      | 0.80      | 21          | Integration    | [SOURCE:INFERRED] |
| EP-005    | Patient Intake (AI Conversational + Manual)  | 40      | 0.85      | 34          | Feature        | [SOURCE:INFERRED] |
| EP-006    | Clinical Document Upload & Processing        | 28      | 0.80      | 21          | Feature        | [SOURCE:INFERRED] |
| EP-007    | AI Clinical Data Extraction (NER Pipeline)   | 55      | 0.90      | 55          | AI/ML          | [SOURCE:INFERRED] |
| EP-008    | 360-Degree Patient View & Code Mapping       | 40      | 0.85      | 34          | Feature        | [SOURCE:INFERRED] |
| EP-009    | No-Show Risk Scoring                         | 11      | 0.75      | 8           | Feature        | [SOURCE:INFERRED] |
| EP-010    | Admin Dashboard & Audit Logging              | 28      | 0.75      | 21          | Feature        | [SOURCE:INFERRED] |
| EP-011    | Security Hardening & HIPAA Compliance        | 11      | 0.75      | 8           | Non-Functional | [SOURCE:INFERRED] |
| EP-012    | Design System Foundation                     | 28      | 0.70      | 21          | Foundation     | [SOURCE:INFERRED] |
| EP-013    | Accessibility & Error Handling               | 18      | 0.75      | 13          | Non-Functional | [SOURCE:INFERRED] |
| **TOTAL** |                                              | **423** |           | **338**     |                |                   |

**Estimation Methodology**:

- **1 Story Point = 1 person-day** (single-developer day of productive work)
- **Base estimates** derived from requirement count, technical complexity, and integration risk per epic using parametric estimation
- **AI reduction factors** applied per epic based on automatable percentage (scaffolding 0.70, CRUD/forms 0.75, complex logic 0.80, AI/ML 0.90)
- **Fibonacci snap** applied to final values for planning poker compatibility (1, 2, 3, 5, 8, 13, 21, 34, 55, 89)
- **Basis**: Spec (42 FRs), Design (14 NFRs, 13 TRs, 10 DRs, 8 AIRs, 8 ADDs), Epics (121 mapped requirements)

### Cost Breakdown

| Cost Category                          | Duration        | Rate Assumption                                     | Total            |
| -------------------------------------- | --------------- | --------------------------------------------------- | ---------------- |
| Development Effort (4 devs × 40 weeks) | 800 person-days | Market rate not applied (team assumed in-place)     | 338 SP delivered |
| QA Effort (1 QA × 40 weeks)            | 200 person-days | Embedded in team cost                               | Included         |
| PM Effort (1 PM × 40 weeks)            | 200 person-days | Embedded in team cost                               | Included         |
| Infrastructure (Hosting)               | Ongoing         | Free-tier only (Vercel, Railway/Render, Supabase)   | $0               |
| Third-Party Services                   | Ongoing         | Free-tier APIs (Google Calendar, SMTP)              | $0               |
| Tooling & Licenses                     | Ongoing         | Open-source stack (React, .NET, PostgreSQL, ML.NET) | $0               |

> **Note**: No budget ceiling provided. Cost tracking is effort-based (story points). Infrastructure costs are constrained to $0 by spec requirement CON-003/CON-004.

## Cost Control Plan

### Budget Monitoring

| Metric                         | Frequency  | Owner | Threshold                               | Escalation                               |
| ------------------------------ | ---------- | ----- | --------------------------------------- | ---------------------------------------- |
| Sprint Velocity (SP completed) | Per sprint | PM    | <14 SP/sprint for 2 consecutive sprints | Team retrospective + capacity review     |
| Cumulative SP Burndown         | Per sprint | PM    | >10% deviation from planned trajectory  | PM escalates to stakeholders             |
| Scope Change Requests          | Per sprint | PM    | >3 pending change requests in backlog   | Change Control Board review              |
| Defect Escape Rate             | Per sprint | QA    | >5 defects escaping to UAT per sprint   | QA process review                        |
| BE Dev Utilization             | Weekly     | PM    | >95% sustained for 3+ weeks             | RK-001 trigger — activate cross-training |
| Risk Trigger Watch             | Bi-weekly  | PM    | Any risk score increase ≥2 points       | Risk review meeting                      |

### Change Control Process

1. **Request**: Any team member submits a change request with scope, effort estimate, and impacted requirements
2. **Triage**: PM assesses impact on timeline, effort, and risk within 1 business day
3. **Classification**:
   - **Minor** (<3 SP, no milestone impact): PM approves; added to current sprint backlog
   - **Moderate** (3–8 SP, no milestone shift): PM + Product Owner approve; scheduled for next sprint
   - **Major** (>8 SP or milestone shift): Requires stakeholder sign-off; formal re-planning
4. **Documentation**: All approved changes logged with rationale, updated traceability, and revised estimates
5. **Scope Freeze**: No new feature requests accepted after MS-006 (Feature Complete — Sprint 19)

## Risk Management Plan

### Risk Register

---

#### RK-001 — Backend Developer Bottleneck

- **Category**: Resource
- **Probability**: High | **Impact**: High | **Risk Score**: 9
- **Description**: Single backend developer handles 60% of total platform effort including Clean Architecture, MediatR handlers, Hangfire background jobs, OpenIddict authentication, ML.NET pipeline, PostgreSQL schema design, and all API endpoints. This creates a critical-path single point of failure.
- **Trigger**: BE developer utilization exceeds 95% for 3+ consecutive weeks; sprint velocity drops below 14 SP for 2 sprints
- **Mitigation**:
  1. Cross-train 1 FE developer on simple BE tasks (CRUD endpoints, basic MediatR handlers) starting Sprint 3
  2. Front-load BE-heavy epics (EP-TECH, EP-DATA) to reduce late-stage BE pressure
  3. AI-pair programming to accelerate BE boilerplate generation (30% reduction factor applied)
  4. Identify contract BE developer for surge capacity if velocity drops below threshold
- **Contingency**: Defer EP-010 (Admin Dashboard) to post-release if BE capacity is exhausted by clinical pipeline work
- **Owner**: PM
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: Team composition 3 FE:1 BE; backend comprises ~60% of total adjusted SP

---

#### RK-002 — ML.NET NER Model Accuracy Below Target

- **Category**: Technical
- **Probability**: Medium | **Impact**: High | **Risk Score**: 6
- **Description**: The custom ML.NET Named Entity Recognition model may fail to achieve the >98% AI-Human Agreement Rate target (NFR-003) due to insufficient labeled training data for clinical domains (medications, procedures, diagnoses, vitals).
- **Trigger**: Model accuracy plateaus below 95% after Sprint 14; domain-specific entity types have >5% error rate
- **Mitigation**:
  1. Start with rule-based extraction for high-frequency patterns (vitals, common medications) as a baseline
  2. Build iterative training pipeline: extract → human-review → label → retrain
  3. Maintain human-in-the-loop verification for all AI-extracted data (spec: all data is "suggested", never auto-confirmed)
  4. Curate domain-specific training corpus starting in Sprint 5
- **Contingency**: Deploy rule-based extraction as primary method; NER as supplementary suggestion engine with lower confidence threshold
- **Owner**: BE Developer
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: AIR-002, AIR-008, NFR-003; custom NER training requires domain-specific labeled data not yet available

---

#### RK-003 — QA Throughput Bottleneck

- **Category**: Resource
- **Probability**: Medium | **Impact**: Medium | **Risk Score**: 4
- **Description**: 1 QA engineer supporting 4 developers (1:4 ratio vs recommended 1:3) may create testing backlogs, especially during Sprints 14–19 when clinical pipeline, AI features, and admin dashboard converge.
- **Trigger**: Test execution falls >1 sprint behind development completion; untested features accumulate past 2 sprints
- **Mitigation**:
  1. Shift-left testing: developers write unit tests (≥80% coverage target) and component tests
  2. Automated E2E test suite using Playwright from Sprint 2
  3. CI pipeline gates: no merge without passing unit + integration tests
  4. QA focuses on exploratory testing, accessibility audits, and HIPAA compliance validation
- **Contingency**: Developers assume testing responsibility for their own features during peak sprints; QA prioritizes critical-path and security-sensitive flows
- **Owner**: QA Engineer
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: Team composition 1 QA:4 devs; 36 screens + HIPAA compliance + AI verification

---

#### RK-004 — Healthcare Domain Knowledge Gap

- **Category**: Resource
- **Probability**: Medium | **Impact**: Medium | **Risk Score**: 4
- **Description**: Development team may lack healthcare domain expertise including HIPAA compliance requirements, clinical workflows, ICD-10-CM/CPT-4 coding systems, HL7/FHIR data formats, and DICOM imaging standards.
- **Trigger**: Repeated incorrect implementation of clinical data handling; compliance review reveals gaps in PHI handling
- **Mitigation**:
  1. HIPAA compliance training for all team members in Sprint 1
  2. Domain expert consultation sessions bi-weekly during Sprints 5–16 (clinical pipeline development)
  3. ICD-10/CPT reference documentation integrated into development workflow
  4. Clinical workflow walkthrough with end-users before Sprint 5 planning
- **Contingency**: Engage healthcare IT consultant for targeted reviews of clinical pipeline implementation
- **Owner**: PM
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: Stack includes FHIR/HL7, DICOM, ICD-10-CM, CPT-4; FR-019, DR-003, DR-006

---

#### RK-005 — Free-Tier Infrastructure Limitations

- **Category**: Technical
- **Probability**: Medium | **Impact**: Medium | **Risk Score**: 4
- **Description**: Free-tier hosting platforms (Vercel, Railway/Render, Supabase) impose limits on compute, storage, egress, and background job execution that may not support production workloads — especially Hangfire background jobs, ML.NET model inference, and multi-format document processing.
- **Trigger**: Cold start latency exceeds 3 seconds; background job queue exceeds 100 pending tasks; database storage approaches free-tier limit
- **Mitigation**:
  1. Architecture designed for cloud portability (containerized .NET, standard PostgreSQL)
  2. Monitor resource usage dashboards from Sprint 4 onward
  3. Optimize background job scheduling (off-peak batching, job priority queues)
  4. Document processing pipeline designed for async, chunked execution
- **Contingency**: Migrate to alternative free tiers (Fly.io, Koyeb) or negotiate sponsored tier with hosting provider
- **Owner**: BE Developer
- **Signal Source**: [SOURCE:INPUT]
- **Basis**: NFR-014, CON-003, CON-004; spec explicitly constrains to free/open-source hosting

---

#### RK-006 — Third-Party API Rate Limits and Downtime

- **Category**: External
- **Probability**: Low | **Impact**: Medium | **Risk Score**: 2
- **Description**: Google Calendar API, Microsoft Graph API, and SMS/email gateways impose rate limits or may experience unplanned downtime, affecting calendar sync and notification delivery.
- **Trigger**: API rate limit responses (HTTP 429) exceed 5% of requests; notification delivery rate drops below 95%
- **Mitigation**:
  1. Async notification delivery with exponential backoff (max 3 retries, per ADD-6)
  2. Graceful degradation: booking flow never blocked by calendar sync failures
  3. Multi-channel fallback: email → in-app notification if SMS fails
  4. Rate limit monitoring and request throttling at application layer
- **Contingency**: Queue failed notifications for batch retry during off-peak hours
- **Owner**: BE Developer
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: FR-011, FR-012, NFR-011; ADD-6 async notification architecture

---

#### RK-007 — Clinical Document Processing Complexity

- **Category**: Technical
- **Probability**: Medium | **Impact**: High | **Risk Score**: 6
- **Description**: The 7-stage document processing pipeline (upload → validation → malware scan → format detection → extraction → NER → consolidation) must handle multi-format inputs (PDF, DOCX, OCR images, DICOM, HL7/FHIR). Edge cases in format detection and OCR quality may cause pipeline failures.
- **Trigger**: Document processing success rate drops below 90%; pipeline error rate exceeds 10% for any single format
- **Mitigation**:
  1. Prioritize PDF and DOCX support first (highest volume formats) — deliver in Sprint 10
  2. Add OCR (image), DICOM, and HL7/FHIR formats incrementally in subsequent sprints
  3. Robust per-stage error handling with partial success support (process what's extractable)
  4. Document processing status dashboard for staff visibility
- **Contingency**: Unsupported formats logged with manual review flag; staff can manually enter data from unprocessable documents
- **Owner**: BE Developer
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: DR-003, FR-019; ADD-7 (7-stage pipeline); 5+ document format types

---

#### RK-008 — Holiday Season Schedule Impact

- **Category**: Schedule
- **Probability**: High | **Impact**: Low | **Risk Score**: 3
- **Description**: Sprints 15–16 (December 2026) coincide with year-end holidays, reducing team availability by an estimated 30–40% due to vacation and reduced working hours.
- **Trigger**: Team availability falls below 60% for any sprint during November–January
- **Mitigation**:
  1. Plan reduced sprint capacity (10–12 SP instead of 17 SP) for Sprints 15–16
  2. Front-load complex AI/ML work to complete before Sprint 15
  3. Use holiday sprint for documentation, tech debt reduction, and test coverage improvement
  4. 20% schedule buffer already accounts for reduced-capacity periods
- **Contingency**: Extend timeline by 1 sprint if holiday impact exceeds buffer
- **Owner**: PM
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: Likely timeline places Sprints 15–16 in December 2026; standard holiday impact

---

#### RK-009 — Preferred Slot Swap Race Conditions

- **Category**: Technical
- **Probability**: Medium | **Impact**: Medium | **Risk Score**: 4
- **Description**: Multiple patients selecting the same preferred appointment slot creates a concurrency challenge. Without proper locking, double-bookings or inconsistent slot states may occur.
- **Trigger**: Concurrent slot swap requests for the same time slot; test scenario with simultaneous bookings fails
- **Mitigation**:
  1. First-come-first-served ordering by booking timestamp (spec requirement)
  2. Optimistic concurrency control with row versioning in PostgreSQL
  3. Redis distributed locks for slot reservation window (30-second hold per ADD-5)
  4. Comprehensive concurrency test suite covering race condition scenarios
- **Contingency**: Serialize slot swap requests through a single-threaded queue if distributed locking proves insufficient
- **Owner**: BE Developer
- **Signal Source**: [SOURCE:INPUT]
- **Basis**: FR-007; spec Risks section; ADD-5 slot management architecture

---

#### RK-010 — Scope Creep from Clinical Edge Cases

- **Category**: Scope
- **Probability**: Medium | **Impact**: High | **Risk Score**: 6
- **Description**: Healthcare clinical workflows are inherently complex. Development may surface edge cases in clinical data handling, code mapping, or document processing not captured in the 42 functional requirements — leading to unplanned scope expansion.
- **Trigger**: >3 scope change requests in any single sprint; total unplanned work exceeds 10% of sprint capacity for 2+ consecutive sprints
- **Mitigation**:
  1. Strict change control process (see Cost Control Plan)
  2. Bi-weekly backlog refinement with clinical workflow validation
  3. Scope freeze after MS-006 (Sprint 19) — no new features accepted
  4. Edge cases documented as enhancement requests for post-release backlog
- **Contingency**: Defer non-critical edge case handling to post-release Phase 2
- **Owner**: PM
- **Signal Source**: [SOURCE:INFERRED]
- **Basis**: 42 FRs cover primary scenarios; clinical domain inherent complexity

---

### Risk Summary Matrix

| Risk ID | Title                                | Category  | Probability | Impact | Score | Priority |
| ------- | ------------------------------------ | --------- | ----------- | ------ | ----- | -------- |
| RK-001  | Backend Developer Bottleneck         | Resource  | High        | High   | 9     | Critical |
| RK-002  | ML.NET NER Model Accuracy            | Technical | Medium      | High   | 6     | High     |
| RK-007  | Document Processing Complexity       | Technical | Medium      | High   | 6     | High     |
| RK-010  | Scope Creep from Clinical Edge Cases | Scope     | Medium      | High   | 6     | High     |
| RK-003  | QA Throughput Bottleneck             | Resource  | Medium      | Medium | 4     | Medium   |
| RK-004  | Healthcare Domain Knowledge Gap      | Resource  | Medium      | Medium | 4     | Medium   |
| RK-005  | Free-Tier Infrastructure Limitations | Technical | Medium      | Medium | 4     | Medium   |
| RK-009  | Preferred Slot Swap Race Conditions  | Technical | Medium      | Medium | 4     | Medium   |
| RK-008  | Holiday Season Schedule Impact       | Schedule  | High        | Low    | 3     | Low      |
| RK-006  | Third-Party API Rate Limits          | External  | Low         | Medium | 2     | Low      |

## Communication Plan

| Ceremony                     | Frequency                                       | Duration       | Participants            | Purpose                                            |
| ---------------------------- | ----------------------------------------------- | -------------- | ----------------------- | -------------------------------------------------- |
| Daily Standup                | Daily                                           | 15 min         | All team members        | Progress updates, impediment identification        |
| Sprint Planning              | Bi-weekly (Sprint start)                        | 2 hours        | PM, FE Devs, BE Dev, QA | Sprint goal, backlog selection, task decomposition |
| Sprint Review / Demo         | Bi-weekly (Sprint end)                          | 1 hour         | All team + stakeholders | Feature demonstration, stakeholder feedback        |
| Sprint Retrospective         | Bi-weekly (Sprint end)                          | 1 hour         | All team members        | Process improvement, team health check             |
| Backlog Refinement           | Weekly (mid-sprint)                             | 1 hour         | PM, FE Devs, BE Dev, QA | Story estimation, acceptance criteria review       |
| Risk Review                  | Bi-weekly                                       | 30 min         | PM, Tech Lead (BE Dev)  | Risk register update, trigger monitoring           |
| Stakeholder Report           | Monthly                                         | Written report | PM → Stakeholders       | Progress summary, milestone status, risk dashboard |
| Architecture Decision Review | Per milestone or when new technology introduced | 1 hour         | PM, FE Lead, BE Dev     | Technical decision documentation (ADD format)      |

## Success Metrics

| Metric                    | Target                                       | Measurement Method               | Evaluation Point          |
| ------------------------- | -------------------------------------------- | -------------------------------- | ------------------------- |
| Sprint Velocity Stability | 17 ± 3 SP/sprint (after Sprint 3)            | Sprint burndown charts           | Per sprint                |
| Requirement Coverage      | 100% of 121 requirements implemented         | Traceability matrix vs. epics.md | MS-006 (Feature Complete) |
| Unit Test Coverage        | ≥80% line coverage (FE + BE)                 | CI pipeline coverage reports     | Per sprint                |
| Defect Escape Rate        | <5 defects per sprint escaping to UAT        | Defect tracking log              | Sprint 19–21              |
| Accessibility Compliance  | WCAG 2.2 Level AA — zero critical violations | Automated + manual audit         | MS-006                    |
| API Response Time (P95)   | <200ms for read operations                   | Performance test suite           | MS-005, MS-007            |
| No-Show Rate Reduction    | Measurable reduction from 15% baseline       | Platform analytics post-launch   | 90 days post-release      |
| Clinical Prep Time        | ≤2 minutes per patient (from 20+ min)        | Staff time-motion study          | 60 days post-release      |
| AI-Human Agreement Rate   | >98% for suggested clinical data             | Verification audit sampling      | MS-005, post-release      |
| Platform Uptime           | ≥99.9% over rolling 30-day periods           | Monitoring dashboard             | Post-release              |
| HIPAA Compliance          | Zero violations                              | Compliance audit                 | MS-007, post-release      |

## Sprint Planning Bridge

| Parameter              | Value                                                | Basis                                                                                                                    |
| ---------------------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| Sprint Duration        | 2 weeks                                              | Derived from team size (6) and project complexity; enables rapid feedback cycles for clinical domain                     |
| Team Velocity          | 17 SP/sprint                                         | 4 developers × 10 working days × 0.70 focus factor × 0.60 utilization = ~17 SP; conservative for green-field ramp-up     |
| Story Point Definition | 1 SP = 1 person-day of productive development effort | Parametric estimation basis; includes coding, code review, and developer-level testing                                   |
| Buffer                 | 20% schedule buffer applied to likely estimate       | Default buffer per elicitation; accounts for RK-001 (BE bottleneck), RK-008 (holiday impact), and estimation uncertainty |

**Sprint Allocation Overview**

| Sprint    | Dates                 | Epics                                     | Planned SP | Milestone                  |
| --------- | --------------------- | ----------------------------------------- | ---------- | -------------------------- |
| Sprint 1  | May 19 – May 30, 2026 | EP-TECH                                   | 13         | MS-001 (Kickoff)           |
| Sprint 2  | Jun 2 – Jun 13, 2026  | EP-DATA, EP-012 (start)                   | 17         | —                          |
| Sprint 3  | Jun 16 – Jun 27, 2026 | EP-012 (complete), EP-013                 | 17         | —                          |
| Sprint 4  | Jun 30 – Jul 10, 2026 | EP-DATA (complete), EP-013 (complete)     | 17         | MS-002 (Foundation)        |
| Sprint 5  | Jul 14 – Jul 25, 2026 | EP-001                                    | 17         | —                          |
| Sprint 6  | Jul 28 – Aug 7, 2026  | EP-001 (complete), EP-011, EP-002 (start) | 17         | —                          |
| Sprint 7  | Aug 11 – Aug 22, 2026 | EP-002                                    | 17         | —                          |
| Sprint 8  | Aug 25 – Sep 4, 2026  | EP-002 (complete)                         | 17         | MS-003 (MVP)               |
| Sprint 9  | Sep 8 – Sep 19, 2026  | EP-003, EP-006 (start)                    | 17         | —                          |
| Sprint 10 | Sep 22 – Oct 2, 2026  | EP-006 (complete), EP-009                 | 17         | —                          |
| Sprint 11 | Oct 5 – Oct 16, 2026  | EP-007 (start)                            | 17         | —                          |
| Sprint 12 | Oct 19 – Oct 30, 2026 | EP-007                                    | 17         | —                          |
| Sprint 13 | Nov 3 – Nov 13, 2026  | EP-007 (complete)                         | 17         | MS-004 (Clinical Pipeline) |
| Sprint 14 | Nov 16 – Nov 27, 2026 | EP-005 (start)                            | 17         | —                          |
| Sprint 15 | Dec 1 – Dec 12, 2026  | EP-005 (complete), EP-008 (start)         | 12\*       | —                          |
| Sprint 16 | Dec 15 – Dec 26, 2026 | EP-008                                    | 12\*       | —                          |
| Sprint 17 | Jan 5 – Jan 16, 2027  | EP-008 (complete), EP-004 (start)         | 17         | MS-005 (AI Intelligence)   |
| Sprint 18 | Jan 19 – Jan 30, 2027 | EP-004 (complete), EP-010 (start)         | 17         | —                          |
| Sprint 19 | Feb 2 – Feb 13, 2027  | EP-010 (complete)                         | 17         | MS-006 (Feature Complete)  |
| Sprint 20 | Feb 16 – Feb 27, 2027 | UAT, bug fixes, performance tuning        | 13         | MS-007 (Release)           |

_\*Sprints 15–16: Reduced capacity (12 SP) due to holiday season impact (RK-008)_

## Traceability

### Requirement Coverage

| Epic ID   | Epic Title                                   | Requirements Mapped | Requirement IDs                                                                             | Coverage |
| --------- | -------------------------------------------- | ------------------- | ------------------------------------------------------------------------------------------- | -------- |
| EP-TECH   | Project Scaffolding & Infrastructure         | 8                   | TR-001, TR-002, TR-003, TR-007, TR-009, TR-011, TR-012, TR-013                              | 100%     |
| EP-DATA   | Core Data Layer                              | 8                   | DR-001, DR-002, DR-004, DR-005, DR-008, DR-010, NFR-008, NFR-014                            | 100%     |
| EP-001    | Patient Authentication & Profile Management  | 9                   | FR-001, FR-002, FR-003, FR-004, FR-005, FR-038, NFR-012, NFR-013, TR-004                    | 100%     |
| EP-002    | Appointment Search, Booking & Management     | 10                  | FR-006, FR-007, FR-008, FR-009, FR-010, FR-026, FR-027, FR-031, FR-032, NFR-009             | 100%     |
| EP-003    | Staff Walk-In & Queue Management             | 6                   | FR-013, FR-014, FR-015, FR-016, FR-017, FR-036                                              | 100%     |
| EP-004    | Calendar Integration & Notification Pipeline | 8                   | FR-011, FR-012, FR-028, FR-029, FR-030, NFR-010, NFR-011, AIR-007                           | 100%     |
| EP-005    | Patient Intake (AI Conversational + Manual)  | 10                  | FR-018, FR-019, FR-020, FR-021, AIR-001, AIR-003, AIR-004, AIR-006, DR-003, NFR-002         | 100%     |
| EP-006    | Clinical Document Upload & Processing        | 8                   | FR-019, FR-020, FR-021, DR-003, DR-006, TR-005, TR-006, TR-010                              | 100%     |
| EP-007    | AI Clinical Data Extraction (NER Pipeline)   | 8                   | FR-022, FR-023, AIR-002, AIR-005, AIR-008, NFR-003, DR-009, TR-008                          | 100%     |
| EP-008    | 360-Degree Patient View & Code Mapping       | 9                   | FR-024, FR-025, FR-035, AIR-004, AIR-006, DR-003, DR-006, NFR-002, NFR-003                  | 100%     |
| EP-009    | No-Show Risk Scoring                         | 4                   | FR-033, FR-034, AIR-007, DR-009                                                             | 100%     |
| EP-010    | Admin Dashboard & Audit Logging              | 8                   | FR-037, FR-039, FR-040, FR-041, FR-042, NFR-007, DR-007, TR-007                             | 100%     |
| EP-011    | Security Hardening & HIPAA Compliance        | 7                   | FR-038, NFR-004, NFR-005, NFR-006, NFR-012, NFR-013, TR-004                                 | 100%     |
| EP-012    | Design System Foundation                     | 9                   | UXR-001 through UXR-009 (design system tokens, typography, spacing, color, responsive grid) | 100%     |
| EP-013    | Accessibility & Error Handling               | 9                   | UXR-010 through UXR-018 (WCAG 2.2 AA, focus management, screen reader, error states)        | 100%     |
| **TOTAL** |                                              | **121**             |                                                                                             | **100%** |
