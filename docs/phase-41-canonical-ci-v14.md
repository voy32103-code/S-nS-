# Phase 41 — Canonical CI V14

Ngày cấu hình/kiểm chứng local: 2026-08-24.

## Vấn đề cũ

CI V12 build solution V6, chạy migrator V4, verify OpenAPI V4 và gọi một E2E workspace cũ. Nó không chứng minh canonical V14/V8 hiện tại.

## Workflow V14

1. PostgreSQL 17 Alpine health-checked service.
2. .NET 9 và Node 22 setup.
3. Release build solution V10.
4. Manifest 001–015; migrator V8 chạy hai lượt apply/skip.
5. Backend tests với `SANSO_RUNTIME_POSTGRES`.
6. OpenAPI V14 verifier.
7. Locked frontend install, Vitest và V8 build.
8. Bundle/log secret/PII scan sau build.
9. Playwright Chromium install và 10 browser/accessibility tests.

Playwright config chọn Edge khi local, không khai báo channel khi `CI=true` để dùng Chromium bundled.

## Bằng chứng local

- Client artifact scan: 3 files, 6 rules, pass.
- Playwright local Edge sau channel change: 10/10 pass.
- Workflow source inspection xác nhận đúng solution/migrator/OpenAPI/frontend commands.

## Giới hạn bằng chứng

Workspace không có Git repository/remote trong phiên này, nên chưa thể push hoặc quan sát một GitHub Actions run. Vì vậy CI V14 là “configured + locally validated commands”, chưa phải “remote workflow green”. Không dùng source inspection để thay cho run remote.

