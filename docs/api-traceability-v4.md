# API traceability matrix V4

Ngày: 2026-08-24

| Requirement | API | Module/store | Database | Executable evidence |
|---|---|---|---|---|
| Auth + tenant + RBAC | auth login/logout/me; protected V4 workflows | IdentityService, ProductionTenantMiddleware | users/memberships/sessions + RLS V2 | identity/security tests; V4 Viewer 403 and owner MFA tests |
| Demo first reconciliation | demo import, dashboard, orders, reconciliation | DemoStore / PostgresCommerceStore | raw/orders/ledger/settlements/reconciliation | API + browser dashboard evidence |
| CSV/XLSX fallback | imports preview/confirm | CommerceFileImporter, ImportConfirmationWorkflow, PostgresImportStagingStore | import_staging_batches/rows, raw_events | 10 parser/core, 4 HTTP, 3 component, 2 browser tests |
| Immutable/idempotent raw | raw-events; import confirm | RawIngestion / PostgresCommerceStore | raw_events unique tenant/source/event | domain/API tests; PostgreSQL runtime pending |
| Reconciliation explanation | reconciliation current/export | DemoStore/PostgresCommerceStore/TraceableExport | ledger/settlement/reconciliation | API/component/browser/export tests |
| Refund/cross-period | refunds, period freeze/lock | FinancialLifecycle/PostgresLifecycleStore | ledger/tax periods/audit | domain + authorized API tests; DB E2E pending |
| Tax no-guess | tax calculate/transition | TaxCenter | tax rule/period/calculation/exception | negative golden + browser NEEDS_REVIEW; approved positive rule pending |
| Inventory concurrency | inventory reserve/release/get | InventoryService/PostgresCommerceStore | movement/balance + RLS | concurrent domain/API; DB serializable pending |
| Onboarding | onboarding ordered endpoints | OnboardingWorkflow | onboarding_profiles (protected fields) | full owner MFA HTTP state machine; persistence/UI pending |
| Notification | notifications/ack | NotificationCenter | alerts/notification_deliveries | masking/dedupe/retry/RBAC API tests; provider/persistence pending |
| Team/support | invitations/support grants | TeamAndSupport/PostgresLifecycleStore/AuditTrail | invitations/support_grants/audit | domain + RBAC; authenticated browser pending |
| Billing | trial/transition | BillingLifecycle/PostgresLifecycleStore | plans/subscriptions | domain/API regression; provider billing non-goal |
| AI safety | Copilot explain | SafeCopilot | evidence references only | authorized REFUSED test and browser regression |
| Traceable export | reconciliation CSV | TraceableExport | exports/audit metadata | checksum/formula-escape tests |

`docs/openapi-v4.yaml` is the authoritative pilot contract. Runtime PostgreSQL behavior remains unproven until integration tests execute against migration chain V2.
