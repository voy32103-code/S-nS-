# Phase 34 — Settlement preview/confirm V13

Ngày kiểm chứng: 2026-08-24.

## Phạm vi đã triển khai

- Migration `015_settlement_import_previews.sql` tạo preview bền vững, RLS bắt buộc theo organization, token hash, payload chuẩn hóa, trạng thái, thời hạn và reconciliation run đã xác nhận.
- `SettlementFileParserV1` nhận CSV UTF-8 và XLSX OpenXML; giới hạn 10 MiB, 10.000 data rows, 20 cột; từ chối formula; giữ đúng vị trí cột khi workbook bỏ qua ô trống.
- `PostgresSettlementImportWorkflowV1.Stage` chỉ lưu preview. Không ghi `import_batches`, `settlements`, `raw_events`, `ledger_lines` hoặc `reconciliation_runs`.
- `Confirm` yêu cầu đồng thời token và checksum, kiểm tra TTL, gọi importer idempotent và đánh dấu preview đã xác nhận.
- Token ngẫu nhiên 256 bit chỉ lưu SHA-256; plaintext chỉ trả cho client ở lần preview chưa confirm.
- Preview lại cùng checksum đã confirm trả `alreadyConfirmed` và `confirmedRunId`, không phát token mới.
- `/api/imports/settlements/direct` chỉ được map trong Development. Route này không xuất hiện trong endpoint datasource Production và không có trong OpenAPI public.

## Lỗi được test live phát hiện và đã sửa

1. XLSX sparse cells ban đầu làm lệch cột; parser nay dùng cell reference A–T.
2. Mảng ô trống ban đầu chứa `null` trước bước CSV escape; nay khởi tạo bằng chuỗi rỗng.
3. Retry confirm từng commit cùng transaction hai lần; nhánh đã-confirm nay đi qua importer idempotent rồi commit đúng một lần.
4. Test helper từng trả response body đã đọc; nay tái tạo response JSON cho trạng thái `alreadyConfirmed`.
5. Frontend `npm test` không chọn jsdom config; script canonical nay dùng `vitest.v8.config.ts`.
6. Production TypeScript build từng gồm test sources và hai effect trả Promise; test được loại khỏi production compile và effect dùng `void load()`.

## Bằng chứng kiểm chứng

- PostgreSQL 16, cluster cô lập cổng 55441:
  - lượt 1: 15 migration `APPLIED`;
  - lượt 2: 15 migration `SKIP`;
  - manifest verifier: count 15, first 001, last 015.
- Test settlement chuyên biệt: parser sparse/formula, HTTP CSV, token hash/pre-confirm invariants, confirm retry và Production route đều qua.
- Toàn solution live: 110/110 test qua.
- Build solution: 0 warning, 0 error.
- Frontend: 6 files, 17/17 test qua; Vite production build thành công.
- OpenAPI V13 verifier: 39 paths, 134 refs, 28 source routes, UTF-8, `directPublic=false`.

## Giới hạn có chủ đích

- XLSX không tính công thức; workbook có formula bị từ chối để tránh dữ liệu phụ thuộc engine Excel.
- Pilot lưu payload preview và report export trong PostgreSQL; production scale cần object storage, KMS và retention policy được duyệt.
- Tax engine không tự suy đoán thuế suất hoặc kết quả pháp lý. Mọi rule chưa được chuyên gia duyệt tiếp tục `NEEDS_REVIEW`.

