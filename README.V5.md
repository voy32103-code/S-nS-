# SànSổ — authoritative guide V5

V5 supersedes README.V4 where paths differ.

## Current artifacts

- Solution: `SanSo.V3.slnx`
- API: `backend/SanSo.Api.V4` (`ProgramFixed.cs` + authorized composition)
- Migrator: `backend/SanSo.Migrator.V2`
- Frontend: `index-v4.html`, `main-v4.tsx`, `vite.v4.config.js`
- API contract: `docs/openapi-v4-canonical.json`

## Verification

```powershell
dotnet restore SanSo.V3.slnx
dotnet build SanSo.V3.slnx --no-restore
dotnet test SanSo.V3.slnx --no-build --no-restore
npm --prefix frontend test -- --run
cd frontend
npx tsc -b
npx vite build --config vite.v4.config.js
cd ..
./scripts/verify-migration-manifest-v2.ps1
./scripts/verify-openapi-v4-canonical.ps1
./scripts/scan-client-secrets-v2.ps1
cd e2e
npx playwright test --config playwright.import-v4-cors.config.ts
```

Run development API:

```powershell
dotnet run --project backend/SanSo.Api.V4 --no-launch-profile -- --urls http://127.0.0.1:5080 --environment Development
```

Run frontend V4:

```powershell
cd frontend
npx vite --config vite.v4.config.js --host 127.0.0.1 --port 5174
```

Production still requires PostgreSQL, bearer session, tenant header and approved migration V2. Do not use old migrator/API compositions as current runtime. Do not claim automatic filing, certified tax correctness or partner connectivity without external evidence.
