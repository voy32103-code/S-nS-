# Integration contracts, sync engine and event/job catalog

## Connector interface

```text
GetCapabilities() -> orders, settlements, refunds, inventoryRead, inventoryWrite, webhooks
Authorize()/Refresh()/Revoke()
Poll(resource, cursor, fromUtc, pageSize) -> page + nextCursor + sourceSchemaVersion
VerifyWebhook(headers, rawBody, receivedAt) -> sourceEventId + replayDecision
Normalize(rawEnvelope, adapterVersion) -> canonical events + mapping exceptions
PushStock(command, idempotencyKey) -> accepted/result/retry-after
Health() -> status, lastSuccess, lag, errorCode, tokenState
```

No production connector claim is made until partner authorization, permissions and contract fixtures are verified. CSV/XLSX remains mandatory fallback.

## Source acceptance contract

1. Verify request/file size, MIME, signature where applicable and tenant connection.
2. Compute checksum over original bytes and store immutable raw payload/file reference first.
3. Insert inbox key `(tenant, source, source_event_id)`; duplicate returns the original outcome.
4. Acknowledge only after durable raw acceptance.
5. Normalize using explicit source schema + adapter version.
6. Preserve unknown fields in raw source; schema drift creates alert and quarantines affected records.
7. Publish canonical event and outbox atomically with domain mutation.

## CSV/XLSX fallback

- Template/version detection; encoding delimiter/date/timezone preview.
- Column mapping preview and required-field errors before commit.
- SHA-256 file checksum; repeated file has zero second effect.
- Maximum compressed/uncompressed size; MIME/magic-byte validation and malware scan gate.
- Escape cells beginning `=`, `+`, `-`, `@` in exports; never evaluate imported formulas.
- Large imports become async jobs with accepted/duplicate/rejected row report.

## Retry and circuit behavior

Transient: timeout, 429, selected 5xx → exponential backoff with jitter, respect `Retry-After`, max attempt then DLQ. Permanent: invalid scope/schema/business rejection → no blind retry, create exception. Token expired/revoked → pause connection and stock write-back, alert owner, retain raw/outbox for recovery. Circuit opens per tenant+connection+operation, not globally.

## Event catalog

| Event | Producer | Consumers | Idempotency key | PII class |
|---|---|---|---|---|
| RawEventAccepted | Integrations | Normalize worker | tenant/source/eventId | potentially high |
| ImportBatchAccepted | Integrations | Import worker | tenant/checksum | file metadata |
| OrderUpserted | Orders | Ledger, Inventory, Reporting | tenant/channel/order/version | commerce |
| OrderCancelled | Orders | Inventory, Ledger | source state event | commerce |
| RefundRecorded | Orders | Ledger, Tax, Reconciliation | source refund id | finance |
| SettlementImported | Settlements | Reconciliation | tenant/settlement code | finance |
| LedgerLinePosted | Ledger | Reconciliation, Tax, Reporting | source key | finance |
| ReconciliationCompleted | Reconciliation | Alerts, Reporting | run id/input checksum | finance |
| TaxPeriodCalculated | Tax | Alerts, Reporting | period/input/rule versions | sensitive tax |
| InventoryReserved | Inventory | Stock outbox | order/item/reservation | operations |
| InventoryReleased | Inventory | Stock outbox | cancel/return state | operations |
| ConnectionDegraded | Integrations | Alerts/Admin | connection/error epoch | secret-adjacent |
| SubscriptionChanged | Billing | Entitlements/Sync | external event id | billing |
| AuditEntryAppended | Every module | Compliance archive | entry id/hash | sensitive metadata |

## Job catalog

| Job | Trigger | Retry | Timeout | Failure action |
|---|---|---|---|---|
| Webhook normalize | RawEventAccepted | 5 transient | 2 min | quarantine + schema alert |
| Poll orders/settlements | schedule/manual | cursor-safe | 15 min | degraded source |
| Import parse | batch accepted | worker crash safe | size-dependent | row report/DLQ |
| Ledger projection | canonical event | idempotent | 2 min | finance-critical alert |
| Reconciliation run | settlement/import/manual | deterministic rerun | 30 min | keep previous run |
| Tax calculation | frozen period snapshot | no ambiguous retry | 30 min | NEEDS_REVIEW |
| Inventory projection | order movement | optimistic retry | 30 sec | oversell alert |
| Stock push | outbox | rate-limited 5 | 2 min | pause on auth/permanent |
| Export generation | user request | 3 transient | 30 min | safe failure + correlation |
| Notification delivery | alert policy | channel-specific | 1 min | retain in-app alert |
| Retention/delete | approved request | resumable | long | compliance escalation |

Every job records correlation ID, masked tenant ID, job/version, attempt, queued/start/end, duration, outcome and safe error code.
