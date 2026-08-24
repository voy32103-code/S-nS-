# SànSổ — canonical handoff V10

- Solution: `SanSo.V6.slnx`
- API: `backend/SanSo.Api.V6`
- Worker: `backend/SanSo.Worker.V2`
- Migrator: `backend/SanSo.Migrator.V4` (001–011)
- Frontend: `frontend/index-v8.html`
- CI: `.github/workflows/ci-v12-canonical.yml`

Inventory Get/Reserve/Release is durable and live-verified. See `docs/phase-26-inventory-lifecycle-api-v6.md` and `docs/completion-audit-2026-08-24-v16.md`.

The next implementation decision requires choosing a production identity provider/storage model. No provider credential or Vietnamese tax rule is invented by this project.

