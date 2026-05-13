# Figma Design Specification - Unified Patient Access & Clinical Intelligence Platform

## 1. Figma Specification

**Platform**: Responsive Web (Mobile-first)

---

## 2. Source References

### Primary Source

| Document                   | Path                           | Purpose                                                                      |
| -------------------------- | ------------------------------ | ---------------------------------------------------------------------------- |
| Requirements Specification | `.propel/context/docs/spec.md` | Personas, use cases (UC-001–UC-020), functional requirements (FR-001–FR-042) |

### Optional Sources

| Document            | Path                             | Purpose                                        |
| ------------------- | -------------------------------- | ---------------------------------------------- |
| Architecture Design | `.propel/context/docs/design.md` | Technology stack, NFR, architecture decisions  |
| UML Models          | `.propel/context/docs/model.md`  | Sequence diagrams, ERD, component architecture |

### Related Documents

| Document      | Path                                   | Purpose                                    |
| ------------- | -------------------------------------- | ------------------------------------------ |
| Design System | `.propel/context/docs/designsystem.md` | Tokens, branding, component specifications |

---

## 3. UX Requirements

### UXR Requirements Table

| UXR-ID  | Category       | Requirement                                                                                                                                         | Acceptance Criteria                                                                                                                                     | Screens Affected                                     | Basis                                                                                                                    |
| ------- | -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| UXR-001 | Responsiveness | [SOURCE:INPUT] System MUST render all screens in a mobile-first responsive layout with fluid scaling across breakpoints                             | All screens render without horizontal scroll at 375px viewport width; layout adapts fluidly to 768px, 1024px, and 1440px                                | All screens                                          | Patient actor defined as "mobile-first responsive design" in spec.md system boundary                                     |
| UXR-002 | Visual Design  | [SOURCE:INPUT] System MUST use Shadcn UI component library with TailwindCSS for all UI elements                                                     | Every interactive element maps to a Shadcn primitive or documented custom component; no raw HTML controls                                               | All screens                                          | User confirmed Shadcn UI + TailwindCSS as component/styling choice                                                       |
| UXR-003 | Visual Design  | [SOURCE:INPUT] System MUST use Lucide icon pack for all iconography with consistent sizing and stroke weight                                        | All icons sourced from Lucide; icon size follows 16/20/24px scale; stroke width consistent at 1.5px                                                     | All screens                                          | User confirmed Lucide as sole icon library                                                                               |
| UXR-004 | Interaction    | [SOURCE:INFERRED] System MUST display a session timeout warning dialog 2 minutes before the 15-minute sliding expiry                                | Warning modal appears at 13 minutes of inactivity; countdown timer visible; "Extend Session" and "Log Out" actions available                            | All authenticated screens                            | FR-003 specifies 15-min sliding expiry; proactive warning prevents data loss during intake or booking flows              |
| UXR-101 | Usability      | [SOURCE:INFERRED] System MUST enable completion of any primary task within 3 clicks from the role-specific dashboard                                | Navigation depth audit passes for booking (2 clicks), intake (2 clicks), document upload (2 clicks), patient view (2 clicks)                            | SCR-002, SCR-003, SCR-004                            | Healthcare efficiency — staff under time pressure need immediate access to key workflows                                 |
| UXR-102 | Usability      | [SOURCE:INFERRED] System MUST display search results within 200ms perceived response time using skeleton loading                                    | Skeleton placeholders appear within 100ms of search initiation; full results render within 200ms on standard connection                                 | SCR-005, SCR-017, SCR-018, SCR-023, SCR-024          | Slot search and queue lookup are time-critical operations for both patients and staff                                    |
| UXR-103 | Usability      | [SOURCE:INFERRED] System MUST provide consistent navigation patterns across all role-specific dashboards using shared layout shell                  | All dashboards share header, sidebar (desktop), and bottom nav (mobile) structure; role determines visible menu items                                   | SCR-002, SCR-003, SCR-004                            | Three distinct roles sharing one platform need consistent spatial memory for navigation elements                         |
| UXR-104 | Usability      | [SOURCE:INPUT] System MUST provide real-time status feedback for all asynchronous operations with progress indicators                               | Document upload shows progress bar; processing shows step indicator; slot lock shows countdown; notifications show delivery status                      | SCR-005, SCR-011, SCR-012, SCR-017                   | FR-026 requires real-time processing status for uploaded documents                                                       |
| UXR-105 | Usability      | [SOURCE:INPUT] System MUST preserve all user-entered data when toggling between AI conversational and manual form intake modes                      | Switching from AI to Manual pre-fills form fields with AI-extracted data; switching back retains manual entries; zero data loss                         | SCR-009, SCR-010                                     | FR-017 explicitly requires free toggle with data preservation                                                            |
| UXR-106 | Usability      | [SOURCE:INFERRED] System MUST visually distinguish walk-in appointments from scheduled appointments in queue views                                  | Walk-in entries display a distinct badge ("Walk-in") with differentiated background tint; scheduled entries use default styling                         | SCR-017, SCR-022                                     | UC-005 creates walk-in bookings mixed with scheduled; staff need instant visual differentiation for triage               |
| UXR-201 | Accessibility  | [SOURCE:INPUT] System MUST maintain color contrast ratios of ≥4.5:1 for normal text and ≥3:1 for large text and UI components                       | Automated contrast check passes for all text/background combinations in all color modes                                                                 | All screens                                          | WCAG 2.2 Level AA confirmed by user; 1.4.3 Contrast (Minimum)                                                            |
| UXR-202 | Accessibility  | [SOURCE:INPUT] System MUST support full keyboard navigation for all interactive elements with logical tab order                                     | Every interactive element is reachable via Tab; activation via Enter/Space; no keyboard traps; skip-to-content link present                             | All screens                                          | WCAG 2.2 Level AA; 2.1.1 Keyboard, 2.1.2 No Keyboard Trap                                                                |
| UXR-203 | Accessibility  | [SOURCE:INPUT] System MUST display visible focus indicators on all interactive components that meet 3:1 contrast                                    | Focus ring visible on buttons, links, inputs, and custom widgets; ring contrast ≥3:1 against adjacent colors                                            | All screens                                          | WCAG 2.2 Level AA; 2.4.7 Focus Visible                                                                                   |
| UXR-204 | Accessibility  | [SOURCE:INPUT] System MUST provide ARIA labels and roles for all complex widgets including data tables, modals, chat interface, and step indicators | Screen readers announce widget purpose, state, and available actions; live regions for toast notifications and chat messages                            | SCR-009, SCR-017, SCR-019, SCR-024, all modals       | WCAG 2.2 Level AA; 4.1.2 Name, Role, Value                                                                               |
| UXR-205 | Accessibility  | [SOURCE:INPUT] System MUST ensure all touch targets are minimum 44x44px on mobile viewports                                                         | Interactive elements measure ≥44x44px at 375px viewport; spacing between adjacent targets ≥8px                                                          | All screens (mobile)                                 | WCAG 2.2 Level AA; 2.5.8 Target Size (Minimum)                                                                           |
| UXR-206 | Accessibility  | [SOURCE:INFERRED] System MUST not convey information by color alone — all color-coded indicators include redundant icon or text label               | Risk tiers show icon + text label + color; document status shows step label + icon + color; conflict highlights show icon + border + background         | SCR-017, SCR-019, SCR-022, SCR-012                   | WCAG 2.2 AA 1.4.1 Use of Color; risk tiers and status indicators are safety-critical in healthcare                       |
| UXR-301 | Responsiveness | [SOURCE:INFERRED] System MUST implement responsive breakpoints at 375px (mobile), 768px (tablet), 1024px (desktop), and 1440px (wide desktop)       | Layout reflows at each breakpoint; no content overflow; navigation adapts per breakpoint                                                                | All screens                                          | Standard responsive breakpoints aligned with TailwindCSS defaults (sm/md/lg/xl)                                          |
| UXR-302 | Responsiveness | [SOURCE:INFERRED] System MUST collapse sidebar navigation to bottom navigation bar on mobile viewports (<768px)                                     | Below 768px: sidebar hidden, bottom nav visible with 4-5 primary items; above 768px: sidebar visible, bottom nav hidden                                 | All authenticated screens                            | Mobile-first design requires thumb-reachable navigation; sidebar wastes mobile screen space                              |
| UXR-303 | Responsiveness | [SOURCE:INFERRED] System MUST transform data tables to stacked card layouts on mobile viewports (<768px)                                            | Tables with >3 columns render as cards on mobile; each card shows key fields; sort/filter remains accessible                                            | SCR-017, SCR-023, SCR-024, SCR-016                   | Dense tables are unusable on small screens; card layout preserves information hierarchy                                  |
| UXR-401 | Visual Design  | [SOURCE:INFERRED] System MUST apply consistent spacing using TailwindCSS spacing scale (4px base unit) across all layouts                           | Padding, margin, and gap values use only Tailwind spacing tokens (4, 8, 12, 16, 24, 32, 48px); no arbitrary values                                      | All screens                                          | Design system consistency prevents visual drift across 29 screens                                                        |
| UXR-402 | Visual Design  | [SOURCE:INFERRED] System MUST display no-show risk tiers using combined color, icon, and text label indicators                                      | Low = green + Shield icon + "Low"; Medium = amber + AlertCircle icon + "Medium"; High = red + AlertTriangle icon + "High"                               | SCR-017, SCR-022                                     | FR-034 requires risk tier display; UXR-206 requires non-color-only encoding                                              |
| UXR-403 | Visual Design  | [SOURCE:INFERRED] System MUST display document processing status as a horizontal step indicator showing pipeline stages                             | Steps: Uploading → Scanning → Processing → Completed/Failed; current step highlighted; completed steps show checkmark                                   | SCR-012                                              | FR-026 requires real-time processing status; step indicator maps naturally to the pipeline stages                        |
| UXR-404 | Visual Design  | [SOURCE:INFERRED] System MUST highlight data conflicts using a combination of amber border, tinted background, and AlertTriangle icon               | Conflicting fields show amber-200 background, amber-500 left border, AlertTriangle icon with "Conflict" label; both values and source documents visible | SCR-019, SCR-020                                     | FR-024 requires explicit conflict highlighting; healthcare data conflicts are safety-critical                            |
| UXR-501 | Interaction    | [SOURCE:INFERRED] System MUST display a loading spinner on submit buttons during form submission with disabled state                                | Button shows Loader2 spinning icon, text changes to "Submitting...", button disabled; prevents double-click submission                                  | All form screens                                     | Double-submission prevention is critical for booking (could create duplicate appointments) and user management           |
| UXR-502 | Interaction    | [SOURCE:INFERRED] System MUST display a visible countdown timer during the 30-second slot lock period                                               | Countdown ring or bar shows seconds remaining (30→0); at 10s, timer changes to destructive color; on expiry, lock auto-releases                         | SCR-005                                              | UC-003 specifies temporary lock on slot; patient needs awareness of time constraint                                      |
| UXR-503 | Interaction    | [SOURCE:INFERRED] System MUST display toast notifications for asynchronous event outcomes with auto-dismiss after 5 seconds                         | Toast appears at bottom-right (desktop) or bottom-center (mobile); success/error/info variants; manual dismiss available; stacks up to 3                | All screens                                          | Async operations (swap notifications, calendar sync, email delivery) need non-blocking feedback                          |
| UXR-504 | Interaction    | [SOURCE:INFERRED] System MUST validate form fields inline in real-time as the user types or on blur                                                 | Validation message appears below field within 300ms of blur or after 500ms debounce on keystroke; field border changes to red on error                  | SCR-001, SCR-010, SCR-014, SCR-015, SCR-018, SCR-023 | UC-010 specifies real-time field validation; immediate feedback reduces form completion errors                           |
| UXR-505 | Interaction    | [SOURCE:INFERRED] System MUST display a typing indicator animation during AI response generation in the conversational intake                       | Three-dot bounce animation in chat area during LLM processing; "AI is thinking..." screen reader announcement                                           | SCR-009                                              | UC-009 involves multi-turn conversation with AI; visual feedback prevents user confusion during response latency         |
| UXR-506 | Interaction    | [SOURCE:INFERRED] System MUST allow staff to reorder same-day queue entries via drag handle (desktop) or long-press move (mobile)                   | Drag handle (GripVertical icon) on each queue row; drag preview shows item; drop zone highlighted; order persisted immediately                          | SCR-017                                              | UC-006 queue management implies staff control over patient order; drag-reorder is the standard queue interaction pattern |
| UXR-601 | Error Handling | [SOURCE:INFERRED] System MUST display actionable recovery options on all error states (retry, navigate back, contact support)                       | Every error screen/state includes at least one action button; no dead-end error states; error message explains what happened                            | All screens                                          | Healthcare users cannot be blocked by dead-end errors; every failure must have a recovery path                           |
| UXR-602 | Error Handling | [SOURCE:INFERRED] System MUST display form validation errors inline below the invalid field with destructive-colored border                         | Error text appears in text-sm below field; field border changes to destructive color; error icon (AlertCircle) prepended to message                     | All form screens                                     | Standard form UX pattern; inline errors are more scannable than summary-only validation                                  |
| UXR-603 | Error Handling | [SOURCE:INFERRED] System MUST display a persistent top banner on network connectivity failure with retry action                                     | Banner appears below header with WifiOff icon, "Connection lost" message, and "Retry" button; auto-retries every 30s; auto-dismisses on reconnection    | All screens                                          | Healthcare workflows cannot silently fail; persistent banner ensures staff awareness of connectivity issues              |
| UXR-604 | Error Handling | [SOURCE:INPUT] System MUST display specific rejection reasons for failed document uploads including a list of accepted formats                      | Error toast or inline alert shows: reason (format, size, malware), accepted formats list (.pdf, .docx, .jpg, .png, .dicom, .fhir), and max size (50MB)  | SCR-011                                              | FR-020 requires clear error message on format/size rejection; patients need guidance to upload correct files             |
| UXR-605 | Error Handling | [SOURCE:INFERRED] System MUST redirect to login page with "Session expired" message when session token expires                                      | On 401 response, redirect to SCR-001 with toast "Your session has expired. Please log in again."; preserve return URL for post-login redirect           | All authenticated screens                            | FR-003 specifies session expiry; graceful handling prevents user confusion and data loss                                 |

