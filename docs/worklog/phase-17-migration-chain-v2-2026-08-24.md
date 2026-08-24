# Phase 17 — Authoritative migration chain V2

Ngày: 2026-08-24

## Vì sao cần V2

Audit manifest phát hiện chain cũ 005–007 không chạy được:

- 001 đã tạo `import_batches`, trong khi 005 lại `CREATE TABLE import_batches`;
- 006 tham chiếu bảng không tồn tại `reconciliations`, tên đúng là `reconciliation_runs`;
- 002 chỉ bật RLS cho 6 bảng và không `FORCE RLS`;
- nhiều tenant tables quan trọng như raw events, products, reconciliation, tax calculations, alerts, subscriptions và audit logs chưa có policy.

Không sửa migration đã công bố vì sẽ phá checksum. Không dùng rename hack vì tạo hai khái niệm mơ hồ và FK khó kiểm soát.

## Chain V2 authoritative

Project: `backend/SanSo.Migrator.V2`.

- 001–004: reuse baseline/seed/workflow hiện có.
- 005: bảng riêng `import_staging_batches` và `import_staging_rows`, không collision legacy import batch.
- 006: onboarding FK đúng `reconciliation_runs`.
- 007: notification delivery RLS/index.
- 008: enable + force RLS trên toàn bộ tenant tables; bổ sung policy còn thiếu.

V2 dùng advisory lock và bảng checksum riêng `schema_migrations_v2`. Mỗi migration chạy transactionally và checksum drift làm fail closed.

## Verifier

`scripts/verify-migration-manifest-v2.ps1` kiểm tra binary output:

- đúng 8 version liên tục;
- có staging table tên riêng;
- FK onboarding đúng;
- có notification/RLS guards;
- không tái tạo legacy `import_batches`;
- không chứa FK sai `reconciliations`.

## Trạng thái

Chain cũ được coi là **superseded / không dùng**. V2 là chain duy nhất được phép dùng cho clean environment mới.

PostgreSQL execution vẫn `UNVERIFIED` cho tới khi daemon chạy; verifier chỉ chứng minh manifest và static safety guards.
