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
