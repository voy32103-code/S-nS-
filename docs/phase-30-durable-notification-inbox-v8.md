# Phase 30 — Durable notification inbox V8

Date: 2026-08-24  
Status: implemented and live-verified for InApp; external Email remains fail-closed.

## Canonical artifacts

- Solution: `SanSo.V7.slnx`
- Entry point: `backend/SanSo.Api.V6/ProgramCanonicalV8.cs`
- Repository: `backend/SanSo.Api.V6/PostgresNotificationStoreV1.cs`
- Migration: `backend/SanSo.Migrator.V5/Migrations/012_notification_inbox_persistence.sql`
- Migrator: `backend/SanSo.Migrator.V5`
- OpenAPI: `docs/openapi-v8-canonical.json`
- Repository/RLS test: `PostgresNotificationPersistenceV2Tests.cs`
- HTTP test: `PostgresNotificationHttpV8Tests.cs`

## Behavior

- InApp notifications persist title/body, masked recipient, resource reference, dedupe key and delivery state.
- Repeated raise in the same tenant/type/resource/hour returns the same delivery.
- Inbox survives repository/application restart.
- Acknowledge is idempotent at the stored-state level and tenant-scoped.
- Every list/acknowledge query has an explicit `organization_id` predicate in addition to forced RLS.
- HTTP routes require tenant-bound bearer authorization.
- Demo without PostgreSQL explicitly returns `persisted=false`.
- Email returns `503 EMAIL_PROVIDER_NOT_CONFIGURED`; no fake send is reported.
- JSON enum names such as `LowStock` and `InApp` are accepted and emitted as strings.

## Live PostgreSQL evidence

An isolated PostgreSQL 16 cluster was created under the workspace with trust auth limited to localhost and a non-superuser application role for RLS verification.

- migrations 001–012 applied;
- second migration pass skipped all 12 files;
- repository test proved persistence, dedupe, acknowledgement and cross-tenant isolation;
- HTTP test proved anonymous 401, authenticated persistence, dedupe, acknowledgement and Email 503;
- both tests passed;
- temporary clusters and logs were stopped and removed (`True`).

## Defects found and corrected during verification

1. Npgsql 9 rejected multi-command prepared setup SQL; setup now uses separate commands in one transaction.
2. Enum strings initially caused body binding failure; `JsonStringEnumConverter` is now configured.
3. Admin datasource bypassed RLS and exposed the absence of explicit tenant predicates; list/acknowledge now enforce both query predicates and RLS.
4. Acknowledge initially missed the second tenant parameter; live tests caught and verified the fix.

## Gates

- `dotnet build SanSo.V7.slnx -c Release`: 0 warnings, 0 errors.
- `dotnet test SanSo.V7.slnx -c Release --no-build`: 97 passed, 0 failed.
- OpenAPI V8: 25 paths, 65 internal refs, 12 source-matched routes, UTF-8 clean.

