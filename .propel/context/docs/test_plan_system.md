# E2E Test Plan: Unified Patient Access and Clinical Intelligence Platform

## 1. Test Objectives
- Validate end-to-end patient and staff workflows for booking, intake, clinical document processing, and administration with requirement traceability to FR-001 through FR-042 and UC-001 through UC-020. [SOURCE:INPUT] Basis: Objectives are derived from `.propel/context/docs/spec.md` functional requirements and use cases.
- Mitigate highest business and safety risks: no-show reduction, double booking prevention, clinical conflict detection, secure PHI handling, and audit integrity. [SOURCE:INFERRED] Basis: Risk emphasis is inferred from success criteria and HIPAA-focused requirements in spec/design.
- Establish release confidence gates across functional, NFR, technical integration, data integrity, and AI quality/safety requirements for full-scope validation. [SOURCE:INFERRED] Basis: Full-scope test planning requires cross-category gates per the test-plan workflow.

## 2. Scope

### In Scope
| Category | Items | Requirement IDs |
|----------|-------|-----------------|
| Functional | Authentication, booking/scheduling, intake mode switch, document upload and extraction, reminders, no-show scoring, insurance pre-check, admin and audit features | FR-001 to FR-042 |
| User Journeys | Core patient, staff, and admin journeys including cross-UC transitions and exception handling | UC-001 to UC-020 |
| Non-Functional | Availability, performance, security, access control, graceful degradation, upload limits, retention, deployability constraints | NFR-001 to NFR-014 |
| Technical | Frontend/backend stack implementation, auth server, ORM and migrations, background jobs, malware scan, monitoring, AI integrations, calendar APIs | TR-001 to TR-014 |
| Data | PHI encryption, caching/locking, document formats, dedup/conflict persistence, code-set versioning, deletion and integrity rules | DR-001 to DR-010 |
| AI Models | Conversational intake NLP, document NER extraction, human-in-loop verification, confidence thresholds, code mapping quality, safety/guardrails | AIR-001 to AIR-008 |

### Out of Scope
- Provider-facing login/workflows and direct provider actions are excluded in Phase 1. [SOURCE:INPUT] Basis: Explicitly marked out of scope in spec.
- Payment gateway processing and claims submission are excluded. [SOURCE:INPUT] Basis: Explicitly out of scope in spec.
- Bi-directional EHR integration and advanced DICOM imaging analysis are excluded from this plan baseline. [SOURCE:INPUT] Basis: Explicitly constrained in spec/design.

## 3. Test Strategy

### Test Pyramid Allocation
| Level | Coverage Target | Focus |
|-------|-----------------|-------|
| E2E | 10% [SOURCE:INFERRED] Basis: Critical healthcare journeys need stronger E2E confidence while staying within 5-10% guidance. | Critical user journeys only |
| Integration | 30% [SOURCE:INFERRED] Basis: Heavy external integrations (OAuth, calendar, SMS/email, AI, malware scan) require robust contract testing. | API contracts, service boundaries |
| Unit | 60% [SOURCE:INFERRED] Basis: Core business logic and validation rules are best validated with fast deterministic unit tests. | Business logic, edge cases |

### E2E Approach
- **Horizontal**: UI-driven flows for patient registration, booking, intake, upload, and admin operations.
- **Vertical**: API to DB validation for slot locks, audit immutability, risk tiering, and data aggregation correctness.

### Environment Strategy
| Environment | Purpose | Data Strategy |
|-------------|---------|---------------|
| DEV | Smoke and feature sanity checks per commit | Mocked and seeded synthetic data |
| QA | Full regression and defect verification | Versioned snapshot datasets with synthetic PHI |
| Staging | Pre-prod release validation and go/no-go | Production-like anonymized datasets |

## 4. Test Cases

### 4.1 Functional Test Cases

#### TC-FR-001-01: Patient Registration and Login Across Auth Methods
| Field | Value |
|-------|-------|
| Requirement | FR-001 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement explicitly mandates Google, Microsoft, and email/password login paths for patients. |
| Use Case | UC-001 |
| Type | happy_path |

**Preconditions:**
- Patient registration page is accessible.
- OAuth providers and email verification service are reachable.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | New patient is on auth page | Patient chooses Google login | OAuth consent flow starts successfully |
| 2 | OAuth consent granted | System exchanges token | Patient account is created/linked |
| 3 | Patient logs in with email/password | Credentials are valid | JWT issued and dashboard is shown |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| auth_provider | google | unknown_provider | microsoft |
| email | patient.one@test.local | patient.one | a@b.co |

