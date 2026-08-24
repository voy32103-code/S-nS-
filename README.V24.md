# SànSổ V14 — Traceable CSV/XLSX report data packs

Canonical:

- Solution: `SanSo.V10.slnx`
- API entrypoint: `backend/SanSo.Api.V6/ProgramCanonicalV14.cs`
- Report store: `PostgresReportExportStoreV2`
- OpenAPI: `docs/openapi-v14-canonical.json`
- Frontend: V8

Report lifecycle hỗ trợ `RECONCILIATION_CSV` và `RECONCILIATION_XLSX`: preview → checksum confirm → download, step-up MFA và audit như nhau. XLSX có sheet Metadata/Reconciliation, không chứa công thức.

Bằng chứng: backend live 112/112, Playwright/Axe 10/10, Vitest 17/17, build solution 0 warning/error, V8 production build thành công.

