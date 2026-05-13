# Task - TASK_001

## Requirement Reference

- **User Story:** US_007
- **Story Location:** .propel/context/tasks/EP-TECH/us_007/us_007.md
- **Acceptance Criteria:**
  - AC-02: Vitest frontend test runner executes sample component test
  - AC-03: Playwright E2E configuration is functional with HTML report
  - AC-04: Frontend tests follow standard directory conventions (src/\*\*/**tests**/, e2e/)
- **Edge Cases:**
  - Playwright browser download failure — manual download instructions
  - Parallel test execution collision — test isolation

---

## Applicable Technology Stack

| Layer              | Technology                     | Version              | Justification               |
| ------------------ | ------------------------------ | -------------------- | --------------------------- |
| Testing — Frontend | React Testing Library + Vitest | Latest stable        | TR-012 — component testing  |
| Testing — E2E      | Playwright                     | Latest stable        | TR-012 — end-to-end testing |
| Frontend           | React + Vite                   | React 18.x, Vite 5.x | TR-001 — test target        |

---

## Task Overview

Configure Vitest with React Testing Library for frontend component testing and Playwright for E2E testing, with sample passing tests and standard directory conventions.

## Dependent Tasks

- US_001 task (Frontend Scaffold) — frontend project must exist

## Impacted Components

- frontend/vitest.config.ts — Vitest configuration
- frontend/playwright.config.ts — Playwright configuration
- frontend/src/**tests**/App.test.tsx — sample component test
- frontend/e2e/home.spec.ts — sample E2E test

## Implementation Plan

1. Install Vitest, @testing-library/react, @testing-library/jest-dom, jsdom
2. Configure vitest.config.ts with jsdom environment
3. Create sample App component test
4. Install Playwright with browser binaries
5. Configure playwright.config.ts with HTML report
6. Create sample E2E test navigating to root and asserting page title

## Current Project State

- Frontend scaffold exists (US_001)
- No testing configuration

## Expected Changes

| Action | File Path                           | Description                           |
| ------ | ----------------------------------- | ------------------------------------- |
| CREATE | frontend/vitest.config.ts           | Vitest with jsdom environment         |
| CREATE | frontend/src/**tests**/App.test.tsx | Sample component test                 |
| CREATE | frontend/playwright.config.ts       | Playwright configuration              |
| CREATE | frontend/e2e/home.spec.ts           | Sample E2E test                       |
| MODIFY | frontend/package.json               | Add test scripts and dev dependencies |

## External References

- [Vitest Documentation](https://vitest.dev/guide/)
- [Playwright Documentation](https://playwright.dev/docs/intro)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — pnpm test reports Passed: 1
- [ ] Integration tests pass — Playwright generates HTML report

## Implementation Checklist

- [ ] Install and configure Vitest with React Testing Library and jsdom (AC-02)
- [ ] Create sample component test that renders App and asserts (AC-02)
- [ ] Install Playwright and download browser binaries (AC-03)
- [ ] Create sample E2E test with HTML report generation (AC-03)
- [ ] Verify standard directory conventions (src/**tests**/, e2e/) (AC-04)
- [ ] Add pnpm test and pnpm test:e2e scripts to package.json (AC-02, AC-03)
