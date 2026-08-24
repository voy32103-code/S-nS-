# SànSổ — canonical handoff V8

Canonical authenticated frontend: `frontend/index-v8.html` using `frontend/vite.v8.config.js`.

Canonical backend remains:

- solution `SanSo.V4.slnx`;
- API `backend/SanSo.Api.V4`;
- worker `backend/SanSo.Worker`;
- migrator `backend/SanSo.Migrator.V2` with migrations 001–009;
- OpenAPI `docs/openapi-v4-canonical.json`.

Verification:

```powershell
dotnet build SanSo.V4.slnx --configuration Release
dotnet test SanSo.V4.slnx --no-build --configuration Release
cd frontend
npm exec tsc -- --project tsconfig.v8.json
npm exec vitest -- run --config vitest.v8.config.ts
npm exec vite -- build --config vite.v8.config.js
cd ../e2e
npx playwright test --config playwright.auth-ui-v8.config.ts
```

Current completion evidence and honest remaining gaps are recorded in `docs/completion-audit-2026-08-24-v14.md`. PostgreSQL runtime verification still requires a disposable connection string.

