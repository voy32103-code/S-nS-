# Prioritized implementation backlog

Estimate: S ≤2 dev-days, M 3–5, L 1–2 weeks, XL >2 weeks; estimates assume two product engineers plus shared design/QA/security.

| ID | Story | Pri | Est | Depends | Acceptance criteria |
|---|---|---:|---:|---|---|
| IAM-01 | Authenticated tenant context + MFA/session endpoints | P0 | L | DB | no production trust of tenant header; revoke/IDOR tests |
| DB-01 | PostgreSQL repository and migration runner | P0 | L | Docker | clean apply/rollback plan; integration tests |
| ING-01 | Immutable raw/file import + inbox/outbox | P0 | L | DB-01 | duplicate/poll+webhook/out-of-order tests |
| ING-02 | CSV/XLSX preview/mapping/scan | P0 | L | ING-01 | encoding/schema/formula/large-file cases |
| ORD-01 | Canonical order/items/status timeline | P0 | L | ING-01 | split/refund/cancel/source drill-down |
| LED-01 | Append-only ledger/reversal | P0 | L | ORD-01 | sum/reversal/multi-settlement invariants |
| REC-01 | Reconciliation matching and resolution workspace | P0 | L | LED-01 | line evidence/reason/audit/locked behavior |
| TAX-01 | Profile and rule approval/version engine | P0 | L | IAM/DB/LED | missing→review; golden suite; legal sign-off |
| TAX-02 | Period snapshot/lock/export/amendment | P0 | L | TAX-01 | old export preserved after amendment |
| INV-01 | SKU map/movement/balance/ATP | P0 | L | ORD/DB | concurrent last unit + quarantine tests |
| INV-02 | Feature-flagged stock outbox | P0 | M | INV/ING | rate limit/revoke/kill-switch tests |
| RPT-01 | Traceable async CSV/XLSX data pack | P0 | M | REC/TAX | provenance, permission, formula protection |
| ALT-01 | Alert policy + in-app/email | P0 | M | event catalog | dedupe/ack/escalation tests |
| BILL-01 | plans/trial/usage/entitlements | P0 | L | IAM/DB | expiry stops work, retains data |
| UI-01 | Switch V2 as default + route/features | P0 | M | API | all UX states and accessibility checks |
| E2E-01 | Six core Playwright journeys | P0 | L | all P0 | clean seeded environment green |
| SEC-01 | security headers/rate/upload/webhook/log scans | P0 | L | API/ING | security matrix green |
| OBS-01 | OTel, metrics, alerts and dashboards | P0 | M | jobs/API | correlation end-to-end/runbook links |
| ADM-01 | consented time-bound support console | P1 | L | IAM/audit | masked and no access after expiry |
| PROF-01 | allocation and profitability UI | P1 | M | ledger | signed bridge and source explanation |
| AGENCY-01 | multi-client accountant dashboard | P1 | XL | IAM/TAX | strict client isolation/bulk status |
| AI-01 | evidence-cited explanation copilot | P1 | L | TAX/REC/security | no calculation/rate decision/cross-tenant context |
| CONN-01 | verified Shopee connector | P1 | XL | partner access | official scope/contract/fixtures verified |
| CONN-02 | verified TikTok connector | P1 | XL | partner access | official scope/contract/fixtures verified |

Backlog order prioritizes tenant isolation, source integrity, money and tax correctness before feature breadth. A partner-credential dependency never blocks demo/CSV paths.
