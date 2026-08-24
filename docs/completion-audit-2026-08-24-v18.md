# SànSổ completion audit V18

Date: 2026-08-24  
Verdict: **not fully complete**.

## Newly completed

- API V7 canonical composition replaces the unsafe V6 import preview implementation at compile time.
- Multipart, file-presence, 10 MiB, extension/MIME and UTF-8 controls are enforced.
- Import preview security regression suite passes 4/4.
- API V6/V7 project suite passes 6/6.
- Canonical API build passes with 0 warnings and 0 errors.

## Previously proven and still applicable

- PostgreSQL migrations 001–011 are repeatable and hash-tracked.
- forced tenant RLS is live-verified on 34 tables.
- import confirmation projects raw events, orders, ledger entries and tax review state idempotently.
- tax calculation remains fail-closed with `NEEDS_REVIEW`; no rate is invented.
- worker claim/lease/recovery/dead-letter behavior is live-verified.
- inventory reserve/release is durable, guarded and idempotent.
- seven-step onboarding repository is durable and AES-256-GCM protected.
- frontend V8 passes 17/17 tests and production build.

## Required external decisions or inputs

1. Production identity architecture: managed IdP (recommended) or in-house PostgreSQL identity.
2. Tax rules and golden cases approved by a qualified Vietnamese tax/accounting owner.
3. Official sandbox credentials and approved scopes for marketplace and notification providers.
4. Production field-encryption key and key version supplied through a secret manager.

## Remaining engineering work

- wire the durable onboarding repository into HTTP routes after secret configuration exists;
- implement durable notification preferences/delivery and real provider adapters after credentials exist;
- refresh and verify the public OpenAPI contract for the V7 canonical runtime;
- ingest settlement and fee evidence required for complete marketplace reconciliation;
- run browser-to-real-IdP acceptance after the IdP decision;
- remove or disable superseded CI workflow `ci-v10-import-projection.yml` when the existing-file sandbox issue is cleared.

No production-readiness claim is made while these items remain open.

