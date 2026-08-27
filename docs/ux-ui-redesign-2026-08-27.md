# UX/UI redesign SànSổ V8

**Ngày:** 2026-08-27  
**Phạm vi:** frontend canonical `index-v8.html`; giữ nguyên API, information architecture, nav labels, form field order và browser-test contracts.

## Design read

Ứng dụng vận hành thương mại điện tử và thuế cho chủ shop Việt Nam, ưu tiên tin cậy, trạng thái rõ ràng và thao tác nhanh. Ngôn ngữ enterprise calm, dựa trên Fluent UI.

- `DESIGN_VARIANCE: 4`
- `MOTION_INTENSITY: 2`
- `VISUAL_DENSITY: 6`
- Chế độ: redesign-preserve / targeted evolution

## Audit trước redesign

### Giữ lại

- Brand xanh lá và wordmark SànSổ.
- Bốn khu vực: Tổng quan, Import dữ liệu, Onboarding, Workflow.
- Label form, focusable navigation, loading/error/denied states.
- Tenant, role và trạng thái MFA luôn nhìn thấy.
- Nội dung evidence-first và không suy đoán thuế suất.

### Thay đổi

- Bỏ radial gradient ở login và sidebar xanh đặc nặng thị giác.
- Dùng Fluent UI React làm design-system foundation.
- Chuyển sang neutral lạnh, một accent emerald và semantic warning/error.
- Chuẩn hóa radius: surface 14px, controls 8px, status badge pill.
- Metrics chuyển thành segmented surface, giảm card hóa.
- Sidebar desktop sáng, sticky; mobile dùng bottom navigation 4 mục.
- Bảng hỗ trợ horizontal overflow trên màn hình nhỏ.
- Bổ sung focus ring, hover, active feedback và reduced-motion override.
- Thay ký tự em-dash trong trạng thái thiếu dữ liệu bằng nội dung rõ nghĩa.

## Bằng chứng kiểm thử

- Unit/component: 17/17 pass.
- Production TypeScript/Vite build: pass.
- Playwright core journeys: 6/6 pass.
- Playwright login/overview Axe: 2/2 pass.
- Playwright authenticated tabs Axe: 1/1 pass.
- Playwright viewer denied state + Axe: 1/1 pass.
- Tổng browser journeys: 10/10 pass.
- Taste pre-flight scan canonical UI: không còn em-dash/en-dash.
- `npm audit --audit-level=high`: 0 vulnerabilities.

## Giới hạn và việc tiếp theo

- Chromium đã tạo screenshot desktop trong `.tmp`, nhưng Codex image viewer gặp lỗi Windows sandbox helper nên chưa kiểm tra ảnh bằng mắt trong phiên này.
- Dark mode chưa thêm vì đây là authenticated enterprise product UI và scope hiện tại khóa light theme. Có thể thêm theme switch trong một task riêng sau khi chốt brand behavior.