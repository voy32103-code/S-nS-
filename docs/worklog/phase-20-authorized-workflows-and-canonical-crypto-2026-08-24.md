# Phase 20 — Authorized workflows và canonical field protection

Ngày xác minh: 2026-08-24

## Authorization correction

Một composition draft bị security review từ chối vì dùng synthetic owner fallback và chưa gọi permission check tại endpoint. Draft đó không được áp dụng.

Composition hiện hành:

- mọi refund/period/team/support/billing/Copilot/onboarding/notification endpoint gọi `IdentityEndpoints.Require`;
- session bắt buộc gắn đúng tenant;
- dùng RBAC permission hiện có;
- support grant không còn synthetic principal và domain vẫn yêu cầu step-up verified;
- notification mutation chỉ development và luôn trả `persisted=false`; production trả 503 cho tới khi có persistence;
- anonymous onboarding bị 401;
- Viewer bị 403 khi quản lý onboarding/notification.

## HTTP evidence

API V4 tests tăng lên **7/7**:

- import preview/confirm safety (4 tests);
- owner MFA hoàn thành đủ onboarding HTTP state machine;
- Viewer không thể quản lý onboarding/notification;
- notification email được mask;
- authorized Copilot từ chối chọn tax rate.

## Canonical Base64URL security fix

Full suite phát hiện test tamper flaky: thay ký tự cuối đôi khi chỉ đổi padding bits và decode ra cùng bytes. Implementation mới:

- chỉ chấp nhận Base64URL không padding;
- decode xong encode lại và yêu cầu representation giống hệt;
- non-canonical encoding bị từ chối trước AES-GCM;
- authentication tag mismatch được chuẩn hoá thành `CryptographicException`;
- backend suite chạy ba lần liên tiếp: 59/59 mỗi lần.

## Final evidence for phase

```text
Build: 0 warnings, 0 errors
Domain/API tests: 59/59
API V2 regression: 11/11
Import library: 10/10
API V4: 7/7
Total .NET: 87/87
Import browser E2E: 2/2
Frontend components: 12/12
```

## Remaining gaps

- Onboarding/notification persistence chưa có repository PostgreSQL runtime.
- Frontend chưa có authenticated login/onboarding UI; workflow drawer cũ cần bearer integration.
- PostgreSQL RLS/runtime vẫn chưa chạy.
