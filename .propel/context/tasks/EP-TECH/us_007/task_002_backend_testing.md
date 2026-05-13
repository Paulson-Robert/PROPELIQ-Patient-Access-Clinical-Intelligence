# Task - TASK_002

## Requirement Reference

- **User Story:** US_007
- **Story Location:** .propel/context/tasks/EP-TECH/us_007/us_007.md
- **Acceptance Criteria:**
  - AC-01: xUnit backend test project builds and runs sample test (PingCommandHandler_Returns_Pong)
  - AC-04: Backend tests reside in tests/ with \*.Tests naming convention
  - AC-05: All testing dependencies use OSI-approved licenses
- **Edge Cases:**
  - Test runner version conflicts — clear NuGet version error
  - Parallel test execution — IClassFixture for shared resources

---

## Applicable Technology Stack

| Layer             | Technology                     | Version       | Justification               |
| ----------------- | ------------------------------ | ------------- | --------------------------- |
| Testing — Backend | xUnit + FluentAssertions + Moq | Latest stable | TR-012 — .NET testing stack |
| Backend           | ASP.NET Core                   | 9.0           | TR-002 — test target        |

---

## Task Overview

Configure xUnit test project with FluentAssertions and Moq for backend unit testing, create sample PingCommandHandler test, and establish test directory conventions.

## Dependent Tasks

- US_002 task (Backend Scaffold) — backend solution must exist

## Impacted Components

- backend/tests/Application.Tests/Application.Tests.csproj — test project
- backend/tests/Application.Tests/Handlers/PingCommandHandlerTests.cs — sample test

## Implementation Plan

1. Create Application.Tests project with xUnit, FluentAssertions, Moq references
2. Add project reference to Application layer
3. Create PingCommandHandlerTests with sample test
4. Verify dotnet test discovers and runs test successfully

## Current Project State

- Backend solution exists (US_002)
- No test projects exist

## Expected Changes

| Action | File Path                                                           | Description                  |
| ------ | ------------------------------------------------------------------- | ---------------------------- |
| CREATE | backend/tests/Application.Tests/Application.Tests.csproj            | xUnit test project           |
| CREATE | backend/tests/Application.Tests/Handlers/PingCommandHandlerTests.cs | Sample unit test             |
| MODIFY | backend/Backend.sln                                                 | Add test project to solution |

## External References

- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [FluentAssertions](https://fluentassertions.com/introduction)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — dotnet test reports Passed: 1, Failed: 0

## Implementation Checklist

- [ ] Create Application.Tests project with xUnit, FluentAssertions, Moq NuGet packages (AC-01)
- [ ] Create PingCommandHandler unit test verifying "pong" response (AC-01)
- [ ] Verify tests reside in tests/ directory with \*.Tests naming convention (AC-04)
- [ ] Audit all testing packages for OSI-approved licenses (AC-05)
- [ ] Add test project to solution and verify dotnet test discovers test (AC-01)
- [ ] Configure test isolation patterns for future shared resource tests (Edge Cases)
