# Task - TASK_002

## Requirement Reference

- **User Story:** US_013
- **Story Location:** .propel/context/tasks/EP-001/us_013/us_013.md
- **Acceptance Criteria:**
  - AC-01: Google OAuth flow creates/links patient account with JWT
  - AC-02: Microsoft OAuth flow creates/links patient account with JWT
  - AC-03: Email/password registration creates account with Pending Verification status
  - AC-04: Email verification link activates account
  - AC-05: OAuth consent denied returns user to login
  - AC-06: Duplicate email detection prevents registration
- **Edge Cases:**
  - Expired verification link — resend mechanism
  - Social login email mismatch — account linking logic
  - OAuth provider outage — graceful handling

---

## Applicable Technology Stack

| Layer       | Technology      | Version | Justification                 |
| ----------- | --------------- | ------- | ----------------------------- |
| Backend     | ASP.NET Core    | 9.0     | TR-002 — API endpoints        |
| Auth Server | OpenIddict      | 5.x     | NFR-007 — OAuth 2.0/OIDC, JWT |
| Security    | BCrypt.Net-Next | Latest  | NFR-004 — password hashing    |

---

## Task Overview

Implement backend OAuth endpoints for Google and Microsoft social login, email/password registration with email verification, JWT token issuance with 15-minute sliding expiry, and account linking logic using OpenIddict.

## Dependent Tasks

- US_002 task (Backend Scaffold) — API project must exist
- US_008 task (Database Schema) — User entity must exist

## Impacted Components

- API/Controllers/AuthController.cs — auth endpoints
- Application/Commands/RegisterPatientCommand.cs — registration command
- Application/Commands/VerifyEmailCommand.cs — email verification
- Infrastructure/Auth/OpenIddictConfiguration.cs — OpenIddict setup
- Infrastructure/Auth/GoogleOAuthService.cs — Google OAuth handler
- Infrastructure/Auth/MicrosoftOAuthService.cs — Microsoft OAuth handler
- Infrastructure/Services/EmailService.cs — verification email dispatch

## Implementation Plan

1. Configure OpenIddict with JWT token generation and 15-minute expiry
2. Implement Google OAuth callback endpoint and account creation/linking
3. Implement Microsoft OAuth callback endpoint and account creation/linking
4. Create registration endpoint with password hashing (bcrypt work factor 12)
5. Create email verification endpoint with one-time token validation
6. Implement account linking when social login email matches existing account
7. Configure verification email dispatch via background job
8. Return appropriate JWT claims (userId, email, role)

## Current Project State

- Backend scaffold exists (US_002)
- User entity schema exists (US_008)
- No auth endpoints exist

## Expected Changes

| Action | File Path                                          | Description                           |
| ------ | -------------------------------------------------- | ------------------------------------- |
| CREATE | src/API/Controllers/AuthController.cs              | Registration, login, OAuth callbacks  |
| CREATE | src/Application/Commands/RegisterPatientCommand.cs | Registration command                  |
| CREATE | src/Application/Commands/VerifyEmailCommand.cs     | Email verification                    |
| CREATE | src/Infrastructure/Auth/OpenIddictConfiguration.cs | OpenIddict setup                      |
| CREATE | src/Infrastructure/Auth/GoogleOAuthService.cs      | Google OAuth                          |
| CREATE | src/Infrastructure/Auth/MicrosoftOAuthService.cs   | Microsoft OAuth                       |
| CREATE | src/Infrastructure/Services/EmailService.cs        | Email dispatch                        |
| MODIFY | src/API/Program.cs                                 | Register OpenIddict and auth services |

## External References

- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [Google OAuth 2.0](https://developers.google.com/identity/protocols/oauth2)
- [Microsoft Identity Platform](https://learn.microsoft.com/en-us/entra/identity-platform/)

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — command handlers, token generation
- [ ] Integration tests pass — OAuth flow, registration, verification

## Implementation Checklist

- [x] Configure OpenIddict with JWT issuance, 15-minute expiry, signing keys (AC-01, AC-02)
- [x] Implement Google OAuth callback with account creation and linking (AC-01)
- [x] Implement Microsoft OAuth callback with account creation and linking (AC-02)
- [x] Create registration endpoint with bcrypt hashing and Pending Verification status (AC-03)
- [x] Create email verification endpoint with one-time token and account activation (AC-04)
- [x] Handle OAuth consent denial with appropriate error response (AC-05)
- [x] Implement duplicate email detection returning conflict error (AC-06)
- [x] Implement account linking when OAuth email matches existing account (Edge Cases)