**Expected Results:**
- [ ] Patient can authenticate using each allowed method.
- [ ] Session token is issued and user lands on dashboard.

**Postconditions:**
- Active patient session exists and role is Patient.

---

#### TC-FR-002-01: Staff/Admin MFA Enforcement
| Field | Value |
|-------|-------|
| Requirement | FR-002 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement explicitly mandates MFA for staff/admin authentication. |
| Use Case | UC-002 |
| Type | error |

**Preconditions:**
- Staff/admin user exists with MFA enabled.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Staff enters valid email/password | MFA step appears | Access is not granted yet |
| 2 | Staff submits invalid MFA code | System validates code | Login is rejected |
| 3 | Staff submits valid MFA code | System validates code | JWT issued and role dashboard loaded |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| role | staff | patient | admin |
| mfa_code | 123456 | 000000 | 999999 |

**Expected Results:**
- [ ] MFA is required for staff/admin every login.
- [ ] Invalid MFA prevents access.

**Postconditions:**
- Auth audit record exists for successful and failed MFA attempts.

---

#### TC-FR-010-01: Double-Booking Conflict Prevention
| Field | Value |
|-------|-------|
| Requirement | FR-010 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement explicitly mandates optimistic concurrency and conflict handling. |
| Use Case | UC-003 |
| Type | edge_case |

**Preconditions:**
- Same slot is visible to two authenticated patients.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Two patients select same slot | Both attempt booking nearly simultaneously | One request succeeds |
| 2 | Losing request reaches server | Lock/version check runs | Conflict response is returned |
| 3 | Losing patient refreshes availability | Slot list reloads | Booked slot is no longer available |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| slot_id | SLOT-1001 | SLOT-NOT-FOUND | SLOT-1001 with near-simultaneous submit |
| concurrent_users | 2 | 0 | 3 |

**Expected Results:**
- [ ] No double-booking is persisted.
- [ ] Conflict is clearly surfaced to the losing requester.

**Postconditions:**
- Exactly one appointment exists for the slot.

---

#### TC-FR-007-01: Preferred Slot Swap Execution
| Field | Value |
|-------|-------|
| Requirement | FR-007 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement defines automatic preferred-slot swap and slot release behavior. |
| Use Case | UC-004 |
| Type | happy_path |

**Preconditions:**
- Patient has booked an available fallback slot and selected a preferred unavailable slot.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Preferred slot becomes free | Swap engine checks queue | Waiting patient is selected |
| 2 | Swap is executed | Appointment record updates | Preferred slot is assigned to patient |
| 3 | Original slot is released | Availability recalculates | Released slot appears for booking |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| preferred_slot | SLOT-2002 | SLOT-UNKNOWN | SLOT-2002 reopened during retry |
| queue_status | waiting | expired | waiting with multiple contenders |

**Expected Results:**
- [ ] Appointment is auto-swapped correctly.
- [ ] Original slot is released immediately.

**Postconditions:**
- Staff notification event is queued for swap execution.

---

#### TC-FR-017-01: Intake Toggle Data Preservation
| Field | Value |
|-------|-------|
| Requirement | FR-017 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement explicitly requires free toggle between AI/manual modes with data preservation. |
| Use Case | UC-009 |
| Type | edge_case |

**Preconditions:**
- Patient has partially completed AI intake responses.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | AI intake has collected symptoms | Patient switches to manual mode | Previously captured fields are prefilled |
| 2 | Patient edits medications in manual form | Patient switches back to AI mode | Edited values remain visible to patient |
| 3 | Intake is submitted | System persists intake | Final record contains merged responses |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| symptoms | fever and cough | <script>alert(1)</script> | single-character symptom |
| mode_switch_count | 2 | -1 | 10 |

**Expected Results:**
- [ ] No data is lost across repeated mode switches.
- [ ] Latest patient edits win across both modes.

**Postconditions:**
- Intake record marked complete with traceable modification history.

---

#### TC-FR-018-01: AI Intake Failure Auto-Fallback
| Field | Value |
|-------|-------|
| Requirement | FR-018 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement explicitly mandates graceful fallback to manual form without data loss. |
| Use Case | UC-009 |
| Type | error |

**Preconditions:**
- AI intake session is active.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | AI endpoint times out | Patient submits next message | Timeout is detected |
| 2 | Timeout is detected | Fallback logic executes | Manual form loads automatically |
| 3 | Manual form loads | Existing responses are mapped | Patient continues without re-entry |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| ai_response_time_ms | 1200 | -10 | 30000 |
| fallback_status | triggered | not_triggered | triggered after 3 retries |