### UXR Categories

- **Usability** (UXR-1XX): Navigation depth, discoverability, efficiency, status feedback, data preservation
- **Accessibility** (UXR-2XX): WCAG 2.2 AA — contrast, keyboard, focus, ARIA, touch targets, color independence
- **Responsiveness** (UXR-3XX): Breakpoints, navigation adaptation, table-to-card transformation
- **Visual Design** (UXR-4XX): Spacing consistency, risk indicators, status indicators, conflict highlighting
- **Interaction** (UXR-5XX): Loading states, countdown timers, toasts, inline validation, typing indicators, drag-reorder
- **Error Handling** (UXR-6XX): Recovery actions, inline validation, connectivity, upload errors, session expiry

---

## 4. Personas Summary

| Persona | Role                        | Primary Goals                                                                                                                        | Key Screens                                        |
| ------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------- |
| Patient | Primary end user            | Book appointments efficiently, complete intake with minimal friction, upload clinical documents, view consolidated health data       | SCR-001, SCR-002, SCR-005–SCR-016, SCR-028–SCR-029 |
| Staff   | Front desk / clinical staff | Manage walk-ins and queues, mark arrivals, verify AI-extracted data, resolve conflicts, review suggested codes, view risk indicators | SCR-001, SCR-003, SCR-017–SCR-022, SCR-026–SCR-027 |
| Admin   | System administrator        | Manage user accounts and roles, review audit logs for compliance, monitor platform adoption metrics                                  | SCR-001, SCR-004, SCR-023–SCR-025, SCR-026–SCR-027 |

---

## 5. Information Architecture

### Site Map

```text
Unified Patient Access & Clinical Intelligence Platform
+-- Auth
|   +-- SCR-001: Login / Registration
|   +-- SCR-026: MFA Verification
|   +-- SCR-027: MFA Setup
|   +-- SCR-028: Email Verification
|   +-- SCR-029: Password Reset
+-- Patient Portal
|   +-- SCR-002: Patient Dashboard
|   +-- Appointments
|   |   +-- SCR-005: Search & Book
|   |   +-- SCR-006: Booking Confirmation
|   |   +-- SCR-007: Appointment Detail
|   |   +-- SCR-008: Reschedule Slot Picker
|   |   +-- SCR-016: Appointment History
|   +-- Intake
|   |   +-- SCR-009: AI Conversational Intake
|   |   +-- SCR-010: Manual Form Intake
|   +-- Documents
|   |   +-- SCR-011: Document Upload
|   |   +-- SCR-012: Document List & Status
|   +-- SCR-013: Calendar Sync Settings
|   +-- SCR-014: Insurance Form
|   +-- SCR-015: Patient Profile / Settings
+-- Staff Portal
|   +-- SCR-003: Staff Dashboard
|   +-- SCR-017: Same-Day Queue
|   +-- SCR-018: Walk-in Booking
|   +-- SCR-019: 360-Degree Patient View
|   +-- SCR-020: Conflict Resolution Panel
|   +-- SCR-021: Code Mapping Interface
|   +-- SCR-022: Staff Schedule View
+-- Admin Portal
    +-- SCR-004: Admin Dashboard
    +-- SCR-023: User Management
    +-- SCR-024: Audit Log Viewer
    +-- SCR-025: Platform Metrics Dashboard
```

### Navigation Patterns

| Pattern       | Type                                    | Platform Behavior                                                                           |
| ------------- | --------------------------------------- | ------------------------------------------------------------------------------------------- |
| Primary Nav   | Sidebar (desktop) / Bottom Nav (mobile) | Desktop: collapsible sidebar with icon+label; Mobile: 4-5 tab bottom bar with Lucide icons  |
| Secondary Nav | Tabs                                    | Horizontal tab bar for sub-sections (e.g., Intake mode toggle, Dashboard sections)          |
| Utility Nav   | Header right                            | User avatar dropdown: profile, settings, logout; notification bell                          |
| Breadcrumb    | Hierarchical                            | Admin views (User Management → Edit User), Staff views (Patient View → Conflict Resolution) |
| Quick Search  | Command Menu (Cmd+K)                    | Global search across patients, appointments, documents (Staff/Admin only)                   |

---

## 6. Screen Inventory

### Screen List

