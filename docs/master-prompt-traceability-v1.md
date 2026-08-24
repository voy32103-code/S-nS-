# Master prompt traceability — V1

Date: 2026-08-24  
Source: `D:\Downloads\Master_Prompt_Ecom_Ops_Tax_Automation_VN.md`  
Rule: missing or indirect evidence is not counted as complete.

## Required outputs (section 26)

| # | Required output | Evidence | Status |
|---:|---|---|---|
| 1 | Executive summary/product thesis | `docs/product-blueprint.md` | Complete |
| 2 | Assumptions/unknowns/decision log | `docs/product-blueprint.md`, completion audits | Complete, decisions open |
| 3 | Legal applicability matrix | `docs/legal-and-market-validation.md` | Draft; legal review open |
| 4 | Market validation and scenario TAM/SAM/SOM | `docs/legal-and-market-validation.md` | Hypothesis only; primary validation open |
| 5 | Personas/JTBD/pain points | `docs/product-blueprint.md`, `docs/prd.md` | Complete |
| 6 | PRD functional/non-functional requirements | `docs/prd.md` | Complete |
| 7 | MVP/P1/P2/non-goals/dependencies | `docs/prd.md` | Complete |
| 8 | Journeys and state machines | `docs/product-blueprint.md`, `docs/ux-specification.md` | Complete |
| 9 | Sitemap/IA/screen specification | `docs/ux-specification.md` | Complete |
| 10 | Design tokens/components/Vietnamese microcopy | `docs/ux-specification.md`, frontend V8 | Complete for MVP shell |
| 11 | Architecture/module/data-flow diagrams | `docs/architecture-and-data.md` | Complete |
| 12 | ERD/data dictionary | `docs/architecture-and-data.md`, migrations 001–011 | Complete for implemented schema |
| 13 | OpenAPI/API traceability | `docs/openapi-v4-canonical.json`, `docs/api-traceability-v4.md` | Stale for V7; refresh required |
| 14 | Event/job catalog | `docs/integrations-and-events.md` | Complete for implemented jobs |
| 15 | Integration contracts/fallback | `docs/integrations-and-events.md`, CSV/XLSX adapter, mock adapters | Complete as fail-closed fallback; production credentials open |
| 16 | Tax engine specification/sample without invented rate | `docs/tax-rule-engine.md` | Complete specification; approved production rules absent |
| 17 | Threat model/privacy inventory | `docs/security-and-privacy.md` | Complete design; production operational controls remain deployment work |
| 18 | Test/acceptance/golden plan | `docs/test-strategy.md`, test projects | Plan complete; expert-approved tax golden dataset absent |
| 19 | Observability/SLO/runbooks | `docs/operations-and-runbooks.md` | Complete design/MVP instrumentation; production drills open |
| 20 | Pricing/unit economics/pilot | `docs/pricing-pilot-and-risks.md` | Hypothesis complete; interviews/WTP research open |
| 21 | Prioritized backlog | `docs/prioritized-backlog.md` | Complete |
| 22 | Code/migrations/seed/tests/Docker/CI/env/README/demo | source tree, migrations, `docker-compose.yml`, CI V12, README V12 | Implemented, but Docker runtime not locally verified |

## Product acceptance criteria (section 27)

| # | Criterion | Current evidence | Verdict |
|---:|---|---|---|
| 1 | New user reaches first demo reconciliation without credentials | Demo/onboarding/UI tests | Partially proven; full real-browser/backend E2E not proven |
| 2 | Re-import does not duplicate order/money/tax/inventory | PostgreSQL projection/idempotency live checks | Proven for implemented import path |
| 3 | Settlement drills to order/raw source | schema/services/tests | Partially proven; complete UI drill-down absent |
| 4 | Expected payout/difference explained line-by-line | reconciliation core tests | Partially proven; production fee/settlement ingestion incomplete |
| 5 | Partial refund/return preserves history | lifecycle tests | Proven at domain/storage scope |
| 6 | Tax result has rule/source/effective date/explanation | deterministic engine/spec/tests | Proven structurally; no approved production rule set |
| 7 | Missing tax data creates exception | PostgreSQL projection and tests | Proven |
| 8 | Locked period is not silently mutated | lifecycle tests and schema controls | Proven for implemented workflows |
| 9 | Inventory invariant/concurrency | PostgreSQL HTTP/store live tests | Proven |
| 10 | Cross-tenant read/write/export prevented | forced RLS live test and authorization tests | Proven for covered tables/routes; export surface incomplete |
| 11 | No token/secret/PII in bundle/log/error | scans, masking tests, encrypted fields | Partially proven; production log pipeline not available |
| 12 | Outage preserves raw data and recovery exists | outbox worker live tests | Proven for implemented outbox/import path |
| 13 | Export has trace metadata | export domain/tests | Partially proven; complete report export surface incomplete |
| 14 | Core E2E/integration/golden/tenant tests pass | 95 .NET + 17 frontend; live PostgreSQL checks | Not complete: expert-approved golden tax and full real-system E2E absent |
| 15 | UI empty/loading/error/degraded/no-permission | frontend V8 components/tests | Proven for canonical shell screens |
| 16 | No unproven legal/integration claims | fail-closed copy/adapters/audits | Proven in current artifacts |
| 17 | New developer can run locally from README | README series and Docker files | Partially proven; canonical one-command Docker run not verified locally |

## Blocking inputs (do not infer)

1. Identity architecture decision and real IdP tenant/application configuration.
2. Tax/legal approval, source mapping and golden expected outcomes.
3. Marketplace, email/Zalo and billing provider sandbox credentials/scopes.
4. Production AES-256 field key and key version from a secret manager.
5. Pilot customer interviews and willingness-to-pay observations.

The MVP must not be marked complete until every `Partial`, `Draft`, `Stale`, or `Not complete` row is resolved with authoritative evidence.