**Expected Results:**
- [ ] Fallback occurs automatically.
- [ ] Previously captured data is preserved.

**Postconditions:**
- Failure event is logged and session remains active.

---

#### TC-FR-020-01: Upload Validation for Size and Format
| Field | Value |
|-------|-------|
| Requirement | FR-020 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement defines 50MB max and strict format whitelist validation. |
| Use Case | UC-011 |
| Type | edge_case |

**Preconditions:**
- Patient is authenticated and on document upload page.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Valid 10MB PDF is selected | Patient uploads file | Upload is accepted |
| 2 | 51MB PDF is selected | Patient uploads file | Upload is rejected with size error |
| 3 | Unsupported EXE file selected | Patient uploads file | Upload is rejected with format error |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| file_format | pdf | exe | dicom |
| file_size_mb | 10 | 51 | 50 |

**Expected Results:**
- [ ] Only whitelisted formats are accepted.
- [ ] Files above 50MB are rejected with clear feedback.

**Postconditions:**
- Rejected files are not persisted.

---

#### TC-FR-021-01: Malware Scan Quarantine Path
| Field | Value |
|-------|-------|
| Requirement | FR-021 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement mandates malware scanning and quarantine/rejection behavior. |
| Use Case | UC-011 |
| Type | error |

**Preconditions:**
- Malware scanner integration is available.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Upload request received | Scanner runs on file | Scan status is recorded |
| 2 | Scanner returns infected | Processing pipeline continues | File is quarantined and rejected |
| 3 | Rejection generated | Patient UI refreshes | Patient sees security rejection message |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| scan_result | clean | infected | scanner_unavailable |
| file_name | report.pdf | malware-sample.exe | report-50mb.pdf |

**Expected Results:**
- [ ] Infected files never enter extraction stage.
- [ ] Quarantine and notification behavior is executed.

**Postconditions:**
- Security event is written to audit/ops logs.

---

#### TC-FR-024-01: Clinical Conflict Highlighting
| Field | Value |
|-------|-------|
| Requirement | FR-024 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement requires explicit highlighting of conflicting clinical values with provenance. |
| Use Case | UC-013 |
| Type | happy_path |

**Preconditions:**
- Two processed documents contain conflicting medication values.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Aggregation runs on extracted data | Conflicts are detected | Conflict entity is created |
| 2 | Staff opens patient 360 view | Conflict panel is rendered | Both values and source docs are shown |
| 3 | Staff resolves conflict | Resolution is saved | View updates and audit is captured |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| medication_value_1 | Metformin 500mg | blank | Metformin |
| medication_value_2 | Metformin 1000mg | same as value_1 | Metformin 500mg |

**Expected Results:**
- [ ] Conflicting values are visible simultaneously with source references.
- [ ] Resolution workflow updates final patient view state.

**Postconditions:**
- Conflict marked resolved or remains open with explicit status.

---

#### TC-FR-029-01: Notification Retry with Exponential Backoff
| Field | Value |
|-------|-------|
| Requirement | FR-029 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement explicitly defines asynchronous delivery with exponential backoff and max 3 retries. |
| Use Case | UC-015 |
| Type | error |

**Preconditions:**
- Reminder notification job is queued.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | First delivery attempt fails | Retry policy executes | Retry count becomes 1 |
| 2 | Second attempt fails | Backoff delay doubles | Retry count becomes 2 |
| 3 | Third attempt fails | Max retry reached | Permanent failure is recorded |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| retry_limit | 3 | -1 | 3 |
| channel | sms | unknown_channel | email |

**Expected Results:**
- [ ] Retries follow configured exponential pattern.
- [ ] Processing stops after final allowed retry.

**Postconditions:**
- Staff failure alert trigger condition is met.

---

#### TC-FR-033-01: No-Show Risk Tier Classification
| Field | Value |
|-------|-------|
| Requirement | FR-033 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement specifies the exact risk factors and tiering logic inputs. |
| Use Case | UC-016 |
| Type | happy_path |

**Preconditions:**
- Appointment record exists with historical behavior data.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Historical factors are available | Risk engine runs | Numeric risk score is computed |
| 2 | Risk score computed | Tier thresholds applied | Tier is set to Low/Medium/High |
| 3 | Staff schedule page loads | Risk metadata fetched | Tier badge is displayed |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| no_show_count | 2 | -1 | 0 |
| lead_time_days | 10 | -3 | 7 |

