# Công việc chưa hoàn thành theo Master Prompt SànSổ

**Ngày rà soát:** 2026-08-27  
**Phạm vi:** đối chiếu Master Prompt với source, test, tài liệu và bằng chứng runtime hiện có trong `D:\WebAppCodex`.  
**Nguyên tắc:** một mục chỉ được coi là hoàn thành khi có bằng chứng trực tiếp; cấu hình, mock hoặc tài liệu không thay thế bằng chứng production/pilot.

## 1. Kết luận ngắn

MVP đã có vertical slice chạy được với PostgreSQL, migration 001–015, API V14, frontend V8, reconciliation/settlement import, inventory, onboarding, notification, report CSV/XLSX, worker, OpenAPI, CI, test backend/frontend/browser và Docker Compose ở mức cấu hình. Tuy nhiên, toàn bộ Master Prompt **chưa hoàn thành** vì còn các đầu vào pháp lý/đối tác/pilot bắt buộc và một số gate production chưa có bằng chứng.

## 2. Việc kỹ thuật còn thiếu hoặc chưa đủ bằng chứng

| ID | Việc chưa hoàn thành | Trạng thái/bằng chứng hiện tại | Điều kiện để đóng |
|---|---|---|---|
| TECH-01 | Chứng minh Docker Compose full stack chạy thật | `docker compose config` đã pass; daemon Docker Desktop trên máy kiểm chứng không khởi động được | Build image, migrate DB, khởi động PostgreSQL/Redis/API/frontend/worker và chạy smoke/E2E qua Compose |
| TECH-02 | Cấu hình IdP production và quyết định migration auth | Có development E2E seed, guard không chạy ở Production và test authorization; chưa có IdP thật | Chọn provider, cấu hình tenant/client/MFA/session revoke, lưu secret ngoài source và chạy IDOR/auth E2E production-like |
| TECH-03 | Secret manager/KMS, object storage và immutable audit sink production | Có field encryption abstraction, env contract và audit model; chưa có hạ tầng production | Provision provider, rotation/recovery drill, retention/WORM policy và bằng chứng restore |
| TECH-04 | Connector Shopee/TikTok thật | Có CSV/mock/fallback, chưa tuyên bố partner integration | Có credential/scope chính thức, contract fixtures, rate-limit/token-refresh/schema-drift tests và sandbox certification nếu áp dụng |
| TECH-05 | Email/Zalo/billing provider thật | Có module/fallback nhưng không có credential/provider contract production | Chọn provider, xác minh permission/webhook, idempotency, retry/DLQ và failure drill |
| TECH-06 | Observability production và SLO drill | Có correlation/telemetry/runbook ở phạm vi ứng dụng; chưa có dashboard/alert backend production | Kết nối OTel/metrics/log sink, đo P95, queue depth, ingest lag; chạy alert, RPO/RTO và outage drill |
| TECH-08 | Manual accessibility coverage | Playwright/Axe canonical đã pass 10/10 | Kiểm tra bàn phím, screen reader, zoom/reflow, reduced motion trên browser mục tiêu và lưu biên bản |
| TECH-09 | Clean-machine onboarding proof | README có lệnh rõ; test hiện tại dùng môi trường đã chuẩn bị | Clone mới trên máy/runner sạch, làm đúng README, migrate/seed/build/test/E2E thành công |
| TECH-10 | Production security assessment | Có automated authorization, tenant, upload/formula, client-secret và safe-error tests | DAST/pentest có phạm vi, webhook spoof/replay drill, log/PII review và remediation report |

## 3. Pháp lý và TaxTech chưa hoàn thành

| ID | Việc chưa hoàn thành | Vì sao chưa thể đánh dấu Done | Đầu vào/bằng chứng cần có |
|---|---|---|---|
| LEGAL-01 | Xác minh hiệu lực và phạm vi từng văn bản tại `LEGAL_AS_OF_DATE` | Tài liệu hiện chỉ là research/assumption; không thay thế legal review | Văn bản gốc chính thức, điều/khoản, ngày hiệu lực, văn bản thay thế/sửa đổi và người review |
| LEGAL-02 | Phê duyệt legal applicability matrix | Matrix chưa có sign-off chuyên gia | Chuyên gia thuế/pháp lý Việt Nam duyệt từng dòng và ghi trạng thái/version |
| LEGAL-03 | Tax rule production | Hệ thống cố ý không bịa rate; rule chưa phê duyệt luôn `NEEDS_REVIEW` | Rule version có căn cứ điều/khoản, effective date, profile/channel scope và approval workflow |
| LEGAL-04 | Golden tax dataset chính thức | Test hiện có kiểm tra cơ chế và negative cases, không chứng minh mức thuế pháp định | Dataset do chuyên gia duyệt gồm boundary, refund/reversal, effective date và expected outcome |
| LEGAL-05 | Privacy/data-retention production assessment | Có threat model/data inventory, chưa có quyết định triển khai theo hạ tầng thật | DPIA/assessment, data residency, retention/deletion, DSR process, processor contracts và incident workflow |

