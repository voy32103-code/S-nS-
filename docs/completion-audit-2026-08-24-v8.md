# Completion audit V8 — API V4 and import HTTP

Ngày: 2026-08-24

## Authoritative artifacts

- Solution: `SanSo.V3.slnx`.
- API: `SanSo.Api.V4` / `ProgramFixed.cs`.
- Migrator: `SanSo.Migrator.V2`.
- Guide: `README.V4.md`.

## Verified gates

| Gate | Result |
|---|---|
| Restore | PASS — 9 projects |
| Build | PASS — 0 warnings, 0 errors |
| Domain/API tests | 59/59 |
| API V2 regression tests | 11/11 |
| Import library tests | 10/10 |
| API V4 HTTP tests | 4/4 |
| Total .NET | **84/84** |
| Migration V2 static manifest | PASS — 001–008 |

## Acceptance status change

Repeat import/confirmation nâng từ core-only lên HTTP executable:

- preview token/checksum returned;
- explicit confirm;
- one-time token;
- checksum tamper rejection;
- cross-tenant rejection;
- production anonymous request rejection before DB.

Requirement vẫn `PARTIAL`, không `DONE`, vì PostgreSQL staging/raw projection, duplicate impact trên order/money/tax/inventory và browser diff/confirmation chưa được chứng minh.

## Remaining top gates

1. PostgreSQL migrations 001–008 execution and non-owner RLS matrix.
2. Authenticated database-backed API integration tests.
3. Import confirm → raw → projection → reconciliation invariant E2E.
4. Onboarding/notification HTTP and browser journeys.
5. UI preview/diff/two-step confirmation for sensitive actions.
6. Hosted worker restart/recovery and OTLP/exporter runtime.
7. Positive tax golden suite only after expert-approved rule data.

MVP chưa đạt product acceptance và không được mô tả production-ready.
