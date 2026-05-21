# Task - TASK_001

## Requirement Reference

- **User Story:** US_025
- **Story Location:** .propel/context/tasks/EP-004/us_025/us_025.md
- **Acceptance Criteria:**
  - AC-01: OAuth 2.0 flow connects Google Calendar account
  - AC-02: Access/refresh tokens stored encrypted
  - AC-03: Appointments sync to Google Calendar as events
  - AC-04: Cancellations remove events from Google Calendar
  - AC-05: Background job syncs periodically (5-minute interval)
- **Edge Cases:**
  - Token refresh failure — mark integration as disconnected, notify user
  - Calendar API rate limit — exponential backoff

---

## Applicable Technology Stack

| Layer      | Technology                   | Version  | Justification              |
| ---------- | ---------------------------- | -------- | -------------------------- |
| Backend    | ASP.NET Core                 | 9.0      | TR-002 — OAuth flow        |
| Background | Hangfire                     | 1.8.x    | TR-005 — periodic sync     |
| Security   | ASP.NET Core Data Protection | Built-in | NFR-004 — token encryption |

---

## Task Overview

Implement Google Calendar integration via OAuth 2.0, encrypted token storage, bidirectional event sync for appointments, and Hangfire periodic sync job.

## Dependent Tasks

- US_004 task (Hangfire Setup) — background jobs
- US_018 task (Backend Booking) — appointment events

## Impacted Components

- Infrastructure/Calendar/GoogleCalendarService.cs — Google API client
- Infrastructure/Calendar/CalendarTokenStore.cs — encrypted token persistence
- Application/Commands/ConnectGoogleCalendarCommand.cs — OAuth flow
- Infrastructure/Jobs/GoogleCalendarSyncJob.cs — periodic sync

## Implementation Plan

1. Implement OAuth 2.0 authorization code flow for Google Calendar
2. Create encrypted token store using Data Protection API
3. Create GoogleCalendarService wrapping Google Calendar API
4. Implement event creation/deletion on appointment changes
5. Create Hangfire recurring job for 5-minute sync interval
6. Handle token refresh failure and rate limiting

## Expected Changes

| Action | File Path                                                | Description   |
| ------ | -------------------------------------------------------- | ------------- |
| CREATE | src/Infrastructure/Calendar/GoogleCalendarService.cs     | Google API    |
| CREATE | src/Infrastructure/Calendar/CalendarTokenStore.cs        | Token storage |
| CREATE | src/Application/Commands/ConnectGoogleCalendarCommand.cs | OAuth         |
| CREATE | src/Infrastructure/Jobs/GoogleCalendarSyncJob.cs         | Sync job      |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — token storage, event mapping
- [ ] Integration tests pass — OAuth flow (mocked)

## Implementation Checklist

- [x] Implement Google OAuth 2.0 authorization code flow (AC-01)
- [x] Store access/refresh tokens encrypted via Data Protection (AC-02)
- [x] Sync appointments to Google Calendar as events (AC-03)
- [x] Remove events on appointment cancellation (AC-04)
- [x] Create Hangfire recurring job with 5-minute interval (AC-05)
- [x] Handle token refresh failure marking integration disconnected (Edge Cases)
- [x] Implement exponential backoff on rate limits (Edge Cases)
