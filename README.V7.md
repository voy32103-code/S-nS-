# SànSổ — canonical handoff V7

This workspace implements a Vietnamese e-commerce operations, reconciliation, inventory, and tax-control pilot. The project is **not yet production complete**; see `docs/completion-audit-2026-08-24-v13.md` for evidence and remaining gates.

## Authoritative artifacts

- Backend solution: `SanSo.V4.slnx`
- API: `backend/SanSo.Api.V4` (`ProgramFixed.cs` plus `V4AuthorizedWorkflowComposition.cs`)
- Worker: `backend/SanSo.Worker`
- Migrator: `backend/SanSo.Migrator.V2` (migrations 001–009)
- Authenticated onboarding frontend: `frontend/index-v7.html`, `main-v7.tsx`, `vite.v7.config.js`
- Broader operations/import frontend: `frontend/index-v4.html`, `main-v4.tsx`, `vite.v4.config.js`
- OpenAPI: `docs/openapi-v4-canonical.json`
- CI: `.github/workflows/ci-v4-canonical.yml`
- Live RLS verifier: `scripts/verify-postgres-tenant-rls-v5.sql`

## Verified commands

```powershell
dotnet build SanSo.V4.slnx --configuration Release
dotnet test SanSo.V4.slnx --no-build --configuration Release
pwsh ./scripts/verify-migration-manifest-v4.ps1 -Configuration Release
pwsh ./scripts/verify-openapi-v4-canonical.ps1
cd frontend
npm exec tsc -- --project tsconfig.v7.json
npm exec vitest -- run --config vitest.v8.config.ts
npm exec vite -- build --config vite.v7.config.js
```

## PostgreSQL runtime gate

Set `SANSO_POSTGRES` only in the process environment or a secret manager. Do not commit it. Then:

```powershell
dotnet run --project backend/SanSo.Migrator.V2 --configuration Release
psql $env:SANSO_POSTGRES -X -v ON_ERROR_STOP=1 -f scripts/verify-postgres-tenant-rls-v5.sql
```

Run the migrator twice to verify checksum/idempotent behavior. Use an isolated test database, never a production or shared development database.

