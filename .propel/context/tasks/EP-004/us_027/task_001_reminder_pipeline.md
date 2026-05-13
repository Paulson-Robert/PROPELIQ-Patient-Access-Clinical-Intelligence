# Task - TASK_001

## Requirement Reference

- **User Story:** US_027
- **Story Location:** .propel/context/tasks/EP-004/us_027/us_027.md
- **Acceptance Criteria:**
  - AC-01: Reminder sent 24 hours before appointment
  - AC-02: Reminder sent 2 hours before appointment
  - AC-03: Risk-based escalation — High risk gets additional SMS reminder
  - AC-04: Failed delivery retried with exponential backoff (max 3 attempts)
  - AC-05: Delivery status tracked per notification
- **Edge Cases:**
  - All retries exhausted — marked as failed, staff alerted
  - Appointment cancelled after reminder queued — reminder suppressed

---

## Applicable Technology Stack

| Layer      | Technology   | Version | Justification                 |
| ---------- | ------------ | ------- | ----------------------------- |
| Backend    | ASP.NET Core | 9.0     | TR-002 — notification service |
| Background | Hangfire     | 1.8.x   | TR-005 — scheduled reminders  |

---

## Task Overview

Implement appointment reminder pipeline with 24h and 2h scheduled notifications, risk-based escalation for high-risk patients, retry with exponential backoff, and delivery tracking.

## Dependent Tasks

- US_004 task (Hangfire Setup) — job scheduling
- US_041 task (Risk Algorithm) — risk tier for escalation

## Impacted Components

- Infrastructure/Notifications/ReminderPipelineService.cs — pipeline orchestration
- Infrastructure/Jobs/ScheduleRemindersJob.cs — reminder scheduling
- Infrastructure/Notifications/NotificationDeliveryService.cs — delivery with retry

## Implementation Plan

1. Create ScheduleRemindersJob scanning upcoming appointments
2. Schedule 24h and 2h reminder Hangfire jobs on booking
3. Implement risk-based escalation adding SMS for high-risk patients
4. Create NotificationDeliveryService with exponential backoff retry
5. Track delivery status (queued, sent, failed) per notification
6. Suppress reminders for cancelled appointments

## Expected Changes

| Action | File Path                                                       | Description |
| ------ | --------------------------------------------------------------- | ----------- |
| CREATE | src/Infrastructure/Notifications/ReminderPipelineService.cs     | Pipeline    |
| CREATE | src/Infrastructure/Jobs/ScheduleRemindersJob.cs                 | Scheduling  |
| CREATE | src/Infrastructure/Notifications/NotificationDeliveryService.cs | Delivery    |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — scheduling logic, retry backoff, suppression

## Implementation Checklist

- [ ] Schedule 24h reminder on appointment booking (AC-01)
- [ ] Schedule 2h reminder on appointment booking (AC-02)
- [ ] Add SMS escalation for high-risk patients (AC-03)
- [ ] Implement exponential backoff retry (max 3 attempts) (AC-04)
- [ ] Track delivery status per notification (AC-05)
- [ ] Suppress reminders for cancelled appointments (Edge Cases)
- [ ] Alert staff when all retries exhausted (Edge Cases)
