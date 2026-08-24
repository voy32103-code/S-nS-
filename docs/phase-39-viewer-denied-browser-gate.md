# Phase 39 — Viewer denied-state browser gate

Ngày kiểm chứng: 2026-08-24.

## Triển khai

- `DevelopmentE2ESeedV2` tạo Owner hoặc Viewer chỉ khi Development và đủ biến môi trường.
- Viewer không có TOTP vì không phải privileged Owner/Admin; password vẫn phải qua policy tối thiểu.
- Playwright đăng nhập Viewer qua API thật, mở tab Onboarding, xác nhận `.denied` và nội dung không có quyền.
- Axe WCAG 2.0/2.1 A/AA chạy trên denied state.

## Bằng chứng

- API build: 0 warning, 0 error.
- Viewer denied browser test: pass.
- Viewer denied Axe: 0 violation.
- Tổng Playwright matrix: 10/10 pass.
- Fixture V1 đã xóa; props/program/generator đều trỏ V2.

## Security boundary

Fixture không có endpoint HTTP để tạo/bỏ qua session. Nó gọi cùng `IdentityService`, password policy, membership và TOTP validation như app. `Apply` trả về ngay ngoài Development. Không biến nào được đặt trong production artifacts.