**Expected Results:**
- [ ] Risk score uses all required factors.
- [ ] Tier displays correctly in staff-facing views.

**Postconditions:**
- Stored appointment includes risk score and tier.

---

#### TC-FR-037-01: Immutable Audit Logging
| Field | Value |
|-------|-------|
| Requirement | FR-037 |
| Source | [SOURCE:INPUT] |
| Basis | Requirement explicitly mandates append-only immutable audit logging for key actions. |
| Use Case | UC-019 |
| Type | edge_case |

**Preconditions:**
- Audit logging is enabled and protected by DB constraints.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | User performs protected action | Audit writer runs | Insert-only record is created |
| 2 | Admin attempts to update audit row | DB operation executes | Update is denied |
| 3 | Admin attempts deletion | DB operation executes | Delete is denied |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| action_type | AppointmentBooked | null | DataConflictResolved |
| audit_operation | insert | update | delete |

**Expected Results:**
- [ ] Audit rows are insert-only.
- [ ] Mandatory fields (timestamp, actor, action, resource) are present.

**Postconditions:**
- Unauthorized mutation attempts are logged as security events.

---

### 4.2 NFR Test Cases

#### TC-NFR-001-PERF: Availability and Performance Validation
| Field | Value |
|-------|-------|
| Requirement | NFR-001 |
| Category | Performance |

**Preconditions:**
- System deployed in staging.
- Baseline load established.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | System at baseline load | 200 concurrent users execute booking and dashboard flows | Response time P95 < 2.5s |
| 2 | System under load | Monitor error rates for 30 minutes | Error rate < 1% |
| 3 | Peak load reached | Measure throughput | Throughput >= 50 req/sec |

**Acceptance Criteria:**
- [ ] Response time P95 < 2.5s
- [ ] Error rate < 1%
- [ ] Throughput >= 50 requests/second
- [ ] No memory leaks detected
- [ ] CPU utilization < 75%

---

#### TC-NFR-004-SEC: Security and PHI Protection Validation
| Field | Value |
|-------|-------|
| Requirement | NFR-004 |
| Category | Security |

**Preconditions:**
- Application endpoints accessible.
- Test credentials available.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Protected endpoints exposed | Attempt unauthenticated PHI access | Access denied (401/403) |
| 2 | Input fields available | Submit SQL/XSS payloads | Payloads are neutralized |
| 3 | Session established | Attempt token replay/hijack | Session validation blocks replay |

**Acceptance Criteria:**
- [ ] No Critical/High vulnerabilities
- [ ] OWASP Top 10 mitigations validated
- [ ] Authentication enforced on protected endpoints
- [ ] Authorization rules validated
- [ ] Sensitive data encrypted in transit and at rest

---

#### TC-NFR-010-SCALE: Graceful Degradation and Scalability Validation
| Field | Value |
|-------|-------|
| Requirement | NFR-010 |
| Category | Scalability |

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | AI service is unavailable | Patient continues intake | Auto-fallback to manual form occurs |
| 2 | Calendar API returns 5xx | Sync jobs retry asynchronously | User booking remains successful |
| 3 | External dependency recovers | Queued jobs drain | State converges without data loss |

**Acceptance Criteria:**
- [ ] Degraded dependencies do not block core booking flow
- [ ] Retry and queue behavior stays within configured limits
- [ ] No data loss across failover and recovery windows

---

### 4.3 Technical Requirement Test Cases

#### TC-TR-003: OAuth/OIDC Auth Server Integration
| Field | Value |
|-------|-------|
| Requirement | TR-003 |
| Category | Integration/API |

**Preconditions:**
- OpenIddict auth server and social providers are configured.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Auth server is running | OIDC discovery endpoint is called | Metadata is returned per spec |
| 2 | Valid OAuth auth code flow | Token exchange requested | ID/access tokens are returned |
| 3 | Invalid client assertion | Token endpoint called | Proper error response is returned |

**Validation Points:**
- [ ] Contract compliance verified
- [ ] Response format matches specification
- [ ] Error codes returned correctly
- [ ] Timeout handling works

---

#### TC-TR-005: Background Job Processing Pipeline
| Field | Value |
|-------|-------|
| Requirement | TR-005 |
| Category | Platform |

**Preconditions:**
- Hangfire workers and dashboard are operational.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Appointment booked event emitted | Notification job enqueued | Job is visible and scheduled |
| 2 | Delivery fails transiently | Retry policy triggers | Retry backoff and count are correct |
| 3 | Max retries exhausted | Job transitions terminally | Failure alert path is triggered |

