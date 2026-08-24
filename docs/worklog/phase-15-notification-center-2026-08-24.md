# Phase 15 — Notification Center core và RLS correction

Ngày xác minh: 2026-08-24

## Lỗi schema đã phát hiện và sửa

Migration 004 tạo `notification_deliveries` nhưng bỏ sót RLS. Migration 007 bổ sung theo cách forward-only:

- `ENABLE ROW LEVEL SECURITY`;
- `FORCE ROW LEVEL SECURITY`;
- policy tenant có cả `USING` và `WITH CHECK`;
- dedupe key/type/resource reference;
- unique index theo tenant + dedupe key + channel;
- index cho pending/retry deliveries đến hạn.

Không sửa migration lịch sử để tránh checksum drift trên môi trường đã chạy.

## Notification Center core

Hỗ trợ bốn loại cảnh báo MVP:

- sync failure;
- discrepancy lớn;
- tồn khả dụng thấp;
- kỳ cần review.

Kênh:

- in-app;
- email delivery envelope.

Invariant đã triển khai:

- dedupe theo tenant/type/resource/hour/channel;
- email được mask trước khi lưu delivery view;
- tenant khác không đọc hoặc mutate notification;
- transient failure retry exponential và dead-letter ở attempt 5;
- lỗi provider tự do được map sang safe error code;
- chỉ in-app notification được acknowledge;
- Vietnamese microcopy không chứa raw payload, secret hoặc claim tự động nộp thuế.

## Bằng chứng

```text
Build: 0 warnings, 0 errors
SanSo.Api.Tests: 59/59
SanSo.Api.V2.Tests: 11/11
SanSo.Import.Tests: 10/10
Total .NET: 80/80
```

## Chưa đạt

- Chưa có provider email thật hoặc adapter mock chạy qua hosted worker.
- Chưa persist notification core vào PostgreSQL runtime.
- Chưa có preference/unsubscribe/bounce handling.
- Chưa có API/UI inbox và browser E2E.
- RLS migration 007 chưa chạy trên PostgreSQL do Docker daemon không hoạt động.

Notification requirement chuyển từ schema-only sang executable core + schema correction, nhưng vẫn `PARTIAL` ở cấp sản phẩm.
