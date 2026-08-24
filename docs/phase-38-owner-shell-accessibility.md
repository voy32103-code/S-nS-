# Phase 38 — Accessibility toàn Owner shell

Ngày kiểm chứng: 2026-08-24.

## Phạm vi

- Login.
- Overview sau MFA và data render.
- Import.
- Onboarding.
- Workflow.

Mỗi màn hình chạy Axe với WCAG 2.0/2.1 A/AA trong Edge qua frontend/API thật.

## Phát hiện và sửa

Onboarding có bảy nhãn bước `#809088` trên nền trắng, contrast 3,35:1, thấp hơn 4,5:1 cho text 16px normal. Token được đổi thành `#566b62`. Axe scan lại không còn violation.

## Bằng chứng

- Playwright/Axe: 9/9 pass.
- Owner tabs aggregate scan: 0 violation sau fix.
- Vitest: 17/17 pass.
- V8 production build: thành công.
- npm audit: 0 vulnerability tại thời điểm thêm Axe.

## Giới hạn

Automated coverage hiện phủ toàn Owner shell canonical nhưng chưa phủ Viewer/denied shell trong browser, dynamic error/expired-token states, keyboard-only, screen reader thật, zoom/reflow và reduced motion manual audit.

