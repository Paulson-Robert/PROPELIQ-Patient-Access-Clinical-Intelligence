# Information Architecture — HealthAccess

## 1. Wireframe Specification

- **Fidelity Level**: High
- **Screen Type**: Web (Responsive)
- **Viewport**: 1440 × 900 (primary), 768 px (tablet), 375 px (mobile)

## 2. System Overview

HealthAccess is a Unified Patient Access & Clinical Intelligence Platform that streamlines appointment booking, AI-powered clinical intake, document processing, and medical code mapping. Three user personas (Patient, Staff, Admin) interact through role-specific dashboards sharing a common design system built on Shadcn UI / Tailwind CSS tokens.

## 3. Wireframe References

### HTML Wireframes

| Screen                           | File Path                                                                                            | Description                                                                      | Fidelity |
| -------------------------------- | ---------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- | -------- |
| SCR-001 Login / Registration     | [wireframe-SCR-001-login-registration.html](Hi-Fi/wireframe-SCR-001-login-registration.html)         | Split auth layout with social login, tab-switch login/register                   | High     |
| SCR-002 Patient Dashboard        | [wireframe-SCR-002-patient-dashboard.html](Hi-Fi/wireframe-SCR-002-patient-dashboard.html)           | Patient home with quick actions, upcoming appointments, documents, notifications | High     |
| SCR-003 Staff Dashboard          | [wireframe-SCR-003-staff-dashboard.html](Hi-Fi/wireframe-SCR-003-staff-dashboard.html)               | Staff home with stat cards, queue preview, alerts panel                          | High     |
| SCR-004 Admin Dashboard          | [wireframe-SCR-004-admin-dashboard.html](Hi-Fi/wireframe-SCR-004-admin-dashboard.html)               | Admin shell with overview cards linking to sub-sections                          | High     |
| SCR-005 Appointment Search       | [wireframe-SCR-005-appointment-search.html](Hi-Fi/wireframe-SCR-005-appointment-search.html)         | Provider/specialty/date filter bar, slot grid, lock countdown                    | High     |
| SCR-006 Booking Confirmation     | [wireframe-SCR-006-booking-confirmation.html](Hi-Fi/wireframe-SCR-006-booking-confirmation.html)     | Success screen with appointment summary and next actions                         | High     |
| SCR-007 Appointment Detail       | [wireframe-SCR-007-appointment-detail.html](Hi-Fi/wireframe-SCR-007-appointment-detail.html)         | Provider card, info grid, reschedule/cancel actions                              | High     |
| SCR-008 Reschedule Slot Picker   | [wireframe-SCR-008-reschedule-slot-picker.html](Hi-Fi/wireframe-SCR-008-reschedule-slot-picker.html) | Current appointment summary + available slot grid                                | High     |
| SCR-009 AI Conversational Intake | [wireframe-SCR-009-ai-intake.html](Hi-Fi/wireframe-SCR-009-ai-intake.html)                           | Chat panel + extracted data sidebar with confidence badges                       | High     |
| SCR-010 Manual Form Intake       | [wireframe-SCR-010-manual-intake.html](Hi-Fi/wireframe-SCR-010-manual-intake.html)                   | Accordion form with AI-extracted pre-fill badges                                 | High     |
| SCR-011 Document Upload          | [wireframe-SCR-011-document-upload.html](Hi-Fi/wireframe-SCR-011-document-upload.html)               | Dropzone with upload queue and progress bars                                     | High     |
| SCR-012 Document List            | [wireframe-SCR-012-document-list.html](Hi-Fi/wireframe-SCR-012-document-list.html)                   | Table with pipeline status step indicators                                       | High     |
| SCR-013 Calendar Sync            | [wireframe-SCR-013-calendar-sync.html](Hi-Fi/wireframe-SCR-013-calendar-sync.html)                   | Google/Outlook provider cards with connect/disconnect                            | High     |
| SCR-014 Insurance Form           | [wireframe-SCR-014-insurance-form.html](Hi-Fi/wireframe-SCR-014-insurance-form.html)                 | Autocomplete provider, ID input, validation result                               | High     |
| SCR-015 Patient Profile          | [wireframe-SCR-015-patient-profile.html](Hi-Fi/wireframe-SCR-015-patient-profile.html)               | Profile edit, password change, notification toggles, data deletion               | High     |
| SCR-016 Appointment History      | [wireframe-SCR-016-appointment-history.html](Hi-Fi/wireframe-SCR-016-appointment-history.html)       | Paginated table with status filter and date range                                | High     |
| SCR-017 Same-Day Queue           | [wireframe-SCR-017-same-day-queue.html](Hi-Fi/wireframe-SCR-017-same-day-queue.html)                 | Drag-reorderable queue with risk tier badges, summary bar                        | High     |
| SCR-018 Walk-in Booking          | [wireframe-SCR-018-walk-in-booking.html](Hi-Fi/wireframe-SCR-018-walk-in-booking.html)               | Patient search, create new, provider assign, add-to-queue                        | High     |
| SCR-019 Patient 360° View        | [wireframe-SCR-019-patient-360.html](Hi-Fi/wireframe-SCR-019-patient-360.html)                       | Patient header + tabbed sections (Vitals, Meds, Allergies, Dx, Procedures)       | High     |
| SCR-020 Conflict Resolution      | [wireframe-SCR-020-conflict-resolution.html](Hi-Fi/wireframe-SCR-020-conflict-resolution.html)       | Side-by-side conflict values with accept/custom actions                          | High     |
| SCR-021 Code Mapping             | [wireframe-SCR-021-code-mapping.html](Hi-Fi/wireframe-SCR-021-code-mapping.html)                     | AI-suggested codes table with confidence, manual entry                           | High     |
| SCR-022 Staff Schedule           | [wireframe-SCR-022-staff-schedule.html](Hi-Fi/wireframe-SCR-022-staff-schedule.html)                 | Day/week calendar grid by provider with color-coded blocks                       | High     |
| SCR-023 User Management          | [wireframe-SCR-023-user-management.html](Hi-Fi/wireframe-SCR-023-user-management.html)               | DataTable with search, role filter, pagination, create user                      | High     |
| SCR-024 Audit Log                | [wireframe-SCR-024-audit-log.html](Hi-Fi/wireframe-SCR-024-audit-log.html)                           | Read-only log table with action/resource filters, export                         | High     |
| SCR-025 Platform Metrics         | [wireframe-SCR-025-platform-metrics.html](Hi-Fi/wireframe-SCR-025-platform-metrics.html)             | 4 stat cards + 3 chart placeholders with date range filter                       | High     |
| SCR-026 MFA Verification         | [wireframe-SCR-026-mfa-verification.html](Hi-Fi/wireframe-SCR-026-mfa-verification.html)             | 6-digit OTP input, resend link, alternate method                                 | High     |
| SCR-027 MFA Setup                | [wireframe-SCR-027-mfa-setup.html](Hi-Fi/wireframe-SCR-027-mfa-setup.html)                           | TOTP QR code + SMS phone input options                                           | High     |
| SCR-028 Email Verification       | [wireframe-SCR-028-email-verification.html](Hi-Fi/wireframe-SCR-028-email-verification.html)         | Success/pending states with sign-in CTA                                          | High     |
| SCR-029 Password Reset           | [wireframe-SCR-029-password-reset.html](Hi-Fi/wireframe-SCR-029-password-reset.html)                 | Two-step: email request → new password form                                      | High     |
| MOD-001 Session Timeout          | [wireframe-MOD-001-session-timeout.html](Hi-Fi/wireframe-MOD-001-session-timeout.html)               | AlertDialog with countdown ring, extend/logout                                   | High     |
| MOD-002 Booking Confirm          | [wireframe-MOD-002-booking-confirm.html](Hi-Fi/wireframe-MOD-002-booking-confirm.html)               | Appointment summary dialog with confirm/cancel                                   | High     |
| MOD-003 Cancel Confirm           | [wireframe-MOD-003-cancel-confirm.html](Hi-Fi/wireframe-MOD-003-cancel-confirm.html)                 | Destructive AlertDialog with warning                                             | High     |
| MOD-004 Create/Edit User         | [wireframe-MOD-004-create-edit-user.html](Hi-Fi/wireframe-MOD-004-create-edit-user.html)             | Dialog with name, email, role, status fields                                     | High     |
| MOD-005 Conflict Detail          | [wireframe-MOD-005-conflict-detail.html](Hi-Fi/wireframe-MOD-005-conflict-detail.html)               | Side-by-side values with source, accept, custom entry                            | High     |
| MOD-006 File Rejection           | [wireframe-MOD-006-file-rejection.html](Hi-Fi/wireframe-MOD-006-file-rejection.html)                 | AlertDialog with rejection reason, accepted formats                              | High     |
| MOD-007 Slot Lock Timer          | [wireframe-MOD-007-slot-lock.html](Hi-Fi/wireframe-MOD-007-slot-lock.html)                           | Countdown ring overlay with proceed/release                                      | High     |

