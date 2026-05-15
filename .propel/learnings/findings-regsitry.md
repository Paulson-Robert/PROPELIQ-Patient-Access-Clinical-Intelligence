# Findings Registry

- Date: 2026-05-14
- Source: User correction
- Finding: Endpoint implementation drifted from task spec (`POST /api/ping` in `PingController.cs`) to `POST /api/health/ping` in `HealthController`.
- Prevention Rule: For scaffolded API endpoints, cross-check route attributes and controller naming against the task spec before marking implementation complete.

- Date: 2026-05-14
- Source: User correction
- Finding: `global.json` used `9.0.0`, which resembles a runtime version and may not resolve to an installed SDK.
- Prevention Rule: Pin `global.json` to a real SDK feature band version (for example `9.0.100`, `9.0.101`) rather than runtime-style values.
