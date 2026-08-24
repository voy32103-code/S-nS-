# SànSổ — canonical handoff V14

Canonical runtime:

- solution: `SanSo.V7.slnx`;
- API project: `backend/SanSo.Api.V6`;
- compiled entry point: `backend/SanSo.Api.V6/ProgramCanonicalV8.cs`;
- migrator: `backend/SanSo.Migrator.V5` (001–012);
- worker: `backend/SanSo.Worker.V2`;
- frontend: V8;
- OpenAPI: `docs/openapi-v8-canonical.json`.

Latest engineering evidence:

- `docs/phase-30-durable-notification-inbox-v8.md`;
- `docs/completion-audit-2026-08-24-v20.md`;
- `docs/master-prompt-traceability-v1.md` plus V20 deltas.

Verified: backend build 0/0, backend tests 97/97, frontend tests 17/17, frontend production build successful, OpenAPI V8 verified.

This remains a pilot MVP with explicit fail-closed gaps, not a production-ready tax filing or marketplace integration claim.
