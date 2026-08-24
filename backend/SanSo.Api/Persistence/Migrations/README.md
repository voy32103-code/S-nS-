# Database migrations

Apply in lexical order against PostgreSQL 16:

```powershell
Get-Content backend/SanSo.Api/Persistence/Migrations/001_initial.sql -Raw | docker compose exec -T postgres psql -U sanso -d sanso
Get-Content backend/SanSo.Api/Persistence/Migrations/002_tenant_guards.sql -Raw | docker compose exec -T postgres psql -U sanso -d sanso
```

`002_tenant_guards.sql` is defense-in-depth. The API must set `app.current_organization_id` only after authenticating the user and verifying active membership. Application authorization remains mandatory.