**Validation Points:**
- [ ] Contract compliance verified
- [ ] Response format matches specification
- [ ] Error codes returned correctly
- [ ] Timeout handling works

---

#### TC-TR-010: Calendar API Synchronization
| Field | Value |
|-------|-------|
| Requirement | TR-010 |
| Category | Integration/API |

**Preconditions:**
- Google and Microsoft test tenants are configured.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Appointment is created | Sync worker sends create event | Event appears in external calendar |
| 2 | Appointment is rescheduled | Sync worker sends update event | External event is updated |
| 3 | Invalid/expired token | Sync attempt runs | Refresh/retry logic executes and logs failure if final |

**Validation Points:**
- [ ] Contract compliance verified
- [ ] Response format matches specification
- [ ] Error codes returned correctly
- [ ] Timeout handling works

---

### 4.4 Data Requirement Test Cases

#### TC-DR-001: PHI Encryption at Rest Validation
| Field | Value |
|-------|-------|
| Requirement | DR-001 |
| Category | Integrity |

**Preconditions:**
- PostgreSQL with `pgcrypto` enabled.
- Test patient PHI seeded.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | PHI write operation occurs | Patient record is persisted | Encrypted storage is used for protected fields |
| 2 | Direct DB query without decrypt function | Query executed | Ciphertext is returned |
| 3 | Authorized app read path | Data requested via app | Decrypted values are available only through authorized service |

**Validation Points:**
- [ ] Data integrity preserved
- [ ] Referential integrity maintained
- [ ] Audit trail complete
- [ ] Data retention policy enforced

---

#### TC-DR-004: Dedup and Provenance Aggregation Validation
| Field | Value |
|-------|-------|
| Requirement | DR-004 |
| Category | Integrity |

**Preconditions:**
- Multiple extracted records from different documents exist for same patient.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Aggregation starts | Matching entities are merged | Duplicates are collapsed |
| 2 | Source references are available | Aggregated view built | Provenance links remain intact |
| 3 | Staff verifies records | Verification persisted | Final patient view reflects approved data |

**Validation Points:**
- [ ] Data integrity preserved
- [ ] Referential integrity maintained
- [ ] Audit trail complete
- [ ] Data retention policy enforced

---

#### TC-DR-007: Right-to-Delete Cascade Validation
| Field | Value |
|-------|-------|
| Requirement | DR-007 |
| Category | Retention |

**Preconditions:**
- Patient has profile, appointments, documents, extracted data, and cache entries.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Valid deletion request submitted | Deletion workflow starts | Request is authorized and tracked |
| 2 | Deletion workflow executes | Primary and dependent records removed | No orphan records remain |
| 3 | Post-delete query runs | Any patient data lookup attempted | Data is irrecoverable in active stores |

**Validation Points:**
- [ ] Data integrity preserved
- [ ] Referential integrity maintained
- [ ] Audit trail complete
- [ ] Data retention policy enforced

---

### 4.5 AI Requirement Test Cases [CONDITIONAL: If AIR-XXX in scope]

#### TC-AIR-002-RQ: Clinical Extraction Retrieval Quality
| Field | Value |
|-------|-------|
| Requirement | AIR-002 |
| Category | Quality |
| Type | retrieval |

**Preconditions:**
- Local NER model is deployed.
- Curated clinical test corpus is available.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Known clinical document input | Extraction pipeline runs | Expected entities are detected |
| 2 | Extracted entities produced | Ground truth comparison runs | Precision/recall metrics are computed |
| 3 | Quality threshold check | Metrics evaluated | Threshold gates pass/fail deterministically |

**Test Data:**
| Input Type | Value | Expected Output | Evaluation Metric |
|------------|-------|-----------------|-------------------|
| Valid query | "Medication list with dosages" | Medication entities with dosages | Recall >= 0.95 |
| Edge case | "Scanned low-contrast image" | Partial extraction with explicit uncertainty | No silent failure |
| Adversarial | "Malformed OCR text" | Safe failure with review flag | Safety = Pass |

**Acceptance Criteria:**
- [ ] Response relevance score >= 0.90
- [ ] Faithfulness score >= 0.95
- [ ] Latency P95 < 3000ms
- [ ] No hallucinated content
- [ ] PII properly redacted where required
- [ ] Guardrails triggered appropriately

**Postconditions:**
- Model quality metrics and run metadata are logged.
- Audit events recorded for extraction and verification actions.

---

