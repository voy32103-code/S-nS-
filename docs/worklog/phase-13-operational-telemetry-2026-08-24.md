# Phase 13 — Operational telemetry và worker execution boundary

Ngày xác minh: 2026-08-24

## Đã triển khai

`OperationalTelemetry` dùng API chuẩn .NET `Meter` và `ActivitySource`:

- counter `sanso.sync.attempts`;
- counter `sanso.raw.events`;
- histogram `sanso.sync.duration` theo milliseconds;
- observable gauge `sanso.sync.queue.depth`;
- trace `sync.execute` với correlation ID đã validate;
- tenant chỉ xuất hiện trong trace dưới fingerprint SHA-256 rút gọn 12 ký tự;
- metric tuyệt đối không có tenant tag;
- job/channel/outcome/error được normalize vào allowlist hữu hạn, giá trị lạ thành `other`.

`InstrumentedSyncExecutor`:

- tạo outbox work trước khi gọi handler;
- trace toàn execution;
- success cập nhật health `HEALTHY`;
- lỗi transient chuyển `RetryScheduled`;
- token revoke chuyển `Paused`, tắt write-back và tạo alert;
- metric chỉ nhận error class an toàn, không nhận payload hoặc free-form exception message.

## Bằng chứng test

- Metric không chứa tenant hoặc free-form tags.
- Trace chứa fingerprint, không chứa tenant plaintext.
- Correlation ID không hợp lệ được thay bằng `invalid`.
- Fingerprint ổn định và khác nhau giữa tenant.
- Success, transient retry và revoked-token pause paths.
- Backend suite được chạy 3 lần liên tiếp sau khi tắt test parallelism cho global Meter listener: 43/43 mỗi lần.
- Full solution: `43 + 11 + 10 = 64/64` tests passed.
- Build gần nhất: 0 warnings, 0 errors.

## Chưa đạt production observability

- Chưa đăng ký exporter OTLP/Prometheus.
- Chưa nối executor vào hosted background worker thực tế.
- Chưa có dashboard/alert rule runtime và chưa đo SLO trên môi trường deploy.
- Queue depth hiện nhận từ caller, chưa đọc PostgreSQL outbox.

Vì vậy hạng mục observability chuyển từ `design-only` sang `instrumentation core implemented`, nhưng vẫn `PARTIAL` ở cấp sản phẩm.
