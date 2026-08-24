# SànSổ completion audit V15

Date: 2026-08-24  
Verdict: **not fully complete**, but the former PostgreSQL runtime blocker is closed with direct evidence.

## Newly proven since V14

| Requirement | Evidence | Status |
|---|---|---|
| Append-only migrations execute atomically and idempotently | Migrator V4 live runs, 001–011 | Proven |
| Tenant RLS blocks cross-tenant reads for non-BYPASSRLS role | live SQL verifier | Proven across 34 tenant tables |
| Confirmed import stores immutable raw evidence | API V5 HTTP PostgreSQL test | Proven |
| Raw import projects deterministically to order and SALE ledger | migration 010+011 and live assertions | Proven |
| Missing approved rule/category never produces monetary tax result | live tax assertion | Proven: `NEEDS_REVIEW`, amount/rule null |
| Last-unit concurrent reserve | store and HTTP live tests | Proven: one success, one conflict |
| Reserve source idempotency | live store test | Proven |
| Two-worker claim contention | live worker test | Proven |
| Lease recovery | live worker test | Proven |
| Unconfigured provider fails closed to dead letter | live worker test | Proven |
| Canonical aggregate solution | `SanSo.V5.slnx` | Build 0/0; tests 89/89 |

## Current canonical stack

- API: `backend/SanSo.Api.V5`
- Worker: `backend/SanSo.Worker.V2`
- Migrator: `backend/SanSo.Migrator.V4`, migrations 001–011
- Frontend: V8
- Solution: `SanSo.V5.slnx`
- CI: `ci-v11-canonical.yml`

## Remaining incomplete requirements

1. Identity, onboarding, and notification persistence are still in-process. Database tables exist, but API V5 does not yet use durable repositories for them.
2. PostgreSQL inventory read and release endpoints deliberately return 501; reserve is durable and proven.
3. Full reconciliation projection requires settlement/fee inputs; CSV order rows do not contain enough evidence, so no settlement or inventory movement is invented.
4. Marketplace/email/Zalo connectors remain unconfigured. No official capability claim is made without credentials and partner approval.
5. Positive tax monetary golden cases still require expert-approved legal rule versions, source articles, effective dates, subject type, and category. The safe no-guess path is complete.
6. Browser-to-real-backend V8 login needs a provisioned test identity/managed IdP test tenant. Current V8 browser acceptance mocks API responses and is labeled accordingly.
7. Superseded CI V10 should be disabled/removed; CI V11 is the correct pipeline.

## Inputs required from the user or domain experts

- Expert-approved Vietnamese tax rule dataset and expected golden outcomes.
- Official sandbox credentials/authorization for any marketplace or delivery provider to be activated.
- Decision on production identity provider versus building database-backed password/session storage in-house.

