# Phase 32 — Settlement/fee ingestion and line reconciliation V11

Date: 2026-08-24  
Status: CSV vertical slice implemented and live-verified; XLSX settlement import remains open.

## Canonical artifacts

- Solution: `SanSo.V8.slnx`
- Entry point: `backend/SanSo.Api.V6/ProgramCanonicalV11.cs`
- Parser/store: `backend/SanSo.Api.V6/PostgresSettlementImportStoreV1.cs`
- Migration: `backend/SanSo.Migrator.V6/Migrations/013_settlement_line_traceability.sql`
- Migrator: `backend/SanSo.Migrator.V6` (001–013)
- OpenAPI: `docs/openapi-v11-canonical.json`
- Parser tests: `SettlementCsvParserV1Tests.cs`
- HTTP/live test: `PostgresSettlementImportHttpV10Tests.cs`
- Safe error middleware: `SafeProblemMiddlewareV2.cs`

## CSV contract

Required columns:

```text
settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount
```

Supported line types are `SALE`, `PLATFORM_FEE`, `SHIPPING_FEE`, `AFFILIATE_FEE`, `REFUND`, and `ADJUSTMENT`. Signed integer VND minor units are required. Actual amount is explicit per line; the service never invents an allocation from settlement total.

Text identities reject spreadsheet formula prefixes. CSV must be valid UTF-8, source-line IDs must be unique, and every row must share settlement code and paid-at timestamp.

## Transaction and traceability

One transaction creates:

- confirmed import batch keyed by file checksum;
- immutable raw event for each source line;
- settlement;
- ledger line linked to settlement/raw/order when matched;
- reconciliation run with expected, actual and difference;
- line reconciliation with explicit reason code;
- audit log bound to the authenticated actor.

Each drill-down line returns source-line identity, optional order code, ledger type/key/explanation and raw source event ID.

Re-importing the identical file returns the original run with `duplicate=true` and writes nothing new. Reusing a settlement code with a different file is rejected as `SETTLEMENT_CODE_CONFLICT`; revisions are not silently applied.

## Live PostgreSQL evidence

An isolated PostgreSQL 16 cluster applied migrations 001–013 and skipped all 13 on the second pass. The HTTP test proved:

- anonymous import returns 401;
- sale 1,000, fee −100/actual −120 and unmatched adjustment 20 produce expected 920, actual 900, difference −20;
- status is `NEEDS_REVIEW`;
- mismatch and missing-order reason codes are retained;
- raw-to-ledger-to-reconciliation drill-down returns three lines;
- identical retry returns the same run;
- conflicting file returns 409;
- database counts remain exactly one batch, one settlement, three raw events, three ledger lines, one run, three reconciliation lines and one audit entry.

The temporary cluster and log were stopped and removed.

## Defects found and corrected during live verification

1. Missing order references required typed-null-safe SQL via `NULLIF`.
2. Local `+07:00` timestamps are preserved in raw payload but normalized to UTC at the PostgreSQL boundary.
3. Detail reader is explicitly closed before transaction commit.
4. Raw library `ArgumentException` details could leak; middleware V2 now exposes only safe uppercase domain codes and otherwise returns generic validation/conflict messages.

## Gates

- build: 0 warnings, 0 errors;
- backend tests: 105/105;
- OpenAPI V11: 33 paths, 110 refs, 22 source-matched routes, UTF-8 clean;
- migration manifest: 13 sequential files.

