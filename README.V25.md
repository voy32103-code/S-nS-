# SànSổ — Canonical CI V14

Workflow `.github/workflows/ci-v14-canonical.yml` là CI mới nhất:

- PostgreSQL 17 service.
- Build `SanSo.V10.slnx` Release.
- Verify migration manifest 15 files; migrate V8 hai lần.
- Full backend live tests.
- OpenAPI V14 verifier.
- `npm ci`, 17 Vitest, V8 production build, client artifact secret/PII scan.
- Chromium install và 10 Playwright/Axe tests.

Local Playwright dùng Edge; CI dùng Chromium khi `CI=true`. Local evidence: artifact scan pass và 10/10 Edge tests. GitHub-hosted execution vẫn cần một workflow run thật để được đánh dấu verified remotely.

