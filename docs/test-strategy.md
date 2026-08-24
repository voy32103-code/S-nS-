# Test strategy, acceptance matrix and golden sets

## Test pyramid and environments

- Unit/property: pure money, tax selection, state machines, inventory and entitlement.
- Component/API: authorization, validation, RFC 7807, tenant boundaries.
- Integration: real PostgreSQL 16/Redis 7 containers, transactions, RLS, inbox/outbox, file import and jobs.
- Contract: versioned sanitized connector fixtures and failure semantics.
- E2E: production-like API/worker/DB/browser with synthetic tenants only.
- Security: SAST/SCA/secret scan plus targeted abuse tests.

## Mandatory unit/property cases

1. Checked signed money and explicit rounding.
2. Duplicate event/file causes one effect.
3. Out-of-order status mapping cannot regress terminal state incorrectly.
4. Matched + unmatched lines equals run scope.
5. Reversal leaves original history and nets correctly.
6. Same tax snapshot + rule version gives identical result.
7. Missing/ambiguous tax input produces `NEEDS_REVIEW`.
8. Inventory movement sum equals balance projection.
9. Concurrent last-unit orders produce one reservation.
10. Locked period rejects mutation; amendment preserves old export.
11. Expired entitlement stops new work without data loss.

## Golden tax dataset schema

No fabricated rate is checked in. A golden case becomes executable only after expert approval:

```json
{
  "caseId": "GOLDEN-LEGAL-APPROVED-001",
  "approvedBy": "tax-expert-id",
  "inputSnapshot": {},
  "ruleCode": "approved-code",
  "ruleVersion": 1,
  "legalSource": "official-document-url-and-article",
  "effectiveDate": "YYYY-MM-DD",
  "expectedBasis": 0,
  "expectedAmount": 0,
  "expectedExplanation": "reviewed explanation"
}
```

Required cases: effective-date boundary, threshold boundary where legally applicable, refund/reversal, cross-period adjustment, profile change, platform-withheld difference, missing category and overlapping/no rule. CI must fail if approved rule changes without full golden regression.

## Connector contract cases

Valid signature; bad signature; replay; duplicate; out-of-order; poll+webhook same event; pagination/cursor; 429/Retry-After; timeout/5xx; token refresh then revoke; partial page; unknown field/schema drift; split shipment; multi-settlement; payout without direct order ID.

## E2E journeys

| ID | Journey | Expected |
|---|---|---|
| E2E-01 | Register → MFA → organization/profile → demo import → reconciliation | activation event recorded |
| E2E-02 | Import settlement → discrepancy → evidence → reason → adjustment/resolve | audit and new result |
| E2E-03 | Tax exception → review → snapshot → export → lock → amendment | immutable versions |
| E2E-04 | SKU map → reserve last unit concurrently → cancel release | no oversell/double effect |
| E2E-05 | Invite accountant → tenant switch attempt | invited tenant allowed; other denied |
| E2E-06 | Entitlement expires during queued sync | safe stop; existing data available |

## Security tests

IDOR path/body/query; role escalation; stale session after membership removal; missing MFA/step-up; export authorization; webhook spoof/replay; malicious/oversized/renamed/zip-bomb upload; CSV formulas; SQLi/XSS/SSRF/path; token/PII in logs/problem responses/frontend bundle; support access without consent/after expiry.

## Acceptance traceability

| Product AC | Primary test |
|---:|---|
| 1 | E2E-01 |
| 2 | duplicate property + DB/import integration |
| 3–4 | E2E-02 + raw/ledger API contract |
| 5 | reversal/cross-period integration |
| 6–8 | golden suite + E2E-03 |
| 9 | inventory property + E2E-04 |
| 10 | IDOR suite + RLS integration + E2E-05 |
| 11 | secret/log/bundle scan |
| 12 | kill-worker/retry recovery integration |
| 13 | export metadata/formula test |
| 14 | CI required suites |
| 15 | UI component/E2E state matrix |
| 16 | content/legal claim scan |
| 17 | clean-machine README verification |

## Current evidence

As of 2026-08-24: 23 backend unit/invariant/security/reliability tests and 1 frontend unit test pass; Phase 1 and V2 builds pass. This does **not** yet prove integration, connector, golden or E2E completion.
