# SànSổ canonical frontend V8

Canonical stack sau Phase 35:

- Backend solution: `SanSo.V10.slnx`
- API: `backend/SanSo.Api.V6/ProgramCanonicalV13.cs`
- Migrator: `backend/SanSo.Migrator.V8` (001–015)
- Frontend entrypoint: `frontend/index-v8.html` → `frontend/main-v8.tsx` → `frontend/src/AppV8.tsx`
- Frontend build config: `frontend/vite.v8.config.js`
- OpenAPI: `docs/openapi-v13-canonical.json`

`npm run dev` và `npm run build` hiện đều dùng V8. Microcopy canonical đã được khôi phục UTF-8 tiếng Việt. Kiểm chứng: 17/17 Vitest, Vite V8 production build, HTTP 200, Edge screenshot tạo thành công và rendered DOM chứa đúng “SànSổ”/“Đăng nhập”.

Xem `docs/phase-35-canonical-frontend-v8-utf8.md` và `docs/completion-audit-2026-08-24-v25.md`.

