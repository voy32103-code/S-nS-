# SànSổ — E2E browser gate

Canonical vẫn là solution V10, API V13, migrator V8 và frontend V8. Phase 36 bổ sung browser acceptance thật:

```powershell
cd frontend
npm test
npm run test:e2e
npm run build
```

`npm run test:e2e` dùng Edge hệ thống, tự chạy API Development tại `127.0.0.1:5080` và Vite V8 tại `127.0.0.1:5176`. Identity fixture chỉ tồn tại khi Development và có đủ bốn biến `SANSO_E2E_*`; Production bỏ qua hoàn toàn.

Bằng chứng 2026-08-24: backend 110/110, frontend 17/17, Playwright 6/6, V8 production build thành công.

Chi tiết: `docs/phase-36-playwright-core-journeys.md` và `docs/completion-audit-2026-08-24-v26.md`.

