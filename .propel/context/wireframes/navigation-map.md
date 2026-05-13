# Navigation Map — HealthAccess

## Flow Index

| Flow   | Name                  | Screens                                             | Persona      |
| ------ | --------------------- | --------------------------------------------------- | ------------ |
| FL-001 | Patient registration  | SCR-001 → SCR-028                                   | Patient      |
| FL-002 | Staff/Admin MFA login | SCR-001 → SCR-026 → SCR-003 / SCR-004               | Staff, Admin |
| FL-003 | Book appointment      | SCR-002 → SCR-005 → MOD-007 → MOD-002 → SCR-006     | Patient      |
| FL-004 | Complete intake       | SCR-002 → SCR-009 ↔ SCR-010 → SCR-002               | Patient      |
| FL-005 | Upload documents      | SCR-002 → SCR-011 → SCR-012                         | Patient      |
| FL-006 | Walk-in booking       | SCR-003 → SCR-018 → SCR-017                         | Staff        |
| FL-007 | Conflict resolution   | SCR-003 → SCR-019 → SCR-020 (→ MOD-005)             | Staff        |
| FL-008 | Code mapping          | SCR-003/019 → SCR-021                               | Staff        |
| FL-009 | User management       | SCR-004 → SCR-023 → MOD-004                         | Admin        |
| FL-010 | Reschedule / cancel   | SCR-002/016 → SCR-007 → MOD-003 / SCR-008 → SCR-006 | Patient      |
| FL-011 | Session timeout       | Any → MOD-001 → Continue / SCR-001                  | All          |
| FL-012 | Calendar sync         | SCR-002 → SCR-013                                   | Patient      |
| FL-013 | Profile settings      | SCR-002 → SCR-015                                   | Patient      |
| FL-014 | Staff schedule        | SCR-003 → SCR-022 → SCR-019                         | Staff        |
| FL-015 | Insurance pre-check   | SCR-002 → SCR-014                                   | Patient      |
| FL-016 | Audit log review      | SCR-004 → SCR-024                                   | Admin        |
| FL-017 | Platform metrics      | SCR-004 → SCR-025                                   | Admin        |

## Screen-to-Screen Links

### SCR-001 Login / Registration

| Element                | Target  | Condition                      |
| ---------------------- | ------- | ------------------------------ |
| Login submit (Patient) | SCR-002 | Role = Patient                 |
| Login submit (Staff)   | SCR-003 | Role = Staff (via SCR-026 MFA) |
| Login submit (Admin)   | SCR-004 | Role = Admin (via SCR-026 MFA) |
| Register submit        | SCR-028 | New account                    |
| Google OAuth           | SCR-002 | Social login                   |
| Microsoft OAuth        | SCR-002 | Social login                   |
| Forgot password        | SCR-029 | —                              |

### SCR-002 Patient Dashboard

| Element            | Target  |
| ------------------ | ------- |
| Book appointment   | SCR-005 |
| Start intake       | SCR-009 |
| Upload document    | SCR-011 |
| Appointment row    | SCR-007 |
| Nav: Appointments  | SCR-005 |
| Nav: Intake        | SCR-009 |
| Nav: Documents     | SCR-011 |
| Nav: History       | SCR-016 |
| Nav: Insurance     | SCR-014 |
| Nav: Calendar sync | SCR-013 |
| Nav: Profile       | SCR-015 |
| Nav: Log out       | SCR-001 |

### SCR-003 Staff Dashboard

| Element             | Target  |
| ------------------- | ------- |
| Walk-in booking     | SCR-018 |
| View full queue     | SCR-017 |
| Patient lookup      | SCR-019 |
| Queue row (patient) | SCR-019 |
| Conflict alert link | SCR-020 |
| Code mapping alert  | SCR-021 |
| Nav: Queue          | SCR-017 |
| Nav: Walk-in        | SCR-018 |
| Nav: Schedule       | SCR-022 |
| Nav: Patient lookup | SCR-019 |
| Nav: Code mapping   | SCR-021 |

### SCR-004 Admin Dashboard

| Element               | Target  |
| --------------------- | ------- |
| User management card  | SCR-023 |
| Audit log card        | SCR-024 |
| Platform metrics card | SCR-025 |
| Nav: Users            | SCR-023 |
| Nav: Audit log        | SCR-024 |
| Nav: Metrics          | SCR-025 |

### SCR-005 Appointment Search

| Element     | Target                |
| ----------- | --------------------- |
| Select slot | MOD-007 (inline lock) |
| Confirm bar | MOD-002               |
| Back        | SCR-002               |

### SCR-006 Booking Confirmation

| Element             | Target  |
| ------------------- | ------- |
| View appointment    | SCR-007 |
| Book another        | SCR-005 |
| Return to dashboard | SCR-002 |

### SCR-007 Appointment Detail

| Element    | Target  |
| ---------- | ------- |
| Reschedule | SCR-008 |
| Cancel     | MOD-003 |
| Back       | SCR-002 |

### SCR-008 Reschedule Slot Picker

| Element         | Target  |
| --------------- | ------- |
| Select new slot | SCR-006 |
| Back            | SCR-007 |

### SCR-009 AI Intake ↔ SCR-010 Manual Intake

| Element           | Target  |
| ----------------- | ------- |
| Tab: Manual form  | SCR-010 |
| Tab: AI assistant | SCR-009 |
| Submit            | SCR-002 |
| Back              | SCR-002 |

