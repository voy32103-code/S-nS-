# SànSổ — E-commerce Operations & Tax Readiness

SànSổ là B2B SaaS pilot cho nhà bán hàng Việt Nam: đối soát từng dòng settlement, truy vết tiền về đến raw source/ledger/order, import có preview/confirm, tồn kho đa kênh và Tax Center evidence-first.

Sản phẩm không tự quyết định thuế suất, không tự nộp hồ sơ/tiền và không thay thế chuyên gia thuế. Rule chưa được phê duyệt hoặc dữ liệu thiếu luôn ở `NEEDS_REVIEW`.

## Canonical artifacts

- Solution: `SanSo.V10.slnx`
- API: `backend/SanSo.Api.V6/ProgramCanonicalV14.cs`
- Migrator: `backend/SanSo.Migrator.V8` — migrations 001–015
- Worker: `backend/SanSo.Worker.V2`
- Frontend: `frontend/index-v8.html` / AppV8
- OpenAPI: `docs/openapi-v14-canonical.json`
- CI: `.github/workflows/ci-v14-canonical.yml`

## Prerequisites

- .NET SDK 9.x.
- Node.js 22 và npm.
- PostgreSQL 16+ cho persistence/live integration tests.
- Microsoft Edge cho Playwright local; CI tự cài Chromium.

Docker files có trong workspace nhưng Docker runtime chưa được chứng minh trên máy hiện tại.

## Chạy Demo Development không cần provider credential

Terminal 1:

```powershell
dotnet run --project backend/SanSo.Api.V6/SanSo.Api.V6.csproj --no-launch-profile --urls http://127.0.0.1:5080
```

Terminal 2:

```powershell
cd frontend
npm ci
$env:VITE_API_URL='http://127.0.0.1:5080'
npm run dev -- --host 127.0.0.1 --port 5176
```

Mở `http://127.0.0.1:5176/index-v8.html`.

Demo browser có fixture tự động chỉ khi đặt đủ biến `SANSO_E2E_*`; không dùng fixture đó cho tài khoản thật. Production bỏ qua fixture theo code guard.

## PostgreSQL

Không ghi password vào source. Đặt connection string trong environment/secret manager:

```powershell
$env:SANSO_POSTGRES='Host=127.0.0.1;Port=5432;Database=sanso;Username=sanso_app;Password=<secret>'
$env:SANSO_RUNTIME_POSTGRES=$env:SANSO_POSTGRES
dotnet run --project backend/SanSo.Migrator.V8/SanSo.Migrator.V8.csproj --no-launch-profile
```

Migrator dùng advisory lock, checksum bất biến và chạy lại an toàn (`SKIP` migration đã áp dụng).

## Kiểm thử

```powershell
dotnet build SanSo.V10.slnx --nologo
dotnet test SanSo.V10.slnx --nologo
./scripts/verify-migration-manifest-v13.ps1
./scripts/verify-openapi-v14.ps1

cd frontend
npm test
npm run test:e2e
npm run build
```

Live database tests chạy khi có `SANSO_RUNTIME_POSTGRES`; nếu thiếu, các test đó chủ động skip. Bằng chứng gần nhất: backend 112/112 trên PostgreSQL thật, frontend 17/17, Playwright/Axe 10/10, solution build 0 warning/error.

## Docker Compose V14

```powershell
Copy-Item .env.example .env
# thay local passwords; khĂ´ng dĂ¹ng local values á»Ÿ production
docker compose config
docker compose up --build
```

Compose gá»“m PostgreSQL, Redis, migrator V8, API V14 vĂ  frontend V8. Static config Ä‘Ă£ pass; runtime trĂªn mĂ¡y kiá»ƒm chá»©ng hiá»‡n bá»‹ cháº·n vĂ¬ Docker Desktop daemon khĂ´ng start. Xem `docs/phase-42-docker-compose-v14.md`.
## Security-sensitive configuration

- `SANSO_FIELD_ENCRYPTION_KEY_BASE64`: AES-256 field key từ secret manager.
- `SANSO_FIELD_ENCRYPTION_KEY_VERSION`: version key.
- Không commit provider token, connection password, TOTP secret hoặc production key.
- Production yêu cầu PostgreSQL; development-only direct settlement route không được map trong Production.

## Tài liệu

- Product/PRD: `docs/product-blueprint.md`, `docs/prd.md`.
- Architecture/data: `docs/architecture-and-data.md`.
- Legal/market: `docs/legal-and-market-validation.md`.
- Security/privacy: `docs/security-and-privacy.md`.
- Operations: `docs/operations-and-runbooks.md`.
- Latest traceability: `docs/master-prompt-traceability-v3.md`.
- Latest audit: `docs/completion-audit-2026-08-24-v32.md`.

Các tích hợp production, tax rules/golden outcomes và legal sign-off chỉ được đánh dấu hoàn tất khi có authoritative evidence; dữ liệu demo không thay thế các đầu vào đó.

