# Export Manifest — HealthAccess

## Export Settings

| Setting       | Value           |
| ------------- | --------------- |
| Format        | JPG (JPEG)      |
| Quality       | High (85%)      |
| Scale         | 2×              |
| Color Profile | sRGB            |
| Background    | White (#FFFFFF) |

## Naming Convention

```
HealthAccess__Web__<ScreenName>__<State>__v1.jpg
```

## Breakpoint Dimensions

| Breakpoint | Label   | Width  | Height |
| ---------- | ------- | ------ | ------ |
| Mobile     | Mobile  | 375px  | 844px  |
| Tablet     | Tablet  | 768px  | 1024px |
| Desktop    | Desktop | 1024px | 1024px |
| Wide       | Wide    | 1440px | 1024px |

## Screen Exports (29 Screens × 5 States × 4 Breakpoints = 580 Files)

### Auth & Shared (5 screens × 20 = 100 files)

| #      | File Name                                                      | Screen  | State      | Breakpoint | Dimensions     |
| ------ | -------------------------------------------------------------- | ------- | ---------- | ---------- | -------------- |
| 1      | `HealthAccess__Web__SCR-001-Login__Default__v1.jpg`            | SCR-001 | Default    | Wide       | 2880×2048      |
| 2      | `HealthAccess__Web__SCR-001-Login__Default-Desktop__v1.jpg`    | SCR-001 | Default    | Desktop    | 2048×2048      |
| 3      | `HealthAccess__Web__SCR-001-Login__Default-Tablet__v1.jpg`     | SCR-001 | Default    | Tablet     | 1536×2048      |
| 4      | `HealthAccess__Web__SCR-001-Login__Default-Mobile__v1.jpg`     | SCR-001 | Default    | Mobile     | 750×1688       |
| 5      | `HealthAccess__Web__SCR-001-Login__Loading__v1.jpg`            | SCR-001 | Loading    | Wide       | 2880×2048      |
| 6      | `HealthAccess__Web__SCR-001-Login__Loading-Desktop__v1.jpg`    | SCR-001 | Loading    | Desktop    | 2048×2048      |
| 7      | `HealthAccess__Web__SCR-001-Login__Loading-Tablet__v1.jpg`     | SCR-001 | Loading    | Tablet     | 1536×2048      |
| 8      | `HealthAccess__Web__SCR-001-Login__Loading-Mobile__v1.jpg`     | SCR-001 | Loading    | Mobile     | 750×1688       |
| 9      | `HealthAccess__Web__SCR-001-Login__Empty__v1.jpg`              | SCR-001 | Empty      | Wide       | 2880×2048      |
| 10     | `HealthAccess__Web__SCR-001-Login__Empty-Desktop__v1.jpg`      | SCR-001 | Empty      | Desktop    | 2048×2048      |
| 11     | `HealthAccess__Web__SCR-001-Login__Empty-Tablet__v1.jpg`       | SCR-001 | Empty      | Tablet     | 1536×2048      |
| 12     | `HealthAccess__Web__SCR-001-Login__Empty-Mobile__v1.jpg`       | SCR-001 | Empty      | Mobile     | 750×1688       |
| 13     | `HealthAccess__Web__SCR-001-Login__Error__v1.jpg`              | SCR-001 | Error      | Wide       | 2880×2048      |
| 14     | `HealthAccess__Web__SCR-001-Login__Error-Desktop__v1.jpg`      | SCR-001 | Error      | Desktop    | 2048×2048      |
| 15     | `HealthAccess__Web__SCR-001-Login__Error-Tablet__v1.jpg`       | SCR-001 | Error      | Tablet     | 1536×2048      |
| 16     | `HealthAccess__Web__SCR-001-Login__Error-Mobile__v1.jpg`       | SCR-001 | Error      | Mobile     | 750×1688       |
| 17     | `HealthAccess__Web__SCR-001-Login__Validation__v1.jpg`         | SCR-001 | Validation | Wide       | 2880×2048      |
| 18     | `HealthAccess__Web__SCR-001-Login__Validation-Desktop__v1.jpg` | SCR-001 | Validation | Desktop    | 2048×2048      |
| 19     | `HealthAccess__Web__SCR-001-Login__Validation-Tablet__v1.jpg`  | SCR-001 | Validation | Tablet     | 1536×2048      |
| 20     | `HealthAccess__Web__SCR-001-Login__Validation-Mobile__v1.jpg`  | SCR-001 | Validation | Mobile     | 750×1688       |
| 21-40  | `HealthAccess__Web__SCR-026-MFAVerification__*__v1.jpg`        | SCR-026 | All 5      | All 4      | Per breakpoint |
| 41-60  | `HealthAccess__Web__SCR-027-MFASetup__*__v1.jpg`               | SCR-027 | All 5      | All 4      | Per breakpoint |
| 61-80  | `HealthAccess__Web__SCR-028-EmailVerification__*__v1.jpg`      | SCR-028 | All 5      | All 4      | Per breakpoint |
| 81-100 | `HealthAccess__Web__SCR-029-PasswordReset__*__v1.jpg`          | SCR-029 | All 5      | All 4      | Per breakpoint |

### Patient Portal (14 screens × 20 = 280 files)

| Range   | Screen ID | Screen Name            | States | Breakpoints | Files   |
| ------- | --------- | ---------------------- | ------ | ----------- | ------- |
| 101-120 | SCR-002   | PatientDashboard       | 5      | 4           | 20      |
| 121-140 | SCR-005   | AppointmentSearch      | 5      | 4           | 20      |
| 141-160 | SCR-006   | BookingConfirmation    | 5      | 4           | 20      |
| 161-180 | SCR-007   | AppointmentDetail      | 5      | 4           | 20      |
| 181-200 | SCR-008   | RescheduleSlotPicker   | 5      | 4           | 20      |
| 201-220 | SCR-009   | AIConversationalIntake | 5      | 4           | 20      |
| 221-240 | SCR-010   | ManualFormIntake       | 5      | 4           | 20      |
| 241-260 | SCR-011   | DocumentUpload         | 5      | 4           | 20      |
| 261-280 | SCR-012   | DocumentListStatus     | 5      | 4           | 20      |
| 281-300 | SCR-013   | CalendarSyncSettings   | 5      | 4           | 20      |
| 301-320 | SCR-014   | InsuranceForm          | 5      | 4           | 20      |
| 321-340 | SCR-015   | PatientProfile         | 5      | 4           | 20      |
| 341-360 | SCR-016   | AppointmentHistory     | 5      | 4           | 20      |
| —       | —         | **Subtotal**           | —      | —           | **280** |

### Staff & Admin Portal (10 screens × 20 = 200 files)

| Range   | Screen ID | Screen Name          | States | Breakpoints | Files   |
| ------- | --------- | -------------------- | ------ | ----------- | ------- |
| 361-380 | SCR-003   | StaffDashboard       | 5      | 4           | 20      |
| 381-400 | SCR-004   | AdminDashboard       | 5      | 4           | 20      |
| 401-420 | SCR-017   | SameDayQueue         | 5      | 4           | 20      |
| 421-440 | SCR-018   | WalkinBooking        | 5      | 4           | 20      |
| 441-460 | SCR-019   | Patient360View       | 5      | 4           | 20      |
| 461-480 | SCR-020   | ConflictResolution   | 5      | 4           | 20      |
| 481-500 | SCR-021   | CodeMappingInterface | 5      | 4           | 20      |
| 501-520 | SCR-022   | StaffScheduleView    | 5      | 4           | 20      |
| 521-540 | SCR-023   | UserManagement       | 5      | 4           | 20      |
| 541-560 | SCR-024   | AuditLogViewer       | 5      | 4           | 20      |
| 561-580 | SCR-025   | PlatformMetrics      | 5      | 4           | 20      |
| —       | —         | **Subtotal**         | —      | —           | **200** |

### Modals & Overlays (7 modals × 5 States × 2 Breakpoints = 70 files)

| Range   | Screen ID | Screen Name              | States | Breakpoints         | Files  |
| ------- | --------- | ------------------------ | ------ | ------------------- | ------ |
| 581-590 | MOD-001   | SessionTimeoutWarning    | 5      | 2 (Desktop, Mobile) | 10     |
| 591-600 | MOD-002   | BookingConfirmation      | 5      | 2                   | 10     |
| 601-610 | MOD-003   | CancellationConfirmation | 5      | 2                   | 10     |
| 611-620 | MOD-004   | CreateEditUser           | 5      | 2                   | 10     |
| 621-630 | MOD-005   | ConflictDetail           | 5      | 2                   | 10     |
| 631-640 | MOD-006   | FileRejectionAlert       | 5      | 2                   | 10     |
| 641-650 | MOD-007   | SlotLockTimer            | 5      | 2                   | 10     |
| —       | —         | **Subtotal**             | —      | —                   | **70** |

## Export Summary

| Category       | Screens | States | Breakpoints | Total Files |
| -------------- | ------- | ------ | ----------- | ----------- |
| Auth & Shared  | 5       | 5      | 4           | 100         |
| Patient Portal | 14      | 5      | 4           | 280         |
| Staff & Admin  | 10      | 5      | 4           | 200         |
| Modals         | 7       | 5      | 2           | 70          |
| **Total**      | **36**  | **5**  | **4/2**     | **650**     |

## Calculation Verification

```
Screens: (29 screens × 5 states × 4 breakpoints) = 580
Modals:  (7 modals × 5 states × 2 breakpoints) = 70
Total:   580 + 70 = 650 files
```

## Output Folder Structure

```
/exports/
├── auth/
│   ├── HealthAccess__Web__SCR-001-Login__Default__v1.jpg
│   ├── HealthAccess__Web__SCR-001-Login__Default-Desktop__v1.jpg
│   ├── HealthAccess__Web__SCR-001-Login__Default-Tablet__v1.jpg
│   ├── HealthAccess__Web__SCR-001-Login__Default-Mobile__v1.jpg
│   ├── ... (16 more per screen)
│   ├── HealthAccess__Web__SCR-026-MFAVerification__*__v1.jpg
│   ├── HealthAccess__Web__SCR-027-MFASetup__*__v1.jpg
│   ├── HealthAccess__Web__SCR-028-EmailVerification__*__v1.jpg
│   └── HealthAccess__Web__SCR-029-PasswordReset__*__v1.jpg
├── patient/
│   ├── HealthAccess__Web__SCR-002-PatientDashboard__*__v1.jpg
│   ├── HealthAccess__Web__SCR-005-AppointmentSearch__*__v1.jpg
│   ├── ... (12 more screens)
│   └── HealthAccess__Web__SCR-016-AppointmentHistory__*__v1.jpg
├── staff-admin/
│   ├── HealthAccess__Web__SCR-003-StaffDashboard__*__v1.jpg
│   ├── HealthAccess__Web__SCR-004-AdminDashboard__*__v1.jpg
│   ├── ... (9 more screens)
│   └── HealthAccess__Web__SCR-025-PlatformMetrics__*__v1.jpg
└── modals/
    ├── HealthAccess__Web__MOD-001-SessionTimeout__*__v1.jpg
    ├── HealthAccess__Web__MOD-002-BookingConfirm__*__v1.jpg
    ├── ... (5 more modals)
    └── HealthAccess__Web__MOD-007-SlotLockTimer__*__v1.jpg
```
