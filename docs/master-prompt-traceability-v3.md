# Master prompt traceability — V3 delta

Ngày audit: 2026-08-24. Đọc cùng `master-prompt-traceability-v2.md`.

## Delta đã có bằng chứng

| Requirement | V2 | V3 evidence | V3 verdict |
|---|---|---|---|
| Acceptance 1 demo first reconciliation | Partial | 6 core Playwright journeys; route-order bug fixed | Đạt demo path |
| Six core E2E | Missing | Edge + V8 + API real, 6/6 | Đạt demo E2E |
| UI automated accessibility | Partial | Axe login, Owner 4 tabs, Viewer denied; contrast bug fixed | Đạt automated canonical scope |
| Role denied browser state | Unit only | Viewer API login + denied UI + Axe | Đạt |
| RPT-01 CSV/XLSX pack | CSV only | Store V2 live XLSX + OpenAPI V14 | Đạt reconciliation pack |
| Backend regression | 110 | 112/112 live PostgreSQL | Đạt current implemented scope |
| Browser regression | 0/6 then 6/6 | 10/10 including accessibility/Viewer | Đạt current automated scope |

## Vẫn bắt buộc nhưng thiếu authoritative evidence

1. IdP production configuration and migration decision.
2. Approved Vietnamese tax rules, legal citations mapped to clauses and expert golden outcomes.
3. Official marketplace/email/Zalo/billing credentials/scopes.
4. Production secret manager/KMS, object storage and immutable audit sink.
5. Pilot interviews/WTP/acceptance evidence.
6. Docker runtime proof and production observability/security drills.
7. Manual accessibility across keyboard, screen reader, zoom/reflow and reduced motion.

