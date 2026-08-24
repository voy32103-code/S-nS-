# Completion audit V12 — Durable worker core

Ngày: 2026-08-24

## Latest authoritative evidence

| Gate | Result |
|---|---|
| Solution | `SanSo.V4.slnx` — 11 projects |
| Build | PASS — 0 warnings, 0 errors |
| .NET tests | PASS — **92/92** |
| Worker tests | PASS — 5/5 |
| Migration manifest | PASS — 001–009, worker lease guard present |
| Canonical OpenAPI | PASS — 23 paths, 48 refs |
| Frontend tests | PASS — 12/12 |
| Import browser E2E | PASS — 2/2 |
| Client secret/PII scan | PASS — 3 artifacts / 6 rules |
| NuGet vulnerability audit | PASS — 11/11 projects, no known vulnerable package reported |
| PostgreSQL/RLS/lease runtime | UNVERIFIED |

## Acceptance movement

Outage/recovery requirement now has executable worker core:

- startup expired-lease recovery;
- SKIP LOCKED claim source;
- exactly-once completion per held lease;
- retry/dead-letter policy;
- safe error codes;
- fail-closed external adapters;
- tenant-scoped database context.

It remains `PARTIAL` because process-kill/restart against PostgreSQL, multi-worker contention and raw-to-projection recovery are not runtime-tested.

## Verdict

MVP product acceptance remains incomplete. The work can continue on non-DB UI/contracts, but the strongest remaining gate requires a functioning PostgreSQL instance/Docker daemon or an explicitly provided external test database.
