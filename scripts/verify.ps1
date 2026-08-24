$ErrorActionPreference='Stop'
dotnet build SanSo.sln
dotnet test SanSo.sln --no-build
Push-Location frontend
try { npm ci; npm test; npm run build; npm audit --audit-level=high } finally { Pop-Location }
Push-Location e2e
try { npm ci; npm test; npm run test:browser; npm audit --audit-level=high } finally { Pop-Location }
Write-Host 'Verification completed. PostgreSQL integration requires SANSO_POSTGRES and a running migrated database.'
