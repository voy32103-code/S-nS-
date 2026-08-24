# Phase 3 — Identity, reliability, workflow UI and legal validation

**Date:** 2026-08-24  
**Status:** in progress

## Implemented and verified

- Identity core: PBKDF2-SHA256 password hashing, random opaque session token stored only as SHA-256 hash, tenant-bound session, expiry/revocation and immediate revocation on membership removal.
- RFC 6238-style TOTP validation with ±1 time-step tolerance; Owner/Admin login requires MFA.
- Deny-by-default permission matrix for organization, finance, tax, inventory, billing and sensitive export.
- Tamper-evident audit chain; reason mandatory for sensitive export, support access and period reopen.
- Reliable outbox model with immutable payload, exponential bounded retry, attempt/status/error, dead letter and token-revoked pause.
- Connection health and in-app alerts; write-back disabled on revoked token/permanent failure.
- Signed-integer profitability bridge.
- React V2 workflow UI: dashboard, orders, reconciliation, Tax Center, inventory, integrations, reports, team and billing; loading/error/empty/degraded/no-permission components are represented.
- Production V2 artifact verified by explicit build: `dist/index-v2.html`, CSS 7.54 kB, JS 207.84 kB; bundle contains new workflow strings.
- Legal source metadata verified through official Government pages; legal/market matrix added in `docs/legal-and-market-validation.md`.

## Test evidence

- Backend: **23 passed, 0 failed** after Identity/reliability additions.
- Frontend unit: **1 passed, 0 failed**.
- Frontend V2 production bundle: build pass with 1.804 transformed modules.

## Known gaps

- V2 is served as `/index-v2.html`; sandbox filesystem helper repeatedly fails when updating the original `index.html`. Default-root entry still points to Phase 1 UI.
- Identity core is not yet wired into all HTTP endpoints; demo header remains restricted conceptually to demo mode but enforcement middleware is pending.
- Legal metadata/source scope verified; transaction-level tax rates, thresholds, forms and transition clauses remain intentionally unpublished until expert review.
- 650.000 remains unverified; scenario ranges are explicit assumptions only.