| Screen ID | Screen Name                  | Derived From                           | Personas Covered      | States Required                            |
| --------- | ---------------------------- | -------------------------------------- | --------------------- | ------------------------------------------ |
| SCR-001   | Login / Registration         | UC-001, UC-002                         | Patient, Staff, Admin | Default, Loading, Empty, Error, Validation |
| SCR-002   | Patient Dashboard            | UC-001, UC-003, UC-004, UC-008, UC-042 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-003   | Staff Dashboard              | UC-005, UC-006, UC-013, UC-016         | Staff                 | Default, Loading, Empty, Error, Validation |
| SCR-004   | Admin Dashboard (Shell)      | UC-018, UC-019, UC-020                 | Admin                 | Default, Loading, Empty, Error, Validation |
| SCR-005   | Appointment Search & Booking | UC-003, UC-004                         | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-006   | Booking Confirmation         | UC-003                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-007   | Appointment Detail           | UC-008                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-008   | Reschedule Slot Picker       | UC-008                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-009   | AI Conversational Intake     | UC-009                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-010   | Manual Form Intake           | UC-010                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-011   | Document Upload              | UC-011                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-012   | Document List & Status       | UC-011, UC-012                         | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-013   | Calendar Sync Settings       | UC-007                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-014   | Insurance Form               | UC-017                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-015   | Patient Profile / Settings   | UC-001                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-016   | Appointment History          | UC-042                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-017   | Same-Day Queue               | UC-006                                 | Staff                 | Default, Loading, Empty, Error, Validation |
| SCR-018   | Walk-in Booking              | UC-005                                 | Staff                 | Default, Loading, Empty, Error, Validation |
| SCR-019   | 360-Degree Patient View      | UC-013                                 | Staff                 | Default, Loading, Empty, Error, Validation |
| SCR-020   | Conflict Resolution Panel    | UC-013                                 | Staff                 | Default, Loading, Empty, Error, Validation |
| SCR-021   | Code Mapping Interface       | UC-014                                 | Staff                 | Default, Loading, Empty, Error, Validation |
| SCR-022   | Staff Schedule View          | UC-016                                 | Staff                 | Default, Loading, Empty, Error, Validation |
| SCR-023   | User Management              | UC-018                                 | Admin                 | Default, Loading, Empty, Error, Validation |
| SCR-024   | Audit Log Viewer             | UC-019                                 | Admin                 | Default, Loading, Empty, Error, Validation |
| SCR-025   | Platform Metrics Dashboard   | UC-020                                 | Admin                 | Default, Loading, Empty, Error, Validation |
| SCR-026   | MFA Verification             | UC-002                                 | Staff, Admin          | Default, Loading, Empty, Error, Validation |
| SCR-027   | MFA Setup                    | UC-002                                 | Staff, Admin          | Default, Loading, Empty, Error, Validation |
| SCR-028   | Email Verification           | UC-001                                 | Patient               | Default, Loading, Empty, Error, Validation |
| SCR-029   | Password Reset               | UC-001                                 | Patient, Staff, Admin | Default, Loading, Empty, Error, Validation |

### Screen-to-Persona Coverage Matrix

| Screen  | Patient | Staff   | Admin   | Notes                                       |
| ------- | ------- | ------- | ------- | ------------------------------------------- |
| SCR-001 | Primary | Primary | Primary | Shared entry point; role detected post-auth |
| SCR-002 | Primary | -       | -       | Patient-only dashboard                      |
| SCR-003 | -       | Primary | -       | Staff-only dashboard                        |
| SCR-004 | -       | -       | Primary | Admin-only dashboard                        |
| SCR-005 | Primary | -       | -       | Slot search and booking                     |
| SCR-006 | Primary | -       | -       | Post-booking confirmation                   |
| SCR-007 | Primary | -       | -       | View/cancel/reschedule appointment          |
| SCR-008 | Primary | -       | -       | Reschedule flow                             |
| SCR-009 | Primary | -       | -       | AI conversational intake                    |
| SCR-010 | Primary | -       | -       | Manual form intake                          |
| SCR-011 | Primary | -       | -       | Document upload                             |
| SCR-012 | Primary | -       | -       | Document status tracking                    |
| SCR-013 | Primary | -       | -       | Calendar sync configuration                 |
| SCR-014 | Primary | -       | -       | Insurance pre-check                         |
| SCR-015 | Primary | -       | -       | Profile and settings                        |
| SCR-016 | Primary | -       | -       | Appointment history                         |
| SCR-017 | -       | Primary | -       | Same-day queue with arrival marking         |
| SCR-018 | -       | Primary | -       | Walk-in booking with patient search         |
| SCR-019 | -       | Primary | -       | 360-degree patient view                     |
| SCR-020 | -       | Primary | -       | Conflict resolution detail                  |
| SCR-021 | -       | Primary | -       | ICD-10/CPT code review                      |
| SCR-022 | -       | Primary | -       | Schedule with risk indicators               |
| SCR-023 | -       | -       | Primary | CRUD user accounts                          |
| SCR-024 | -       | -       | Primary | Searchable audit log                        |
| SCR-025 | -       | -       | Primary | Platform metrics and trends                 |
| SCR-026 | -       | Primary | Primary | MFA code entry                              |
| SCR-027 | -       | Primary | Primary | First-time MFA configuration                |
| SCR-028 | Primary | -       | -       | Email verification landing                  |
| SCR-029 | Primary | Primary | Primary | Password reset flow                         |

### Screen State Specifications

#### SCR-001: Login / Registration

- **Default**: Two-tab layout (Login / Register). Login tab: email + password fields, social login buttons (Google, Microsoft), "Forgot password?" link. Register tab: email, password, confirm password fields, social signup buttons, terms checkbox.
- **Loading**: Submit button shows Loader2 spinner + "Signing in..." / "Creating account..."; social buttons disabled; form fields disabled.
- **Empty**: N/A — form is always present.
- **Error**: Invalid credentials → inline alert below form with "Invalid email or password" message. Account locked → destructive alert with "Account locked after 5 failed attempts. Reset your password." + reset link. Social login denied → info alert "Login cancelled. Try another method."
- **Validation**: Password field shows complexity requirements checklist (8+ chars, uppercase, lowercase, digit, special char) with real-time check/x indicators. Email field validates format on blur. Confirm password validates match on blur.

#### SCR-002: Patient Dashboard

- **Default**: Welcome header with patient name. Quick action cards: "Book Appointment", "Start Intake", "Upload Documents". Upcoming appointments list (next 3) with status badges. Recent documents with processing status. Notification feed.
- **Loading**: Skeleton cards for quick actions; skeleton list items for appointments and documents.
- **Empty**: First-time user → illustration + "Welcome! Start by booking your first appointment" + CTA button. No upcoming appointments → "No upcoming appointments" + "Book Now" button.
- **Error**: Data fetch failure → alert banner "Unable to load dashboard. Please try again." + "Retry" button.
- **Validation**: N/A — display-only screen.

#### SCR-003: Staff Dashboard

- **Default**: Today's summary cards (Appointments, Walk-ins, Arrived, Pending). Queue preview (next 5 patients). Alerts panel (failed notifications, conflicts pending, low-confidence extractions). Quick actions: "Walk-in Booking", "View Queue", "Patient Lookup".
- **Loading**: Skeleton cards and skeleton list items.
- **Empty**: No appointments today → "No appointments scheduled for today" with date displayed.
- **Error**: Data fetch failure → alert banner with retry.
- **Validation**: N/A — display-only screen.

#### SCR-004: Admin Dashboard (Shell)

- **Default**: Navigation to admin sub-sections (Users, Audit Log, Metrics). Overview cards showing total users, today's activity count, system health status.
- **Loading**: Skeleton cards.
- **Empty**: New deployment → "System initialized. Add your first staff user." + CTA.
- **Error**: Data fetch failure → alert banner with retry.
- **Validation**: N/A.

#### SCR-005: Appointment Search & Booking

- **Default**: Search filters (provider name, specialty, date range) in a collapsible filter bar. Results grid showing available slots as time cards grouped by date. Each slot card shows: provider name, specialty, date, time, duration. Selected slot highlights with primary border.
- **Loading**: Skeleton slot cards while searching; selected slot shows lock countdown overlay (MOD-007).
- **Empty**: No matching slots → EmptyState with "No available slots for your search. Try different dates or providers." + "Clear Filters" button.
- **Error**: Search failure → inline alert above results area. Slot conflict (409) → toast "This slot was just booked. Refreshing availability..." + auto-refresh.
- **Validation**: Date range validates (start ≤ end). Provider/specialty select validates selection.

#### SCR-006: Booking Confirmation

- **Default**: Success checkmark animation. Appointment summary card (provider, date, time, location). Actions: "Add to Calendar", "View Appointment", "Book Another". Info text: "A confirmation PDF has been sent to your email."
- **Loading**: "Generating confirmation..." with spinner (during PDF generation).
- **Empty**: N/A — always shows confirmation data.
- **Error**: PDF generation failure → warning alert "Appointment confirmed but PDF could not be generated. Details sent via email." Appointment still confirmed.
- **Validation**: N/A.

#### SCR-007: Appointment Detail

- **Default**: Full appointment card (provider, date, time, status badge, booking type). Actions: "Cancel Appointment" (destructive), "Reschedule" (secondary). Preferred slot preference section (if active). Calendar sync status. Associated intake status link.
- **Loading**: Skeleton card.
- **Empty**: N/A — always has appointment data (navigated from list).
- **Error**: Appointment not found → "Appointment not found. It may have been cancelled." + "Back to Dashboard".
- **Validation**: N/A.

#### SCR-008: Reschedule Slot Picker

- **Default**: Current appointment summary (read-only). Available slots grid (same as SCR-005 but with "Reschedule to this slot" action). Current slot highlighted with "Current" badge.
- **Loading**: Skeleton slot cards.
- **Empty**: No available slots → EmptyState with suggestion to try different dates.
- **Error**: Slot conflict → toast + auto-refresh. Reschedule failure → alert with retry.
- **Validation**: Cannot select current slot.

#### SCR-009: AI Conversational Intake

