# Phase 21 — OpenAPI V4 và API traceability

Ngày: 2026-08-24

## Artifacts

- `docs/openapi-v4.yaml`: contract pilot authoritative.
- `docs/api-traceability-v4.md`: requirement → API → module/store → database → test.
- `scripts/verify-openapi-v4-v2.ps1`: kiểm tra required paths, safety markers và source mapping.

Contract mô tả rõ:

- bearer auth và `X-Tenant-Id`;
- opaque token không được dùng như dữ liệu hiển thị;
- import preview/confirm, checksum/token/expiry/`persisted`;
- tax result có thể `NEEDS_REVIEW`, không có API nộp hồ sơ/tiền thuế;
- onboarding ordered steps;
- notification development persistence limitation;
- traceable CSV export;
- 400/401/403/503 boundaries.

Verifier V1 thiếu `IdentityEndpoints.cs` nên báo auth/login không mapped. V2 bổ sung đúng source file auth; V1 không còn authoritative.

OpenAPI là contract pilot, không phải bằng chứng PostgreSQL runtime hoặc chứng nhận pháp lý.
