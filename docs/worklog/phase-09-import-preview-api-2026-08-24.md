# Phase 09 — Import Preview API

Ngày xác minh: 2026-08-24  
Phạm vi: CSV/XLSX offline fallback, không kết nối sàn thật.

## Kết quả

- Thêm thư viện `SanSo.Import` dùng chung cho CSV và XLSX.
- CSV bắt buộc UTF-8, tự nhận dấu phẩy, chấm phẩy hoặc tab.
- Chuẩn hoá alias tiếng Việt: `Mã đơn`, `Số tiền`, `Ngày đơn`.
- Chuẩn hoá ngày về `DateTimeOffset`; ngày không có múi giờ được hiểu theo UTC+7.
- Không đoán giá trị sai: dòng lỗi trả mã lỗi theo dòng.
- Giới hạn tệp 10 MB ở cả HTTP endpoint và importer.
- XLSX chỉ đọc giá trị; ô có công thức bị gắn `FORMULA_NOT_ALLOWED` và không được tính.
- SHA-256 checksum và registry theo tenant phát hiện upload trùng.
- Endpoint `POST /api/imports/preview` chỉ chấp nhận multipart field `file`, MIME và phần mở rộng CSV/XLSX phù hợp.
- Import hiện là bước preview; chưa ghi dữ liệu kinh doanh. Đây là chủ đích để người dùng kiểm tra trước khi xác nhận.

## Bằng chứng kiểm thử

Lệnh:

```powershell
dotnet build SanSo.sln --no-restore
dotnet test SanSo.sln --no-restore
```

Kết quả:

- Build: 0 warnings, 0 errors.
- `SanSo.Import.Tests`: 6/6 passed.
- `SanSo.Api.Tests`: 37/37 passed.
- `SanSo.Api.V2.Tests`: 11/11 passed.
- Tổng .NET: 54/54 passed.

Các ca import được chứng minh:

1. CSV dấu chấm phẩy với header tiếng Việt, ngày Việt Nam và UTC+7.
2. CSV có trường được quote và dấu phẩy trong dữ liệu.
3. Thiếu cột, số tiền sai, ngày sai được báo lỗi và không tự đoán.
4. Cùng checksum trong cùng tenant được đánh dấu trùng.
5. Tệp quá 10 MB bị từ chối.
6. Công thức XLSX bị từ chối tính toán.
7. API từ chối phần mở rộng không hỗ trợ.

## Giới hạn còn lại

- Chưa có bước confirm/commit hai pha từ preview vào raw event store.
- Registry checksum hiện ở bộ nhớ của tiến trình API; production cần lưu vào PostgreSQL với unique key theo tenant.
- PostgreSQL/RLS chưa được chạy end-to-end tại máy này vì Docker daemon chưa hoạt động.
- Mẫu XLSX tải xuống và UI mapping cột chưa được nối vào giao diện.

Không có thuế suất, ngưỡng thuế hoặc kết luận pháp lý nào được suy đoán trong phase này.
