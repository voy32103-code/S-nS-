# SànSổ completion audit V13

Date: 2026-08-24  
Verdict: **not complete**. Material progress is verified, but the master prompt cannot yet be declared fully satisfied.

## Newly proven since V12

| Requirement | Evidence | Result |
|---|---|---|
| Browser login form supports tenant plus MFA | `frontend/src/AppV7.tsx`, component tests | Proven at component level |
| Bearer and authenticated tenant are attached centrally | `frontend/src/api-v6.ts`, Owner test assertions | Proven at component level |
| Session avoids persistent browser storage | V7 source inspection | Proven |
| Owner/Admin versus Viewer onboarding state | `AppV7.test.tsx` | Proven, 2 scenarios |
| Ordered seven-step onboarding payloads match backend records | `OnboardingV7.tsx` against `OnboardingWorkflow.cs` | Proven statically; HTTP execution still needs a provisioned user |
| Canonical frontend type safety | `tsconfig.v7.json` | Passed |
| Canonical frontend component suite | Vitest V8 config | 14/14 passed |
| Canonical frontend production bundle | Vite V7 | Passed |
| CI points to V4 solution and Migrator V2 | `ci-v4-canonical.yml` | Proven statically |
| CI has a real non-owner RLS isolation gate | `verify-postgres-tenant-rls-v5.sql` | Implemented; local runtime pending |

## Current aggregate evidence

- Release solution build: 0 warnings, 0 errors.
- .NET tests: 92/92 passed.
- Frontend tests: 14/14 passed.
- Import browser acceptance previously verified: 2/2 passed.
- Migration manifest: 001–009 verified.
- Canonical OpenAPI: 23 paths and 48 valid component references.
- Client secret/PII scan: passed.
- NuGet project-by-project vulnerability audit previously passed; CI now enforces it.

## Completion blockers and incomplete requirements

1. **Live PostgreSQL proof is missing locally.** PostgreSQL 17 is running but password authentication prevents creation of the isolated test database. This blocks live migration, RLS, concurrency, worker, and persisted-import evidence.
2. **Production auth persistence is incomplete.** `IdentityService` is process memory, so sessions and memberships do not survive restart and are unsuitable as final production identity storage.
3. **Onboarding and notification persistence is incomplete.** The API explicitly returns `persisted=false` and production notification creation fails closed.
4. **End-to-end import projection is incomplete/unproven.** Preview and confirmation are implemented, but durable projection through order/ledger/reconciliation/tax/inventory is not proven.
5. **Authenticated product UI is split.** V7 authenticates and onboards; the broad V4 operational screens/import workflow still need migration to the same central authenticated client.
6. **Provider integrations remain adapters/fail-closed stubs.** No claim is made about Shopee, TikTok Shop, Zalo, or email capabilities without credentials and official authorization.
7. **Approved tax rules are absent.** The engine correctly produces review states, but positive monetary golden cases require expert-approved legal rule inputs.

## Required next evidence

After receiving a disposable PostgreSQL connection string:

1. create an isolated database and least-privilege runtime roles;
2. apply Migrator V2 twice and validate checksums;
3. execute `verify-postgres-tenant-rls-v5.sql`;
4. run API/worker against PostgreSQL, including crash recovery and two-worker claim contention;
5. run persisted import preview/confirm and validate canonical projection/idempotency;
6. record commands and outputs in V14.

