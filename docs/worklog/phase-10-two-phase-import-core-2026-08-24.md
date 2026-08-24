# Phase 10 — Lõi import hai pha

Ngày xác minh: 2026-08-24

## Mục tiêu

Tách import thành hai thao tác có chủ ý:

1. `preview`: đọc và kiểm tra tệp nhưng không ghi dữ liệu nghiệp vụ.
2. `confirm`: người dùng xác nhận đúng checksum của preview trước khi sinh raw event.

## Đã triển khai và chứng minh

- Preview token ngẫu nhiên 256-bit, hết hạn sau 30 phút.
- Token gắn cứng với tenant; tenant khác bị từ chối.
- Confirm bắt buộc checksum khớp preview.
- Token bị tiêu thụ một lần; không thể confirm lại.
- Chỉ dòng không có validation error mới sinh `ORDER_IMPORTED` raw-event envelope.
- Event ID ổn định theo `file:{checksum}:row:{rowNumber}`.
- Cùng checksum được confirm lại trong cùng tenant trả `Duplicate=true` và không sinh event lần hai.
- Payload dùng JSON có cấu trúc, giữ raw mapping và giá trị chuẩn hoá.

## Bằng chứng

```powershell
dotnet test backend/SanSo.Import.Tests/SanSo.Import.Tests.csproj --no-restore
```

Kết quả: **10/10 passed**.

Các test mới chứng minh:

- valid-only event generation;
- checksum tamper rejection;
- one-time preview token;
- cross-tenant confirmation rejection;
- repeated committed checksum idempotency.

## Chưa đạt

- Core workflow chưa được expose thành endpoint HTTP confirm.
- Pending/committed state hiện ở RAM; restart sẽ mất trạng thái.
- Raw event envelopes chưa được ghi transactionally vào PostgreSQL.
- Chưa có audit record chứa actor, preview checksum và confirmation timestamp.
- Chưa có UI diff + checkbox + step-up confirmation.

Do đó phase này là **core complete, production workflow incomplete**. Không được xem là bằng chứng thỏa acceptance criterion import end-to-end.
