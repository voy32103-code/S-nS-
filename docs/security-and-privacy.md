# Security threat model and privacy inventory

## Trust boundaries

Browser, public API, partner/webhook network, worker queue, PostgreSQL/Redis/object store, email provider, observability backend and internal support are distinct trust zones. Tenant context crosses a boundary only after session authentication and active membership lookup.

## Threat model

| Threat | Example | Prevent | Detect/recover | Verification |
|---|---|---|---|---|
| Cross-tenant IDOR | Change path/body tenant ID | Server-derived tenant, RBAC/ABAC, RLS | audit denied requests | API/security tests |
| Privilege escalation | Warehouse calls tax/export | deny-by-default permissions | alert repeated denies | RBAC matrix tests |
| Session theft/stale user | Removed member token remains valid | opaque hashed sessions, expiry/rotation, immediate revoke | session anomaly review | revocation tests |
| Credential brute force | login spraying | rate limit, progressive delay/lock, MFA | auth metrics/alert | abuse tests |
| Token compromise | marketplace token in DB/log | field encryption/KMS, redaction, least scope | revoke/re-auth runbook | secret scan/log tests |
| Webhook spoof/replay | forged duplicate callback | signature + timestamp/nonce + inbox uniqueness | reject metrics | contract tests |
| Upload attack | malware, zip bomb, path/CSV formula | size/MIME/magic/scan, isolated parsing, safe export | quarantine | upload tests |
| Injection | SQL/XSS/SSRF/path traversal | parameterization, validation, encoding, allowlist, CSP | WAF/app telemetry | SAST/DAST tests |
| Rule tampering | unapproved tax version | four-eyes approval, signed version, effective dates | audit/release gate | golden tests |
| Ledger history rewrite | silent payout correction | append-only permissions, reversal model | hash/checksum reconciliation | invariant tests |
| Support abuse | browse tenant without consent | time-bound reasoned grant, masking, step-up | immutable audit + review | support-access test |
| Sensitive export | unauthorized tax/PII download | permission + MFA step-up + preview | watermark/provenance/audit | export tests |
| AI data leakage/hallucination | cross-tenant context/rate invention | tenant-scoped retrieval, no training consent, tool policy | citation/response monitoring | AI safety tests |
| Supply chain | malicious dependency/action | lockfiles, pin actions, dependabot/SCA/SAST/secret scan | SBOM/incident runbook | CI gates |

## Privacy data inventory

| Data | Purpose | Sensitivity | Location | Access | Retention decision |
|---|---|---|---|---|---|
| Name/email/phone | account/contact | personal | PostgreSQL | tenant admins/support masked | policy + legal review |
| Business/tax identifiers/profile | Tax Center | highly sensitive | encrypted DB | owner/finance/accountant | legal/accounting retention |
| Orders/customer shipping data | operations | personal/high | raw + canonical | ops need-to-know | minimize/tokenize/delete workflow |
| Payout/bank references | reconciliation | financial | DB/raw storage | finance | accounting retention |
| Marketplace access tokens | sync | secret | KMS-encrypted field | worker only | until revoke/connection delete |
| Raw webhook/files | provenance | mixed/high | encrypted object store | narrowly scoped | source/legal configurable |
| Inventory/SKU | operations | business | DB | ops/warehouse | customer contract |
| Audit/security logs | compliance/security | metadata/high | append-only store | security/compliance | fixed policy, protect from subject mutation |
| Billing identifiers/invoices | subscription | financial | provider + DB refs | owner/finance | statutory/provider policy |
| AI prompts/outputs | explanation only | potentially sensitive | tenant-scoped store | authorized user | opt-in/short retention; no training absent consent |

## Required controls

- TLS; encryption at rest for DB/object/backups; KMS envelope encryption for secrets.
- Secret manager/environment only; no credential in source, browser bundle, log or error.
- MFA Owner/Admin and step-up for support, sensitive export, filing/signing and major billing changes.
- Secure cookie if cookies are used: HttpOnly/Secure/SameSite and CSRF token; otherwise short-lived opaque bearer with rotation.
- Exact-origin CORS; CSP `default-src 'self'`; self-host production fonts; clickjacking and MIME headers.
- Rate limits by IP/account/tenant/operation; account risk controls.
- PII masking in UI, support and telemetry; payload logging prohibited by default.
- Encrypted backup + quarterly restore exercise; documented data export/delete/retention workflow.
- Incident response, breach assessment/notification workflow and quarterly access review.

## Data subject/tenant workflows

Export request → authenticate + step-up → preview scope → approval if required → async encrypted artifact → short-lived download → audit. Delete request → legal/retention evaluation → reversible hold → delete/anonymize eligible data → evidence report. Organization deletion never deletes immutable records still required by law/contract without a reviewed retention decision.

## Open legal privacy work

Vietnam privacy/data/cybersecurity applicability must be verified by counsel at production date, including controller/processor roles, consent/legal basis, cross-border transfer, incident notification and retention. This document is an engineering inventory, not a legal compliance certification.
