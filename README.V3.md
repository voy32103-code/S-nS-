# SànSổ — authoritative local guide V3

V3 supersedes README.V2 only for migration/build commands. Product/legal limitations in README.V2 remain applicable.

## Authoritative projects

- API runtime: `backend/SanSo.Api.V2` (`ProgramV3.cs` composition).
- Import parser/core: `backend/SanSo.Import`.
- Database migrator: **`backend/SanSo.Migrator.V2`**.
- Solution gate: **`SanSo.V2.slnx`**.

Do not use `backend/SanSo.Migrator` or migration chain 005–007 under `SanSo.Api/Persistence/Migrations` for a new database. That chain is superseded because audit found a table-name collision and invalid FK. The V2 migrator reuses baseline 001–004 and supplies corrected 005–008.

## Build and static verification

```powershell
dotnet restore SanSo.V2.slnx
dotnet build SanSo.V2.slnx --no-restore
dotnet test SanSo.V2.slnx --no-build --no-restore
powershell -ExecutionPolicy Bypass -File scripts/verify-migration-manifest-v2.ps1
npm --prefix frontend ci
npm --prefix frontend test -- --run
npm --prefix frontend run build
powershell -ExecutionPolicy Bypass -File scripts/scan-client-secrets-v2.ps1
```

## PostgreSQL migration

Set the connection string only in the environment/secret manager:

```powershell
$env:SANSO_POSTGRES = '<secret connection string>'
dotnet run --project backend/SanSo.Migrator.V2/SanSo.Migrator.V2.csproj --no-build
```

Never commit the connection string. Migration success is not proven in this workspace until PostgreSQL is available and runtime RLS tests pass.

## Known production gaps

- PostgreSQL/Docker runtime unverified locally.
- Import confirm HTTP/UI wiring incomplete.
- Hosted worker/exporter/provider adapters incomplete.
- Onboarding and notification API/UI E2E incomplete.
- Positive tax rule cases require expert-approved rule/rate; do not invent them.
