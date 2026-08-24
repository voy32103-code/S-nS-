# SànSổ — authoritative local guide V4

V4 supersedes README.V3 for API and solution commands.

## Authoritative runtime

- API: `backend/SanSo.Api.V4` (`ProgramFixed.cs`).
- Migrator: `backend/SanSo.Migrator.V2`.
- Solution gate: `SanSo.V3.slnx`.
- Frontend: Vite V3 composition configured by existing npm scripts.

`SanSo.Api.V2` remains a referenced compatibility/store assembly and regression target. Do not launch its Program as the current API. `SanSo.Migrator` and its old 005–007 chain are superseded.

## Verified local commands

```powershell
dotnet restore SanSo.V3.slnx
dotnet build SanSo.V3.slnx --no-restore
dotnet test SanSo.V3.slnx --no-build --no-restore
powershell -ExecutionPolicy Bypass -File scripts/verify-migration-manifest-v2.ps1
npm --prefix frontend test -- --run
npm --prefix frontend run build
powershell -ExecutionPolicy Bypass -File scripts/scan-client-secrets-v2.ps1
```

Run development API:

```powershell
dotnet run --project backend/SanSo.Api.V4/SanSo.Api.V4.csproj
```

Development mode supports demo reads and in-memory import preview/confirm with `persisted=false`. Production requires PostgreSQL and authenticated tenant-bound requests; import staging/confirm uses migration chain V2.

## Import HTTP flow

1. `POST /api/imports/preview` multipart field `file`.
2. Inspect rows/errors/checksum, retain `previewToken`.
3. Explicitly confirm with `POST /api/imports/confirm` and `{ previewToken, checksum }`.
4. A token is tenant-bound and one-time. Checksum tamper is rejected without consuming the valid token.

Production runtime/RLS is still unverified locally while Docker/PostgreSQL is unavailable. Do not interpret development `persisted=false` confirmation as a database commit.
