# Reliability, observability, SLO and runbooks

## Service objectives

| SLI | Target | Window/exclusions |
|---|---|---|
| Interactive API availability | 99.9% | monthly; announced maintenance excluded |
| Interactive API latency | P95 <800ms | ordinary tenant queries |
| Raw acceptance durability | 100% after acknowledged | checksum/audit comparison |
| Sync freshness | per connector capability/SLA | show P50/P95 and last success |
| RPO / RTO | ≤15 min / ≤4 h target | cost and restore-test gate |
| Reconciliation reproducibility | 100% same input/version | invariant monitoring |

Heavy report/export and backfill are asynchronous and excluded from interactive latency, but have separate queue-age/completion SLIs.

## Telemetry standard

Every request/job: `trace_id`, `correlation_id`, route/job type, version, masked tenant ID, actor class, result code, duration. Jobs also include attempt, queue age, source/adapter version and retry outcome. Never log access token, password, OTP, full tax identifier, bank account, raw payload or customer address.

Metrics: request RED; PostgreSQL pool/locks; Redis/queue depth/oldest age; raw accepted/duplicate/quarantined; sync lag/success/token state; reconciliation duration/matched/unmatched/difference; tax needs-review/failure by safe reason; inventory conflict/negative/write-back failure; exports; notifications; entitlement denials.

Tracing: receive/import → raw store → inbox → normalize → canonical event → ledger/inventory → reconciliation/tax → outbox/export. Alert on business failure and error budget, not only CPU/RAM.

## Alert severity

- SEV1: confirmed cross-tenant exposure, destructive money/inventory corruption, widespread unavailable data path.
- SEV2: raw acceptance unavailable, bad approved tax rule, sustained queue backlog, stock write-back broadly wrong.
- SEV3: single connection revoked/degraded, isolated export/import failure, freshness threshold.
- SEV4: informational/customer action required.

## Runbook — token revoked

1. Mark connection `DEGRADED_AUTH`; atomically disable write-back.
2. Preserve accepted raw/outbox; do not discard or endlessly retry.
3. Alert authorized Owner/Admin with re-auth instructions.
4. After re-auth, validate scopes/capabilities, resume from stored cursor and reconcile missed window.
5. Audit who reconnected and resulting backfill.

## Runbook — platform outage / queue backlog

Confirm vendor versus internal failure; pause aggressive polling; respect rate limit; scale worker only after DB health; preserve order with source event IDs; surface freshness; drain oldest safely; compare raw accepted to canonical outcomes; close only after backlog age and discrepancy rates normalize.

## Runbook — bad tax rule

1. Disable rule for new selection without mutating historical version.
2. Identify calculations/exports by exact rule version and effective dates.
3. Notify security/legal/product incident owners; require four-eyes corrected version.
4. Recalculate only unlocked periods automatically; locked periods create amendment/review queue.
5. Preserve old reports, diff and customer communication/audit evidence.

## Runbook — incorrect stock push

Activate global/tenant/connection kill switch; stop outbox sends; retain attempts/responses; fetch current channel snapshots read-only; quantify affected SKUs/orders; require human-approved correction batch; use new idempotency keys; monitor oversell/undersell after recovery.

## Runbook — failed migration

Stop write traffic if compatibility is unsafe; do not destructive rollback blindly; use expand/migrate/contract; restore verified backup/PITR if required; record migration version/checksum; run financial/inventory reconciliation before reopening.

## Runbook — restore exercise

Quarterly: restore encrypted backup into isolated environment, verify tenant counts/checksums/raw-to-ledger invariants, rotate temporary credentials, measure RPO/RTO, destroy isolated copy securely and file signed exercise report.

## Incident and breach handling

Triage/contain → preserve evidence → assess data/tenant/scope/timeline → legal notification decision under current Vietnamese requirements/contracts → eradicate/rotate/recover → customer communication → postmortem/actions/access review. Do not claim regulatory compliance until counsel confirms obligations and deadlines.
