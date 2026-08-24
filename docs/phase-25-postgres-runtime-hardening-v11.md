# Phase 25 — PostgreSQL runtime hardening and canonical V11 pipeline

Date: 2026-08-24  
Status: implemented and verified against isolated PostgreSQL 17 clusters.

## Runtime environment

Two disposable PostgreSQL 17 clusters were created inside `D:\WebAppCodex`, bound only to `127.0.0.1` on ports 55432/55433. They did not touch the installed services on port 5432. Both clusters and logs were stopped and permanently removed after verification.

## Runtime defects found and fixed

### 1. Nested migration transactions

Baseline SQL files contain `BEGIN/COMMIT`, while Migrator V2 also opened an Npgsql transaction. The SQL committed the transaction and the runner then attempted a second commit (`NpgsqlTransaction has completed`).

Fix: Migrator V3/V4 computes checksums from the original file, removes only the outer transaction wrapper, and runs SQL plus migration-record insert in one transaction.

### 2. Import confirmation multi-command prepared statement

`PostgresImportStagingStore` sent parameterized `INSERT ...; UPDATE ...` as one Npgsql command. Npgsql 9/PostgreSQL rejected it with `42601 cannot insert multiple commands into a prepared statement`.

Fix: `PostgresImportStagingStoreV2` executes raw-event insertion and batch update as two commands inside the same Serializable transaction. API V5 uses this store and authenticates bearer directly when Development middleware has not populated `HttpContext.Items`.

### 3. Worker claim reader lifecycle

The no-row claim path committed while the Npgsql reader was still active, producing `NpgsqlOperationInProgressException`.

Fix: `PostgresOutboxStoreV2` scopes and disposes the reader before commit in every branch. Worker V2 uses this store.

### 4. Projection period-key ambiguity

Migration 010 used PL/pgSQL variable `period_key`, colliding with the `tax_periods.period_key` column in an `ON CONFLICT` target.

Fix: append-only migration 011 replaces the function using `target_period_key` and names the unique constraint explicitly. Migration 010 was not rewritten after application.

### 5. Inventory multi-command and isolation behavior

The old reserve method combined `UPDATE; INSERT` in one parameterized command and Serializable contention surfaced SQLSTATE 40001 rather than a domain conflict.

Fix: `PostgresInventoryStoreV3` uses separate commands in one ReadCommitted transaction with `SELECT ... FOR UPDATE`, unique source key, and domain `InventoryConflictException`. Exactly one last-unit claimant succeeds; retrying the winning source key is idempotent.

## Canonical artifacts

- `SanSo.V5.slnx`
- `backend/SanSo.Api.V5`
- `backend/SanSo.Api.V5.Tests`
- `backend/SanSo.Migrator.V4`
- `backend/SanSo.Migrator.V4/Migrations/011_fix_projection_period_conflict.sql`
- `backend/SanSo.Worker.V2`
- `backend/SanSo.Worker/PostgresOutboxStoreV2.cs`
- `scripts/verify-migration-manifest-v6.ps1`
- `scripts/verify-postgres-tenant-rls-v5.sql`
- `scripts/verify-import-projection-v5.sql`
- `.github/workflows/ci-v11-canonical.yml`

## Verified evidence

- Migrator V4 first run: APPLIED 001–011.
- Migrator V4 second run: SKIP 001–011 with unchanged checksums.
- RLS verifier: tenant A=11 events, tenant B=1 event, 34 FORCE RLS tables, no cross-tenant visibility under `NOSUPERUSER NOBYPASSRLS` role.
- Projection verifier: one order, one SALE ledger line, tax status `NEEDS_REVIEW`, calculated amount null, rule version null, idempotent second raw insert.
- API V5 HTTP PostgreSQL test: MFA login → persisted preview → persisted confirm → raw/order/ledger/no-guess tax assertions.
- Inventory store test: exactly one winner and one domain conflict for last ATP; source retry does not create a second movement.
- Inventory HTTP test: anonymous request 401; concurrent Owner requests yield one 200 and one 409 `INSUFFICIENT_ATP`.
- Worker test: one claim across two concurrent stores, one expired lease recovered, unconfigured Shopee action dead-lettered with `PARTNER_ADAPTER_NOT_CONFIGURED`.
- `dotnet build SanSo.V5.slnx --configuration Release`: 0 warnings, 0 errors.
- `dotnet test SanSo.V5.slnx --no-build --configuration Release` with runtime PostgreSQL: 89/89 passed.
- Database snapshot before disposal: migrations=11, forced_rls=34, projected_orders=4, no_guess_tax=4, dead_letters=3.
- New-project NuGet vulnerability audit: no known vulnerable packages.

## CI note

`.github/workflows/ci-v11-canonical.yml` is authoritative. `ci-v10-import-projection.yml` is superseded because it stops at ambiguous migration 010. It should be disabled or removed in repository administration. The local sandbox helper prevented updating/deleting that already-existing workflow file safely; no database-wide compatibility setting was accepted as a workaround.

