# Phase 37 — Accessibility browser gate

Ngày kiểm chứng: 2026-08-24.

## Triển khai

- Thêm `@axe-core/playwright` (npm audit: 0 vulnerability).
- Scan login V8 trước authentication.
- Scan overview V8 sau Owner login MFA và sau khi reconciliation đã render.
- Tags: `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa`.
- Axe chạy trong cùng Edge/API thật với core Playwright journeys.

## Kết quả

- Login: 0 violation.
- Authenticated overview: 0 violation.
- Toàn Playwright matrix: 8/8 pass (2 accessibility + 6 core), một worker, 18,7 giây.

## Giới hạn

Automated Axe không chứng minh đầy đủ accessibility. Chưa có bằng chứng manual cho keyboard-only, focus order ở mọi tab, screen reader announcements, zoom/reflow 200–400%, reduced motion và contrast ở mọi dynamic state. Vì vậy chỉ hai màn hình được quét mới được đánh dấu đạt gate tự động.

