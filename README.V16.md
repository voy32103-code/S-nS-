# SànSổ — canonical handoff V16

Canonical stack:

- solution: `SanSo.V8.slnx`;
- API entry point: `backend/SanSo.Api.V6/ProgramCanonicalV11.cs`;
- migrator: `backend/SanSo.Migrator.V6` (001–013);
- worker: `backend/SanSo.Worker.V2`;
- frontend: V8;
- OpenAPI: `docs/openapi-v11-canonical.json`;
- environment template: `.env.v9.example`.

Latest evidence:

- `docs/phase-32-settlement-line-reconciliation-v11.md`;
- `docs/completion-audit-2026-08-24-v22.md`;
- backend build 0 warnings/errors;
- backend tests 105/105;
- frontend tests 17/17 and production build successful;
- OpenAPI V11 and 13-migration manifest verified.

Settlement CSV is supported and evidence-backed. Settlement XLSX, external providers, production identity and approved tax rules remain explicit gaps.