- **Default**: Chat interface with message bubbles. AI messages (left-aligned, muted background). Patient messages (right-aligned, primary background). Extracted data panel (right sidebar on desktop, bottom sheet on mobile) showing parsed fields with confidence badges. Mode toggle tab ("AI" active, "Manual Form" inactive). Input field with send button at bottom.
- **Loading**: AI typing indicator (three-dot bounce animation) in chat area. "AI is thinking..." for screen readers (aria-live).
- **Empty**: Initial state → AI greeting message: "Hi! I'll help you complete your intake. Let's start with your medical history." + suggested response chips.
- **Error**: AI service unavailable → banner "AI assistant is temporarily unavailable. Switching to manual form." + auto-redirect to SCR-010 with data preserved. Individual parse failure → "I didn't catch that. Could you rephrase?" in chat.
- **Validation**: Extracted data panel shows confidence indicators per field (green ≥0.8, amber 0.5–0.8, red <0.5). Low-confidence fields flagged with "Please verify" badge.

#### SCR-010: Manual Form Intake

- **Default**: Structured form with sections: Medical History, Current Symptoms, Medications, Allergies, Reason for Visit. Each section collapsible. Mode toggle tab ("Manual Form" active, "AI" inactive). Pre-filled fields from AI data (if toggled from SCR-009) marked with "AI-extracted" badge. Submit button at bottom.
- **Loading**: Submit button shows spinner + "Submitting...".
- **Empty**: Fresh form with placeholder text in each field. Pre-filled from AI data if available.
- **Error**: Submission failure → alert "Could not save intake form. Please try again." + retry button. Server validation errors → mapped to inline field errors.
- **Validation**: Required fields (Reason for Visit) highlighted. Date fields validate format. Medication fields validate against known format patterns. Real-time inline validation per UXR-504.

#### SCR-011: Document Upload

- **Default**: Dropzone area (dashed border, Upload icon, "Drag files here or click to browse"). Supported formats list below dropzone. Max file size notice (50MB). Upload queue showing files being uploaded with individual progress bars.
- **Loading**: Per-file progress bars (0–100%). Overall upload count ("Uploading 2 of 3 files").
- **Empty**: No files selected → dropzone prompt only.
- **Error**: Format rejection → inline error per file with MOD-006 detail. Size exceeded → inline error per file. Malware detected → destructive alert "File rejected for security reasons. Please contact support if you believe this is an error."
- **Validation**: Client-side format whitelist check before upload. Client-side size check (<50MB).

#### SCR-012: Document List & Status

- **Default**: Table/card list of all uploaded documents. Columns: File name, Format, Upload date, Status (step indicator per UXR-403), Actions (view, delete). Status pipeline: Uploading → Scanning → Processing → Completed | Failed.
- **Loading**: Skeleton rows/cards. Status badges animate during active processing.
- **Empty**: No documents → EmptyState "No documents uploaded yet. Upload your clinical records to build your patient view." + "Upload" CTA.
- **Error**: Failed documents show red status badge + "Processing failed. Staff has been notified." + "Re-upload" action.
- **Validation**: N/A.

#### SCR-013: Calendar Sync Settings

- **Default**: Two provider cards (Google Calendar, Microsoft Outlook). Each shows: provider logo, connection status (Connected/Not connected), last sync time, "Connect" / "Disconnect" button. Connected providers show sync toggle.
- **Loading**: "Connecting..." spinner on provider card during OAuth flow.
- **Empty**: No calendars connected → both cards show "Not connected" with "Connect" buttons.
- **Error**: OAuth consent denied → info toast "Calendar sync was not enabled. You can connect anytime." Token refresh failure → warning "Re-authorization needed" + "Reconnect" button.
- **Validation**: N/A.

#### SCR-014: Insurance Form

- **Default**: Insurance provider name input (with autocomplete suggestions from internal list). Insurance ID input. "Validate" button. Result area (initially hidden). "Skip" link to proceed without insurance.
- **Loading**: "Validating..." spinner on button during lookup.
- **Empty**: Form fields empty, no validation result shown.
- **Error**: Validation service unavailable → warning "Insurance check unavailable. You may proceed without validation."
- **Validation**: Insurance not found → amber alert "Insurance not found in our records. This is a soft check and does not block your booking." Insurance found → green alert "Insurance verified" with provider details.

#### SCR-015: Patient Profile / Settings

- **Default**: Profile information (name, email, phone, date of birth) in editable card. Password change section. Notification preferences (email/SMS toggles). Data management section ("Request Data Deletion" destructive link). Active sessions section.
- **Loading**: Skeleton fields during data fetch. Spinner on save.
- **Empty**: N/A — always has user data.
- **Error**: Save failure → toast "Could not update profile. Please try again."
- **Validation**: Email format validation. Phone number format validation. Password complexity validation (same as registration).

#### SCR-016: Appointment History

- **Default**: Paginated table/card list of past and upcoming appointments. Columns: Date, Provider, Specialty, Status (Scheduled/Completed/Cancelled/No-Show badges), Actions (View). Filter bar: status filter, date range. Sort by date (default: newest first).
- **Loading**: Skeleton rows/cards.
- **Empty**: No appointments → EmptyState "No appointment history yet. Book your first appointment." + CTA.
- **Error**: Fetch failure → alert with retry.
- **Validation**: Date range filter validates start ≤ end.

#### SCR-017: Same-Day Queue

- **Default**: Real-time queue list showing today's patients. Columns: Queue position, Patient name, Time, Provider, Status (Scheduled/Walk-in/Arrived), Risk tier badge (per UXR-402), Actions (Mark Arrived, View Patient). Walk-in entries have distinct "Walk-in" badge per UXR-106. Drag handles for reorder per UXR-506. Summary bar: Total, Arrived, Pending, Walk-ins.
- **Loading**: Skeleton rows with animated pulse.
- **Empty**: No patients today → "No patients in queue for today."
- **Error**: Fetch failure → alert with retry. Already arrived → warning toast "Patient already marked as arrived."
- **Validation**: N/A.

#### SCR-018: Walk-in Booking

- **Default**: Patient search bar (name, phone, email). Search results list. "Create New Patient" button (if not found). Slot selection or "Add to Queue" option. Booking summary before confirmation.
- **Loading**: Search spinner. "Creating..." on patient creation.
- **Empty**: No search performed → prompt "Search for an existing patient or create a new record."
- **Error**: Patient creation failure → alert with retry. No slots → auto-suggest "Add to same-day queue with estimated wait time."
- **Validation**: Patient search requires minimum 2 characters. New patient form validates required fields (name, phone or email).

#### SCR-019: 360-Degree Patient View

- **Default**: Patient header (name, DOB, insurance status). Tabbed sections: Vitals, Medications, Allergies, Diagnoses, Procedures. Each section shows aggregated data from all documents. Conflict indicators per UXR-404 on conflicting fields. Source document references per data point. Confidence badges on AI-extracted values. "Verified" / "Pending Review" status per section.
- **Loading**: Skeleton sections.
- **Empty**: No documents processed → "No clinical data available. Patient needs to upload documents."
- **Error**: Aggregation failure → alert with retry.
- **Validation**: N/A — display-only (edits happen in SCR-020 and SCR-021).

#### SCR-020: Conflict Resolution Panel

- **Default**: List of detected conflicts for a patient. Each conflict card shows: Field name, Value 1 (Source: Document A), Value 2 (Source: Document B), resolution actions (Select Value 1, Select Value 2, Enter Custom Value, Mark for Clinical Review). Resolution history below.
- **Loading**: Spinner on resolution submission.
- **Empty**: No conflicts → success state "No data conflicts detected. All data is consistent."
- **Error**: Resolution save failure → toast with retry.
- **Validation**: Custom value entry validates against field type constraints.

#### SCR-021: Code Mapping Interface

- **Default**: Patient header summary. Suggested codes table: Code (ICD-10/CPT), Description, Confidence indicator (High/Medium/Low badge), Source data reference, Actions (Accept, Modify, Reject). Manual code entry section at bottom ("Add Code Manually"). Accepted/rejected code summary.
- **Loading**: "Generating code suggestions..." with skeleton table.
- **Empty**: No codes suggested → "No medical codes could be mapped from available data."
- **Error**: Code engine failure → alert "Code mapping service unavailable. Manual entry available."
- **Validation**: Manual code entry validates against ICD-10/CPT format patterns.

#### SCR-022: Staff Schedule View

- **Default**: Calendar view (day/week toggle) showing appointments by provider/time. Each appointment block shows: patient name, time, status, risk tier badge per UXR-402. Color-coded by status. Legend showing risk tier meanings.
- **Loading**: Skeleton calendar blocks.
- **Empty**: No appointments → "No appointments scheduled for this period."
- **Error**: Fetch failure → alert with retry.
- **Validation**: N/A.

#### SCR-023: User Management

- **Default**: DataTable with columns: Name, Email, Role (Patient/Staff/Admin badge), Status (Active/Inactive), Last Login, Actions (Edit, Deactivate). Search bar. Filter by role. "Create User" button. Pagination controls.
- **Loading**: Skeleton table rows.
- **Empty**: No users (beyond admin) → "No additional users created yet." + "Create User" CTA.
- **Error**: Create/update failure → toast with retry. Last admin deactivation attempt → destructive alert "Cannot deactivate the last admin account."
- **Validation**: Email uniqueness validated on create. Role selection required.

#### SCR-024: Audit Log Viewer

- **Default**: DataTable (read-only) with columns: Timestamp, Actor (name + role badge), Action Type, Resource Type, Resource ID, Details (expandable). Filter bar: date range, actor, action type, resource type. Export button (CSV/JSON). Pagination.
- **Loading**: Skeleton rows.
- **Empty**: No entries matching filter → "No audit entries found for the applied filters." with filter summary.
- **Error**: Query failure → alert with retry.
- **Validation**: Date range validates start ≤ end. Note: Immutable log — no edit/delete controls rendered.

