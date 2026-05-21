# Task - TASK_002

## Requirement Reference

- **User Story:** US_042
- **Story Location:** .propel/context/tasks/EP-009/us_042/us_042.md
- **Acceptance Criteria:**
  - AC-01: Risk tier classification (Low: 0-30, Medium: 31-70, High: 71-100)
  - AC-02: Tier endpoint returns tier with contributing factors
- **Edge Cases:**
  - Boundary scores — consistent tier assignment

---

## Applicable Technology Stack

| Layer   | Technology   | Version | Justification     |
| ------- | ------------ | ------- | ----------------- |
| Backend | ASP.NET Core | 9.0     | TR-002 — tier API |

---

## Task Overview

Implement risk tier classification mapping numeric scores to Low/Medium/High tiers with factor breakdown endpoint.

## Dependent Tasks

- US_041 task (Risk Algorithm) — numeric score

## Impacted Components

- Application/Queries/GetPatientRiskTierQuery.cs — tier query

## Expected Changes

| Action | File Path                                          | Description |
| ------ | -------------------------------------------------- | ----------- |
| CREATE | src/Application/Queries/GetPatientRiskTierQuery.cs | Tier query  |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — tier classification, boundary cases

## Implementation Checklist

- [x] Classify scores into Low/Medium/High tiers (AC-01)
- [x] Return tier with contributing factor breakdown (AC-02)
- [x] Handle boundary scores consistently (Edge Cases)
