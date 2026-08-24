# Migration and seed operations

## Apply

Set secret from environment and run the checksum-protected migrator:

```powershell
$env:SANSO_POSTGRES='Host=localhost;Port=5432;Database=sanso;Username=sanso;Password=<local-secret>'
dotnet run --project backend/SanSo.Migrator
```

The migrator acquires PostgreSQL advisory lock `7482152026`, creates `schema_migrations`, applies lexical SQL files transactionally and refuses a changed checksum after application.

## Current migrations

1. `001_initial.sql`: product schema and critical indexes/constraints.
2. `002_tenant_guards.sql`: RLS defense-in-depth for high-risk tenant tables.
3. `003_demo_seed.sql`: two isolated tenants, subscriptions/connections, 12 synthetic scenarios and inventory balances.

## Rollback strategy

MVP uses forward-only expand/migrate/contract. Never automatically drop financial/tax/raw/audit data. A deployment rollback runs old compatible binaries; data correction is a new reviewed migration. Destructive rollback requires backup/PITR verification, exact target and explicit incident authorization.

## Pending evidence

Docker Desktop daemon is unavailable on this machine as of 2026-08-24, so clean PostgreSQL execution, RLS session tests and restore exercise are not yet evidenced. Source/build alone is not counted as database completion.
