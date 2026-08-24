# Phase 4 — Required artifacts, API boundaries and E2E

**Date:** 2026-08-24  
**Status:** in progress

## Documents completed in this phase

- `prd.md`: ten-line thesis, personas/JTBD, functional/non-functional requirements, scope/dependencies/DoD.
- `ux-specification.md`: sitemap, screen specifications, journeys, all UX states, design tokens, components and Vietnamese microcopy.
- `architecture-and-data.md`: deployment/context, 15 module boundaries, data flow, ERD and critical data dictionary.
- `openapi.yaml`: version-controlled API contract and security/idempotency semantics.
- `integrations-and-events.md`: connector/fallback contract, event catalog, job catalog and retry behavior.
- `security-and-privacy.md`: trust boundaries, threat model, privacy inventory and data workflows.
- `test-strategy.md`: property/golden/contract/E2E/security cases and acceptance traceability.
- `operations-and-runbooks.md`: SLIs/SLOs, telemetry, alert severity and six operational runbooks.
- `pricing-pilot-and-risks.md`: A/B price hypotheses, unit economics, interview/pilot plan and owned risk register.
- `prioritized-backlog.md`: estimates, dependencies and acceptance criteria.

## API/security implementation

- Login/logout/current-session endpoints.
- MFA-safe unauthorized response; tenant-bound identity core remains the authority.
- Safe RFC-7807-style error middleware with correlation ID and no stack/secret in response.
- `nosniff`, frame denial, referrer and restrictive API CSP headers.
- Fixed-window rate-limiter registration.
- Four API boundary tests added using `WebApplicationFactory`.

## Verification evidence

- `dotnet test SanSo.sln`: **27 passed, 0 failed** including live in-memory API boundary tests.
- Six Playwright request-level E2E journeys pass with a real ASP.NET server: demo reconciliation, discrepancy explanation, tax no-guess, inventory concurrency, source tenant isolation and traceable export.
- Initial Playwright 1.55.0 audit found GHSA-7mvr-c777-76hp; upgraded to 1.62.1. `npm audit`: **0 vulnerabilities**; all six E2E pass again after upgrade.

## Honest limitations

- These are API-level Playwright journeys, not yet browser interaction through every UI screen.
- Core demo/module endpoints still accept demo tenant header without bearer enforcement; production authorization filter remains P0.
- PostgreSQL migration/integration and RLS tests remain pending Docker engine.
- Golden tax suite intentionally has schema only; executable rate cases require expert-approved legal rule data.