### Component Inventory

**Reference**: See [component-inventory.md](./component-inventory.md)

## 4. User Personas & Flows

### Persona 1: Patient (Maria Santos)

- **Role**: Patient
- **Goals**: Book appointments, complete intake, upload documents, manage insurance
- **Key Screens**: SCR-001, SCR-002, SCR-005–SCR-016, SCR-026–SCR-029
- **Primary Flow**: SCR-001 → SCR-002 → SCR-005 → MOD-007 → MOD-002 → SCR-006
- **Decision Points**: Login vs. register, AI vs. manual intake, reschedule vs. cancel

### Persona 2: Staff (Dr. Sarah Chen / Jennifer Walsh)

- **Role**: Clinical staff / front desk
- **Goals**: Manage queue, process walk-ins, review patient data, resolve conflicts, map codes
- **Key Screens**: SCR-003, SCR-017–SCR-022
- **Primary Flow**: SCR-003 → SCR-017 → SCR-019 → SCR-020/SCR-021
- **Decision Points**: Auto-assign vs. manual provider, accept EHR vs. intake value

### Persona 3: Admin

- **Role**: Platform administrator
- **Goals**: Manage users, review audit logs, monitor platform health
- **Key Screens**: SCR-004, SCR-023–SCR-025
- **Primary Flow**: SCR-004 → SCR-023 → MOD-004
- **Decision Points**: Activate/deactivate user, export format

