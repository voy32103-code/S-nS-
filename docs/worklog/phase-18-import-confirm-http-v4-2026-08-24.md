# Phase 18 — Import confirm HTTP V4

Ngày xác minh: 2026-08-24

## API V4

API V4 giữ auth/tenant middleware, module endpoints, dashboard/orders/reconciliation và thêm flow import hai pha:

- preview kiểm tra multipart, size, extension/MIME, UTF-8/CSV/XLSX;
- response có batch/token/expiry/checksum/rows/errors và cờ `persisted`;
- confirm bắt buộc token + checksum;
- token gắn tenant và chỉ dùng một lần;
- checksum sai không tiêu thụ token;
- development trả `persisted=false` rõ ràng;
- production yêu cầu DB + authenticated principal trước store operation.

`PostgresImportStagingStore` dùng bảng V2, transaction SERIALIZABLE, row lock, hash token, checksum check, duplicate confirmed checksum guard, idempotent raw-event insert và actor/timestamp confirmation.

## HTTP tests

1. Preview → confirm → token reuse rejected.
2. Checksum tamper rejected; token đúng vẫn dùng được sau đó.
3. Cross-tenant confirm rejected.
4. Production anonymous import rejected before database access.

Kết quả API V4 tests: **4/4 passed**.

## Chưa đạt

- PostgreSQL store chưa chạy runtime.
- UI chưa hiển thị diff/checkbox/two-step confirmation.
- API V4 chưa có onboarding/notification endpoints.
- Raw events sau confirm chưa có worker projection integration test.

Flow HTTP core được coi là executable; acceptance criterion import end-to-end vẫn `PARTIAL` cho tới khi PostgreSQL/projection/UI E2E qua.
