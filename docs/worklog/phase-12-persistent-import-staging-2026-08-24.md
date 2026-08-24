# Phase 12 — PostgreSQL import staging (source complete, runtime unverified)

Ngày: 2026-08-24

## Đã thêm

Migration `005_import_confirmation.sql`:

- `import_batches` lưu checksum, hash của preview token, format, template version, expiry và actor xác nhận;
- `import_rows` lưu normalized JSON, lỗi theo dòng và event ID ổn định;
- RLS `USING` + `WITH CHECK` cho cả hai bảng;
- `FORCE ROW LEVEL SECURITY`;
- partial unique index ngăn cùng checksum được confirmed hai lần trong một tenant;
- token chỉ lưu SHA-256, không lưu plaintext;
- quan hệ `ON DELETE RESTRICT` giữ bằng chứng import.

`PostgresImportStore`:

- stage batch + rows trong một transaction;
- confirm dùng `SERIALIZABLE` và `FOR UPDATE`;
- kiểm tra tenant, token hash, checksum, status và expiry;
- insert raw events bằng `ON CONFLICT DO NOTHING`;
- cập nhật actor và thời điểm confirm trong cùng transaction.

## Bằng chứng hiện tại

- Source build: 0 warnings, 0 errors.
- Toàn bộ test .NET hiện tại: 58/58 passed.
- Frontend component tests: 9/9 passed.
- PostgreSQL runtime: chưa chạy vì Docker daemon không tồn tại.

## Rủi ro phải sửa trước khi wiring endpoint

- Query `COUNT(*)` cần được kiểm chứng kiểu trả về Npgsql tại runtime; source hiện dùng typed getter và chưa có DB test.
- Migration cần chạy trên PostgreSQL 16 để kiểm chứng `sha256(bytea)`, RLS, partial unique index và transaction race.
- Store chưa được đăng ký DI và endpoint confirm chưa gọi store.
- Chưa có cleanup job chuyển preview hết hạn sang `EXPIRED`.

Vì các điểm trên, artifact này không được coi là production-ready và không nâng acceptance criterion import lên `DONE`.
