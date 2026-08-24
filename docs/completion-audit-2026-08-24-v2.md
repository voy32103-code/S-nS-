# Completion audit V2 — Master Prompt

**Audit date:** 2026-08-24  
**Rule:** `DONE` requires direct current evidence; source-only or planned behavior is `PARTIAL`.

## Product acceptance criteria

| # | Status | Current evidence | Remaining proof/work |
|---:|---|---|---|
| 1 demo first reconciliation | DONE | API E2E-01 + browser dashboard | — |
| 2 repeat import has no duplicate order/money/tax/inventory | PARTIAL | raw/order/inventory idempotency unit/API tests | real PostgreSQL repeated migration/import and tax effects |
| 3 settlement drill-down to order and raw source | PARTIAL | lines/order/source refs and raw endpoint | UI evidence drawer linking exact raw ID |
| 4 expected payout/difference line explanation | DONE | unit, API E2E-02, browser bridge |
| 5 partial refund/return adjustment preserves history | PARTIAL | schema/seed scenario and reversal design | executable posting/integration/E2E |
| 6 every tax result has rule/source/effective date/explanation | PARTIAL | no-guess calculation carries nullable provenance | executable approved legal golden case and persisted calculation |
| 7 missing tax data creates exception, never guess | DONE | unit, API E2E-03, component/browser tax checks |
| 8 locked period immutable | PARTIAL | state-machine unit test | persisted period concurrency/amendment/export E2E |
| 9 inventory invariant/concurrent no double subtraction | PARTIAL | concurrent unit/API E2E + PostgreSQL serializable implementation | real PostgreSQL concurrency/RLS integration |
| 10 tenant isolation read/write/export | PARTIAL | production bearer test, API source cross-tenant test, RLS SQL | real RLS DB tests and authenticated export/IDOR matrix |
| 11 no token/secret/PII in bundle/log/error | PARTIAL | safe-error tests, npm audits, synthetic `.invalid` data | automated secret/bundle/log fixture scan in executed CI |
| 12 outage preserves raw and has retry/recovery | PARTIAL | raw-before-process design + retry/DLQ/token-revoke tests/runbook | worker crash/restart integration |
| 13 export trace metadata | DONE | checksum/tenant/time/rule metadata unit + E2E-06 |
| 14 core E2E/integration/golden/tenant tests pass | PARTIAL | 32 backend, 6 component, 6 API E2E, 4 browser E2E | PostgreSQL integration + legal-approved golden suite |
| 15 all UI states | PARTIAL | reusable/component specs and loading/error/degraded tests | browser no-permission/empty/async/locked coverage |
| 16 no unsupported legal/integration claim | DONE | legal matrix, copy and no rate/partner capability claim |
| 17 clean local README | PARTIAL | `README.V2.md` commands/scripts verified locally except DB | clean-machine PostgreSQL path pending daemon |

## Section 26 output audit

| Output | Status | Artifact |
|---|---|---|
| Executive summary/thesis | DONE | `prd.md` |
| Assumptions/unknowns/decision log | DONE | `product-blueprint.md`, legal/market doc, worklogs |
| Legal applicability matrix | DONE for research baseline | `legal-and-market-validation.md`; production rule sign-off deliberately open |
| Market/TAM-SAM-SOM scenarios | DONE as explicit assumptions | `legal-and-market-validation.md` |
| Personas/JTBD/pains | DONE | `prd.md` |
| Functional/non-functional PRD | DONE | `prd.md` |
| MVP/P1/P2/non-goals/dependencies | DONE | `prd.md` |
| Journeys/state machines | DONE | `prd.md`, `ux-specification.md` |
| Sitemap/IA/screens | DONE | `ux-specification.md` |
| Tokens/components/Vietnamese copy | DONE | `ux-specification.md` |
| Architecture/modules/data flow | DONE | `architecture-and-data.md` |
| ERD/data dictionary | DONE | `architecture-and-data.md`, migrations |
| OpenAPI/traceability | DONE baseline | `openapi.yaml`, `prd.md`; runtime V2 contract drift check pending |
| Event/job catalog | DONE | `integrations-and-events.md` |
| Integration/fallback strategy | DONE | `integrations-and-events.md` |
| Tax engine specification/no fake rate | DONE | blueprint, PRD, tests and `TaxCenter.cs` |
| Threat model/privacy inventory | DONE engineering baseline | `security-and-privacy.md`; legal privacy sign-off open |
| Test/acceptance/golden plan | DONE as plan; execution partial | `test-strategy.md` |
| Observability/SLO/runbooks | DONE as design; instrumentation partial | `operations-and-runbooks.md` |
| Pricing/unit economics/pilot | DONE as hypotheses | `pricing-pilot-and-risks.md` |
| Prioritized backlog | DONE | `prioritized-backlog.md` |
| Source/migrations/seed/tests/Docker/CI/env/README | PARTIAL | all exist; PostgreSQL runtime/worker missing |

## Current verified command evidence

- `dotnet build SanSo.sln --no-restore`: 5 projects, 0 warnings, 0 errors.
- `dotnet test SanSo.sln --no-build`: 29 + 3 = **32 passed**.
- `frontend npm test`: **6 passed**; V2 build pass; audit 0.
- `e2e npm test`: **6 API E2E passed**.
- `e2e npm run test:browser`: **4 Chromium E2E passed**.
- `e2e npm audit`: 0 vulnerabilities.
- Docker daemon/PostgreSQL runtime: **not available**, therefore no database completion claim.

## Next completion gate

Run migrations against PostgreSQL 16, execute RLS/idempotency/concurrency/recovery integration suite, replace remaining in-memory tax/audit paths in API V2, implement refund/amendment/authenticated team/entitlement workflows, then rerun this audit.