### SCR-011 Document Upload

| Element            | Target  |
| ------------------ | ------- |
| View all documents | SCR-012 |
| Invalid file drop  | MOD-006 |
| Back               | SCR-002 |

### SCR-012 Document List

| Element       | Target  |
| ------------- | ------- |
| Upload button | SCR-011 |
| Back          | SCR-002 |

### SCR-013 Calendar Sync

| Element | Target  |
| ------- | ------- |
| Back    | SCR-002 |

### SCR-014 Insurance Form

| Element   | Target  |
| --------- | ------- |
| Skip link | SCR-002 |
| Back      | SCR-002 |

### SCR-015 Patient Profile

| Element | Target  |
| ------- | ------- |
| Back    | SCR-002 |

### SCR-016 Appointment History

| Element    | Target  |
| ---------- | ------- |
| View (row) | SCR-007 |
| Book new   | SCR-005 |
| Back       | SCR-002 |

### SCR-017 Same-Day Queue

| Element            | Target  |
| ------------------ | ------- |
| View (patient row) | SCR-019 |
| + Walk-in          | SCR-018 |
| Back               | SCR-003 |

### SCR-018 Walk-in Booking

| Element      | Target  |
| ------------ | ------- |
| Add to queue | SCR-017 |
| Back         | SCR-003 |

### SCR-019 Patient 360° View

| Element          | Target  |
| ---------------- | ------- |
| Conflicts button | SCR-020 |
| Code mapping     | SCR-021 |
| Back             | SCR-017 |

### SCR-020 Conflict Resolution

| Element        | Target  |
| -------------- | ------- |
| Details button | MOD-005 |
| Back           | SCR-019 |

### SCR-021 Code Mapping

| Element | Target  |
| ------- | ------- |
| Back    | SCR-019 |

### SCR-022 Staff Schedule

| Element           | Target  |
| ----------------- | ------- |
| Appointment block | SCR-019 |
| Back              | SCR-003 |

### SCR-023 User Management

| Element     | Target  |
| ----------- | ------- |
| Create user | MOD-004 |
| Edit (row)  | MOD-004 |
| Back        | SCR-004 |

### SCR-024 Audit Log

| Element | Target  |
| ------- | ------- |
| Back    | SCR-004 |

### SCR-025 Platform Metrics

| Element | Target  |
| ------- | ------- |
| Back    | SCR-004 |

### SCR-026 MFA Verification

| Element          | Target                                   |
| ---------------- | ---------------------------------------- |
| Verify           | SCR-002 / SCR-003 / SCR-004 (role-based) |
| Different method | SCR-001                                  |

### SCR-027 MFA Setup

| Element         | Target  |
| --------------- | ------- |
| Verify & enable | SCR-026 |
| Send code       | SCR-026 |

### SCR-028 Email Verification

| Element | Target  |
| ------- | ------- |
| Sign in | SCR-001 |

### SCR-029 Password Reset

| Element         | Target  |
| --------------- | ------- |
| Reset password  | SCR-001 |
| Back to sign in | SCR-001 |

### MOD-001 Session Timeout

| Element        | Target                                 |
| -------------- | -------------------------------------- |
| Extend session | Closes modal (stays on current screen) |
| Log out        | SCR-001                                |

### MOD-002 Booking Confirm

| Element         | Target                           |
| --------------- | -------------------------------- |
| Confirm booking | SCR-006                          |
| Cancel          | Closes dialog (stays on SCR-005) |

### MOD-003 Cancel Confirm

| Element          | Target                           |
| ---------------- | -------------------------------- |
| Yes, cancel      | SCR-002                          |
| Keep appointment | Closes dialog (stays on SCR-007) |

### MOD-004 Create/Edit User

| Element     | Target                                        |
| ----------- | --------------------------------------------- |
| Create/Save | Closes dialog (stays on SCR-023, shows toast) |
| Cancel      | Closes dialog                                 |

### MOD-005 Conflict Detail

| Element                     | Target                           |
| --------------------------- | -------------------------------- |
| Accept value / Apply custom | Closes dialog (stays on SCR-020) |
| Close                       | Closes dialog                    |

### MOD-006 File Rejection

| Element | Target                           |
| ------- | -------------------------------- |
| OK      | Closes dialog (stays on SCR-011) |

### MOD-007 Slot Lock Timer

| Element            | Target                            |
| ------------------ | --------------------------------- |
| Proceed to confirm | MOD-002                           |
| Release slot       | Closes overlay (stays on SCR-005) |

## Dead Ends and Exceptions

| Screen                   | Observation                                     | Resolution                                                |
| ------------------------ | ----------------------------------------------- | --------------------------------------------------------- |
| SCR-013 Calendar Sync    | No forward navigation beyond connect/disconnect | By design — settings screen, back to dashboard            |
| SCR-014 Insurance Form   | No mandatory forward navigation                 | By design — optional pre-check, skip returns to dashboard |
| SCR-015 Patient Profile  | No forward navigation                           | By design — settings screen, back to dashboard            |
| SCR-024 Audit Log        | No forward navigation beyond expand/export      | By design — read-only log viewer                          |
| SCR-025 Platform Metrics | No forward navigation                           | By design — analytics dashboard                           |
| SCR-021 Code Mapping     | No forward navigation                           | By design — code review workspace, back to patient view   |
| MOD-001 Session Timeout  | Timer expiry auto-redirects to SCR-001          | Handled by countdown reaching 0:00                        |
