# Phase 36 — Sáu Playwright core journeys

Ngày kiểm chứng: 2026-08-24.

## Kiến trúc test

- Browser: Microsoft Edge cài sẵn, headless.
- UI: frontend V8 thật tại cổng 5176.
- API: canonical V13 thật tại cổng 5080, Development.
- Test identity: tạo trong memory khi và chỉ khi Development cùng bốn biến `SANSO_E2E_EMAIL`, `SANSO_E2E_PASSWORD`, `SANSO_E2E_TENANT`, `SANSO_E2E_TOTP_SECRET` đều có giá trị.
- MFA: Playwright tính TOTP RFC 6238 từ secret fixture; không có endpoint bypass.
- Network: browser gọi API thật; không dùng `page.route().fulfill()` hoặc mock HTTP.
- Runner separation: Vitest chỉ nhận `src/**/*.test.ts(x)`; Playwright chỉ nhận `e2e`.

## Journey đã qua

1. Owner đăng nhập MFA và thấy reconciliation demo đầu tiên.
2. Credential sai giữ user ngoài authenticated shell và hiện safe error.
3. CSV import bắt buộc preview, checksum, checkbox xác nhận rồi confirm.
4. Owner mở onboarding có thể resume và hoàn tất bước kế tiếp.
5. Controlled tax workflow trả kết quả evidence-first/`NEEDS_REVIEW` thay vì tự chọn thuế suất.
6. Logout thu hồi session phía UI và quay lại login shell.

## Bug product được E2E phát hiện

Canonical route order ưu tiên composition yêu cầu database, làm dashboard trả `DATABASE_REQUIRED` trong Demo Development dù các demo routes tồn tại phía dưới. Ba read route dashboard/orders/current-reconciliation nay được bọc `Prefer(...)` trong V12 source và V13 canonical. Production posture không đổi; write/import persistence vẫn fail-closed theo route tương ứng.

## Bằng chứng regression

- Playwright: 6/6, một worker, 14 giây.
- Vitest: 6 files, 17/17.
- Backend solution: 110/110.
- API build: 0 warning, 0 error.
- V8 production build: thành công, output `dist/index-v8.html`.
- npm audit khi thêm Playwright: 0 vulnerability.

## Giới hạn

Journey demo dùng in-memory DemoStore theo đúng acceptance “không cần credential/provider”. PostgreSQL data-integrity được kiểm chứng riêng bằng live integration suite 110 test ở Phase 34. Chưa có browser journey chạy trên clean PostgreSQL cluster trong cùng Playwright lifecycle; đây vẫn là hardening tiếp theo, không được nhập nhằng với demo E2E.