#### TC-AIR-004-GR: Human-in-the-Loop Guardrail Enforcement
| Field | Value |
|-------|-------|
| Requirement | AIR-004 |
| Category | Safety |
| Type | guardrails |

**Preconditions:**
- AI-generated extraction and code suggestions are available for review.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | AI output exists | Staff has not verified yet | Downstream action is blocked |
| 2 | Staff verifies output | Verification saved | Output becomes eligible for downstream use |
| 3 | Staff rejects output | Rejection saved | Fallback/manual path remains required |

**Test Data:**
| Input Type | Value | Expected Output | Evaluation Metric |
|------------|-------|-----------------|-------------------|
| Valid query | "Verified extraction" | Downstream enabled | Guardrail pass |
| Edge case | "Unverified extraction" | Downstream blocked | Guardrail block |
| Adversarial | "Bypass verification request" | Request denied | Safety = Pass |

**Acceptance Criteria:**
- [ ] Response relevance score >= 0.90
- [ ] Faithfulness score >= 0.95
- [ ] Latency P95 < 1000ms
- [ ] No hallucinated content
- [ ] PII properly redacted
- [ ] Guardrails triggered appropriately

**Postconditions:**
- Verification actions are fully audit-traceable.
- No unverified AI output is marked clinically usable.

---

#### TC-AIR-007-LT: No-Show Risk Operational Latency and Stability
| Field | Value |
|-------|-------|
| Requirement | AIR-007 |
| Category | Operational |
| Type | latency |

**Preconditions:**
- Risk scoring service is enabled with historical patient dataset.

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Appointment creation burst | Risk scoring invoked per event | Tier is computed for each event |
| 2 | Under sustained load | P95 latency is measured | Latency remains under target |
| 3 | Service restart/failure | Retry/recovery executes | Scoring resumes without lost events |

**Test Data:**
| Input Type | Value | Expected Output | Evaluation Metric |
|------------|-------|-----------------|-------------------|
| Valid query | "100 appointments with normal history" | Correct tier distribution | Latency P95 <= 500ms |
| Edge case | "Sparse history new patients" | Conservative tier assignment | No crash |
| Adversarial | "Invalid factor payload" | Validation rejection | Safety = Pass |

**Acceptance Criteria:**
- [ ] Response relevance score >= 0.85
- [ ] Faithfulness score >= 0.90
- [ ] Latency P95 < 500ms
- [ ] No hallucinated content
- [ ] PII properly redacted
- [ ] Guardrails triggered appropriately

**Postconditions:**
- Operational metrics include latency, failures, and retry counts.
- Risk decisions remain reproducible from stored factors.

---

### 4.6 E2E Journey Test Cases

#### E2E-001: Patient Booking to Clinical Readiness Journey
| Field | Value |
|-------|-------|
| UC Chain | UC-001 -> UC-003 -> UC-009 -> UC-011 -> UC-012 -> UC-013 -> UC-015 |
| Session | Auth required |

**Preconditions:**
- Patient account exists and is active.
- At least one bookable slot exists.
- Upload and extraction services are online.

**Journey Flow:**
| Phase | Use Case | Action | Expected State | Checkpoint |
|-------|----------|--------|----------------|------------|
| 1 | UC-001 | Patient authenticates | Active patient session | Y |
| 2 | UC-003 | Patient books appointment | Appointment scheduled, confirmation sent | Y |
| 3 | UC-009 | Patient completes intake | Intake saved with mode metadata | N |
| 4 | UC-011 | Patient uploads document | Document enters processing pipeline | Y |
| 5 | UC-012 | AI extracts data | Structured entities are available | N |
| 6 | UC-013 | Staff reviews 360 view | Conflicts visible and resolvable | Y |
| 7 | UC-015 | Reminder sequence executes | Reminder status recorded | Y |

**Detailed Test Steps:**

**Phase 1: UC-001 - Patient Registration and Login**
| Step | Given | When | Then |
|------|-------|------|------|
| 1.1 | Patient on auth page | Signs in with valid method | Session token issued |
| 1.2 | Active session exists | Dashboard opens | Phase 1 complete |

**Phase 2: UC-003 - Patient Books Appointment**
| Step | Given | When | Then |
|------|-------|------|------|
| 2.1 | Slots available | Patient selects and confirms slot | Appointment created |
| 2.2 | Appointment created | Confirmation generated | Phase 2 complete |

**Phase 3: UC-009 - AI Conversational Intake**
| Step | Given | When | Then |
|------|-------|------|------|
| 3.1 | Appointment is scheduled | Patient completes intake prompts | Intake data saved |
| 3.2 | Intake submitted | Mode/timestamps persisted | Journey success for phase |

