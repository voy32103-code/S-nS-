# Phase 22 — Tenant-scoped durable outbox worker core

Ngày: 2026-08-24

## Worker architecture

Project executable: `backend/SanSo.Worker`.

- Bắt buộc `SANSO_POSTGRES` và `SANSO_WORKER_TENANT`.
- Mỗi process worker chỉ xử lý một tenant và luôn set `app.current_organization_id`.
- Không dùng BYPASS RLS hoặc service role đọc mọi tenant.
- Claim một row bằng transaction + `FOR UPDATE SKIP LOCKED`.
- Khi claim: tăng attempt, đặt status `PROCESSING`, dùng `next_attempt_at` làm lease expiry.
- Startup recovery chuyển lease PROCESSING hết hạn sang RETRY_SCHEDULED.
- Success chỉ complete row đang giữ lease.
- Transient failure dùng exponential retry, tối đa 900 giây.
- Attempt 5 hoặc lỗi non-transient chuyển DEAD_LETTER.
- Exception tự do được map sang safe code, không lưu message/payload.

## Fail-closed adapters

Pilot handler chỉ cho `NOOP_AUDIT`. Shopee, TikTok và email chưa có credential/provider hợp pháp sẽ dead-letter với safe `*_NOT_CONFIGURED`; worker không giả gọi partner hoặc gửi email.

## Schema

Migration 009:

- thêm `error_code`;
- index tenant/status/due/created;
- mô tả rõ lease semantics của `next_attempt_at`.

Migration manifest authoritative hiện là 001–009.

## Executable tests

1. Startup recover expired leases.
2. Success complete exactly once.
3. Transient retry và dead-letter ở attempt 5.
4. External adapter chưa cấu hình fail closed, không gọi network.
5. Unexpected exception lưu safe code, không lưu secret message.

Worker tests: **5/5 passed**.

## Chưa đạt

- PostgreSQL crash/restart integration chưa chạy.
- Chưa có real connector/email handler.
- Chưa có multiple-tenant scheduler/orchestrator.
- Chưa nối OperationalTelemetry/OTLP exporter vào worker executable.
- Chưa có poison-payload quarantine UI/replay approval.

Worker core là executable và testable, nhưng outage/recovery acceptance vẫn `PARTIAL` tới khi DB runtime được chứng minh.
