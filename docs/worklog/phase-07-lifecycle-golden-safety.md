# Phase 7 — Financial lifecycle, team/support, billing and tax safety

**Date:** 2026-08-24 · **Status:** in progress

## Implemented domain behavior

- Append-only finance entries with source-key idempotency.
- Partial refund posts signed adjustment and preserves original sale.
- Refund/return after locked period posts `NEXT_PERIOD_ADJUSTMENT`; old export checksum remains unchanged and amendment references the old snapshot.
- Period freeze checksum, export checksum, lock immutability and amendment reason.
- Hashed one-time invitation token and replay prevention.
- Support grant requires step-up, explicit reason and maximum eight-hour expiry; revoke removes access.
- Trial/active/past-due/expired transitions, usage recording, new-sync denial after expiry and retained existing-data read.
- Copilot refuses tax-rate/final-tax decisions and only explains from tenant-scoped evidence IDs.

## PostgreSQL workflow persistence

- Migration `004_workflow_security.sql`: invitations, sessions, support grants, feature flags and notification deliveries with RLS/indexes/check constraints.
- `PostgresLifecycleStore`: idempotent refund, locked-period exception, deterministic period checksum/freeze, guarded lock, hashed invitation and subscription update.

## Golden tax safety

- `GoldenTax/negative-cases.json`: four executable no-rate cases for missing category/profile/no approved rule/effective-date boundary.
- Regression asserts no rule version or calculated amount is invented.
- `docs/tax-rule-engine.md` specifies selection, approval, reproducibility and sample `DRAFT` rule with `rate: null`.
- Positive amount/rate cases remain deliberately blocked on named expert approval and exact official article/source.

## Verification

- Domain/API V2 total: **40 tests passed, 0 failed**.
- Solution build: **0 warnings, 0 errors** after enabling nullable context for test project.

## Remaining

- Wire lifecycle store commands into the authoritative production API surface.
- Run PostgreSQL migrations and real integration tests.
- Browser flows for refund resolution, period export/lock/amendment, invite/revoke and subscription expiry.