**Test Data:**
| Entity | Field | Value |
|--------|-------|-------|
| User | Role | patient |
| User | Credentials | patient.e2e@test.local |
| Appointment | Preferred specialty | cardiology |
| ClinicalDocument | File | sample-cardiology-report.pdf |

**Expected Results:**
- [ ] All phases complete without errors
- [ ] Session state maintained across phases
- [ ] Checkpoints validate intermediate states
- [ ] Final state matches success criteria

---

#### E2E-002: Staff and Admin Operations Control Journey
| Field | Value |
|-------|-------|
| UC Chain | UC-002 -> UC-005 -> UC-006 -> UC-016 -> UC-019 -> UC-020 |
| Session | Auth required |

**Preconditions:**
- Staff and admin accounts are active with MFA enabled.
- Same-day queue has at least one pending patient.

**Journey Flow:**
| Phase | Use Case | Action | Expected State | Checkpoint |
|-------|----------|--------|----------------|------------|
| 1 | UC-002 | Staff/Admin authenticates with MFA | Privileged session active | Y |
| 2 | UC-005 | Staff creates walk-in booking | Walk-in appointment created | Y |
| 3 | UC-006 | Staff marks arrival | Queue state updated | Y |
| 4 | UC-016 | Risk score evaluated | Tier displayed on queue | N |
| 5 | UC-019 | Admin reviews audit log | Relevant entries searchable | Y |
| 6 | UC-020 | Admin reviews metrics dashboard | KPIs rendered with trends | Y |

**Detailed Test Steps:**

**Phase 1: UC-002 - Staff/Admin Authentication with MFA**
| Step | Given | When | Then |
|------|-------|------|------|
| 1.1 | Staff login page visible | Valid credentials and MFA entered | Role session active |
| 1.2 | Session active | Dashboard loaded | Phase 1 complete |

**Phase 2: UC-005 - Staff Walk-in Booking**
| Step | Given | When | Then |
|------|-------|------|------|
| 2.1 | Queue management page open | Staff creates walk-in | Appointment appears in queue |
| 2.2 | Queue refreshed | Walk-in persisted | Phase 2 complete |

**Phase 3: UC-006 - Queue Management and Arrival Marking**
| Step | Given | When | Then |
|------|-------|------|------|
| 3.1 | Patient in queue | Staff marks as Arrived | Status changes to Arrived |
| 3.2 | Arrival confirmed | Schedule view updates | Journey success |

**Test Data:**
| Entity | Field | Value |
|--------|-------|-------|
| User | Role | staff/admin |
| User | Credentials | staff.ops@test.local |
| Queue | Status | pending -> arrived |

**Expected Results:**
- [ ] All phases complete without errors
- [ ] Session state maintained across phases
- [ ] Checkpoints validate intermediate states
- [ ] Final state matches success criteria

---

## 5. Entry & Exit Criteria

### Entry Criteria
- [ ] All FR-XXX requirements approved and baselined
- [ ] Test environment provisioned and accessible
- [ ] Test data seeded or available
- [ ] Test cases reviewed and approved
- [ ] Relevant NFR/TR/DR requirements reviewed

### Exit Criteria
- [ ] 100% P0 test cases executed
- [ ] >=95% P0 test cases passed
- [ ] >=90% P1 test cases passed
- [ ] No open Critical/High severity defects
- [ ] NFR thresholds validated
- [ ] All E2E journeys pass end-to-end

## 6. Risk Assessment

| Risk-ID | Risk Description | Impact | Likelihood | Mitigation |
|---------|------------------|--------|------------|------------|
| R-001 | Concurrent booking race conditions may still cause slot-state inconsistency under burst traffic | High | Medium | Concurrency lock stress tests and DB-level optimistic concurrency assertions in CI |
| R-002 | AI extraction or code suggestions may be accepted without adequate human verification | High | Medium | Enforce AIR-004 guardrail tests and release-block on bypass defects |
| R-003 | External dependency failures (calendar/SMS/email/LLM) may degrade user trust if retries fail silently | Medium | High | Mandatory failure-notification tests and observability alerts for terminal failures |
| R-004 | PHI leakage risk from misconfigured logs, API responses, or integration payloads | High | Medium | Security regression suite for redaction/encryption plus periodic OWASP scans |
| R-005 | Audit log mutability defects may weaken compliance posture | High | Low | DB constraint validation and mutation-attempt tests per release |

