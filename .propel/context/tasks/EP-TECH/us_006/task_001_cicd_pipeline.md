# Task - TASK_001

## Requirement Reference

- **User Story:** US_006
- **Story Location:** .propel/context/tasks/EP-TECH/us_006/us_006.md
- **Acceptance Criteria:**
  - AC-01: Frontend deploys to Vercel on push to main
  - AC-02: Backend deploys to Railway/Render on push to main
  - AC-03: Supabase PostgreSQL provisioned and accessible
  - AC-04: CI pipeline runs build and test on pull requests
  - AC-05: Secrets securely managed via platform environment variables
  - AC-06: All platforms use free-tier plans
- **Edge Cases:**
  - Build timeout — fail with descriptive message within GitHub Actions limits
  - Free-tier resource limits — health check reports Degraded near limits
  - Deployment rollback — platform retains previous working deployment

---

## Applicable Technology Stack

| Layer                 | Technology       | Version   | Justification                     |
| --------------------- | ---------------- | --------- | --------------------------------- |
| Deployment — Frontend | Vercel           | Free tier | NFR-014 — free static hosting     |
| Deployment — Backend  | Railway / Render | Free tier | NFR-014 — free PaaS for .NET      |
| Deployment — Database | Supabase         | Free tier | NFR-014 — free managed PostgreSQL |
| CI/CD                 | GitHub Actions   | Free tier | TR-011 — automated build and test |

---

## Task Overview

Configure automated CI/CD pipelines: Vercel for frontend deployment, Railway or Render for backend Docker deployment, Supabase for PostgreSQL, and GitHub Actions for PR build/test validation — all on free-tier infrastructure.

## Dependent Tasks

- US_001 task (Frontend Scaffold) — frontend project for Vercel
- US_002 task (Backend Scaffold) — backend project with Dockerfile

## Impacted Components

- .github/workflows/ci.yml — GitHub Actions CI pipeline
- frontend/vercel.json — Vercel configuration (if needed)
- backend/Dockerfile — Backend container definition
- backend/docker-compose.yml — Local development orchestration

## Implementation Plan

1. Create Dockerfile for ASP.NET Core backend
2. Configure Vercel project connection to Git repository
3. Configure Railway/Render project with Docker deployment
4. Provision Supabase PostgreSQL and configure connection string
5. Create GitHub Actions workflow for PR build/test pipeline
6. Configure secrets in each platform's environment variable settings
7. Document deployment architecture and free-tier limits
8. Verify end-to-end deployment pipeline

## Current Project State

- Frontend scaffold exists (US_001)
- Backend scaffold exists (US_002)
- No CI/CD or deployment configuration

## Expected Changes

| Action | File Path                | Description                           |
| ------ | ------------------------ | ------------------------------------- |
| CREATE | .github/workflows/ci.yml | CI pipeline for PR build/test         |
| CREATE | backend/Dockerfile       | Multi-stage Docker build              |
| CREATE | backend/.dockerignore    | Docker ignore rules                   |
| CREATE | docs/deployment.md       | Deployment architecture documentation |

## External References

- [Vercel CLI & Git Integration](https://vercel.com/docs/deployments/git)
- [Railway Docker Deployment](https://docs.railway.app/guides/dockerfiles)
- [GitHub Actions](https://docs.github.com/en/actions)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — Dockerfile builds successfully
- [ ] Integration tests pass — CI pipeline runs on PR creation

## Implementation Checklist

- [ ] Create multi-stage Dockerfile for ASP.NET Core 9 backend (AC-02)
- [ ] Configure Vercel for automatic frontend deployment on main push (AC-01)
- [ ] Configure Railway/Render for automatic backend deployment on main push (AC-02)
- [ ] Provision Supabase PostgreSQL and verify EF Core connectivity (AC-03)
- [ ] Create GitHub Actions CI workflow running dotnet build/test and pnpm build/test (AC-04)
- [ ] Configure platform secrets for database, Sentry DSN, API keys (AC-05)
- [ ] Verify all platforms on free-tier with zero charges (AC-06)
- [ ] Document deployment architecture, limits, and rollback procedures (Edge Cases)
