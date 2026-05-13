# Task - TASK_001

## Requirement Reference

- **User Story:** US_005
- **Story Location:** .propel/context/tasks/EP-TECH/us_005/us_005.md
- **Acceptance Criteria:**
  - AC-01: Serilog captures structured JSON log entries to console and file sinks
  - AC-02: Sentry captures unhandled exceptions with full context within 30 seconds
  - AC-03: Health check returns individual status for DB, Hangfire, Redis
  - AC-04: Uptime Robot monitors health endpoint with 5-minute intervals
  - AC-05: Log levels configurable per environment via appsettings
- **Edge Cases:**
  - Sentry DSN misconfiguration — start normally with warning
  - Health check timeout — 5-second timeout, report Degraded
  - Uptime Robot false positive — 2 consecutive failures before alert

---

## Applicable Technology Stack

| Layer      | Technology   | Version       | Justification                    |
| ---------- | ------------ | ------------- | -------------------------------- |
| Logging    | Serilog      | Latest stable | TR-007 — structured logging      |
| Monitoring | Sentry       | Free tier     | NFR-001 — error tracking         |
| Monitoring | Uptime Robot | Free tier     | NFR-001 — uptime monitoring      |
| Backend    | ASP.NET Core | 9.0           | TR-002 — health check middleware |

---

## Task Overview

Configure Serilog structured logging with environment-based log levels, Sentry error tracking for unhandled exceptions, enhanced health check endpoint reporting individual dependency status, and Uptime Robot monitoring configuration documentation.

## Dependent Tasks

- US_002 task (Backend Scaffold) — backend solution for middleware pipeline

## Impacted Components

- API/Program.cs — Serilog and Sentry configuration
- API/appsettings.json — log level defaults
- API/appsettings.Development.json — Debug log level
- API/appsettings.Production.json — Warning log level
- Infrastructure/HealthChecks/ — custom health check implementations

## Implementation Plan

1. Install Serilog, Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File
2. Configure structured JSON logging with request enrichment
3. Install and configure Sentry.AspNetCore with DSN from environment variables
4. Create custom health checks for PostgreSQL, Hangfire, and Redis
5. Configure health check endpoint with detailed JSON response
6. Document Uptime Robot configuration with 2-failure threshold

## Current Project State

- Backend solution exists (US_002)
- Health check endpoint exists but reports basic status only

## Expected Changes

| Action | File Path                                              | Description                          |
| ------ | ------------------------------------------------------ | ------------------------------------ |
| MODIFY | src/API/Program.cs                                     | Add Serilog and Sentry configuration |
| MODIFY | src/API/appsettings.json                               | Serilog configuration with sinks     |
| CREATE | src/API/appsettings.Production.json                    | Production log level (Warning)       |
| CREATE | src/Infrastructure/HealthChecks/HangfireHealthCheck.cs | Hangfire availability check          |
| CREATE | src/Infrastructure/HealthChecks/RedisHealthCheck.cs    | Redis connectivity check             |

## External References

- [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore)
- [Sentry .NET SDK](https://docs.sentry.io/platforms/dotnet/guides/aspnetcore/)
- [Uptime Robot](https://uptimerobot.com/docs/)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — log configuration, health check logic
- [ ] Integration tests pass — health endpoint returns structured status

## Implementation Checklist

- [ ] Configure Serilog with console and file sinks, JSON format, request enrichment (AC-01)
- [ ] Configure Sentry with DSN from environment variables and graceful fallback (AC-02)
- [ ] Create custom health checks for PostgreSQL, Hangfire, and Redis with 5s timeout (AC-03)
- [ ] Configure per-environment log levels via appsettings overrides (AC-05)
- [ ] Document Uptime Robot setup with 5-minute HTTP monitor and 2-failure alerting (AC-04)
- [ ] Handle Sentry DSN misconfiguration with startup warning (Edge Cases)
