# Task - TASK_001

## Requirement Reference

- **User Story:** US_019
- **Story Location:** .propel/context/tasks/EP-002/us_019/us_019.md
- **Acceptance Criteria:**
  - AC-01: Patients can add preferred appointment slots to swap queue
  - AC-02: FCFS ordering assigns newly available slots to earliest queued patient
  - AC-03: Auto-swap triggered when cancellation releases a preferred slot
  - AC-04: Staff notified of completed swap via real-time notification
  - AC-05: Patient receives confirmation when swap completes
- **Edge Cases:**
  - Multiple patients prefer same slot — FCFS resolves, others remain queued
  - Patient cancels swap request before match — cleanly removed from queue
  - Swapped slot conflicts with patient's updated schedule — notification only, no force-book

---

## Applicable Technology Stack

| Layer      | Technology    | Version    | Justification                  |
| ---------- | ------------- | ---------- | ------------------------------ |
| Backend    | ASP.NET Core  | 9.0        | TR-002 — swap engine           |
| Cache      | Upstash Redis | Serverless | DR-002 — swap queue            |
| Background | Hangfire      | 1.8.x      | TR-005 — async swap processing |

---

## Task Overview

Implement the preferred slot swap engine: patients register preferred slots, cancellations trigger FCFS matching, auto-swap executes booking, and notifications go to staff and patient.

## Dependent Tasks

- US_018 task (Backend Booking) — booking command must exist
- US_009 task (Redis Integration) — queue storage

## Impacted Components

- Application/Commands/RegisterSwapPreferenceCommand.cs — add to queue
- Application/EventHandlers/SlotCancelledHandler.cs — trigger swap check
- Infrastructure/Services/SwapEngineService.cs — FCFS matching and swap execution
- Infrastructure/Services/SwapQueueService.cs — Redis sorted set queue

## Implementation Plan

1. Create SwapQueueService using Redis sorted set (score = registration timestamp)
2. Create RegisterSwapPreferenceCommand adding patient preference to queue
3. Create SlotCancelledHandler triggering swap engine on cancellation
4. Implement SwapEngineService: find FCFS match → execute booking → notify
5. Handle edge cases: remove cancelled requests, skip conflicts

## Current Project State

- Booking logic exists (US_018)
- Redis connected (US_009)
- Hangfire configured (US_004)
- No swap logic exists

## Expected Changes

| Action | File Path                                                 | Description          |
| ------ | --------------------------------------------------------- | -------------------- |
| CREATE | src/Application/Commands/RegisterSwapPreferenceCommand.cs | Register swap        |
| CREATE | src/Application/EventHandlers/SlotCancelledHandler.cs     | Cancellation trigger |
| CREATE | src/Infrastructure/Services/SwapEngineService.cs          | FCFS swap logic      |
| CREATE | src/Infrastructure/Services/SwapQueueService.cs           | Redis queue          |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [x] Unit tests pass — FCFS ordering, swap matching logic

## Implementation Checklist

- [x] Create Redis sorted set swap queue with timestamp-based ordering (AC-01, AC-02)
- [x] Create RegisterSwapPreferenceCommand adding patient to queue (AC-01)
- [x] Create SlotCancelledHandler triggering swap engine (AC-03)
- [x] Implement FCFS matching assigning slot to earliest queued patient (AC-02)
- [x] Execute auto-swap booking and notify staff (AC-03, AC-04)
- [x] Send patient confirmation on completed swap (AC-05)
- [x] Handle patient cancellation of swap request (Edge Cases)
