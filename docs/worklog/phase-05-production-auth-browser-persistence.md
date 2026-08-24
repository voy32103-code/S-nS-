# Phase 5 — Production auth boundary, browser E2E and migration runner

**Date:** 2026-08-24  
**Status:** in progress

## Production/demo authorization

- Added environment-aware tenant middleware.
- Development explicitly permits credentialless demo workflows.
- Production requires bearer token and verifies session tenant equals requested tenant before `/api/*` continues.
- Route-class permission checks enforce finance/tax/inventory/export scopes; sensitive export requires step-up session.
- Tests prove a tenant header without bearer returns safe 401 in Production and demo reconciliation remains available in Development.
- Known gap: safe body currently serializes with `application/json`, not the desired `application/problem+json`; status/body/correlation remain safe and tested.

## Verification evidence

- Backend/API suite: **29 passed, 0 failed**.
- API Playwright E2E: **6 passed, 0 failed** in explicit Development environment.
- Frontend component suite: **6 passed, 0 failed**, including tax no-guess, quarantine ATP, degraded integration and error recovery.
- Chromium browser E2E: initial run on origin 5175 failed because CORS blocked data loading; rerun on allowlisted 5174: **4 passed, 0 failed**.
- Frontend V2 production bundle: build pass, 1.804 modules; npm audit 0 vulnerabilities.

## Persistence progress

- Added `SanSo.Migrator` with Npgsql 9.0.3.
- Migrator requires environment secret, uses PostgreSQL advisory lock, lexical transactional migrations, persistent checksum and refuses mutated applied SQL.
- Added relational seed migration with two tenant IDs, plan/subscription/connections, all 12 scenario raw events and inventory balance/quarantine data.
- `dotnet build SanSo.sln --no-restore`: success, 0 warnings/errors including migrator.
- Docker Desktop was launched hidden but daemon remains unavailable; database execution/RLS evidence is still pending and documented in `migration-and-seed.md`.

## Remaining P0

- Repository-backed API/worker instead of in-memory state.
- Real PostgreSQL/RLS/inbox-outbox integration tests and migration execution.
- Browser E2E for authenticated onboarding, invite/revoke, period lock/amendment and entitlement expiration.
- Make V2 the default root entry after filesystem helper allows editing existing index.
- Executable legal-approved golden tax cases.
