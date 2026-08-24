# Legal and market validation

**Legal as of:** 2026-08-24  
**Tình trạng:** research baseline; mọi tax rule vẫn cần legal counsel/tax expert phê duyệt trước production.

## Fact / assumption / recommendation convention

- **FACT:** có nguồn chính thức truy cập được và metadata được kiểm tra ngày 2026-08-24.
- **ASSUMPTION:** dùng để thiết kế/research, không dùng như claim.
- **RECOMMENDATION:** quyết định sản phẩm có thể thay đổi sau legal review/pilot.
- **OPEN:** chưa có bằng chứng đủ mạnh.

## Verified legal baseline

| Vấn đề | Chủ thể/phạm vi | Nguồn chính thức | Điều/phần | Hiệu lực | Product rule | Review |
|---|---|---|---|---|---|---|
| Luật quản lý thuế được hướng dẫn bởi NĐ 252 | Người nộp thuế, cơ quan/đơn vị liên quan | [Nghị định 252/2026/NĐ-CP](https://vanban.chinhphu.vn/?docid=218690&pageid=27160) | Metadata: ban hành 30/06/2026 | 01/07/2026 | Chỉ dùng rule effective-dated | FACT metadata; nội dung rule cần counsel |
| Thủ tục theo Luật và NĐ 252 | Chủ thể thuộc phạm vi Thông tư | [Thông tư 89/2026/TT-BTC](https://vanban.chinhphu.vn/?docid=218974&pageid=27160) | Metadata: ban hành 30/06/2026 | 01/07/2026 | Form/export phải version hóa | FACT metadata; biểu mẫu cần counsel |
| NĐ 252 thay thế quy định tại NĐ 117/2025 | Các chủ thể thuộc các nghị định được thay thế | [Bài triển khai Nghị định 252 của Cục Thuế/Báo Chính phủ](https://xaydungchinhsach.chinhphu.vn/mot-so-diem-moi-va-huong-dan-trien-khai-thuc-hien-nghi-dinh-252-2026-nd-cp-119260724164608156.htm) | Bài nêu NĐ 252 gồm 76 điều và thay thế NĐ 126/2020, 91/2022, 117/2025 | 01/07/2026 | Không dùng NĐ 117 làm căn cứ hiện hành duy nhất | FACT theo nguồn triển khai chính thức |
| Quản lý thuế hoạt động nền tảng TMĐT/nền tảng số | Phụ thuộc loại nền tảng, chức năng và loại người bán/nhà cung cấp | Cùng bài triển khai trên | Chương III, Điều 40–46 | 01/07/2026 | Tax classification bắt buộc lưu subject/channel/function/effective date | FACT phạm vi chương; từng transaction rule cần counsel |
| Cơ chế nền tảng có đặt hàng + thanh toán so với kênh khác | Chủ quản nền tảng và hộ/cá nhân kinh doanh theo trường hợp | [Hướng dẫn khai/khấu trừ thuế TMĐT](https://xaydungchinhsach.chinhphu.vn/huong-dan-khai-thue-khau-tru-thue-voi-hoat-dong-kinh-doanh-tren-nen-tang-thuong-mai-dien-tu-119260309150311529.htm) | Bài tháng 03/2026 dẫn NĐ 117 cũ | Trước 01/07/2026; không dùng đơn độc sau ngày này | Dùng để hiểu lịch sử/chủ thể, không publish rule hiện hành | SUPERSEDED CONTEXT |

### Important interpretation boundary

Bài tháng 03/2026 mô tả nền tảng có chức năng đặt hàng trực tuyến và thanh toán thực hiện khấu trừ, khai thay, nộp thay theo phạm vi nêu tại NĐ 117; kênh không đủ hai chức năng có trường hợp người bán tự kê khai/nộp. Vì NĐ 117 đã được NĐ 252 thay thế từ 01/07/2026, nội dung lịch sử này **không đủ** để tạo tax rule production. Cần đọc bản PDF ký của NĐ 252, xác định chính xác subject/transaction và được chuyên gia phê duyệt.

## Claims phải sửa hoặc cấm

| Không được dùng | Microcopy được dùng |
|---|---|
| “Tự động nộp thuế 100%” | “Chuẩn bị dữ liệu hỗ trợ kê khai; chỉ gửi qua tích hợp hợp pháp sau xác nhận.” |
| “Đảm bảo đúng thuế tuyệt đối” | “Tính deterministic theo rule version đã được phê duyệt; exception cần rà soát.” |
| “Thay thế kế toán/đại lý thuế” | “Hỗ trợ kế toán kiểm tra dữ liệu có nguồn gốc.” |
| “Được cơ quan thuế chứng nhận” | Không có tuyên bố chứng nhận khi chưa có bằng chứng. |
| “SànSổ tự động khấu trừ thuế” | “Đối soát số sàn báo đã khấu trừ/nộp thay.” |
| “650.000 nhà bán Shopee/TikTok Shop” | “Quy mô thị trường đang được xác minh theo định nghĩa shop/seller/pháp nhân và kỳ dữ liệu.” |

## Market number 650.000

**OPEN:** đến thời điểm audit chưa tìm được nguồn đủ mạnh chứng minh đồng thời:

- đây là gian hàng hay chủ shop/pháp nhân;
- có phát sinh đơn hay chỉ đăng ký;
- gồm nền tảng nào;
- kỳ/năm đo lường;
- có loại trùng một chủ nhiều shop hay không.

Do đó số 650.000 không xuất hiện trong landing page, pitch, pricing hoặc rule kinh doanh. Research owner phải lưu source URL, publication date, methodology, unit definition và deduplication method.

## TAM / SAM / SOM scenario model

Không tạo precision giả. Mô hình dưới đây dùng biến và dải **ASSUMPTION** phục vụ phỏng vấn, không phải market fact.

| Scenario | Eligible organizations `E` | Paid conversion `C` | Monthly ARPA `A` | Annual market formula |
|---|---:|---:|---:|---:|
| Conservative | 10.000–20.000 | 3–5% | 500k–900k VND | `E × C × A × 12` = 1,8–10,8 tỷ VND SOM hypothesis |
| Base | 25.000–50.000 | 6–10% | 900k–1,8m VND | 16,2–108 tỷ VND SOM hypothesis |
| Upside | 50.000–100.000 gồm agency clients | 10–15% | 1,8m–3,5m VND | 108–630 tỷ VND SOM hypothesis |

TAM = mọi organization phù hợp × ARPA; SAM = organization ≥500 đơn/tháng, đa shop/kênh hoặc có finance ops; SOM = khách có thể đạt qua pilot/channel trong 3 năm. Không cộng “shop” trực tiếp như “organization”.

## Validation plan

1. 15–30 interviews: owner, ops, finance, accountant/agency.
2. Ghi order volume, số shop, kênh, giờ/tháng ghép file, giá trị discrepancy, stack hiện tại.
3. Van Westendorp bốn câu và forced-choice A/B theo package.
4. Thu thập invoice/quote của giải pháp thay thế khi được consent; không chỉ hỏi recall.
5. Cập nhật scenario bằng posterior ranges; ghi ngày dữ liệu và sample bias.
6. Legal counsel ký matrix theo rule code/version trước khi chuyển `DRAFT` → `APPROVED`.
