# Completion audit V7 — Migration integrity correction

Ngày: 2026-08-24

## Critical corrections

Audit runtime packaging/static SQL đã bác bỏ chain migration cũ:

- old 005 collision với `import_batches` từ 001;
- old 006 FK tới bảng không tồn tại;
- RLS coverage không đầy đủ;
- table owner bypass vì thiếu FORCE RLS.

Không có claim migration thành công nào từ V1 được giữ lại.

## Authoritative gate

| Artifact/gate | Result |
|---|---|
| Solution | `SanSo.V2.slnx` |
| Migrator | `SanSo.Migrator.V2` |
| Migration manifest | PASS — 001–008 liên tục |
| Static collision/FK/RLS verifier | PASS |
| Build | PASS — 0 warnings, 0 errors |
| .NET tests | PASS — 80/80 |
| PostgreSQL execution | UNVERIFIED |

## RLS V2 scope

Migration 008 enable + force RLS cho organization/membership/profile, integration/import/raw/inbox/outbox, catalog/order/ledger/settlement/reconciliation, tax, inventory, alerts, subscription/export/audit, workflow security, notification, import staging và onboarding.

Global tables `users`, `plans` và `tax_rule_versions` không dùng tenant RLS theo thiết kế; quyền truy cập chúng vẫn phải được giới hạn qua service role/repository authorization.

## Verdict

Static migration integrity đã tốt hơn đáng kể, nhưng tenant isolation vẫn `PARTIAL` vì chưa chạy PostgreSQL bằng non-owner application role với hai tenant sessions. V7 không chứng minh runtime RLS, migration rollback/restore hoặc concurrency.

Developer phải dùng [README.V3.md](D:/WebAppCodex/README.V3.md) và không dùng migrator cũ cho database mới.
