# Phase 2 — Data foundation and domain invariants

**Date:** 2026-08-24  
**Status:** in progress

## Implemented

- PostgreSQL migration `001_initial.sql` covering organizations, memberships, tax profiles, connections, immutable raw events, inbox/outbox, products/SKU maps, orders/items, ledger, settlements/reconciliation, tax rules/periods/calculations/exceptions, inventory, alerts, plans/subscriptions, exports and tamper-evident audit records.
- Defense-in-depth RLS policies in `002_tenant_guards.sql` for high-risk tenant tables.
- Versioned synthetic seed manifest covering all 12 mandatory demo scenarios and two isolated tenants; all emails use `.invalid`, no real PII.
- Raw ingestion service with SHA-256 checksum and `(tenant, source, eventId)` idempotency.
- Tax Center core that refuses approval without a supplied expert-approved rate/legal source, creates `NEEDS_REVIEW` when data/rule is missing, validates state transitions and protects locked periods.
- Inventory core with per-tenant/SKU concurrency guard, idempotent movement source keys, reservation/release and return quarantine excluded from ATP.
- Traceable CSV export with provenance metadata/checksum and spreadsheet formula-injection protection.
- Entitlement behavior that stops new sync/features after expiry without deleting or hiding existing customer data.
- API endpoints for raw events, tax calculate/transition, inventory seed/reserve/release/read and reconciliation CSV export.

## Verification evidence

- `dotnet test SanSo.sln --no-restore`: **11 passed, 0 failed**.
- Invariants covered: duplicate raw input, tax no-guess behavior, approved rule completeness, locked period immutability, concurrent last-unit reservation, quarantine ATP, traceable/formula-safe CSV and expired-subscription read access.
- Docker migration execution attempted; Docker CLI returned missing `dockerDesktopLinuxEngine`. Schema runtime verification remains pending until Docker Desktop runs.

## Remaining before Phase 2 is complete

- Apply migrations against PostgreSQL 16 and verify constraints/RLS.
- Replace in-memory demo persistence with repository-backed PostgreSQL implementation.
- Materialize all 12 scenarios into relational seed rows, not only the scenario manifest.
- Add inbox/outbox worker retry/DLQ and integration tests with real PostgreSQL.
