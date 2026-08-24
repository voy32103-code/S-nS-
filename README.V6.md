# SànSổ — authoritative guide V6

V6 supersedes README.V5 where commands/artifacts differ.

## Current runtime set

- Solution: `SanSo.V4.slnx`
- API: `backend/SanSo.Api.V4`
- Worker: `backend/SanSo.Worker`
- Migrator: `backend/SanSo.Migrator.V2`
- Migration chain: 001–009
- Frontend: V4 composition
- Contract: `docs/openapi-v4-canonical.json`

## Verification

```powershell
dotnet restore SanSo.V4.slnx
dotnet build SanSo.V4.slnx --no-restore
dotnet test SanSo.V4.slnx --no-build --no-restore
./scripts/verify-migration-manifest-v4.ps1
./scripts/verify-openapi-v4-canonical.ps1
npm --prefix frontend test -- --run
cd frontend
npx tsc -b
npx vite build --config vite.v4.config.js
cd ..
./scripts/scan-client-secrets-v2.ps1
cd e2e
npx playwright test --config playwright.import-v4-cors.config.ts
```

## Worker

The worker is intentionally tenant-scoped:

```powershell
$env:SANSO_POSTGRES = '<secret>'
$env:SANSO_WORKER_TENANT = '<organization uuid>'
dotnet run --project backend/SanSo.Worker --no-launch-profile
```

It does not use `BYPASS RLS`. Partner/email types fail closed until an approved adapter/provider is configured. Never commit environment secrets.

## Known blockers

- Docker/PostgreSQL unavailable locally, so migrations/RLS/leases are not runtime-proven.
- Real Shopee/TikTok/email handlers are not configured.
- Authenticated onboarding browser UI is incomplete.
- Positive tax golden rules require expert approval.
