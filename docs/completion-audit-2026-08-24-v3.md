# Completion audit V3 — SànSổ Master Prompt

Ngày audit: 2026-08-24  
Nguyên tắc: chỉ đánh dấu `DONE` khi có bằng chứng runtime trực tiếp đúng phạm vi.

## Bằng chứng mới nhất

- `dotnet build SanSo.sln --no-restore`: 0 warnings, 0 errors (trước phase 10).
- `SanSo.Api.Tests`: 37/37 passed.
- `SanSo.Api.V2.Tests`: 11/11 passed.
- `SanSo.Import.Tests`: 10/10 passed sau phase 10.
- Tổng test .NET hiện có: **58**; cần chạy lại toàn solution sau khi nối HTTP confirm.
- Frontend component: 9 passed (lần chạy gần nhất).
- API Playwright: 6 passed (lần chạy gần nhất).
- Browser base: 4 passed; workflow V3: 3 passed (lần chạy gần nhất).
- Docker/PostgreSQL/RLS: chưa có bằng chứng runtime vì Docker daemon không hoạt động.

## Chênh lệch quan trọng còn lại

| Requirement | Trạng thái | Bằng chứng hiện tại | Cần thêm để DONE |
|---|---|---|---|
| Demo first reconciliation | DONE | API + browser E2E | — |
| Repeat import không nhân đôi mọi projection | PARTIAL | checksum, raw/idempotency và two-phase core tests | HTTP confirm → PostgreSQL raw → order/ledger/tax/inventory projection test |
| Settlement drill-down tới raw source | PARTIAL | source refs ở model/API | browser evidence drawer với raw ID chính xác |
| Partial refund/return giữ lịch sử | PARTIAL | domain + API workflow | PostgreSQL posting và cross-period E2E |
| Tax provenance đầy đủ | PARTIAL | no-guess engine/rule model | expert-approved positive golden rule và persisted calculation |
| Locked period immutable | PARTIAL | state-machine/API | PostgreSQL concurrency + amendment E2E |
| Inventory concurrency | PARTIAL | domain/API + serializable SQL source | PostgreSQL concurrent runtime test |
| Tenant isolation | PARTIAL | auth tests + RLS SQL | RLS read/write/export integration matrix |
| Secret/PII absence | PARTIAL | safe errors + synthetic data | executed bundle/log/fixture secret scan |
| Outage/recovery | PARTIAL | retry/DLQ domain tests | worker crash/restart integration |
| Import preview | DONE cho API preview | 3 API tests + 6 parser tests | — |
| Import confirm | PARTIAL | 4 core workflow tests | HTTP, persistent staging, audit, transaction, UI two-step confirmation |
| UI states | PARTIAL | component/browser coverage | no-permission/empty/locked async browser states |
| Local developer path | PARTIAL | README.V2 + scripts | clean-machine Docker/PostgreSQL execution |

## Kết luận

Sản phẩm đã có vertical slices và tài liệu rộng, nhưng **chưa đạt Definition of Done cấp sản phẩm**. Không được mô tả là production-ready hoặc tax filing automation. Các gate khó còn lại tập trung ở PostgreSQL/RLS, persistence của auth/tax/import, background worker recovery và các thao tác nhạy cảm có preview/diff/two-step confirmation.
