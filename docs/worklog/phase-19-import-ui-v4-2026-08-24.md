# Phase 19 — Import preview/diff/confirm UI V4

Ngày xác minh: 2026-08-24

## UI đã triển khai

`ImportConfirmationPanel` cung cấp workflow có kiểm soát:

1. Chọn CSV/XLSX với accept hint và giới hạn được giải thích.
2. Gọi preview, không tự confirm.
3. Hiển thị format/template version, valid/error count, duplicate state.
4. Hiển thị SHA-256 checksum và token expiry.
5. Preview tối đa 20 dòng, lỗi theo dòng và tổng số dòng thật.
6. Development banner nói rõ `persisted=false`.
7. Checkbox xác nhận người dùng đã kiểm tra checksum/count/error.
8. Confirm button bị disable trước checkbox và khi có file-level errors.
9. Result phân biệt persisted PostgreSQL với demo-only.
10. Có thể bỏ preview và chọn file khác.

Microcopy giữ đúng giới hạn TaxTech: không tự tạo rate, không tự nộp hồ sơ và không chạy công thức XLSX.

## Component evidence

- explicit checkbox trước confirm;
- chỉ render 20/21 rows;
- row error hiển thị;
- file-level error chặn confirm;
- request confirm gửi đúng token/checksum;
- safe server error state;
- demo result hiển thị chưa persist.

Frontend suite: **12/12 passed**.

## Production build

```text
1810 modules transformed
index-v4.html
CSS 12.53 kB
JS 217.45 kB
```

TypeScript build và Vite V4 production build: PASS. Secret/PII scan V2: PASS (3 artifacts, 6 rules).

## Browser E2E

Playwright chạy API V4 thật ở Development và frontend V4:

- upload CSV 21 dòng;
- preview giới hạn 20 dòng;
- phát hiện `AMOUNT_INVALID`;
- checkbox/confirm gate;
- kết quả 20 accepted, 1 rejected, `persisted=false`;
- abandon preview/chọn file khác.

Kết quả: **2/2 passed**.

Lần chạy đầu dùng port 5175 bị CORS từ chối đúng vì allowlist chỉ có 5173/5174. Test được sửa để chạy origin 5174; không nới CORS chỉ nhằm làm test xanh.

## Chưa đạt

- Browser test production authenticated + PostgreSQL chưa có.
- Chưa có column mapping UI trước preview.
- Chưa có XLSX browser fixture; XLSX formula rejection đã có library test.
- Chưa có preview diff với projection/order hiện hữu sau database lookup.
