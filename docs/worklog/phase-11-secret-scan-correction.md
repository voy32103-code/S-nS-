# Phase 11 correction — Secret scanner V2

Scanner V1 cho hai false positive:

1. UUID demo bị nhận nhầm là CCCD 12 chữ số.
2. JavaScript minified có chuỗi gần `password` bị nhận nhầm là connection string.

V2 sửa bằng cách:

- chỉ thu thập `.log`, `.out`, `.err` ở log roots;
- không quét migration/source dưới danh nghĩa log artifact;
- yêu cầu ngữ cảnh `Host`, `Server` hoặc `Data Source` trước password;
- loại chuỗi 12 số nằm trong UUID hex có dấu gạch;
- dùng đúng output mặc định `frontend/dist`.

V1 được giữ lại làm bằng chứng lịch sử nhưng không phải gate có thẩm quyền. Gate hiện hành là `scripts/scan-client-secrets-v2.ps1`.
