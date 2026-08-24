# SànSổ — canonical handoff V9

## Authoritative runtime

- Solution: `SanSo.V5.slnx`
- API: `backend/SanSo.Api.V5`
- Worker: `backend/SanSo.Worker.V2`
- Migrator: `backend/SanSo.Migrator.V4` (001–011)
- Frontend: `frontend/index-v8.html`
- CI: `.github/workflows/ci-v11-canonical.yml`
- OpenAPI: `docs/openapi-v4-canonical.json` (requires V5 refresh before external publication)

## Verified local commands

```powershell
dotnet build SanSo.V5.slnx --configuration Release
$env:SANSO_RUNTIME_POSTGRES = '<isolated test connection string>'
dotnet test SanSo.V5.slnx --no-build --configuration Release
pwsh ./scripts/verify-migration-manifest-v6.ps1 -Configuration Release
```

The isolated runtime verification and all discovered defects are documented in `docs/phase-25-postgres-runtime-hardening-v11.md`. Current incomplete items are in `docs/completion-audit-2026-08-24-v15.md`.

