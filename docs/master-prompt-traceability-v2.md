# Master prompt traceability — V2

Ngày audit: 2026-08-24. Nguồn: `D:\Downloads\Master_Prompt_Ecom_Ops_Tax_Automation_VN.md`.

V2 cập nhật bằng chứng sau OpenAPI V13, settlement preview/confirm và canonical frontend V8. Quy tắc: bằng chứng gián tiếp hoặc chưa chạy không được tính là hoàn tất.

## Output bắt buộc, mục 26

| # | Output | Bằng chứng canonical | Trạng thái |
|---:|---|---|---|
| 1–12 | Product thesis đến ERD/data dictionary | `product-blueprint.md`, `prd.md`, `ux-specification.md`, `architecture-and-data.md` | Đạt ở phạm vi MVP |
| 13 | OpenAPI/API traceability | `openapi-v13-canonical.json`, verifier V13: 39 paths/134 refs/28 routes | Đạt cho API implemented |
| 14–15 | Event/jobs, integration/fallback | `integrations-and-events.md`, Worker V2, file adapters | Đạt fallback; provider thật chờ credentials |
| 16 | Tax rule spec | `tax-rule-engine.md` | Spec đạt; production rules/legal sign-off còn thiếu |
| 17 | Threat/privacy | `security-and-privacy.md`, RLS/encryption/safe errors tests | Đạt thiết kế và controls đã implement; deployment controls còn thiếu |
| 18 | Test/golden | `test-strategy.md`, 110 backend + 17 frontend | Tax golden do chuyên gia duyệt và 6 E2E journeys còn thiếu |
| 19 | Observability/runbooks | `operations-and-runbooks.md` | Đạt thiết kế/MVP; production drills còn thiếu |
| 20 | Pricing/pilot | `pricing-pilot-and-risks.md` | Hypothesis đạt; interviews/WTP còn thiếu |
| 21 | Backlog | `prioritized-backlog.md` | Đạt |
| 22 | Code/migrations/seed/tests/Docker/CI/env/README/demo | solution V10, migrator V8, README V19, CI/Docker files | Code/build/test đạt; Docker runtime chưa chứng minh |

## Acceptance mục 27 — cập nhật trọng yếu

| # | Criterion | Bằng chứng hiện tại | Verdict |
|---:|---|---|---|
| 1 | Demo đến reconciliation đầu tiên không cần credential | Demo/onboarding/API/UI unit; V8 login browser smoke | Một phần: full browser journey chưa có |
| 2 | Re-import không nhân đôi | PostgreSQL live import/projection/settlement confirm retry | Đạt cho các path implemented |
| 3–4 | Settlement drill-down và giải thích từng dòng | line-trace importer, raw/ledger/reconciliation HTTP tests | Đạt backend/API; UI detail đầy đủ còn thiếu |
| 5 | Partial refund/return giữ lịch sử | lifecycle tests | Đạt domain/storage |
| 6–7 | Tax evidence, missing→review | deterministic engine/spec/tests | Cấu trúc đạt; approved rules/golden chưa có |
| 8–9 | Locked period, inventory invariant/concurrency | lifecycle và PostgreSQL tests | Đạt phạm vi implemented |
| 10 | Cross-tenant isolation | forced RLS và authorization/export tests | Đạt covered surfaces; audit lại khi thêm table/route |
| 11 | Không lộ secret/PII | encryption, masking, safe middleware, scans | Một phần: production log pipeline chưa có |
| 12 | Outage/raw recovery | outbox/import worker live tests | Đạt implemented path |
| 13 | Export metadata trace | durable report preview/checksum/download tests | Đạt CSV pilot; production object storage/XLSX pack còn thiếu |
| 14 | Core E2E/integration/golden/tenant pass | 110 backend live, 17 frontend, browser login smoke | Chưa đạt: 6 E2E và expert tax golden |
| 15 | UI states | V8 tests và canonical browser render | Đạt tested screens; accessibility audit còn thiếu |
| 16 | Không claim pháp lý/tích hợp chưa chứng minh | fail-closed copy/adapters/audits | Đạt current artifacts |
| 17 | README chạy local | README V19 và scripts | Một phần: Docker one-command chưa runtime proof |

## Inputs bắt buộc không được suy đoán

1. IdP production và cấu hình tenant/application.
2. Legal/tax approval, source-to-rule mapping và golden expected outcomes.
3. Marketplace/email/Zalo/billing sandbox credentials và scopes.
4. Secret manager/KMS cùng field key/version production.
5. Pilot interviews và willingness-to-pay.

