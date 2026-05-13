# Task - TASK_001

## Requirement Reference

- **User Story:** US_004
- **Story Location:** .propel/context/tasks/EP-TECH/us_004/us_004.md
- **Acceptance Criteria:**
  - AC-01: Hangfire server starts and processes a sample job within 15 seconds
  - AC-02: Hangfire dashboard accessible with authentication; unauthenticated → 401
  - AC-03: PostgreSQL storage tables created in hangfire schema
  - AC-04: Recurring job scheduling is functional
- **Edge Cases:**
  - Dashboard auth bypass — fail-closed default
  - Job table creation failure — fail at startup
  - Stale server cleanup — heartbeat-based detection

---

## Applicable Technology Stack

| Layer           | Technology         | Version | Justification                                     |
| --------------- | ------------------ | ------- | ------------------------------------------------- |
| Background Jobs | Hangfire Community | 1.8.x   | ADD-3 — async job processing; NFR-014 (free, MIT) |
| Database        | PostgreSQL         | 16.x    | ADD-3 — Hangfire storage                          |
| Backend         | ASP.NET Core       | 9.0     | TR-002 — middleware and DI                        |

---

## Task Overview

Integrate Hangfire Community with PostgreSQL storage for background job processing, configure the authenticated dashboard at /hangfire, verify job enqueue/processing lifecycle, and set up recurring job scheduling.

## Dependent Tasks

- US_002 task (Backend Scaffold) — backend solution for service registration
- US_003 task (Database ORM) — PostgreSQL database for Hangfire storage

## Impacted Components

- API/Program.cs — Hangfire server and dashboard registration
- Infrastructure/BackgroundJobs/HangfireConfiguration.cs — Hangfire setup
- API/Filters/HangfireDashboardAuthFilter.cs — dashboard authorization

## Implementation Plan

1. Install Hangfire.Core, Hangfire.AspNetCore, Hangfire.PostgreSql NuGet packages
2. Configure Hangfire with PostgreSQL storage and schema creation
3. Create dashboard authorization filter requiring Admin role
4. Register Hangfire server and dashboard middleware
5. Create sample fire-and-forget job for verification
6. Create sample recurring job for health monitoring

## Current Project State

- Backend solution exists (US_002)
- PostgreSQL configured (US_003)
- No background job infrastructure

## Expected Changes

| Action | File Path                                                  | Description                              |
| ------ | ---------------------------------------------------------- | ---------------------------------------- |
| CREATE | src/Infrastructure/BackgroundJobs/HangfireConfiguration.cs | Hangfire setup                           |
| CREATE | src/API/Filters/HangfireDashboardAuthFilter.cs             | Dashboard auth filter                    |
| MODIFY | src/API/Program.cs                                         | Register Hangfire services and dashboard |

## External References

- [Hangfire Documentation](https://docs.hangfire.io/en/latest/)
- [Hangfire.PostgreSql](https://github.com/frankhommers/Hangfire.PostgreSql)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — job enqueue and dashboard auth filter
- [ ] Integration tests pass — job processes within 15 seconds

## Implementation Checklist

- [ ] Install Hangfire packages and configure PostgreSQL storage (AC-03)
- [ ] Register Hangfire server with processing pipeline (AC-01)
- [ ] Create dashboard authorization filter with fail-closed default (AC-02)
- [ ] Verify sample fire-and-forget job enqueues and processes (AC-01)
- [ ] Configure and verify recurring job scheduling (AC-04)
- [ ] Handle startup failure when Hangfire cannot create schema (Edge Cases)
