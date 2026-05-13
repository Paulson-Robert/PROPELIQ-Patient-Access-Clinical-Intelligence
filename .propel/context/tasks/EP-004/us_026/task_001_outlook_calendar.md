# Task - TASK_001

## Requirement Reference

- **User Story:** US_026
- **Story Location:** .propel/context/tasks/EP-004/us_026/us_026.md
- **Acceptance Criteria:**
  - AC-01: OAuth 2.0 flow connects Microsoft Outlook via Graph API
  - AC-02: Access/refresh tokens stored encrypted
  - AC-03: Appointments sync to Outlook Calendar
  - AC-04: Cancellations remove Outlook events
  - AC-05: Background sync job (5-minute interval)
- **Edge Cases:**
  - Consent revoked — mark disconnected, notify user

---

## Applicable Technology Stack

| Layer      | Technology   | Version | Justification            |
| ---------- | ------------ | ------- | ------------------------ |
| Backend    | ASP.NET Core | 9.0     | TR-002 — Microsoft Graph |
| Background | Hangfire     | 1.8.x   | TR-005 — periodic sync   |

---

## Task Overview

Implement Microsoft Outlook Calendar integration via Graph API OAuth, encrypted token storage, event sync, and periodic background sync.

## Dependent Tasks

- US_025 task (Google Calendar) — shared CalendarTokenStore pattern

## Impacted Components

- Infrastructure/Calendar/OutlookCalendarService.cs — Graph API client
- Application/Commands/ConnectOutlookCalendarCommand.cs — OAuth flow
- Infrastructure/Jobs/OutlookCalendarSyncJob.cs — periodic sync

## Implementation Plan

1. Implement OAuth 2.0 flow for Microsoft Graph Calendar permissions
2. Reuse CalendarTokenStore for encrypted Outlook tokens
3. Create OutlookCalendarService wrapping Graph API calendar endpoints
4. Implement event CRUD on appointment changes
5. Create Hangfire recurring job for sync

## Expected Changes

| Action | File Path                                                 | Description |
| ------ | --------------------------------------------------------- | ----------- |
| CREATE | src/Infrastructure/Calendar/OutlookCalendarService.cs     | Graph API   |
| CREATE | src/Application/Commands/ConnectOutlookCalendarCommand.cs | OAuth       |
| CREATE | src/Infrastructure/Jobs/OutlookCalendarSyncJob.cs         | Sync job    |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — event mapping, token handling

## Implementation Checklist

- [ ] Implement Microsoft Graph OAuth 2.0 flow (AC-01)
- [ ] Store tokens encrypted reusing CalendarTokenStore (AC-02)
- [ ] Sync appointments to Outlook Calendar (AC-03)
- [ ] Remove events on cancellation (AC-04)
- [ ] Create Hangfire recurring sync job (AC-05)
- [ ] Handle consent revocation gracefully (Edge Cases)