### Prototype Flows

| Flow ID | Name                 | Path                                                |
| ------- | -------------------- | --------------------------------------------------- |
| FL-001  | Patient registration | SCR-001 → SCR-028                                   |
| FL-002  | Staff MFA login      | SCR-001 → SCR-026 → SCR-003/SCR-004                 |
| FL-003  | Book appointment     | SCR-002 → SCR-005 → MOD-007 → MOD-002 → SCR-006     |
| FL-004  | Complete intake      | SCR-002 → SCR-009 ↔ SCR-010 → SCR-002               |
| FL-005  | Upload documents     | SCR-002 → SCR-011 → SCR-012                         |
| FL-006  | Walk-in booking      | SCR-003 → SCR-018 → SCR-017                         |
| FL-007  | Conflict resolution  | SCR-003 → SCR-019 → SCR-020                         |
| FL-008  | Code mapping         | SCR-003/019 → SCR-021                               |
| FL-009  | User management      | SCR-004 → SCR-023 → MOD-004                         |
| FL-010  | Reschedule/cancel    | SCR-002/016 → SCR-007 → MOD-003 / SCR-008 → SCR-006 |
| FL-011  | Session timeout      | Any → MOD-001 → Continue / SCR-001                  |
| FL-012  | Calendar sync        | SCR-002 → SCR-013                                   |
| FL-013  | Profile settings     | SCR-002 → SCR-015                                   |
| FL-014  | Staff schedule       | SCR-003 → SCR-022 → SCR-019                         |
| FL-015  | Insurance pre-check  | SCR-002 → SCR-014                                   |
| FL-016  | Audit log review     | SCR-004 → SCR-024                                   |
| FL-017  | Platform metrics     | SCR-004 → SCR-025                                   |

## 5. Screen Hierarchy

### Level 1: Authentication (Public)

- **SCR-001 Login / Registration** (P0) — Entry point for all users
- **SCR-026 MFA Verification** (P0) — Post-login for staff/admin
- **SCR-027 MFA Setup** (P1) — First-time MFA enrollment
- **SCR-028 Email Verification** (P0) — Post-registration
- **SCR-029 Password Reset** (P1) — Self-service recovery

### Level 2: Patient Portal

