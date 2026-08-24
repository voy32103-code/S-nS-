# SànSổ — canonical handoff V17

Canonical stack:

- solution: `SanSo.V9.slnx`;
- API entry point: `backend/SanSo.Api.V6/ProgramCanonicalV12.cs`;
- migrator: `backend/SanSo.Migrator.V7` (001–014);
- worker: `backend/SanSo.Worker.V2`;
- frontend: V8;
- OpenAPI: `docs/openapi-v12-canonical.json`;
- environment template: `.env.v9.example`.

Latest evidence:

- `docs/phase-33-durable-report-export-v12.md`;
- `docs/completion-audit-2026-08-24-v23.md`;
- backend build 0 warnings/errors;
- backend tests 106/106;
- frontend tests 17/17 and production build successful;
- OpenAPI V12 and 14-migration manifest verified.

Pilot export content uses PostgreSQL bytea. Production object storage/KMS is a documented open decision, not an implied capability.
