# Task - TASK_001

## Requirement Reference

- **User Story:** US_047
- **Story Location:** .propel/context/tasks/EP-011/us_047/us_047.md
- **Acceptance Criteria:**
  - AC-01: PHI fields encrypted at rest using pgcrypto AES-256
  - AC-02: TLS 1.2+ enforced for all connections
  - AC-03: Redis transport encrypted
  - AC-04: HSTS header enabled with 1-year max-age
- **Edge Cases:**
  - Key rotation — zero-downtime with dual-key period

---

## Applicable Technology Stack

| Layer    | Technology                   | Version  | Justification            |
| -------- | ---------------------------- | -------- | ------------------------ |
| Backend  | ASP.NET Core                 | 9.0      | TR-002 — security config |
| Database | PostgreSQL + pgcrypto        | 16.x     | NFR-004 — PHI encryption |
| Security | ASP.NET Core Data Protection | Built-in | NFR-004 — key management |

---

## Task Overview

Implement PHI field-level encryption using pgcrypto AES-256, enforce TLS 1.2+, encrypted Redis transport, and HSTS header configuration.

## Dependent Tasks

- US_003 task (Database ORM) — EF Core configuration
- US_009 task (Redis Integration) — Redis connection

## Impacted Components

- Infrastructure/Security/PhiEncryptionService.cs — pgcrypto integration
- Infrastructure/Persistence/EncryptedValueConverter.cs — EF Core value converter
- API/Configuration/SecurityHeadersConfig.cs — HSTS, TLS

## Implementation Plan

1. Create PhiEncryptionService wrapping pgcrypto encrypt/decrypt
2. Create EF Core ValueConverter for transparent field encryption
3. Configure TLS 1.2+ enforcement in Kestrel
4. Configure Redis connection with TLS
5. Add HSTS header middleware with 1-year max-age
6. Document key rotation procedure

## Expected Changes

| Action | File Path                                                 | Description  |
| ------ | --------------------------------------------------------- | ------------ |
| CREATE | src/Infrastructure/Security/PhiEncryptionService.cs       | Encryption   |
| CREATE | src/Infrastructure/Persistence/EncryptedValueConverter.cs | EF converter |
| CREATE | src/API/Configuration/SecurityHeadersConfig.cs            | Headers      |

## Build Commands

- Refer to [.propel/build/](.propel/build/) for applicable build commands

## Implementation Validation Strategy

- [ ] Unit tests pass — encryption/decryption, header presence

## Implementation Checklist

- [ ] Implement pgcrypto AES-256 encryption for PHI fields (AC-01)
- [ ] Enforce TLS 1.2+ for all connections (AC-02)
- [ ] Configure encrypted Redis transport (AC-03)
- [ ] Enable HSTS with 1-year max-age (AC-04)
- [ ] Document key rotation procedure (Edge Cases)
