# Phase 6 — PostgreSQL API V2 and default tooling

**Date:** 2026-08-24 · **Status:** in progress

## Implemented

- New `SanSo.Api.V2` production composition references tested domain services and uses Npgsql 9.0.3.
- PostgreSQL store sets tenant session context, queries tenant orders/dashboard/reconciliation, inserts immutable raw events with conflict idempotency, and reserves inventory in serializable transaction with row lock/source-key dedupe.
- Production startup requires a PostgreSQL secret; Development permits explicit demo reads only. Durable raw/inventory writes return 503 when DB is absent instead of silently degrading to memory.
- V2 composition tests cover Development degraded health/demo read, Production bearer-before-query, and durable-write refusal without database.
- Added authoritative `README.V2.md`, CI V2 with PostgreSQL/Redis services/migrator/E2E, and `scripts/verify.ps1`.
- Standard npm scripts now target frontend V2 and split API versus browser Playwright suites.

## Failures found and corrected

- Initial V2 DI registered `PostgresCommerceStore` without `NpgsqlDataSource` in Development; two tests failed. Composition now registers/resolves repository only when DB is configured.
- Initial default E2E script collected UI tests under API baseURL; corrected scripts select `core.spec.ts` and `ui.spec.ts` separately.

## Current evidence

- Solution build: 0 warnings/errors.
- Backend/API tests: 32 pass.
- Frontend: 6 pass, V2 build pass, audit 0.
- API E2E: 6 pass.
- Chromium browser E2E: 4 pass.
- E2E audit: 0.

## Not yet evidenced

CI V2 workflow has been authored but not executed by a GitHub runner in this local non-repository workspace. Docker Desktop daemon remains unavailable, so PostgreSQL migration/RLS/concurrency tests are pending.