#### SCR-025: Platform Metrics Dashboard

- **Default**: Summary cards: Total Patients, Total Appointments, Current No-Show Rate, AI Agreement Rate. Trend charts (line/bar): appointments over time, no-show rate trend, document processing volume. Date range filter. All charts use Recharts or similar with accessible color palette.
- **Loading**: Skeleton cards + skeleton chart areas.
- **Empty**: New deployment → "No data yet. Metrics will appear as the platform is used." with zero-state illustrations.
- **Error**: Aggregation failure → individual chart error states with retry per chart.
- **Validation**: Date range filter validates.

#### SCR-026: MFA Verification

- **Default**: MFA prompt text ("Enter the code from your authenticator app" or "Enter the code sent to your phone"). OTP input (6 digits). "Verify" button. "Resend Code" link (SMS only). "Use different method" link.
- **Loading**: "Verifying..." spinner on button.
- **Empty**: N/A.
- **Error**: Invalid code → inline error "Invalid code. X attempts remaining." 3 failed attempts → destructive alert "Too many failed attempts. Please restart login." + redirect to SCR-001.
- **Validation**: OTP field validates 6 numeric digits. Auto-submits on complete entry.

#### SCR-027: MFA Setup

- **Default**: Two options: TOTP (authenticator app) and SMS. TOTP: QR code display + manual key. SMS: phone number input. Verification step to confirm setup.
- **Loading**: "Setting up..." during configuration save.
- **Empty**: N/A.
- **Error**: Setup failure → alert with retry.
- **Validation**: SMS phone number validates format. TOTP verification code validates before enabling.

#### SCR-028: Email Verification

- **Default**: Success state: "Email verified! You can now log in." + "Go to Login" button. Pending state: "Check your email for a verification link." + "Resend Email" button.
- **Loading**: "Verifying..." during token validation.
- **Empty**: N/A.
- **Error**: Invalid/expired token → "Verification link expired. Request a new one." + "Resend" button.
- **Validation**: N/A.

#### SCR-029: Password Reset

- **Default**: Two-step flow. Step 1: Email input + "Send Reset Link" button. Step 2 (from email link): New password + confirm password fields + "Reset Password" button.
- **Loading**: Spinner on button during submission.
- **Empty**: N/A.
- **Error**: Email not found → generic "If an account exists, a reset link has been sent." (security — no account enumeration). Token expired → "Reset link expired. Request a new one."
- **Validation**: Password complexity validation (same as registration). Confirm password match.

### Modal/Overlay Inventory

| Name                               | Type                     | Trigger                         | Parent Screen(s)          |
| ---------------------------------- | ------------------------ | ------------------------------- | ------------------------- |
| MOD-001: Session Timeout Warning   | AlertDialog              | 13 minutes of inactivity        | All authenticated screens |
| MOD-002: Booking Confirmation      | Dialog                   | Slot selection confirmed        | SCR-005                   |
| MOD-003: Cancellation Confirmation | AlertDialog              | "Cancel Appointment" clicked    | SCR-007                   |
| MOD-004: Create/Edit User          | Dialog (Sheet on mobile) | "Create User" or "Edit" clicked | SCR-023                   |
| MOD-005: Conflict Detail           | Dialog                   | Conflict row clicked            | SCR-020                   |
| MOD-006: File Rejection Alert      | AlertDialog              | Upload validation fails         | SCR-011                   |
| MOD-007: Slot Lock Timer           | Overlay                  | Slot selected in booking        | SCR-005, SCR-008          |

---

## 7. Content & Tone

### Voice & Tone

- **Overall Tone**: Professional, calm, trustworthy — clinical competence without coldness
- **Error Messages**: Helpful, non-blaming, actionable — "We couldn't process your file. Try uploading a PDF, DOCX, or image file under 50MB."
- **Empty States**: Encouraging, guiding, with clear CTA — "No appointments yet. Book your first visit to get started."
- **Success Messages**: Brief, confirming, next-action oriented — "Appointment confirmed. A PDF has been sent to your email."
- **AI Chat Tone**: Warm but professional — "Thanks for sharing that. Let me confirm: you're currently taking Lisinopril 10mg daily. Is that correct?"

### Content Guidelines

- **Headings**: Sentence case ("Appointment history" not "Appointment History")
- **CTAs**: Action-oriented, specific verbs ("Book appointment", "Upload document", "Resolve conflict" — not "Submit", "Go", "OK")
- **Labels**: Concise, descriptive — field labels above inputs, not inside (placeholder text supplements, not replaces, labels)
- **Placeholder Text**: Helpful examples ("e.g., Dr. Sarah Chen", "e.g., ABC-123456") — never "Lorem ipsum" in any state
- **Medical Terminology**: Use standard clinical terms with plain-language tooltips for patient-facing screens

---

## 8. Data & Edge Cases

### Data Scenarios

| Scenario          | Description                               | Handling                                                                                         |
| ----------------- | ----------------------------------------- | ------------------------------------------------------------------------------------------------ |
| No Data           | New user, no appointments or documents    | Empty state with guided CTA per screen                                                           |
| First Use         | First-time login after registration       | Patient dashboard shows onboarding prompt with 3 quick actions                                   |
| Large Data        | 100+ documents, 500+ appointments         | Paginated tables (20 items/page); virtualized lists for queue; server-side search                |
| Slow Connection   | >3s load time                             | Skeleton screens appear within 100ms; content progressively loads                                |
| Offline           | No network connectivity                   | Persistent top banner per UXR-603; forms preserve local state; retry on reconnect                |
| Concurrent Edit   | Two staff members resolving same conflict | Optimistic locking — second save shows "This conflict was already resolved by [user]." + refresh |
| High Volume Queue | 50+ patients in same-day queue            | Virtualized list with search filter; summary count bar stays visible                             |

### Edge Cases

| Case                              | Screen(s) Affected     | Solution                                                             |
| --------------------------------- | ---------------------- | -------------------------------------------------------------------- |
| Long patient name (50+ chars)     | All with patient names | Truncate with ellipsis + full name in tooltip                        |
| Long provider name / specialty    | SCR-005, SCR-022       | Truncate with ellipsis + tooltip                                     |
| Multiple insurance entries        | SCR-014                | Show latest; link to "View history"                                  |
| Document with no extractable data | SCR-012, SCR-019       | Status "Extraction Failed — Manual Review Required"                  |
| 20+ conflicts for one patient     | SCR-020                | Paginated conflict list with count badge                             |
| Simultaneous slot booking (race)  | SCR-005                | Optimistic lock via Redis; 409 conflict → auto-refresh               |
| Session timeout during intake     | SCR-009, SCR-010       | MOD-001 warns; local draft preserved; post-login returns to intake   |
| MFA code expiry                   | SCR-026                | "Code expired" + "Resend Code" link                                  |
| Malware detected in document      | SCR-011                | Quarantine file; destructive alert with support contact              |
| Admin deactivates own account     | SCR-023                | Prevented server-side; UI shows "Cannot deactivate your own account" |

---

## 9. Branding & Visual Direction

_See `designsystem.md` for all design tokens (colors, typography, spacing, shadows, etc.)_

### Aesthetic Direction

- **Direction**: utilitarian `[SOURCE:INFERRED]`
- **Rationale**: A healthcare platform handling PHI and clinical data needs to project calm competence and institutional trust. Utilitarian design prioritizes information density, scanability, and efficiency over decorative elements — critical for staff processing queues and verifying clinical data under time pressure. For patients, it reduces cognitive load during potentially stressful intake and booking flows. The "Trust-First" positioning maps directly to clean, predictable UI patterns.
- **Precedents**: Epic MyChart (healthcare patient portal), Linear (utilitarian SaaS), Stripe Dashboard (data-rich operational UI)
- **Anti-brief**: Must not resemble gamified health apps (Noom, Headspace) with playful illustrations and gratuitous animation. Must not resemble enterprise ERP (SAP, Oracle) with overwhelmingly dense tables. Must not default to dark-mode-first aesthetic — clinical settings require readability under fluorescent lighting.
- **Basis**: "Trust-First" clinical intelligence positioning from BRD combined with healthcare domain conventions and three distinct user personas with efficiency-focused goals.

### Branding Assets

- **Logo**: Platform wordmark — "HealthAccess" (placeholder) — clean sans-serif, deep blue primary color
- **Icon Style**: Outlined (Lucide default, 1.5px stroke, consistent with utilitarian direction)
- **Illustration Style**: None for MVP — empty states use icons + text only; no decorative illustrations
- **Photography Style**: Not applicable for Phase 1

---

## 10. Component Specifications

_Component specifications defined in designsystem.md. Requirements per screen listed below._

### Component Library Reference

**Source**: `.propel/context/docs/designsystem.md` (Component Specifications section)

### Required Components per Screen

