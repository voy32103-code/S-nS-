# Phase 23 — Authenticated onboarding UI and canonical CI

Date: 2026-08-24  
Status: implemented and statically/component verified; PostgreSQL runtime gate remains pending.

## Outcome

This phase closes two gaps found in the V12 completion audit:

1. The browser now has a real email/password/tenant/TOTP login surface and keeps the opaque bearer token only in React memory. It does not put the token in `localStorage` or `sessionStorage`.
2. Owner/Admin can access the ordered seven-step onboarding UI; Viewer receives an explicit denied state. Requests use both the bearer token and the tenant returned by the authenticated principal.
3. A canonical GitHub Actions workflow now builds `SanSo.V4.slnx`, runs all .NET tests, applies Migrator V2 twice, proves RLS using a non-owner/non-BYPASSRLS role, validates OpenAPI, typechecks/tests/builds frontend V7, scans client artifacts, runs import browser acceptance, and audits dependencies.

No development bypass endpoint or embedded credential was added. An initially considered auto-session design was rejected because a reusable demo password/TOTP in source would be unsafe.

## Canonical frontend artifacts

- `frontend/index-v7.html`
- `frontend/main-v7.tsx`
- `frontend/src/AppV7.tsx`
- `frontend/src/OnboardingV7.tsx`
- `frontend/src/api-v6.ts`
- `frontend/src/styles-v5.css`
- `frontend/tsconfig.v7.json`
- `frontend/vite.v7.config.js`
- `frontend/vitest.v8.config.ts`
- `frontend/src/test-setup-v8.ts`
- `frontend/src/AppV7.test.tsx`

V5/V6 files are superseded development artifacts. V7 is authoritative because it fixes the onboarding contract and React effect cleanup.

## Canonical CI and database verifier

- `.github/workflows/ci-v4-canonical.yml`
- `scripts/verify-postgres-tenant-rls-v5.sql`

The SQL verifier checks all 34 tenant tables have both RLS and FORCE RLS, then changes to `sanso_ci_rls_reader` (`NOSUPERUSER NOBYPASSRLS`) and proves:

- tenant A sees 11 seeded raw events and never tenant B's event;
- tenant B sees 1 seeded raw event and never tenant A's events;
- each tenant sees only its own organization.

## Verification evidence

Executed locally on 2026-08-24:

- `dotnet build SanSo.V4.slnx --configuration Release`: passed, 0 warnings, 0 errors.
- `dotnet test SanSo.V4.slnx --no-build --configuration Release`: 92/92 passed (59 API core, 11 API V2, 7 API V4 HTTP, 10 import, 5 worker).
- `scripts/verify-migration-manifest-v4.ps1 -Configuration Release`: passed; 9 migrations, 001–009, worker lease present.
- `scripts/verify-openapi-v4-canonical.ps1` under PowerShell 7: passed; 23 paths, 48 references, version `0.4.1-pilot`.
- `npm exec tsc -- --project tsconfig.v7.json`: passed.
- `npm exec vitest -- run --config vitest.v8.config.ts`: 14/14 passed across 5 files.
- `npm exec vite -- build --config vite.v7.config.js`: passed; 1,805 modules; JS 199.07 kB, CSS 2.32 kB.
- `scripts/scan-client-secrets-v2.ps1`: passed; 3 artifacts, 6 rules.

## Remaining runtime gate

PostgreSQL 17 is running locally, but `postgres@127.0.0.1:5432` requires a password and no project `.env` contains one. Therefore these claims are not yet locally proven:

- applying migrations 001–009 to an isolated local database;
- the non-owner tenant RLS verifier result;
- worker lease/retry/dead-letter behavior against live PostgreSQL;
- concurrent inventory behavior against live PostgreSQL;
- persisted import staging and projection.

Required user input: the local `postgres` password or a disposable PostgreSQL connection string. The value must be supplied at runtime and must not be written to the repository or reports.

## Known product gaps retained from V12

- Production identity storage remains in memory rather than PostgreSQL/managed IdP.
- Onboarding and notification APIs report `persisted=false`; production persistence is not implemented.
- Raw/import projection into canonical order, ledger, tax, reconciliation, and inventory tables is not runtime-proven.
- Real marketplace/email providers remain unconfigured and fail closed by design.
- Tax positive golden cases still require expert-approved rule versions and legal inputs; no tax rate was invented.
- V7 is the canonical authenticated onboarding surface; the broader V4 operations dashboard has not yet been fully merged into the same authenticated API client.

