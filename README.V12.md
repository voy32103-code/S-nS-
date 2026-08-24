# SànSổ — canonical handoff V12

Canonical stack:

- Solution: `SanSo.V6.slnx`
- API executable project: `backend/SanSo.Api.V6`
- API entry point compiled: `backend/SanSo.Api.V6/ProgramCanonicalV7.cs`
- Worker: `backend/SanSo.Worker.V2`
- Migrator: `backend/SanSo.Migrator.V4` (migrations 001–011)
- Frontend: V8 (`frontend/index-v8.html`, `frontend/main-v8.tsx`, `frontend/vite.v8.config.js`)
- CI: `.github/workflows/ci-v12-canonical.yml`

Latest implementation note: `docs/phase-28-import-preview-hardening-v7.md`.

Latest completion audit: `docs/completion-audit-2026-08-24-v18.md`.

The product is an evidence-backed MVP, not yet a production-complete tax system. Tax results remain fail-closed until approved rules are supplied. Provider integrations remain fail-closed until official credentials and scopes are supplied.
