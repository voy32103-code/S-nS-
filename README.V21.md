# SànSổ — Accessibility gate

Frontend V8 có hai lớp browser gate:

- 6 core Playwright journeys qua Edge + API thật.
- 2 Axe scans cho login và authenticated overview với tags WCAG 2.0/2.1 A/AA.

Kết quả 2026-08-24: 8/8 Playwright tests pass, không có Axe violation trên hai màn hình đã quét. Chạy bằng `cd frontend; npm run test:e2e`.

Xem `docs/phase-37-accessibility-browser-gate.md` và audit V27.

