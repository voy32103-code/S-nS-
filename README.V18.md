# SànSổ V13 — Settlement preview/confirm CSV và XLSX

Canonical hiện tại:

- Solution: `SanSo.V10.slnx`
- API: `backend/SanSo.Api.V6/ProgramCanonicalV13.cs`
- Migrator: `backend/SanSo.Migrator.V8` (migration 001–015)
- Worker: `backend/SanSo.Worker.V2`
- Frontend: V8
- OpenAPI: `docs/openapi-v13-canonical.json`

V13 chuyển import settlement sang quy trình hai bước: upload CSV/XLSX để xem trước, sau đó xác nhận bằng token và checksum. Preview không ghi settlement, raw event, ledger hoặc reconciliation. Token chỉ được lưu dưới dạng SHA-256; preview hết hạn sau 30 phút. Endpoint import trực tiếp chỉ được đăng ký trong Development và không có trong OpenAPI public.

Kiểm chứng ngày 2026-08-24:

- PostgreSQL 16 cô lập: migration 001–015 `APPLIED`, lượt hai 001–015 `SKIP`.
- Backend live: 110/110 test qua.
- Build `SanSo.V10.slnx`: 0 warning, 0 error.
- Frontend: 17/17 test qua; production build thành công.
- OpenAPI V13: 39 paths, 134 refs, 28 source routes, không public route `/direct`.

Chi tiết: `docs/phase-34-settlement-preview-confirm-v13.md` và `docs/completion-audit-2026-08-24-v24.md`.

