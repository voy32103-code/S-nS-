# Phase 31 — Durable encrypted onboarding HTTP V9

Date: 2026-08-24  
Status: implemented and live-verified; production key material remains external.

## Canonical artifacts

- Entry point: `backend/SanSo.Api.V6/ProgramCanonicalV9.cs`
- Repository: `backend/SanSo.Api.V6/PostgresOnboardingStoreV1.cs`
- Generator: `scripts/generate-api-v9-entrypoint.ps1`
- Test: `backend/SanSo.Api.V6.Tests/PostgresOnboardingHttpV9Tests.cs`
- OpenAPI: `docs/openapi-v9-canonical.json`
- Environment template: `.env.v9.example`

## Secret configuration

- `SANSO_FIELD_ENCRYPTION_KEY_BASE64`: Base64 representation of exactly 32 random bytes.
- `SANSO_FIELD_ENCRYPTION_KEY_VERSION`: non-empty version without `.`.

The repository is registered only when PostgreSQL and both values are present. Invalid Base64 or invalid key length/version stops startup with a safe configuration error. With PostgreSQL configured but no field key, onboarding endpoints return `503 FIELD_ENCRYPTION_NOT_CONFIGURED`; they never store tax identifiers or addresses in memory as a silent production fallback.

Development without PostgreSQL retains explicit demo memory mode and returns `persisted=false`.

## HTTP workflow

All seven ordered steps now override the older memory routes:

1. business profile;
2. source selection;
3. backfill range;
4. SKU mapping;
5. opening balances;
6. versioned disclaimer with authenticated actor;
7. first real reconciliation activation.

Every route requires `organization.manage`, enforces the tenant-bound bearer, catches domain validation errors and reports whether data was persisted.

## Live verification

An isolated PostgreSQL 16 cluster ran migrations 001–012. The HTTP integration test proved:

- PostgreSQL without encryption key returns 503;
- a configured 256-bit key enables persistence;
- all seven steps reach `currentStep=8`;
- disclaimer actor equals the authenticated user;
- tax identifier is returned only as `******6789`;
- tax identifier and address plaintext are absent at rest;
- key version is stored;
- a new application factory reads the completed workflow after restart.

The test passed. The temporary cluster and log were stopped and removed.

## Verification gates

- `dotnet build SanSo.V7.slnx -c Release`: 0 warnings, 0 errors.
- `dotnet test SanSo.V7.slnx -c Release --no-build`: 98 passed, 0 failed.
- OpenAPI V9: 31 paths, 97 internal refs, 20 routes matched to source, UTF-8 clean.

