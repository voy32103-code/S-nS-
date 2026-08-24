# Completion audit — Master Prompt SànSổ

**As of:** 2026-08-24  
**Mode:** `MVP_BUILD`  
**Nguyên tắc:** chỉ đánh dấu `DONE` khi có bằng chứng trực tiếp từ source, test hoặc runtime. `PARTIAL` không được tính là hoàn thành.

## Tổng quan

| Nhóm | Trạng thái | Bằng chứng hiện tại | Gap chính |
|---|---|---|---|
| Vertical slice dashboard/reconciliation | PARTIAL | `frontend/src/App.tsx`, `DemoStore.cs`, 4 tests pass | Chưa có raw source UI/API, resolve/audit workflow |
| Identity, organization, RBAC | NOT STARTED | Chỉ có demo tenant header | Auth thật, membership, session revoke, permission tests |
| Integration/raw ingestion | PARTIAL | Demo seed + source key | Immutable raw payload/checksum, CSV/XLSX, inbox/outbox, retries |
| Orders/ledger/settlement | PARTIAL | In-memory canonical records | PostgreSQL, adjustments/refunds, period lock, multi-settlement |
| Tax Center | NOT STARTED | Chỉ disclaimer/status | Profile, versioned rules, period workflow, export, golden tests |
| Inventory | NOT STARTED | Không có implementation | SKU map, ledger, ATP, reservation concurrency, oversell guard |
| Alerts/notifications | NOT STARTED | Không có implementation | In-app/email, degraded/token/rate-limit alerts |
| Reports/export | NOT STARTED | Không có implementation | CSV/XLSX data pack, provenance metadata, formula protection |
| Billing/entitlements | NOT STARTED | Không có implementation | Trial, plans, subscription state, feature/volume limits |
| Admin/support | NOT STARTED | Không có implementation | Consent, time-bound access, masking, audit |
| Persistence/migrations | NOT STARTED | Docker services only | EF Core/Npgsql model, migrations, seed |
| Security/privacy | PARTIAL | CORS allowlist + tenant-filtered in-memory queries | Authentication, encryption, rate limit, upload/webhook defenses, scans |
| Observability/reliability | NOT STARTED | Health endpoint | Structured telemetry, correlation, metrics, retry/DLQ, runbooks |
| Test strategy | PARTIAL | 3 xUnit + 1 Vitest | Integration, contract, golden, security, invariant, Playwright E2E |
| 12 mandatory demo scenarios | PARTIAL | Difference + duplicate + tenant isolation | 9+ scenarios chưa có seed/test |
| Required design outputs | PARTIAL | `product-blueprint.md` | Nhiều artifact còn quá tóm tắt/chưa tách thành tài liệu kiểm chứng được |
| Legal/market verification | BLOCKED BY EVIDENCE | URL trong prompt không truy xuất được ở lần kiểm tra đầu | Cần nguồn chính thức truy cập được; không publish tax rule |

## Acceptance criteria audit

| # | Tiêu chí | Trạng thái | Bằng chứng / việc còn thiếu |
|---:|---|---|---|
| 1 | Demo không cần credential và thấy reconciliation đầu tiên | DONE | Demo seed + dashboard/API smoke test |
| 2 | Re-import không nhân đôi order/money/tax/inventory | PARTIAL | Order/money test pass; tax/inventory chưa có |
| 3 | Settlement drill-down đến order và raw source | PARTIAL | Đến order/ledger line; chưa có raw source store/view |
| 4 | Expected payout/difference giải thích từng dòng | DONE | Reconciliation API trả `lines` và UI render |
| 5 | Partial refund/return adjustment không xóa lịch sử | NOT STARTED | Chưa có model/test |
| 6 | Tax result có version/source/effective date/explanation | NOT STARTED | Chưa có calculation entity |
| 7 | Thiếu tax data tạo exception | PARTIAL | UI status hard-coded; chưa có engine/entity/test |
| 8 | Locked period không mutate âm thầm | NOT STARTED | Chưa có period workflow |
| 9 | Inventory invariant và concurrent oversell guard | NOT STARTED | Chưa có inventory |
| 10 | Cross-tenant read/write/export bị chặn | PARTIAL | Store query test; chưa có auth/API/export tests |
| 11 | Không leak token/secret/PII | PARTIAL | Demo không chứa secret; chưa có automated scans/log tests |
| 12 | Outage không mất raw data, có retry/recovery | NOT STARTED | Chưa có raw store/outbox/retry |
| 13 | Export có trace metadata | NOT STARTED | Chưa có export |
| 14 | E2E/integration/golden/tenant tests pass | NOT STARTED | Chỉ unit tests |
| 15 | UI empty/loading/error/degraded/no-permission | PARTIAL | Loading/error có; thiếu empty/degraded/no-permission đầy đủ |
| 16 | Không có claim pháp lý/tích hợp chưa chứng minh | DONE | Disclaimer + production gaps; không có tax rate/partner claim |
| 17 | README chạy local rõ ràng | PARTIAL | Có hướng dẫn; cần DB migration/seed/demo/E2E hoàn chỉnh |

## Required output audit (Section 26)

1. Executive summary/product thesis — PARTIAL.
2. Assumptions/unknowns/decision log — PARTIAL.
3. Legal applicability matrix — PARTIAL, chưa xác minh nguồn.
4. Market validation/TAM-SAM-SOM scenarios — PARTIAL, chưa có mô hình số/nguồn.
5. Personas/JTBD/pain points — PARTIAL.
6. PRD functional/non-functional — PARTIAL.
7. MVP/P1/P2/non-goals/dependencies — PARTIAL.
8. User journeys/state machines — PARTIAL.
9. Sitemap/IA/screen specification — PARTIAL.
10. Design tokens/components/Vietnamese microcopy — PARTIAL.
11. Architecture/modules/data flow — PARTIAL.
12. ERD/data dictionary — PARTIAL.
13. OpenAPI/API traceability — PARTIAL; runtime OpenAPI exists nhưng spec chưa version-controlled.
14. Event/job catalog — PARTIAL.
15. Integration contracts/fallback — PARTIAL.
16. Tax rule engine spec/sample rule without fabricated rate — PARTIAL.
17. Security threat model/privacy inventory — PARTIAL.
18. Test/acceptance/golden plan — PARTIAL.
19. Observability/SLO/runbooks — PARTIAL.
20. Pricing/unit economics/pilot — PARTIAL.
21. Prioritized backlog estimates/dependencies/AC — PARTIAL.
22. Source/migrations/seed/tests/Docker/CI/env/README/demo — PARTIAL; migrations thiếu.

## Definition of completion

Goal chỉ hoàn thành khi toàn bộ mục trong bảng trên chuyển thành `DONE`, build/test/E2E pass từ clean setup, và các claim pháp lý/thị trường có bằng chứng hoặc được loại khỏi product rules/marketing một cách rõ ràng.
