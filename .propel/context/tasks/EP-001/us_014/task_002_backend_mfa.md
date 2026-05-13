# Task - TASK_002

## Requirement Reference

- **User Story:** US_014
- **Story Location:** .propel/context/tasks/EP-001/us_014/us_014.md
- **Acceptance Criteria:**
  - AC-01: Staff MFA authentication with TOTP/SMS code validation
  - AC-02: Admin MFA authentication with role-specific JWT claims
  - AC-03: First-login MFA device setup with TOTP secret generation
  - AC-04: 3 failed MFA attempts terminates session
  - AC-05: Expired code generates fresh SMS or prompts new TOTP
  - AC-06: Deactivated account rejected before MFA step
- **Edge Cases:**
  - MFA device lost — admin reset only (no self-service bypass)
  - Clock skew — ±1 time step TOTP validation window
  - Concurrent sessions — existing sessions remain valid

---

## Applicable Technology Stack

| Layer       | Technology   | Version | Justification                        |
| ----------- | ------------ | ------- | ------------------------------------ |
| Backend     | ASP.NET Core | 9.0     | TR-002 — MFA endpoints               |
| Auth Server | OpenIddict   | 5.x     | NFR-007 — MFA enforcement            |
| Library     | OtpNet       | Latest  | NFR-007 — TOTP generation/validation |

---

## Task Overview

Implement backend MFA logic: TOTP secret generation, QR code URI generation, code validation with ±1 time step tolerance, 3-attempt lockout, SMS code dispatch, and mandatory MFA enforcement for Staff/Admin roles.

## Dependent Tasks

- US_013 task (Backend Auth) — OpenIddict and auth flow must exist
- US_008 task (Database Schema) — User.MfaEnabled, User.MfaSecret columns

## Impacted Components

- API/Controllers/MfaController.cs — MFA endpoints
- Application/Commands/SetupMfaCommand.cs — MFA device setup
- Application/Commands/VerifyMfaCommand.cs — MFA code verification
- Infrastructure/Auth/TotpService.cs — TOTP generation/validation
- Infrastructure/Services/SmsService.cs — SMS code dispatch

## Implementation Plan

1. Create TotpService using OtpNet with ±1 time step window
2. Implement MFA setup endpoint generating TOTP secret and QR URI
3. Implement MFA verification endpoint with attempt counting
4. Implement 3-attempt lockout with session termination
5. Implement SMS code generation and dispatch via background job
6. Enforce mandatory MFA check after credential verification for Staff/Admin

## Current Project State

- OpenIddict configured (US_013)
- User entity with MFA columns (US_008)
- No MFA endpoints exist

## Expected Changes

| Action | File Path                                    | Description                    |
| ------ | -------------------------------------------- | ------------------------------ |
| CREATE | src/API/Controllers/MfaController.cs         | MFA setup and verify endpoints |
| CREATE | src/Application/Commands/SetupMfaCommand.cs  | MFA setup command              |
| CREATE | src/Application/Commands/VerifyMfaCommand.cs | MFA verify command             |
| CREATE | src/Infrastructure/Auth/TotpService.cs       | TOTP generation/validation     |
| CREATE | src/Infrastructure/Services/SmsService.cs    | SMS dispatch service           |

## External References

- [OtpNet Library](https://github.com/kspearrin/Otp.NET)
- [RFC 6238 TOTP](https://datatracker.ietf.org/doc/html/rfc6238)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — TOTP validation with clock skew, attempt counting
- [ ] Integration tests pass — MFA setup and verify flow

## Implementation Checklist

- [ ] Implement TotpService with TOTP generation, QR URI, and ±1 step validation (AC-01, AC-03)
- [ ] Create MFA setup endpoint with secret generation and encrypted storage (AC-03)
- [ ] Create MFA verification endpoint with TOTP and SMS code support (AC-01, AC-02)
- [ ] Implement 3-attempt lockout with session termination (AC-04)
- [ ] Implement SMS code generation with expiry and resend capability (AC-05)
- [ ] Reject deactivated accounts before reaching MFA step (AC-06)
- [ ] Enforce mandatory MFA for Staff/Admin roles in auth flow (AC-01, AC-02)
- [ ] Handle MFA device loss via admin reset only — no self-service bypass (Edge Cases)