| Screen ID | Components Required                                                                  | Notes                                            |
| --------- | ------------------------------------------------------------------------------------ | ------------------------------------------------ |
| SCR-001   | Input (4), Button (3), Link (2), Tabs (1), Checkbox (1), Separator (1)               | Login/register tabbed form, social login buttons |
| SCR-002   | Card (4), Badge (3), Button (2), Avatar (1), Skeleton (4)                            | Dashboard quick actions + upcoming appointments  |
| SCR-003   | Card (4), Badge (5), Table (1), Button (3), Avatar (N), Alert (1)                    | Summary cards + queue preview + alerts           |
| SCR-004   | Card (3), Button (2), Badge (2)                                                      | Admin navigation shell                           |
| SCR-005   | Input (3), Select (2), DatePicker (1), Card (N), Button (N), Badge (N), Skeleton (N) | Search filters + slot grid                       |
| SCR-006   | Card (1), Button (3), Badge (1)                                                      | Confirmation summary                             |
| SCR-007   | Card (1), Button (2), Badge (3), Link (1)                                            | Appointment detail + actions                     |
| SCR-008   | Card (N), Button (N), Badge (2), Skeleton (N)                                        | Reschedule slot grid                             |
| SCR-009   | ChatBubble (N), Input (1), Button (1), Card (1), Badge (N), Tabs (1), Skeleton (1)   | Chat interface + extracted data panel            |
| SCR-010   | Input (6), Textarea (2), Select (2), Button (2), Tabs (1), Badge (N)                 | Structured intake form                           |
| SCR-011   | FileUpload (1), Progress (N), Card (N), Button (1), Alert (N)                        | Dropzone + upload queue                          |
| SCR-012   | Table (1), Badge (N), StepIndicator (N), Button (N), Skeleton (N)                    | Document list with pipeline status               |
| SCR-013   | Card (2), Button (2), Switch (2), Badge (2)                                          | Provider connection cards                        |
| SCR-014   | Input (2), Button (2), Alert (1), Link (1)                                           | Insurance form + result                          |
| SCR-015   | Input (4), Button (2), Switch (2), Link (1), Separator (2)                           | Profile edit + settings                          |
| SCR-016   | Table (1), Badge (N), DatePicker (1), Select (1), Button (1)                         | Appointment history + filters                    |
| SCR-017   | Table (1), Badge (N), Button (N), Avatar (N), DragHandle (N), Card (N)               | Queue list with drag reorder                     |
| SCR-018   | Input (1), Button (3), Card (N), Select (1), Avatar (N)                              | Patient search + booking form                    |
| SCR-019   | Tabs (1), Card (N), Badge (N), Table (N), Alert (N), Tooltip (N)                     | Tabbed patient view sections                     |
| SCR-020   | Card (N), Button (3N), Input (N), Badge (N), Alert (1)                               | Conflict cards with resolution actions           |
| SCR-021   | Table (1), Badge (N), Button (3N), Input (2), Select (1)                             | Code suggestions + manual entry                  |
| SCR-022   | Calendar (1), Card (N), Badge (N), Select (1), Tabs (1)                              | Schedule calendar view                           |
| SCR-023   | DataTable (1), Button (2), Badge (N), Input (1), Select (1), Dialog (1)              | User list + CRUD                                 |
| SCR-024   | DataTable (1), Input (1), DatePicker (2), Select (3), Button (2)                     | Audit log + filters + export                     |
| SCR-025   | Card (4), Chart (3), DatePicker (2), Skeleton (4)                                    | Metric cards + trend charts                      |
| SCR-026   | OTPInput (1), Button (2), Link (1)                                                   | MFA code entry                                   |
| SCR-027   | Card (2), Input (1), Button (2), QRCode (1)                                          | MFA setup options                                |
| SCR-028   | Card (1), Button (1), Link (1)                                                       | Verification status                              |
| SCR-029   | Input (3), Button (1), Link (1)                                                      | Password reset form                              |

### Component Summary

| Category     | Components                                                                                                        | Variants                                                                                                            |
| ------------ | ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| Actions      | Button, IconButton, Link, FAB                                                                                     | Primary, Secondary, Destructive, Ghost, Outline × S/M/L × States (Default, Hover, Focus, Active, Disabled, Loading) |
| Inputs       | Input, Textarea, Select, Checkbox, RadioGroup, Switch, DatePicker, TimePicker, FileUpload, OTPInput               | States (Default, Focus, Error, Disabled) × Sizes (S/M)                                                              |
| Navigation   | Sidebar, BottomNav, Tabs, Breadcrumb, Header, CommandMenu                                                         | Platform variants (Desktop/Mobile), Collapsed/Expanded                                                              |
| Content      | Card, Table, DataTable, Avatar, Badge, Separator, Skeleton, Progress, StepIndicator, ChatBubble, Timeline, QRCode | Content variants, Size variants                                                                                     |
| Feedback     | Dialog, AlertDialog, Drawer, Sheet, Toast, Alert, Tooltip, Popover                                                | Types (Info, Success, Warning, Destructive) × States                                                                |
| Data Display | Chart (Bar, Line, Pie), Calendar, EmptyState                                                                      | Chart variants, Calendar views (Day/Week)                                                                           |

### Component Constraints

- Use only Shadcn UI primitives — custom components (ChatBubble, StepIndicator, EmptyState) must compose from Shadcn primitives
- All components must support defined states (Default, Hover, Focus, Active, Disabled, Loading)
- Follow naming convention: `C/<Category>/<Name>`
- All interactive components require visible focus indicators (UXR-203)
- All components use semantic design tokens only — no hard-coded color/spacing values

---

## 11. Prototype Flows

### Flow: FL-001 — Patient Registration & Login

**Flow ID**: FL-001
**Derived From**: UC-001
**Personas Covered**: Patient
**Description**: Patient registers a new account or logs in to access the patient portal.

#### Flow Sequence

```text
1. Entry: SCR-001 / Default (Login tab)
   - Trigger: Patient navigates to platform URL
   |
   v
2. Decision: Auth method
   +-- Social (Google/MS) -> External OAuth -> SCR-001 / Loading -> SCR-002 / Default
   +-- Email Register -> SCR-001 / Default (Register tab) -> SCR-001 / Loading -> SCR-028 / Default
   +-- Email Login -> SCR-001 / Loading -> SCR-002 / Default
   +-- Error -> SCR-001 / Error (invalid creds or locked)
```

#### Required Interactions

- Tab switch between Login / Register
- Social login button click → external redirect → return
- Form submission with validation
- "Forgot password?" link → SCR-029

---

### Flow: FL-002 — Staff Authentication with MFA

**Flow ID**: FL-002
**Derived From**: UC-002
**Personas Covered**: Staff, Admin
**Description**: Staff or admin authenticates with credentials and completes mandatory MFA.

#### Flow Sequence

```text
1. Entry: SCR-001 / Default (Login tab)
   - Trigger: Staff navigates to platform URL
   |
   v
2. Step: SCR-001 / Loading (credential validation)
   |
   v
3. Step: SCR-026 / Default (MFA code entry)
   - Action: Enter TOTP or SMS code
   |
   v
4. Decision:
   +-- Valid code -> SCR-003 / Default (Staff) or SCR-004 / Default (Admin)
   +-- Invalid code -> SCR-026 / Error (retry or lockout)
   +-- MFA not configured -> SCR-027 / Default (setup) -> SCR-026 / Default
```

#### Required Interactions

- Credential form submission
- OTP input with auto-submit on 6 digits
- "Resend Code" action (SMS)
- "Use different method" toggle

---

### Flow: FL-003 — Appointment Booking

**Flow ID**: FL-003
**Derived From**: UC-003, UC-004
**Personas Covered**: Patient
**Description**: Patient searches for available slots, selects and locks a slot, optionally selects a preferred slot, and confirms booking.

#### Flow Sequence

```text
1. Entry: SCR-002 / Default
   - Trigger: Patient clicks "Book Appointment"
   |
   v
2. Step: SCR-005 / Default (Search & filter)
   - Action: Enter search criteria
   |
   v
3. Step: SCR-005 / Loading (slot search)
   |
   v
4. Step: SCR-005 / Default (results displayed)
   - Action: Select a slot
   |
   v
5. Step: MOD-007 (30s lock countdown)
   - Action: Optionally select preferred unavailable slot
   |
   v
6. Step: MOD-002 (Booking confirmation dialog)
   - Action: Confirm booking
   |
   v
7. Exit: SCR-006 / Default (Confirmation)
   +-- Slot conflict (409) -> SCR-005 / Error (auto-refresh)
   +-- Patient cancels -> SCR-005 / Default (lock released)
```

#### Required Interactions

- Search filter inputs + search action
- Slot card selection (highlight)
- Lock countdown visualization
- Preferred slot selection (optional toggle)
- Confirmation dialog accept/cancel

---

### Flow: FL-004 — Patient Intake (AI + Manual)

**Flow ID**: FL-004
**Derived From**: UC-009, UC-010
**Personas Covered**: Patient
**Description**: Patient completes intake using AI conversational mode, manual form, or toggling between both.

#### Flow Sequence

```text
1. Entry: SCR-002 / Default
   - Trigger: Patient clicks "Start Intake" for upcoming appointment
   |
   v
2. Decision: Intake mode
   +-- AI mode -> SCR-009 / Default
   +-- Manual mode -> SCR-010 / Default
   |
   v
3a. Step: SCR-009 (AI conversation loop)
    - Action: Patient types responses, AI extracts data
    - Toggle: Switch to SCR-010 (data preserved)
    |
3b. Step: SCR-010 (Manual form fill)
    - Action: Patient fills structured fields
    - Toggle: Switch to SCR-009 (data preserved)
    |
    v
4. Step: Submit intake
   |
   v
5. Exit: SCR-002 / Default (intake status updated)
   +-- AI unavailable -> SCR-010 / Default (auto-fallback, data preserved)
```

#### Required Interactions

- Mode toggle tabs (AI ↔ Manual) with data preservation
- Chat message input + send
- Extracted data review in side panel
- Form field completion with inline validation
- Submit action

---

### Flow: FL-005 — Document Upload & Processing

**Flow ID**: FL-005
**Derived From**: UC-011, UC-012
**Personas Covered**: Patient
**Description**: Patient uploads clinical documents and monitors processing status.

#### Flow Sequence

