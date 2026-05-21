# Findings Registry

- Date: 2026-05-14
- Source: User correction
- Finding: Endpoint implementation drifted from task spec (`POST /api/ping` in `PingController.cs`) to `POST /api/health/ping` in `HealthController`.
- Prevention Rule: For scaffolded API endpoints, cross-check route attributes and controller naming against the task spec before marking implementation complete.

- Date: 2026-05-14
- Source: User correction
- Finding: `global.json` used `9.0.0`, which resembles a runtime version and may not resolve to an installed SDK.
- Prevention Rule: Pin `global.json` to a real SDK feature band version (for example `9.0.100`, `9.0.101`) rather than runtime-style values.

- Date: 2026-05-21
- Source: implement-tasks Step3 decision
- Finding: Applied RBAC policy attributes on `PingController` endpoints to enforce patient/staff/admin access because domain endpoint policy mapping was not specified in task details.
- Prevention Rule: When endpoint targets are omitted, bind new policies to existing non-auth test endpoints and log the decision.

- Date: 2026-05-21
- Source: implement-tasks Step3 decision
- Finding: Sliding expiry is refreshed via middleware against Redis-backed session state while JWT token issuance remains 15 minutes, preserving existing token contract and fail-closed behavior.
- Prevention Rule: Prefer session TTL refresh in middleware unless requirements explicitly mandate per-request token reissuance.

- Date: 2026-05-21
- Source: implement-tasks Step3 decision
- Finding: Account lockout duration is set to 15 minutes after 5 failed local-password attempts; password reset clears lockout and counters.
- Prevention Rule: Reuse a centralized lockout service and keep lock policy constants in one implementation.

- Date: 2026-05-21
- Source: implement-tasks Step3 decision
- Finding: Authentication responses enforce a 250ms minimum duration to reduce user-enumeration and timing side-channel risk.
- Prevention Rule: Add minimum response timing in controller-level auth paths when existence checks can short-circuit early.

- Date: 2026-05-21
- Source: implement-tasks Step3 decision
- Finding: Insurance policy soft validation regex was set to `^[A-Za-z0-9]{2,8}-?[A-Za-z0-9]{4,12}$` and enforced as warning-only to avoid blocking booking flow when format is unusual.
- Prevention Rule: When policy format is unspecified, use a permissive alphanumeric-hyphen pattern and emit non-blocking warnings only.

- Date: 2026-05-21
- Source: implement-tasks Step3 decision
- Finding: Guest walk-in records use generated `guest-{guid}@walkin.local` emails so temporary identities satisfy unique email constraints and can be targeted for later merge.
- Prevention Rule: When guest account identity is unspecified but email is required, generate a reserved-domain placeholder and keep profile demographics minimal.

- Date: 2026-05-21
- Source: implement-tasks Step3 decision
- Finding: Same-day queue insertion was implemented via `PreferredSlotQueues` with `QueueStatus.Waiting` because no dedicated same-day queue entity exists in the current domain model.
- Prevention Rule: Reuse the existing queue aggregate for queue semantics when task intent matches and no alternate queue table is modeled.
