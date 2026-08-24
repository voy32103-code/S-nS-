# SànSổ MVP — authoritative developer guide

This guide supersedes the initial Phase-1 README while the filesystem helper prevents safe replacement of older files.

## Prerequisites

- .NET SDK 9.0.x
- Node.js 22+
- Docker Desktop with Linux engine, PostgreSQL 16 and Redis 7 images

## Development demo — no marketplace credentials

Terminal 1:

```powershell
dotnet run --project backend/SanSo.Api --no-launch-profile -- --urls http://127.0.0.1:5080 --environment Development
```

Terminal 2:

```powershell
cd frontend
npm ci
npm run dev
```

Open `http://127.0.0.1:5174/index-v2.html` or the port Vite prints. Demo mode is explicit Development behavior; production never trusts a tenant header without a bearer session.

## PostgreSQL production path

```powershell
Copy-Item .env.example .env
docker compose up -d postgres redis
$env:SANSO_POSTGRES='Host=127.0.0.1;Port=5432;Database=sanso;Username=sanso;Password=<same-local-password>'
dotnet run --project backend/SanSo.Migrator
dotnet run --project backend/SanSo.Api.V2 --no-launch-profile -- --urls http://127.0.0.1:5080 --environment Production
```

Production requires `SANSO_POSTGRES` or `ConnectionStrings:Postgres`. The migrator applies schema, RLS and two-tenant/12-scenario seed with advisory lock and checksums.

## Verification

```powershell
dotnet build SanSo.sln
dotnet test SanSo.sln --no-build

cd frontend
npm ci
npm test
npm run build
npm audit --audit-level=high

cd ../e2e
npm ci
npm test
npm run test:browser
npm audit --audit-level=high
```

Expected current local evidence (2026-08-24): 32 backend/API tests, 6 frontend tests, 6 API E2E and 4 Chromium browser E2E pass; audits report zero vulnerabilities. PostgreSQL execution/RLS tests remain pending locally until Docker daemon is available.

## Demo scenarios

`backend/SanSo.Api/Persistence/Migrations/003_demo_seed.sql` contains two isolated tenants and S01–S12: exact match, missing fee, voucher mapping, post-settlement partial refund, cross-period return, withholding mismatch, missing category, concurrent last unit, revoked token, duplicate CSV, accountant tenant isolation and effective-dated rule versions. All data is synthetic and uses no real PII/rate.

## Safety boundaries

- No automatic tax filing/payment/signature.
- No invented tax rate or threshold; missing approval/input becomes `NEEDS_REVIEW`.
- Stock write-back is disabled on degraded/revoked connection and requires feature flag.
- Existing data remains readable after subscription expiry according to authorization/retention policy.
- Official marketplace connectors are not claimed until partner access and contract tests are verified; CSV/XLSX fallback remains mandatory.

## Documentation index

- `docs/implementation-status.md` — completion audit
- `docs/prd.md` — requirements and scope
- `docs/legal-and-market-validation.md` — verified facts/open assumptions
- `docs/architecture-and-data.md` — architecture, ERD, data dictionary
- `docs/openapi.yaml` — versioned API contract
- `docs/ux-specification.md` — IA/screens/tokens/microcopy
- `docs/security-and-privacy.md` — threat/privacy inventory
- `docs/test-strategy.md` — tests and traceability
- `docs/operations-and-runbooks.md` — SLO/alerts/runbooks
- `docs/pricing-pilot-and-risks.md` — commercial hypotheses/pilot/risks
- `docs/worklog/` — implementation evidence by phase
