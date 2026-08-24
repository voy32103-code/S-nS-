# Phase 26 — Durable inventory lifecycle API V6

Date: 2026-08-24  
Status: implemented and live-verified against disposable PostgreSQL 17.

## Outcome

API V6 replaces the PostgreSQL inventory 501 responses with authenticated durable operations:

- `GET /api/inventory/{sku}` reads on-hand, reserved, quarantine, available, and version under tenant RLS.
- `POST /api/inventory/{sku}/reserve` uses a row lock, ATP guard, unique source key, movement record, and version increment.
- `POST /api/inventory/{sku}/release` rejects release above reserved, is source-key idempotent, writes a RELEASE movement, and increments version once.

V6 reuses the V5 authorized composition but registers inventory routes with lower route order so the durable handlers are selected without ambiguous matching.

## Canonical artifacts

- `SanSo.V6.slnx`
- `backend/SanSo.Api.V6`
- `backend/SanSo.Api.V6.Tests`
- `backend/SanSo.Api.V5/PostgresInventoryStoreV4.cs`
- `.github/workflows/ci-v12-canonical.yml`

## Live acceptance evidence

An isolated cluster on `127.0.0.1:55434` was initialized, migrated through 001–011, tested, stopped, and permanently deleted.

The HTTP test proved:

- anonymous GET returns 401;
- authenticated Owner GET returns initial available=2;
- reserve 2 returns available=0;
- release 3 returns 400;
- release 1 returns 200;
- retrying the same release source key returns 200 without a second mutation;
- final reserved=1 and version=2.

## Aggregate evidence

- `dotnet build SanSo.V6.slnx --configuration Release`: 0 warnings, 0 errors.
- `dotnet test SanSo.V6.slnx --no-build --configuration Release`: 90/90 passed without a runtime DB environment.

The 90/90 aggregate count is not presented as database proof because conditional integration tests return early without `SANSO_RUNTIME_POSTGRES`. Database proof is the separate live run described above and the prior Phase 25 live aggregate.