- **SCR-002 Patient Dashboard** (P0) — Patient home, entry point after login
  - **SCR-005 Appointment Search** (P0) → **SCR-006 Booking Confirmation** (P0)
  - **SCR-007 Appointment Detail** (P0) → **SCR-008 Reschedule Slot Picker** (P1)
  - **SCR-009 AI Intake** (P0) ↔ **SCR-010 Manual Intake** (P0)
  - **SCR-011 Document Upload** (P1) → **SCR-012 Document List** (P1)
  - **SCR-013 Calendar Sync** (P2)
  - **SCR-014 Insurance Form** (P2)
  - **SCR-015 Patient Profile** (P1)
  - **SCR-016 Appointment History** (P1)

### Level 3: Staff Portal

- **SCR-003 Staff Dashboard** (P0) — Staff home, entry point after login+MFA
  - **SCR-017 Same-Day Queue** (P0)
  - **SCR-018 Walk-in Booking** (P0) → SCR-017
  - **SCR-019 Patient 360° View** (P0) → **SCR-020 Conflict Resolution** (P1) / **SCR-021 Code Mapping** (P1)
  - **SCR-022 Staff Schedule** (P1)

### Level 4: Admin Portal

- **SCR-004 Admin Dashboard** (P0) — Admin home
  - **SCR-023 User Management** (P0)
  - **SCR-024 Audit Log** (P1)
  - **SCR-025 Platform Metrics** (P1)

### Modal/Dialog/Overlay Inventory

| Modal                    | Type        | Trigger Context       | Parent Screen            | Wireframe                                                | Priority |
| ------------------------ | ----------- | --------------------- | ------------------------ | -------------------------------------------------------- | -------- |
| MOD-001 Session Timeout  | AlertDialog | Inactivity timer      | Any authenticated screen | [MOD-001](Hi-Fi/wireframe-MOD-001-session-timeout.html)  | P0       |
| MOD-002 Booking Confirm  | Dialog      | Select slot → Confirm | SCR-005                  | [MOD-002](Hi-Fi/wireframe-MOD-002-booking-confirm.html)  | P0       |
| MOD-003 Cancel Confirm   | AlertDialog | Cancel button         | SCR-007                  | [MOD-003](Hi-Fi/wireframe-MOD-003-cancel-confirm.html)   | P0       |
| MOD-004 Create/Edit User | Dialog      | Create/Edit user      | SCR-023                  | [MOD-004](Hi-Fi/wireframe-MOD-004-create-edit-user.html) | P0       |
| MOD-005 Conflict Detail  | Dialog      | "Details" button      | SCR-020                  | [MOD-005](Hi-Fi/wireframe-MOD-005-conflict-detail.html)  | P1       |
| MOD-006 File Rejection   | AlertDialog | Invalid file upload   | SCR-011                  | [MOD-006](Hi-Fi/wireframe-MOD-006-file-rejection.html)   | P1       |
| MOD-007 Slot Lock Timer  | Overlay     | Slot selection        | SCR-005                  | [MOD-007](Hi-Fi/wireframe-MOD-007-slot-lock.html)        | P0       |

**Modal Behavior Notes:**

- **Focus Management**: Tab trap within modal, return focus to trigger on close
- **Dismissal**: Close button, ESC key; backdrop click disabled on AlertDialogs
- **Responsive**: Modals become full-screen sheets on mobile (375 px)

## 6. Navigation Architecture

```text
SCR-001 Login/Registration
├── SCR-028 Email Verification
├── SCR-029 Password Reset
├── SCR-026 MFA Verification
│   └── SCR-027 MFA Setup
├── SCR-002 Patient Dashboard
│   ├── SCR-005 Appointment Search
│   │   ├── MOD-007 Slot Lock
│   │   └── MOD-002 Booking Confirm → SCR-006
│   ├── SCR-007 Appointment Detail
│   │   ├── SCR-008 Reschedule Slot Picker → SCR-006
│   │   └── MOD-003 Cancel Confirm
│   ├── SCR-009 AI Intake ↔ SCR-010 Manual Intake
│   ├── SCR-011 Document Upload → SCR-012 Document List
│   ├── SCR-013 Calendar Sync
│   ├── SCR-014 Insurance Form
│   ├── SCR-015 Patient Profile
│   └── SCR-016 Appointment History → SCR-007
├── SCR-003 Staff Dashboard
│   ├── SCR-017 Same-Day Queue → SCR-019
│   ├── SCR-018 Walk-in Booking → SCR-017
│   ├── SCR-019 Patient 360°
│   │   ├── SCR-020 Conflict Resolution → MOD-005
│   │   └── SCR-021 Code Mapping
│   └── SCR-022 Staff Schedule → SCR-019
└── SCR-004 Admin Dashboard
    ├── SCR-023 User Management → MOD-004
    ├── SCR-024 Audit Log
    └── SCR-025 Platform Metrics
```