```text
1. Entry: SCR-002 / Default
   - Trigger: Patient clicks "Upload Documents"
   |
   v
2. Step: SCR-011 / Default (Dropzone)
   - Action: Drag files or click to browse
   |
   v
3. Step: SCR-011 / Loading (Upload progress)
   |
   v
4. Decision: Validation
   +-- Valid -> SCR-012 / Default (Status: Scanning)
   +-- Invalid format/size -> MOD-006 (Rejection alert)
   +-- Malware detected -> SCR-011 / Error (quarantine notice)
   |
   v
5. Step: SCR-012 / Default (Status pipeline: Scanning → Processing → Completed)
   |
   v
6. Exit: SCR-012 / Default (all documents processed)
```

#### Required Interactions

- File drag-and-drop or file picker
- Upload progress bar per file
- Status pipeline step indicator animation
- "Re-upload" action on failed documents

---

### Flow: FL-006 — Staff Walk-in & Queue

**Flow ID**: FL-006
**Derived From**: UC-005, UC-006
**Personas Covered**: Staff
**Description**: Staff creates a walk-in booking and manages the same-day queue.

#### Flow Sequence

```text
1. Entry: SCR-003 / Default
   - Trigger: Staff clicks "Walk-in Booking"
   |
   v
2. Step: SCR-018 / Default (Patient search)
   - Action: Search by name/phone/email
   |
   v
3. Decision: Patient found?
   +-- Found -> Select patient -> Book slot or add to queue
   +-- Not found -> Create new patient -> Book slot or add to queue
   |
   v
4. Step: SCR-017 / Default (Queue with new entry)
   - Action: Monitor queue, mark patients arrived
   |
   v
5. Exit: SCR-017 / Default (patient marked "Arrived")
```

#### Required Interactions

- Patient search with results
- "Create New Patient" inline form
- Slot selection or "Add to Queue" action
- "Mark Arrived" button on queue row
- Queue drag reorder

---

### Flow: FL-007 — 360-View & Conflict Resolution

**Flow ID**: FL-007
**Derived From**: UC-013
**Personas Covered**: Staff
**Description**: Staff reviews aggregated patient data and resolves detected conflicts.

#### Flow Sequence

```text
1. Entry: SCR-003 / Default
   - Trigger: Staff clicks patient name or "View Patient"
   |
   v
2. Step: SCR-019 / Default (Consolidated patient view)
   - Action: Review aggregated data across tabs
   |
   v
3. Decision: Conflicts present?
   +-- Yes -> Click conflict indicator -> SCR-020 / Default
   +-- No -> Exit at SCR-019
   |
   v
4. Step: SCR-020 / Default (Conflict resolution)
   - Action: Select correct value per conflict
   |
   v
5. Exit: SCR-019 / Default (conflicts resolved, view updated)
```

#### Required Interactions

- Tab navigation across data sections
- Conflict indicator click to open resolution
- Resolution action buttons (Select Value 1/2, Custom, Clinical Review)
- Audit-logged resolution confirmation

---

### Flow: FL-008 — Code Mapping Verification

**Flow ID**: FL-008
**Derived From**: UC-014
**Personas Covered**: Staff
**Description**: Staff reviews AI-suggested ICD-10/CPT codes and accepts, modifies, or rejects each.

#### Flow Sequence

```text
1. Entry: SCR-003 / Default or SCR-019
   - Trigger: Staff clicks "Code Mapping" for patient
   |
   v
2. Step: SCR-021 / Loading (generating suggestions)
   |
   v
3. Step: SCR-021 / Default (suggested codes displayed)
   - Action: Review each code with confidence indicator
   |
   v
4. Loop: Per code -> Accept / Modify / Reject
   |
   v
5. Optional: Manual code entry at bottom
   |
   v
6. Exit: SCR-021 / Default (all codes verified)
```

#### Required Interactions

- Accept/Modify/Reject buttons per code row
- Modify opens inline edit with code lookup
- Manual code entry with format validation
- Verified code summary display

---

### Flow: FL-009 — Admin User Management

**Flow ID**: FL-009
**Derived From**: UC-018
**Personas Covered**: Admin
**Description**: Admin creates, updates, or deactivates user accounts.

#### Flow Sequence

```text
1. Entry: SCR-004 / Default
   - Trigger: Admin clicks "User Management"
   |
   v
2. Step: SCR-023 / Default (User table)
   - Action: Search, filter, review users
   |
   v
3. Decision: Action type
   +-- Create -> MOD-004 (Create User dialog) -> SCR-023 (updated)
   +-- Edit -> MOD-004 (Edit User dialog) -> SCR-023 (updated)
   +-- Deactivate -> AlertDialog confirmation -> SCR-023 (updated)
   |
   v
4. Exit: SCR-023 / Default
   +-- Last admin protection -> Error alert (blocked)
```

#### Required Interactions

- "Create User" button → dialog with email + role fields
- Inline "Edit" / "Deactivate" actions per row
- Destructive confirmation for deactivation
- Role assignment select
- Activation email trigger

---

### Flow: FL-010 — Appointment Cancellation / Reschedule

**Flow ID**: FL-010
**Derived From**: UC-008
**Personas Covered**: Patient
**Description**: Patient cancels or reschedules an existing appointment.

#### Flow Sequence

```text
1. Entry: SCR-002 or SCR-016
   - Trigger: Patient clicks appointment -> SCR-007
   |
   v
2. Step: SCR-007 / Default (Appointment detail)
   |
   v
3. Decision: Action
   +-- Cancel -> MOD-003 (Confirmation) -> SCR-002 / Default (appointment removed)
   +-- Reschedule -> SCR-008 / Default (Slot picker)
       -> Select new slot -> MOD-002 (Confirm) -> SCR-006 / Default
   |
   v
4. Exit: SCR-002 / Default (updated appointment list)
```

#### Required Interactions

- "Cancel" destructive button → AlertDialog confirmation
- "Reschedule" secondary button → slot picker
- New slot selection + confirmation
- Toast notification for success

---

### Flow: FL-011 — Session Timeout Recovery

**Flow ID**: FL-011
**Derived From**: FR-003, UXR-004
**Personas Covered**: Patient, Staff, Admin
**Description**: System warns user before session expiry and handles timeout gracefully.

#### Flow Sequence

```text
1. Entry: Any authenticated screen
   - Trigger: 13 minutes of inactivity
   |
   v
2. Step: MOD-001 / Default (Session warning)
   - Display: "Your session will expire in 2 minutes."
   - Countdown timer visible
   |
   v
3. Decision:
   +-- "Extend Session" -> Close modal, reset timer, continue
   +-- Timer reaches 0 or "Log Out" -> SCR-001 / Default
       with toast "Session expired. Please log in again."
       Return URL preserved for post-login redirect
```

#### Required Interactions

- Modal with countdown timer
- "Extend Session" button (primary)
- "Log Out" button (secondary)
- Auto-redirect on timer expiry

---

### Flow: FL-012 — Calendar Sync Configuration

**Flow ID**: FL-012
**Derived From**: UC-007
**Personas Covered**: Patient
**Description**: Patient connects or disconnects an external calendar provider for appointment sync.

#### Flow Sequence

```text
1. Entry: SCR-002 / Default
   - Trigger: Patient clicks "Calendar Sync" in settings or sidebar
   |
   v
2. Step: SCR-013 / Default (Calendar Sync Settings)
   - Action: View provider cards (Google Calendar, Microsoft Outlook)
   |
   v
3. Decision: Action type
   +-- Connect -> External OAuth consent screen -> SCR-013 / Loading -> SCR-013 / Default (Connected)
   +-- Disconnect -> Confirmation toast -> SCR-013 / Default (Disconnected)
   +-- Toggle sync -> Switch toggles sync on/off -> Toast confirmation
   |
   v
4. Exit: SCR-013 / Default (updated connection status)
   +-- OAuth denied -> SCR-013 / Default + info toast "Calendar sync was not enabled."
   +-- Token refresh failure -> SCR-013 / Error ("Re-authorization needed" + "Reconnect" button)
```

#### Required Interactions

- "Connect" button on provider card → external OAuth redirect → return
- "Disconnect" button with immediate effect
- Sync toggle switch
- Toast notifications for state changes

---

### Flow: FL-013 — Notification Delivery & Preferences

**Flow ID**: FL-013
**Derived From**: UC-015
**Personas Covered**: Patient
**Description**: Patient configures notification preferences and receives delivery confirmations.

#### Flow Sequence

```text
1. Entry: SCR-002 / Default
   - Trigger: Patient clicks notification bell or navigates to Profile/Settings
   |
   v
2. Step: SCR-015 / Default (Profile / Settings → Notification Preferences section)
   - Action: Toggle email and SMS notification channels
   |
   v
3. Step: Save preferences
   - Action: Click "Save" → SCR-015 / Loading → Toast "Preferences updated"
   |
   v
4. Exit: SCR-015 / Default (preferences saved)
   +-- Save failure -> Toast "Could not update preferences. Please try again."
```

#### Required Interactions

- Email notification toggle (Switch)
- SMS notification toggle (Switch)
- Save action with loading state
- Toast confirmation

---

### Flow: FL-014 — Staff Schedule Review

**Flow ID**: FL-014
**Derived From**: UC-016
**Personas Covered**: Staff
**Description**: Staff reviews appointment schedule with risk tier indicators across day and week views.

#### Flow Sequence

```text
1. Entry: SCR-003 / Default
   - Trigger: Staff clicks "Schedule" in sidebar navigation
   |
   v
2. Step: SCR-022 / Default (Staff Schedule View — Day view)
   - Action: Review today's appointments with risk tier badges
   |
   v
3. Decision: View toggle
   +-- Switch to Week view -> SCR-022 / Default (Week view tab)
   +-- Click appointment block -> SCR-019 / Default (360-Degree Patient View)
   +-- Change date -> Date navigation -> SCR-022 / Loading -> SCR-022 / Default
   |
   v
4. Exit: SCR-022 / Default or SCR-019 / Default
```

