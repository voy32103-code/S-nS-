# Phase 14 — Onboarding state machine và bảo vệ field nhạy cảm

Ngày xác minh: 2026-08-24

## Onboarding core

Đã triển khai state machine theo 7 bước trong master prompt:

1. Hồ sơ chủ thể kinh doanh.
2. Chọn demo/CSV/XLSX/official API pending authorization.
3. Chọn khoảng backfill tối đa 2 năm cho MVP.
4. Xác nhận mapping SKU.
5. Nhập tồn và giá vốn đầu kỳ.
6. Xác nhận disclaimer có version và timestamp.
7. Hoàn thành lần đối soát đầu tiên.

Trạng thái chỉ chuyển sang `Completed` khi lần đối soát đầu tiên có ít nhất một kết quả matched hoặc discrepancy đã được giải thích, đúng activation event của PRD.

Các invariant:

- không được bỏ qua bước;
- draft cách ly theo tenant;
- mã số thuế chỉ kiểm tra định dạng 10/13 chữ số, không tuyên bố hợp lệ pháp lý;
- MVP chỉ nhận VND và `Asia/Ho_Chi_Minh`;
- không nhận backfill tương lai/quá phạm vi;
- tồn và unit cost không âm;
- SKU canonical không trùng không phân biệt hoa thường;
- disclaimer phải được xác nhận rõ ràng và có version.

## Field-level protection

Đã triển khai AES-256-GCM cho mã số thuế/địa chỉ:

- nonce 96-bit ngẫu nhiên cho mỗi lần encrypt;
- authentication tag 128-bit;
- tenant, purpose và key version là authenticated associated data;
- ciphertext của tenant/purpose khác không giải mã được;
- serialized format dùng Base64URL;
- lỗi định dạng/tamper/authentication được chuẩn hoá thành `CryptographicException` an toàn;
- plaintext buffer được zero sau encrypt/decrypt;
- migration chỉ lưu protected value, last4 và key version.

Migration `006_onboarding_profiles.sql` có RLS + FORCE RLS, database checks cho step/currency/timezone/disclaimer/completion và index cho draft chưa hoàn tất.

## Bằng chứng

```text
dotnet build SanSo.sln --no-restore
Build succeeded, 0 warnings, 0 errors

dotnet test SanSo.sln --no-build --no-restore
SanSo.Api.Tests       54/54
SanSo.Api.V2.Tests    11/11
SanSo.Import.Tests    10/10
Total                 75/75
```

## Chưa đạt

- Chưa có API/UI wizard gọi state machine.
- Chưa có PostgreSQL onboarding store thực thi migration 006.
- Chưa nối key material từ production secret manager/KMS và chưa có rotation job.
- Chưa có audit record actor/IP/request ID cho từng bước.
- Chưa có authenticated browser E2E cho onboarding → first reconciliation.

Do đó onboarding chuyển từ `design-only` sang `domain/security core implemented`, nhưng vẫn `PARTIAL` ở cấp MVP.
