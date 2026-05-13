# Task - TASK_001

## Requirement Reference

- **User Story:** US_002
- **Story Location:** .propel/context/tasks/EP-TECH/us_002/us_002.md
- **Acceptance Criteria:**
  - AC-01: Solution builds with 4-layer Clean Architecture separation (API, Application, Domain, Infrastructure)
  - AC-02: API starts and responds to GET /health with HTTP 200
  - AC-03: MediatR pipeline processes a sample PingCommand
  - AC-04: DI configured across all layers with zero unresolved dependencies
  - AC-05: All dependencies use OSI-approved licenses
- **Edge Cases:**
  - .NET SDK version mismatch — global.json error
  - NuGet restore failure — nuget.config fallback instructions
  - Port conflict — Kestrel fails fast with descriptive error

---

## Applicable Technology Stack

| Layer           | Technology   | Version | Justification                               |
| --------------- | ------------ | ------- | ------------------------------------------- |
| Backend         | ASP.NET Core | 9.0     | TR-002 — BRD mandate; NFR-014               |
| Backend Pattern | MediatR      | 12.x    | ADD-2 — CQRS-Light command/query separation |

---

## Task Overview

Create a fully configured ASP.NET Core 9 backend solution with Clean Architecture (API, Application, Domain, Infrastructure layers), MediatR CQRS-Light pattern, dependency injection across all layers, health check endpoint, and a sample PingCommand to verify the pipeline.

## Dependent Tasks

- None — this is a foundational task

## Impacted Components

- backend/ — entire backend solution directory
- backend/src/API/ — presentation layer project
- backend/src/Application/ — business logic and MediatR handlers
- backend/src/Domain/ — entities, value objects, interfaces
- backend/src/Infrastructure/ — data access, external services

## Implementation Plan

1. Create .NET solution with 4 projects following Clean Architecture naming
2. Configure project references enforcing dependency flow (API→Application→Domain; Infrastructure→Application→Domain)
3. Install and configure MediatR with pipeline behaviors
4. Create sample PingCommand/PingCommandHandler
5. Configure DI registration in each layer with extension methods
6. Create health check endpoint at GET /health
7. Create test endpoint POST /api/ping for MediatR verification
8. Add global.json with .NET 9 SDK constraint

## Current Project State

- No backend project exists

## Expected Changes

| Action | File Path                                              | Description                              |
| ------ | ------------------------------------------------------ | ---------------------------------------- |
| CREATE | backend/Backend.sln                                    | Solution file                            |
| CREATE | backend/global.json                                    | .NET 9 SDK version constraint            |
| CREATE | backend/src/API/API.csproj                             | API presentation layer                   |
| CREATE | backend/src/API/Program.cs                             | Application entry with DI and middleware |
| CREATE | backend/src/API/Controllers/PingController.cs          | Sample MediatR test endpoint             |
| CREATE | backend/src/Application/Application.csproj             | Business logic layer                     |
| CREATE | backend/src/Application/Commands/PingCommand.cs        | Sample command                           |
| CREATE | backend/src/Application/Handlers/PingCommandHandler.cs | Sample handler                           |
| CREATE | backend/src/Domain/Domain.csproj                       | Domain entities layer                    |
| CREATE | backend/src/Infrastructure/Infrastructure.csproj       | Data access layer                        |
| CREATE | backend/src/Infrastructure/DependencyInjection.cs      | Infrastructure DI registration           |

## External References

- [ASP.NET Core 9 Documentation](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-9.0)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — PingCommandHandler returns expected response
- [ ] Integration tests pass — health check returns 200, ping endpoint works

## Implementation Checklist

- [ ] Create solution with 4 Clean Architecture projects and correct dependency flow (AC-01)
- [ ] Configure ASP.NET Core API with Kestrel, middleware pipeline, and global.json (AC-02)
- [ ] Install and configure MediatR with pipeline behaviors in Application layer (AC-03)
- [ ] Create PingCommand/Handler and test endpoint POST /api/ping (AC-03)
- [ ] Configure DI registration extension methods in each layer (AC-04)
- [ ] Create GET /health endpoint returning JSON status (AC-02)
- [ ] Verify dotnet build succeeds with zero errors across all layers (AC-01)
- [ ] Audit all NuGet packages for OSI-approved licenses (AC-05)
