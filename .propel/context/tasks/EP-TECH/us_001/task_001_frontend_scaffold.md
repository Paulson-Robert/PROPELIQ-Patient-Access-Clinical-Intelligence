# Task - TASK_001

## Requirement Reference

- **User Story:** US_001
- **Story Location:** .propel/context/tasks/EP-TECH/us_001/us_001.md
- **Acceptance Criteria:**
  - AC-01: Vite project initializes and builds with zero warnings
  - AC-02: Dev server starts and renders root route within 3 seconds
  - AC-03: TailwindCSS utility classes are functional with HMR
  - AC-04: Shadcn UI components render with design tokens
  - AC-05: All dependencies use OSI-approved open-source licenses
- **Edge Cases:**
  - Node.js version mismatch — .nvmrc or engines field error
  - pnpm lock conflict — auto-regeneration
  - Port conflict — Vite auto-increments port

---

## Applicable Technology Stack

| Layer            | Technology   | Version              | Justification                                     |
| ---------------- | ------------ | -------------------- | ------------------------------------------------- |
| Frontend         | React + Vite | React 18.x, Vite 5.x | TR-001 — BRD mandate; NFR-014 (free, open-source) |
| Frontend UI      | Shadcn UI    | Latest               | NFR-009 — accessible component library            |
| Frontend Styling | TailwindCSS  | 3.x                  | TR-001 — utility-first CSS framework              |

---

## Task Overview

Create a fully configured React 18+ / Vite frontend project scaffold with TailwindCSS, Shadcn UI, and proper development tooling (ESLint, Prettier, TypeScript) that serves as the foundation for all frontend feature development.

## Dependent Tasks

- None — this is a foundational task

## Impacted Components

- frontend/ — entire frontend project directory
- frontend/package.json — dependencies and scripts
- frontend/vite.config.ts — Vite configuration
- frontend/tailwind.config.ts — TailwindCSS configuration
- frontend/tsconfig.json — TypeScript configuration
- frontend/src/App.tsx — root application component

## Implementation Plan

1. Initialize Vite project with React + TypeScript template using pnpm
2. Configure .nvmrc with Node.js 20+ requirement and engines field in package.json
3. Install and configure TailwindCSS with PostCSS
4. Initialize Shadcn UI with design token configuration
5. Configure ESLint and Prettier for code quality
6. Create root App component with placeholder page
7. Verify production build produces dist/ with zero warnings
8. Audit all dependencies for OSI-approved licenses

## Current Project State

- No frontend project exists

## Expected Changes

| Action | File Path                   | Description                            |
| ------ | --------------------------- | -------------------------------------- |
| CREATE | frontend/package.json       | Project dependencies and scripts       |
| CREATE | frontend/vite.config.ts     | Vite build configuration               |
| CREATE | frontend/tailwind.config.ts | TailwindCSS with design tokens         |
| CREATE | frontend/tsconfig.json      | TypeScript strict configuration        |
| CREATE | frontend/src/App.tsx        | Root application component             |
| CREATE | frontend/src/main.tsx       | Application entry point                |
| CREATE | frontend/src/index.css      | Global styles with Tailwind directives |
| CREATE | frontend/.nvmrc             | Node.js 20 version constraint          |
| CREATE | frontend/.eslintrc.cjs      | ESLint configuration                   |
| CREATE | frontend/components.json    | Shadcn UI configuration                |

## External References

- [Vite Getting Started](https://vite.dev/guide/)
- [TailwindCSS Installation](https://tailwindcss.com/docs/installation)
- [Shadcn UI Installation](https://ui.shadcn.com/docs/installation/vite)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — App component renders
- [ ] Integration tests pass — build produces dist/ with zero warnings

## Implementation Checklist

- [ ] Initialize Vite + React + TypeScript project with pnpm (AC-01)
- [ ] Configure TailwindCSS with PostCSS and verify utility classes with HMR (AC-03)
- [ ] Initialize Shadcn UI with design token configuration (color, radius, font) (AC-04)
- [ ] Create root App component rendering placeholder page at / route (AC-02)
- [ ] Configure .nvmrc, engines field, ESLint, Prettier, and TypeScript strict mode (AC-01)
- [ ] Verify pnpm build produces dist/ with zero warnings (AC-01)
- [ ] Audit all dependencies for OSI-approved licenses (AC-05)
- [ ] Configure Vite dev server with port fallback on conflict (Edge Cases)
