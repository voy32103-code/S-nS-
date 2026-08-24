# Phase 11 — Client bundle và log secret/PII scan

Ngày: 2026-08-24

## Phạm vi

Gate tĩnh quét:

- toàn bộ artifact trong `frontend/dist-v3`;
- các file `.log`, `.out`, `.err` dưới frontend, E2E và backend;
- bỏ qua dependency/build/test report folders để tránh quét thư viện bên thứ ba.

Rule hiện có:

- AWS access key;
- private key PEM;
- JWT-like token;
- password trong connection string;
- bearer token dài;
- chuỗi 12 chữ số có dạng CCCD Việt Nam.

## Cách chạy

```powershell
npm --prefix frontend run build
powershell -ExecutionPolicy Bypass -File scripts/scan-client-secrets.ps1
```

## Giới hạn

- Đây là heuristic gate, không thay thế secret manager, SAST hay kiểm tra log runtime có tải thật.
- Pattern CCCD có thể có false positive; fixture hợp lệ phải dùng dữ liệu giả rõ ràng và không đưa vào bundle production.
- Chỉ được đánh dấu acceptance criterion 11 là DONE sau khi có cả runtime error/log tests và CI thực thi gate này.
