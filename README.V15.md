# SànSổ — canonical handoff V15

Canonical stack:

- solution: `SanSo.V7.slnx`;
- API entry point: `backend/SanSo.Api.V6/ProgramCanonicalV9.cs`;
- migrator: `backend/SanSo.Migrator.V5` (001–012);
- worker: `backend/SanSo.Worker.V2`;
- frontend: V8;
- OpenAPI: `docs/openapi-v9-canonical.json`;
- environment template: `.env.v9.example`.

Latest evidence:

- `docs/phase-31-durable-onboarding-http-v9.md`;
- `docs/completion-audit-2026-08-24-v21.md`;
- backend build 0 warnings/errors;
- backend tests 98/98;
- frontend tests 17/17 and production build successful;
- OpenAPI V9 verified.

Production tax/provider/identity claims remain explicitly out of scope until the inputs listed in audit V21 are supplied and verified.
