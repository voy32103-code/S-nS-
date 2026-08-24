# Phase 40 — Traceable XLSX report V14

Ngày kiểm chứng: 2026-08-24.

## Thay đổi

- `PostgresReportExportStoreV2` giữ CSV V1 và thêm `RECONCILIATION_XLSX`.
- Request preview nhận `type`; thiếu type mặc định CSV để tương thích client cũ.
- XLSX OpenXML gồm:
  - `Metadata`: tenant, run, settlement, status, expected/actual/difference, input checksum, rule versions, generated timestamp;
  - `Reconciliation`: source line/order, type, expected/actual/difference, reason, ledger key, raw source event.
- Text được ghi bằng `InlineString`; money bằng numeric cell; không tạo `CellFormula`.
- XLSX dùng cùng bảng `exports`, RLS, TTL, content checksum, preview/confirm, step-up, audit và download count như CSV.
- Catalog công bố hai type; OpenAPI V14 công bố request enum và hai MIME download.

## Defense-in-depth

- CSV importer chặn source field bắt đầu `=`, `+`, `-`, `@` trước khi vào ledger (`SOURCE_LINE_ID_FORMULA_NOT_ALLOWED`).
- XLSX exporter luôn dùng string cell cho source text.
- Live test mở workbook sau download và khẳng định không có `CellFormula`.

## Bằng chứng

- PostgreSQL 16 cô lập cổng 55442, migration 001–015 apply thành công.
- CSV export HTTP lifecycle: pass.
- XLSX preview/confirm/download/checksum/sheets/formula-free: pass.
- Full backend live: 112/112.
- OpenAPI V14: 39 paths, 134 refs, 2 export types, direct route public=false.
- Solution build: 0 warning, 0 error.
- Browser/API V14: 10/10 Playwright/Axe.
- Frontend: 17/17 Vitest; V8 production build thành công.
- Cluster test và browser results tạm đã được dọn.

## Giới hạn

Pilot vẫn lưu content bytes trong PostgreSQL. Production scale cần object storage, KMS, retention/legal hold và immutable audit sink đã được chọn. XLSX hiện là reconciliation pack; tax period pack và accounting mapping chưa được triển khai.