#### Required Interactions

- Day/Week tab toggle
- Date navigation (prev/next)
- Appointment block click → patient view navigation
- Risk tier badge display (Low/Medium/High per UXR-402)

---

### Flow: FL-015 — Insurance Pre-Check

**Flow ID**: FL-015
**Derived From**: UC-017
**Personas Covered**: Patient
**Description**: Patient submits insurance details for soft validation before appointment.

#### Flow Sequence

```text
1. Entry: SCR-002 / Default
   - Trigger: Patient clicks "Insurance" in sidebar or prompted during booking
   |
   v
2. Step: SCR-014 / Default (Insurance Form)
   - Action: Enter insurance provider name (autocomplete) and Insurance ID
   |
   v
3. Step: SCR-014 / Loading ("Validating..." spinner)
   |
   v
4. Decision: Validation result
   +-- Found -> SCR-014 / Default with green alert "Insurance verified"
   +-- Not found -> SCR-014 / Default with amber alert "Insurance not found. This does not block your booking."
   +-- Service unavailable -> SCR-014 / Error with warning "Insurance check unavailable."
   |
   v
5. Exit: SCR-014 / Default (result displayed)
   +-- "Skip" link -> SCR-002 / Default (proceed without validation)
```

#### Required Interactions

- Insurance provider autocomplete input
- Insurance ID text input
- "Validate" button with loading state
- Result alert display (success/warning)
- "Skip" link to bypass

---

### Flow: FL-016 — Audit Log Review

**Flow ID**: FL-016
**Derived From**: UC-019
**Personas Covered**: Admin
**Description**: Admin searches, filters, and exports audit log entries for compliance review.

#### Flow Sequence

```text
1. Entry: SCR-004 / Default
   - Trigger: Admin clicks "Audit Log" in sidebar
   |
   v
2. Step: SCR-024 / Default (Audit Log Viewer with default filters)
   - Action: Review recent audit entries
   |
   v
3. Decision: Action type
   +-- Filter -> Apply date range, actor, action type, resource type filters -> SCR-024 / Loading -> SCR-024 / Default (filtered)
   +-- Expand row -> View detail inline (accordion expand)
   +-- Export -> Select format (CSV/JSON) -> Download initiated -> Toast "Export complete"
   +-- Paginate -> Next/Prev page -> SCR-024 / Loading -> SCR-024 / Default
   |
   v
4. Exit: SCR-024 / Default
   +-- Query failure -> SCR-024 / Error with retry
```

#### Required Interactions

- Date range picker filters
- Actor/action/resource type select filters
- Row expand/collapse for entry details
- Export button (CSV/JSON format selection)
- Pagination controls

---

### Flow: FL-017 — Platform Metrics Review

**Flow ID**: FL-017
**Derived From**: UC-020
**Personas Covered**: Admin
**Description**: Admin reviews platform adoption metrics and trend data.

#### Flow Sequence

```text
1. Entry: SCR-004 / Default
   - Trigger: Admin clicks "Metrics" in sidebar
   |
   v
2. Step: SCR-025 / Default (Platform Metrics Dashboard)
   - Action: Review summary cards (Total Patients, Appointments, No-Show Rate, AI Agreement Rate)
   |
   v
3. Decision: Action type
   +-- Change date range -> DatePicker filter -> SCR-025 / Loading -> SCR-025 / Default (charts updated)
   +-- Hover chart data point -> Tooltip with exact value
   |
   v
4. Exit: SCR-025 / Default
   +-- Aggregation failure -> Individual chart error states with retry per chart
```

#### Required Interactions

- Summary metric card display
- Date range filter with DatePicker
- Chart hover tooltips
- Per-chart error/retry states

---

## 12. Export Requirements

### JPG Export Settings

| Property      | Value                                                                   |
| ------------- | ----------------------------------------------------------------------- |
| Format        | JPG                                                                     |
| Quality       | 85%                                                                     |
| Resolution    | 2× (for Retina)                                                         |
| Background    | White (#FFFFFF)                                                         |
| Max Width     | 1440px (wide desktop), 1024px (desktop), 768px (tablet), 375px (mobile) |
| Color Profile | sRGB                                                                    |

### Naming Convention

```text
[ScreenID]-[ScreenName]-[State]-[Breakpoint].jpg
```

**Examples**:

- `SCR-001-Login-Default-Desktop.jpg`
- `SCR-001-Login-Error-Mobile.jpg`
- `SCR-005-AppointmentSearch-Empty-Tablet.jpg`
- `MOD-001-SessionTimeout-Default-Desktop.jpg`

### Export Manifest

- Total screens: 29
- Total modals: 7
- States per screen: 5 (Default, Loading, Empty, Error, Validation)
- Breakpoints per screen: 4 (375px, 768px, 1024px, 1440px)
- **Estimated exports**: (29 × 5 × 4) + (7 × 5 × 2) = 580 + 70 = **650 files**
- File naming: lowercase-hyphenated
- Output folder: `/exports/` within Figma project

---

## 13. Figma File Structure

### Page Organization

```text
Figma File: Unified Patient Access & Clinical Intelligence Platform
|
+-- Page 1: Cover & Overview
|   - Cover frame (project name, version, last updated)
|   - Table of contents linking to all pages
|
+-- Page 2: Design System
|   - Color palette (primitive and semantic tokens)
|   - Typography scale (Plus Jakarta Sans, Inter, JetBrains Mono)
|   - Spacing scale (4px base)
|   - Border radius tokens
|   - Shadow tokens
|   - Icon library (Lucide subset with usage labels)
|   - Component library (all Shadcn components with variants/states)
|
+-- Page 3: Screens — Auth & Shared
|   - SCR-001: Login / Registration (4 breakpoints × 5 states)
|   - SCR-026: MFA Verification (4 breakpoints × 5 states)
|   - SCR-027: MFA Setup (4 breakpoints × 5 states)
|   - SCR-028: Email Verification (4 breakpoints × 5 states)
|   - SCR-029: Password Reset (4 breakpoints × 5 states)
|
+-- Page 4: Screens — Patient Portal
|   - SCR-002: Patient Dashboard (4 breakpoints × 5 states)
|   - SCR-005–SCR-016 (each: 4 breakpoints × 5 states)
|   All patient screens grouped with clear section dividers
|
+-- Page 5: Screens — Staff & Admin
|   - SCR-003: Staff Dashboard (4 breakpoints × 5 states)
|   - SCR-004: Admin Dashboard (4 breakpoints × 5 states)
|   - SCR-017–SCR-025 (each: 4 breakpoints × 5 states)
|
+-- Page 6: Modals & Flows
|   - MOD-001–MOD-007 (each: 2 breakpoints × 5 states)
|   - FL-001–FL-017 (flow connector diagrams)
```

### Frame Naming Convention

```text
[ScreenID] / [State] / [Breakpoint]
```

**Example**: `SCR-005 / Default / Desktop`

---

## 14. Quality Checklist

### Pre-Export Validation

| #   | Check                                                                               | Status |
| --- | ----------------------------------------------------------------------------------- | ------ |
| 1   | All 29 screens have 5 states (Default, Loading, Empty, Error, Validation)           | ☐      |
| 2   | All 29 screens have 4 breakpoints (375px, 768px, 1024px, 1440px)                    | ☐      |
| 3   | All 7 modals have 5 states at 2 breakpoints (Desktop, Mobile)                       | ☐      |
| 4   | Color contrast ratios ≥4.5:1 for normal text, ≥3:1 for large text and UI components | ☐      |
| 5   | All interactive elements show visible focus indicators                              | ☐      |
| 6   | Touch targets ≥44x44px on mobile breakpoints                                        | ☐      |
| 7   | All color-coded indicators include redundant icon or text label                     | ☐      |
| 8   | No placeholder or "Lorem ipsum" text in any state                                   | ☐      |
| 9   | Consistent use of design tokens — no hard-coded colors, sizes, or spacing           | ☐      |
| 10  | All Lucide icons use consistent 1.5px stroke and 16/20/24px sizing                  | ☐      |
| 11  | Sidebar collapses to bottom nav below 768px on all authenticated screens            | ☐      |
| 12  | Data tables transform to stacked cards below 768px                                  | ☐      |
| 13  | All 17 prototype flows have accurate screen-to-screen connections                   | ☐      |
| 14  | Component naming follows `C/<Category>/<Name>` convention                           | ☐      |
| 15  | Frame naming follows `[ScreenID] / [State] / [Breakpoint]` convention               | ☐      |

### Post-Generation Validation

| #   | Check                                                                                                                                        | Status |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| 1   | All 20 use cases (UC-001–UC-020) mapped to at least one screen                                                                               | ☐      |
| 2   | All 3 personas (Patient, Staff, Admin) have dedicated dashboard and workflow screens                                                         | ☐      |
| 3   | All 34 UXRs addressed in screen specifications or component constraints                                                                      | ☐      |
| 4   | All 17 prototype flows (FL-001–FL-017) include complete sequences                                                                            | ☐      |
| 5   | Screen-to-Persona Coverage Matrix has no empty rows                                                                                          | ☐      |
| 6   | Export manifest file count matches screens × states × breakpoints formula                                                                    | ☐      |
| 7   | designsystem.md cross-referenced and consistent with token usage in screens                                                                  | ☐      |
| 8   | No anti-patterns present: `background-clip: text`, `backdrop-filter: blur`, thick side borders, sole-typeface-Inter, purple-to-blue gradient | ☐      |
