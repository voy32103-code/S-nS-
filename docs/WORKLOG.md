# SànSổ implementation worklog

Tài liệu này ghi lại những gì đã thực hiện, quyết định, bằng chứng kiểm thử và việc còn lại theo Master Prompt.

## 2026-08-24 — Phase 0: Intake và design gate

- Đọc Master Prompt ở `MODE = MVP_BUILD`.
- Chọn modular monolith, React/TypeScript/Vite + ASP.NET Core, PostgreSQL/Redis qua Docker.
- Quyết định tiền VND dùng số nguyên `long`; ledger hướng append-only; mọi aggregate có tenant boundary.
- Không nhúng thuế suất hoặc tuyên bố integration chưa xác minh.
- Tạo `docs/product-blueprint.md` với product thesis, assumption, legal caveat, journeys, architecture, ERD, API traceability, security/test/pricing/backlog ở mức khởi đầu.

## 2026-08-24 — Phase 1: Initial vertical slice

- Tạo ASP.NET Core API: health, dashboard, orders, current reconciliation, idempotent demo import.
- Tạo in-memory demo store cho hai đơn, ledger lines, settlement lệch 30.000 VND.
- Tạo React dashboard tiếng Việt với money cards, settlement bridge, line drill-down, tax disclaimer và loading/error states.
- Tạo Docker Compose PostgreSQL/Redis, CI, `.env.example`, README.
- Tạo test cho reconciliation arithmetic, re-import idempotency và tenant filtering; frontend money invariant.
- Bằng chứng: backend 3/3 test pass; frontend 1/1 pass; production build pass; `/health`, dashboard frontend và reconciliation API trả HTTP 200.

## 2026-08-24 — Completion audit

- Đối chiếu lại toàn bộ Master Prompt.
- Xác nhận bản Phase 1 mới là partial vertical slice, chưa phải toàn bộ MVP.
- Tạo `docs/implementation-status.md` với gap theo module, 17 acceptance criteria và 22 output bắt buộc.

## Next

- Phase 2: PostgreSQL persistence, migrations, raw ingestion/inbox/outbox, domain entities và seed 12 scenarios.
- Phase 3: auth/organization/RBAC/audit và security boundaries.
- Phase 4: Tax Center, inventory, alerts, reports/export, billing/entitlements.
- Phase 5: frontend workflows và đầy đủ UX states.
- Phase 6: integration/golden/security/E2E, observability/runbooks và completion audit cuối.