### Navigation Patterns

- **Sidebar**: Persistent left rail (240 px) with role-specific items; collapses to icon-only at 768 px, hidden at 375 px
- **Bottom Nav**: Mobile-only (375 px) with 4 key items per role
- **Breadcrumbs**: Used in booking flow (SCR-005)
- **Back Button**: Present on all child screens

## 7. Interaction Patterns

### Pattern 1: Book Appointment

- **Trigger**: Patient clicks "Book appointment" from SCR-002
- **Flow**: SCR-005 filter → select slot → MOD-007 lock countdown → MOD-002 confirm → SCR-006 success
- **Feedback**: Slot lock countdown (5 min), success animation, toast notification

### Pattern 2: AI Intake with Mode Toggle

- **Trigger**: Patient clicks "Start intake" from SCR-002
- **Flow**: SCR-009 AI chat → extracted data panel updates in real-time → toggle to SCR-010 manual form (pre-filled)
- **Feedback**: Typing indicator, confidence badges, AI-extracted field markers

### Pattern 3: Conflict Resolution

- **Trigger**: Staff clicks conflict alert from SCR-003 or SCR-019
- **Flow**: SCR-020 side-by-side comparison → accept source A, B, or enter custom → MOD-005 for details
- **Feedback**: Conflict card transitions from pending (amber) to resolved (green)

## 8. Error Handling

| Error Scenario      | Trigger                                | Screen/State                       | Recovery                        |
| ------------------- | -------------------------------------- | ---------------------------------- | ------------------------------- |
| Invalid file upload | Upload unsupported format / over 50 MB | MOD-006                            | Dismiss → retry with valid file |
| Failed notification | SMS delivery error                     | SCR-003 alert panel                | Staff re-triggers manually      |
| Slot lock expired   | 5-min countdown ends                   | SCR-005 slot returns to grid       | Select another slot             |
| Network offline     | Connection lost                        | Network banner (shared-tokens.css) | Auto-retry on reconnect         |
| Session timeout     | Inactivity                             | MOD-001                            | Extend or log out               |

## 9. Responsive Strategy

| Breakpoint | Width   | Layout                       | Navigation                     | Key Changes                                     |
| ---------- | ------- | ---------------------------- | ------------------------------ | ----------------------------------------------- |
| Mobile     | 375 px  | Single column                | Bottom nav (4 items)           | Sidebar hidden, modals → sheets, tables → cards |
| Tablet     | 768 px  | Fluid, 2-col where needed    | Sidebar collapsed (icons only) | Reduced grid columns                            |
| Desktop    | 1440 px | Multi-column, sidebar + main | Full sidebar (240 px)          | Full-width tables, side-by-side layouts         |

## 10. Accessibility

### WCAG 2.2 Level AA Compliance

- **Skip links**: All 36 wireframes include skip-to-main link
- **ARIA**: 128 `aria-label` instances, 49 `role` attributes across all files
- **Focus indicators**: 2 px solid ring on all interactive elements via `shared-tokens.css`
- **Touch targets**: Minimum 44 × 44 px on all buttons and links
- **Color contrast**: Semantic token system ensures AA ratios (primary #1e40af on white = 7.5:1)
- **Keyboard navigation**: Tab order follows visual order; modals trap focus

## 11. Content Strategy

### Content Hierarchy

- **H1**: One per page, screen title in header bar
- **H2**: Section headings within page content
- **H3**: Card titles, accordion headers
- **Body**: Inter 400/500, 14 px (var(--text-sm))
- **Monospace**: JetBrains Mono for ICD-10/CPT codes, timestamps, audit IDs

### Sample Data Binding

All wireframes use realistic data from [sample-data.json](data/sample-data.json) — no lorem ipsum. Edge cases include: null insurance (PAT-004), empty phone (PAT-005), hyphenated surnames (James O'Brien-Fitzgerald), resolved conflicts (CNF-003).
