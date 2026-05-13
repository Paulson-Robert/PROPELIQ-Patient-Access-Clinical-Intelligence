# Task - TASK_001

## Requirement Reference

- **User Story:** US_011
- **Story Location:** .propel/context/tasks/EP-DATA/us_011/us_011.md
- **Acceptance Criteria:**
  - AC-01: AvailabilitySlot supports recurring schedule patterns with recurrence fields
  - AC-02: Appointment status tracking covers lifecycle states with timestamps
  - AC-03: PreferredSlotQueue maintains FCFS ordering with status transitions
  - AC-04: NoShowRiskFactor persists all four scoring inputs
  - AC-05: Notification delivery log tracks retry state and channel
- **Edge Cases:**
  - Slot boundary overlap — unique constraint or application validation
  - Orphaned queue entries — cascade status to Expired on appointment cancellation
  - Notification channel independence — SMS/email failures tracked separately

---

## Applicable Technology Stack

| Layer    | Technology            | Version | Justification                                             |
| -------- | --------------------- | ------- | --------------------------------------------------------- |
| Database | PostgreSQL + pgcrypto | 16.x    | DR-008, DR-009, DR-010 — scheduling and notification data |
| ORM      | Entity Framework Core | 9.0     | TR-004 — entity configuration                             |

---

## Task Overview

Configure the database schema for appointment scheduling (AvailabilitySlot with recurrence, Appointment lifecycle statuses, PreferredSlotQueue FCFS ordering), NoShowRiskFactor persistence with all four scoring inputs, and Notification delivery logging with retry state tracking.

## Dependent Tasks

- US_008 task (Core Database Schema) — core entity schema must exist before scheduling entities

## Impacted Components

- Infrastructure/Data/Configurations/AvailabilitySlotConfiguration.cs — slot config with recurrence
- Infrastructure/Data/Configurations/AppointmentConfiguration.cs — status lifecycle config
- Infrastructure/Data/Configurations/PreferredSlotQueueConfiguration.cs — FCFS ordering
- Infrastructure/Data/Configurations/NoShowRiskFactorConfiguration.cs — risk factor fields
- Infrastructure/Data/Configurations/NotificationConfiguration.cs — delivery log config
- Domain/Enums/AppointmentStatus.cs — lifecycle states enum
- Domain/Enums/NotificationChannel.cs — SMS/Email channel enum
- Domain/Enums/NotificationStatus.cs — delivery status enum
- Domain/Enums/QueueStatus.cs — Waiting/Swapped/Expired enum

## Implementation Plan

1. Create enums for AppointmentStatus, NotificationChannel, NotificationStatus, QueueStatus
2. Configure AvailabilitySlot with StartTime, EndTime, IsAvailable, RecurrencePattern, ProviderId
3. Configure Appointment with status lifecycle transitions and UpdatedAt timestamps
4. Configure PreferredSlotQueue with RequestedAt ordering and status transitions
5. Configure NoShowRiskFactor with HistoricalNoShowCount, AverageLeadTimeDays, PreferredTimeOfDay, IsNewPatient, LastCalculatedAt
6. Configure Notification with Channel, NotificationType, Status, RetryCount, LastAttemptAt, FailureReason

## Current Project State

- Core entity schema exists (US_008)
- Entities defined but need scheduling-specific configuration

## Expected Changes

| Action | File Path                                                                 | Description                                      |
| ------ | ------------------------------------------------------------------------- | ------------------------------------------------ |
| CREATE | src/Domain/Enums/AppointmentStatus.cs                                     | Scheduled, Arrived, Completed, Cancelled, NoShow |
| CREATE | src/Domain/Enums/NotificationChannel.cs                                   | SMS, Email                                       |
| CREATE | src/Domain/Enums/NotificationStatus.cs                                    | Queued, Sent, Delivered, Failed                  |
| CREATE | src/Domain/Enums/QueueStatus.cs                                           | Waiting, Swapped, Expired                        |
| MODIFY | src/Infrastructure/Data/Configurations/AvailabilitySlotConfiguration.cs   | Recurrence and overlap prevention                |
| MODIFY | src/Infrastructure/Data/Configurations/AppointmentConfiguration.cs        | Status lifecycle                                 |
| MODIFY | src/Infrastructure/Data/Configurations/PreferredSlotQueueConfiguration.cs | FCFS ordering                                    |
| MODIFY | src/Infrastructure/Data/Configurations/NoShowRiskFactorConfiguration.cs   | Four scoring inputs                              |
| MODIFY | src/Infrastructure/Data/Configurations/NotificationConfiguration.cs       | Retry state fields                               |

## External References

- [EF Core Enum Conversion](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — enum mappings, configuration validation
- [ ] Integration tests pass — status transitions, constraint enforcement

## Implementation Checklist

- [ ] Create all scheduling-related enums (AppointmentStatus, NotificationChannel, NotificationStatus, QueueStatus) (AC-02, AC-03, AC-05)
- [ ] Configure AvailabilitySlot with recurrence pattern support and time boundary validation (AC-01)
- [ ] Configure Appointment status lifecycle with UpdatedAt timestamp per transition (AC-02)
- [ ] Configure PreferredSlotQueue with RequestedAt-based FCFS ordering and status transitions (AC-03)
- [ ] Configure NoShowRiskFactor with all four scoring input fields and LastCalculatedAt (AC-04)
- [ ] Configure Notification entity with channel, retry count, and failure tracking fields (AC-05)
- [ ] Add overlap prevention constraint or index on AvailabilitySlot (Edge Cases)
- [ ] Generate migration and validate successful application (AC-01)
