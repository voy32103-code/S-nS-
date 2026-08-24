# Phase 33 — Durable report export V12

Date: 2026-08-24  
Status: reconciliation CSV export workflow implemented and live-verified.

## Canonical artifacts

- Solution: `SanSo.V9.slnx`
- API entry point: `backend/SanSo.Api.V6/ProgramCanonicalV12.cs`
- Store: `backend/SanSo.Api.V6/PostgresReportExportStoreV1.cs`
- Migration: `backend/SanSo.Migrator.V7/Migrations/014_durable_report_exports.sql`
- Migrator: `backend/SanSo.Migrator.V7` (001–014)
- OpenAPI: `docs/openapi-v12-canonical.json`
- E2E integration test: `PostgresReportExportHttpV12Tests.cs`

## Controlled workflow

1. `GET /api/reports` lists supported reports.
2. `POST /api/reports/exports` creates a durable `PREVIEWED` artifact for a reconciliation run.
3. Response includes filename, input checksum, content checksum, line count and 30-minute preview expiry.
4. User confirms the exact content checksum at `/confirm`.
5. Confirmed artifact becomes `READY` for 24 hours.
6. Download is rejected before confirmation or after expiry.
7. Preview, confirmation and download each append an audit event.

Preview, confirmation and download require `export.sensitive`; mutation/download also require a step-up session. A Finance session without step-up is denied even though the role otherwise has export permission. Owner/Admin MFA sessions provide step-up in the current identity implementation.

## Traceable CSV

The first line records tenant, reconciliation run, settlement, reconciliation status, expected/actual/difference, input checksum, rule versions and generation time. Rows contain source line/order, type, expected/actual/difference, reason, ledger source key and raw source event ID.

All text fields are quoted and spreadsheet-formula prefixes are neutralized. The downloaded bytes must match the confirmed SHA-256 checksum.

## Storage decision

For the pilot, content is stored as PostgreSQL `bytea` with status, checksum, expiry and download count. This provides transactional and testable behavior without inventing an object-storage provider. Production scale should move encrypted content to approved object storage/KMS while retaining database metadata and checksum; that migration is not claimed complete.

## Live verification

An isolated PostgreSQL 16 cluster applied migrations 001–014 and skipped all on the second pass. The test proved:

- Finance without step-up receives 403;
- Owner+MFA creates preview;
- premature download and wrong checksum are rejected;
- correct confirmation changes state to `READY`;
- metadata GET returns the confirmed checksum;
- downloaded bytes match SHA-256 and contain trace metadata/lines;
- download count is one;
- exactly three export audit events exist.

The cluster and log were stopped and removed.

## Gates

- solution build: 0 warnings, 0 errors;
- backend tests: 106/106;
- OpenAPI V12: 38 paths, 127 refs, 27 source-matched routes, UTF-8 clean;
- migration manifest: 14 sequential files.