## 4. Thị trường, pricing và pilot chưa hoàn thành

| ID | Việc chưa hoàn thành | Điều kiện để đóng |
|---|---|---|
| MARKET-01 | Xác minh con số 650.000 và TAM/SAM/SOM | Nguồn có phương pháp/phạm vi/ngày rõ; phân biệt shop, seller, legal entity; ba kịch bản Conservative/Base/Upside |
| MARKET-02 | Phỏng vấn 15–30 khách hàng mục tiêu | Transcript/notes đã ẩn danh, phân khúc, pain frequency và evidence summary |
| MARKET-03 | Willingness-to-pay | Chạy Van Westendorp hoặc phương pháp tương đương và lưu dataset/kết quả |
| MARKET-04 | Unit economics thực đo | Chi phí infra/API/support theo volume, gross margin, CAC payback và sensitivity |
| PILOT-01 | Tuyển 10–20 shop pilot đúng ICP | Có danh sách consented, baseline và owner/ops/finance tham gia |
| PILOT-02 | Chứng minh activation và kết quả pilot | Đo time-to-first-reconciliation, explained discrepancy rate, reconciliation time và inventory/SKU errors |
| PILOT-03 | Chốt pricing A/B | Kết hợp WTP, unit economics, conversion và churn; không dùng claim “tiết kiệm thuế” thiếu căn cứ |

## 5. Acceptance criteria chưa thể tuyên bố đạt hoàn toàn

Các acceptance criteria chức năng cốt lõi đã có độ bao phủ đáng kể, nhưng MVP cấp sản phẩm vẫn chưa thể ký Done vì:

1. Chưa có tax rule và golden outcomes được chuyên gia/pháp lý phê duyệt.
2. Chưa có production IdP, provider credentials và contract evidence.
3. Chưa có Docker full-stack runtime proof trên môi trường sạch.
4. Chưa có production observability, recovery/security drill.
5. Chưa có manual accessibility report.
6. Chưa có pilot/WTP/unit-economics evidence.

## 6. Đầu vào cần người dùng cung cấp hoặc quyết định

1. Danh tính/chuyên gia có thẩm quyền review pháp lý và tax golden dataset.
2. Lựa chọn IdP production.
3. Credential/sandbox access chính thức cho Shopee, TikTok Shop và các provider email/Zalo/billing dự kiến.
4. Lựa chọn cloud/secret manager/KMS/object storage/observability stack.
5. Danh sách hoặc kênh tiếp cận khách hàng pilot và quyền dùng dữ liệu đã ẩn danh.
6. Ngân sách/hạ tầng để chạy production-like load, recovery và security tests.

## 7. Thứ tự đề xuất tiếp theo

1. Chạy full stack trên Docker/runner sạch và lưu evidence.
2. Chốt IdP + production infrastructure decisions.
3. Hoàn thành legal/tax sign-off và golden dataset.
4. Tích hợp provider sandbox theo thứ tự Shopee/TikTok → notification → billing.
5. Chạy accessibility/security/recovery drills.
6. Thực hiện market interviews, WTP và pilot; cập nhật pricing/unit economics.
7. Audit lại từng dòng Master Prompt; chỉ khi mọi dòng có authoritative evidence mới đánh dấu hoàn thành.

## 8. Gap đã đóng sau lần rà soát đầu

| ID | Ngày đóng | Bằng chứng |
|---|---|---|
| TECH-07 | 2026-08-27 | CI canonical có gate audit riêng cho .NET/frontend/E2E; YAML lint pass; script chèn idempotent; local audit: .NET 14 project, frontend và E2E đều không có vulnerability được báo cáo |

## 9. Tài liệu bằng chứng liên quan

- `docs/master-prompt-traceability-v3.md`
- `docs/completion-audit-2026-08-24-v32.md`
- `docs/legal-and-market-validation.md`
- `docs/security-and-privacy.md`
- `docs/operations-and-runbooks.md`
- `docs/phase-41-canonical-ci-v14.md`
- `docs/phase-42-docker-compose-v14.md`
- `README.md`