### Risk-Based Test Prioritization
| Priority | Criteria | Test Focus |
|----------|----------|------------|
| P0 (Must Test) | Impact=High AND Likelihood>=Medium | Critical paths, security, data integrity |
| P1 (Should Test) | Impact=Medium OR Likelihood=High | Feature completeness, edge cases |
| P2 (Could Test) | Remaining scenarios | Nice-to-have validations |

## 7. Traceability Matrix

| Requirement | Type | Priority | Test Cases | E2E Journey | Status |
|-------------|------|----------|------------|-------------|--------|
| FR-001, FR-002, FR-003, FR-004, FR-005 | Functional | P0 | TC-FR-001-01, TC-FR-002-01 | E2E-001, E2E-002 | Planned |
| FR-006, FR-010, FR-013, FR-014, FR-042 | Functional | P0 | TC-FR-010-01 | E2E-001 | Planned |
| FR-007, FR-031 | Functional | P0 | TC-FR-007-01 | E2E-001 | Planned |
| FR-008, FR-009 | Functional | P1 | TC-FR-010-01, TC-FR-037-01 | E2E-002 | Planned |
| FR-011, FR-012 | Functional | P1 | TC-TR-010 | E2E-001 | Planned |
| FR-015, FR-016, FR-017, FR-018 | Functional | P0 | TC-FR-017-01, TC-FR-018-01 | E2E-001 | Planned |
| FR-019, FR-020, FR-021, FR-026, FR-027 | Functional | P0 | TC-FR-020-01, TC-FR-021-01 | E2E-001 | Planned |
| FR-022, FR-023, FR-024, FR-025 | Functional | P0 | TC-FR-024-01, TC-AIR-002-RQ, TC-AIR-004-GR | E2E-001 | Planned |
| FR-028, FR-029, FR-030, FR-032 | Functional | P1 | TC-FR-029-01 | E2E-001 | Planned |
| FR-033, FR-034 | Functional | P1 | TC-FR-033-01, TC-AIR-007-LT | E2E-002 | Planned |
| FR-035 | Functional | P2 | TC-FR-010-01 | E2E-002 | Planned |
| FR-036, FR-037, FR-038, FR-039, FR-040, FR-041 | Functional | P0 | TC-FR-037-01, TC-NFR-004-SEC | E2E-002 | Planned |
| UC-001 to UC-020 | Use Case | P0/P1 | TC-FR-001-01, TC-FR-010-01, TC-FR-017-01, TC-FR-024-01, TC-FR-037-01 | E2E-001, E2E-002 | Planned |
| NFR-001 to NFR-014 | Non-Functional | P0/P1 | TC-NFR-001-PERF, TC-NFR-004-SEC, TC-NFR-010-SCALE | - | Planned |
| TR-001 to TR-014 | Technical | P1 | TC-TR-003, TC-TR-005, TC-TR-010 | E2E-001 | Planned |
| DR-001 to DR-010 | Data | P1 | TC-DR-001, TC-DR-004, TC-DR-007 | - | Planned |
| AIR-001 to AIR-008 | AI | P0/P1 | TC-AIR-002-RQ, TC-AIR-004-GR, TC-AIR-007-LT | E2E-001 | Planned |

**Note:** Priority derived from Risk Assessment (Section 6) based on Impact multiplied by Likelihood.

## 8. Test Data Requirements

| Scenario Type | Data Description | Source | Isolation |
|---------------|------------------|--------|-----------|
| Happy Path | Valid identities, slots, intake answers, upload files, and admin actions | Seeded fixtures | Test-specific |
| Edge Cases | Boundary file sizes, race conditions, retry-limit thresholds, mode-switch loops | Generated | Shared read-only |
| Error Cases | Invalid credentials/MFA, malformed payloads, malware files, external API failures | Static fixtures | Shared read-only |
| E2E Journeys | End-to-end patient/staff/admin datasets with linked appointments and documents | Journey-specific | Journey-isolated |

### Sensitive Data Handling
- [ ] Production data masked/anonymized
- [ ] PII replaced with synthetic data
- [ ] Credentials stored securely

## 9. Defect Management

*Severity scale is a closed set (Critical / High / Medium / Low).*

| Severity | Definition | SLA | Action |
|----------|------------|-----|--------|
| Critical | System unusable, data loss, security breach | Immediate | Block release |
| High | Major feature broken, no workaround | Before release | Must fix |
| Medium | Feature impacted, workaround exists | Next sprint | Should fix |
| Low | Minor issue, cosmetic | Backlog | Could fix |

---
