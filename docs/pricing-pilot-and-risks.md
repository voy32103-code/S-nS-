# Pricing hypotheses, unit economics, pilot and risk register

## Package hypotheses

Prices are research hypotheses, not published offers.

| Plan | Hypothesis A / month | Hypothesis B / month | Core meter |
|---|---:|---:|---|
| Starter | 499.000 VND | 699.000 VND | 1 shop, 1.000 orders, basic reconciliation/Tax Center |
| Growth | 1.490.000 VND | 1.990.000 VND | 3 shops, 10.000 orders, sync/inventory/alerts/team |
| Pro | 3.490.000 VND | 4.990.000 VND | 10 shops, 50.000 orders, approval/profitability/API/export |
| Agency | custom | custom | client organizations, central status, SSO/SLA options |

Annual hypothesis: charge 10–10.5 months, only if gross margin/CAC payback remain healthy. Add-ons: extra shop/order/history/accountant workspace/migration. Never charge as a percentage of “tax saved”.

## Unit economics model

Monthly contribution per tenant = subscription revenue − partner API cost − compute/storage/egress − payment fees − variable support − email/AI usage. Track by order-volume cohort. Gates: target software gross margin ≥75% after stable pilot; support hours/tenant and connector failure cost; CAC payback target ≤12 months; no price publish before 15–30 interviews.

## Van Westendorp interview

For a concrete package ask: price so cheap quality is doubtful; bargain; getting expensive but considered; too expensive. Record current manual/tool cost, time saved and discrepancy value separately; do not lead with tax optimization. Segment answers by monthly orders, shops/channels and finance headcount.

## Pilot plan

- ICP: 10–20 shops, 1.000–20.000 orders/month, multi-shop/channel or explicit spreadsheet reconciliation; Owner + Ops/Finance participate.
- Week 0: data-processing agreement, consent, source sample and baseline time/error measurement.
- Weeks 1–2: profile/import/mapping and first reconciliation.
- Weeks 3–6: weekly exception taxonomy, refund/settlement/inventory feedback.
- Weeks 7–10: Tax Center evidence pack and period close rehearsal; no production filing.
- Weeks 11–12: measured outcome, security/access review and go/no-go.

Activation: successful connection/import plus first reconciliation with at least one matched result or explained discrepancy.

Success: ≥50% median reduction in file-merge/reconciliation time (hypothesis), increased explained-difference rate, fewer SKU/stock errors, and traceable period data pack. Measure baseline and end; do not promise outcome before evidence.

## Risk register

| Risk | Likelihood | Impact | Mitigation / trigger | Owner |
|---|---|---|---|---|
| Law/rate/threshold change | High | Critical | effective-dated approved rules, legal monitoring, kill switch | Legal/Tax |
| Misread platform vs seller duty | Medium | Critical | subject/function classification and counsel sign-off | Legal/Product |
| Partner API unavailable/limited | High | High | capability registry, CSV fallback, no unsupported claim | Integration |
| Schema/fee drift | High | High | versioned raw/adapter, quarantine/alert/contracts | Integration |
| Wrong calculation | Medium | Critical | integer money, golden regression, four-eyes release | Finance/Tax |
| Wrong stock write | Medium | Critical | feature flag, concurrency/idempotency, kill switch | Inventory/Ops |
| Cross-tenant leak | Low | Critical | auth-derived tenant, RLS, IDOR tests | Security |
| Token/PII compromise | Medium | Critical | KMS, masking, minimal scope, incident response | Security |
| Missing/late source data | High | High | completeness/freshness, retry/backfill, review state | Data |
| Support cost too high | Medium | High | diagnostics, onboarding templates, cohort cost | Customer Success |
| ICP/WTP too low | Medium | High | 15–30 interviews, package A/B, pilot gates | Product/Growth |
| User treats Tax Center as absolute advice | High | High | careful copy, evidence/status/expert confirmation | Legal/UX |
| AI hallucination | Medium | High | no rule/rate decisions, cited evidence, tenant isolation | AI/Security |
| Vendor lock-in | Medium | Medium | adapter interfaces, portable data/export, IaC | Architecture |
| Data residency/retention mismatch | Medium | High | counsel-approved inventory/retention/deletion | Privacy |
| One owner has many shops, TAM overcounted | High | Medium | model organizations separately from shops | Growth |

Risk review occurs weekly during pilot and before every approved tax rule or write-back enablement.
