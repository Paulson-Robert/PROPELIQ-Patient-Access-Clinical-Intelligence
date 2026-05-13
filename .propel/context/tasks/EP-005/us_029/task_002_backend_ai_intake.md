# Task - TASK_002

## Requirement Reference

- **User Story:** US_029
- **Story Location:** .propel/context/tasks/EP-005/us_029/us_029.md
- **Acceptance Criteria:**
  - AC-01: AI intake endpoint orchestrates conversation with GPT-4o/Gemini
  - AC-02: Prompts de-identified — no PHI sent to AI provider
  - AC-03: Structured data extracted from conversation responses
  - AC-04: Intake data persisted on completion
- **Edge Cases:**
  - AI provider timeout — retry once, then suggest manual
  - Token limit reached — break into multiple exchanges

---

## Applicable Technology Stack

| Layer   | Technology             | Version | Justification                 |
| ------- | ---------------------- | ------- | ----------------------------- |
| Backend | ASP.NET Core           | 9.0     | TR-002 — intake orchestration |
| AI/ML   | OpenAI GPT-4o / Gemini | Latest  | TR-011 — conversational AI    |

---

## Task Overview

Implement AI intake backend orchestrating conversation with GPT-4o/Gemini using de-identified prompts, extracting structured data from responses, and persisting completed intake.

## Dependent Tasks

- US_003 task (Database ORM) — intake persistence

## Impacted Components

- Application/Commands/ProcessAiIntakeCommand.cs — orchestration
- Infrastructure/AI/AiIntakeService.cs — AI provider integration
- Infrastructure/AI/DeIdentificationService.cs — PHI removal before AI call

## Implementation Plan

1. Create DeIdentificationService stripping PHI from prompts
2. Create AiIntakeService wrapping GPT-4o/Gemini API calls
3. Create ProcessAiIntakeCommand orchestrating multi-turn conversation
4. Extract structured data from AI responses
5. Persist completed intake data
6. Handle provider timeout with retry and fallback suggestion

## Expected Changes

| Action | File Path                                          | Description    |
| ------ | -------------------------------------------------- | -------------- |
| CREATE | src/Application/Commands/ProcessAiIntakeCommand.cs | Orchestration  |
| CREATE | src/Infrastructure/AI/AiIntakeService.cs           | AI integration |
| CREATE | src/Infrastructure/AI/DeIdentificationService.cs   | PHI removal    |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — de-identification, data extraction

## Implementation Checklist

- [ ] Create AI intake orchestration command (AC-01)
- [ ] Implement de-identification removing all PHI before AI calls (AC-02)
- [ ] Extract structured data from AI conversation responses (AC-03)
- [ ] Persist intake data on completion (AC-04)
- [ ] Handle provider timeout with retry (Edge Cases)
