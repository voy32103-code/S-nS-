# SànSổ — Role-aware browser gate

Playwright matrix hiện có 10/10 test:

- 6 core Owner journeys.
- 3 Axe gates cho login, overview và toàn bộ Owner tabs.
- 1 Viewer authorization + denied-state Axe gate.

Development E2E fixture V2 hỗ trợ Owner MFA và Viewer; chỉ chạy khi Development cùng biến môi trường tương ứng. Production không seed.

Xem Phase 39 và completion audit V29.

