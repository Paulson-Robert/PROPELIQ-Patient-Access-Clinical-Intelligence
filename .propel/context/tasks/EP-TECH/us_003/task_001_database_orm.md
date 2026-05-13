# Task - TASK_001

## Requirement Reference

- **User Story:** US_003
- **Story Location:** .propel/context/tasks/EP-TECH/us_003/us_003.md
- **Acceptance Criteria:**
  - AC-01: EF Core connects to PostgreSQL and runs initial migration
  - AC-02: pgcrypto extension is enabled
  - AC-03: Hybrid migration strategy documented and enforced
  - AC-04: Database context resolves through DI
  - AC-05: Connection uses free-tier compatible Supabase configuration
- **Edge Cases:**
  - PostgreSQL connection failure — descriptive error, fail fast
  - pgcrypto not available — clear error message
  - Concurrent migration execution — EF Core migration lock

---

## Applicable Technology Stack

| Layer      | Technology            | Version   | Justification                                   |
| ---------- | --------------------- | --------- | ----------------------------------------------- |
| Database   | PostgreSQL + pgcrypto | 16.x      | DR-001 — primary data store with PHI encryption |
| ORM        | Entity Framework Core | 9.0       | TR-004 — hybrid migration strategy              |
| Backend    | ASP.NET Core          | 9.0       | TR-002 — DI and configuration                   |
| Deployment | Supabase              | Free tier | NFR-014 — free managed PostgreSQL               |

---

## Task Overview

Configure EF Core 9 with PostgreSQL (Npgsql provider), enable pgcrypto extension, establish the hybrid migration strategy (C# migrations + exported SQL scripts for production), and verify connectivity with Supabase free-tier PostgreSQL.

## Dependent Tasks

- US_002 task (Backend API Scaffold) — Infrastructure project and DI must exist

## Impacted Components

- Infrastructure/Data/ApplicationDbContext.cs — DbContext configuration
- Infrastructure/DependencyInjection.cs — EF Core service registration
- API/appsettings.Development.json — connection string
- Infrastructure/Migrations/ — generated migration files

## Implementation Plan

1. Install Npgsql.EntityFrameworkCore.PostgreSQL NuGet package
2. Create ApplicationDbContext extending DbContext
3. Configure connection string in appsettings.Development.json
4. Register DbContext in DI with Npgsql provider
5. Create initial migration with pgcrypto extension enablement
6. Document hybrid migration workflow (C# → SQL script export)

## Current Project State

- Backend solution exists (US_002)
- No database configuration exists

## Expected Changes

| Action | File Path                                       | Description             |
| ------ | ----------------------------------------------- | ----------------------- |
| CREATE | src/Infrastructure/Data/ApplicationDbContext.cs | DbContext with Npgsql   |
| MODIFY | src/Infrastructure/DependencyInjection.cs       | Register DbContext      |
| MODIFY | src/API/appsettings.Development.json            | Add connection string   |
| CREATE | src/Infrastructure/Migrations/                  | Initial migration files |

## External References

- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)
- [Supabase PostgreSQL](https://supabase.com/docs/guides/database)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — DbContext resolves, CanConnect returns true
- [ ] Integration tests pass — migration applies to PostgreSQL 16

## Implementation Checklist

- [ ] Install Npgsql.EntityFrameworkCore.PostgreSQL and configure connection (AC-01)
- [ ] Create ApplicationDbContext with Npgsql provider registration (AC-04)
- [ ] Create initial migration with pgcrypto extension via SQL (AC-02)
- [ ] Document hybrid migration strategy in README (AC-03)
- [ ] Verify Supabase free-tier connection with connection pooling (AC-05)
- [ ] Add startup connection validation with descriptive error logging (Edge Cases)
