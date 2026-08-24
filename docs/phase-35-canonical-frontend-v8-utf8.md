# Phase 35 — Canonical frontend V8 và UTF-8 tiếng Việt

Ngày kiểm chứng: 2026-08-24.

## Sai lệch được phát hiện

- Tài liệu gọi frontend V8 là canonical, nhưng `package.json` vẫn chạy/build `vite.v3.config.js` và tạo bundle `index-v3.html`/`AppV3`.
- AppV8 cùng ImportV8, OnboardingV8, WorkflowV9 và tiêu đề HTML chứa mojibake tiếng Việt.
- `npm test` trước đó không chọn Vitest config nên test React chạy Node environment và báo `document is not defined`.
- Production TypeScript build gồm test sources; hai component cũ dùng effect callback trả Promise.

## Thay đổi

- `dev` và `build` chuyển sang `vite.v8.config.js`.
- `test` dùng `vitest.v8.config.ts` (jsdom và setup canonical).
- Production TypeScript loại `*.test.ts/tsx`; effect async cũ dùng `void load()`.
- Khôi phục microcopy V8 bằng mapping đảo UTF-8 bị decode nhầm cp1258/cp1252; mapping chỉ thay chuỗi mojibake và giữ nguyên Unicode vốn đúng.
- Script tái lập được giữ tại `scripts/promote-and-repair-frontend-v8-v5.ps1`; bốn thử nghiệm thất bại đã bị xóa.

## Bằng chứng

- Vitest: 6 files, 17/17 tests pass.
- Production build chạy chính xác `vite build --config vite.v8.config.js`.
- Output: `dist/index-v8.html`, CSS V8 và JS V8.
- Edge headless:
  - HTTP 200;
  - screenshot 251.155 byte, exit 0;
  - rendered DOM 5.657 ký tự;
  - kiểm tra Unicode runtime: `brand=true`, `login=true`.
- 17 artifact browser/log/profile tạm đã được dọn sau kiểm chứng.

## Tác động traceability

Acceptance UI không còn dựa trên bundle V3 để chứng minh V8. Bằng chứng hiện gắn trực tiếp từ package script → Vite V8 config → index-v8 → main-v8 → AppV8 → browser-rendered DOM.

