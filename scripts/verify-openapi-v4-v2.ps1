$ErrorActionPreference='Stop'
$workspace=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$contract=Get-Content -LiteralPath (Join-Path $workspace 'docs/openapi-v4.yaml') -Raw
$required=@('/health','/api/auth/login','/api/imports/preview','/api/imports/confirm','/api/dashboard','/api/orders','/api/reconciliations/current','/api/raw-events','/api/tax/periods/{period}/calculate','/api/refunds','/api/periods/{period}/lock','/api/inventory/{sku}/reserve','/api/team/invitations','/api/support/grants','/api/billing/trial','/api/copilot/explain','/api/onboarding','/api/onboarding/disclaimer','/api/notifications','/api/notifications/{id}/acknowledge','/api/exports/reconciliation.csv')
foreach($path in $required){if($contract -notmatch [regex]::Escape("  ${path}:")){throw "OPENAPI_REQUIRED_PATH_MISSING: $path"}}
foreach($marker in @('bearerAuth:','X-Tenant-Id','NEEDS_REVIEW','persisted','previewToken','checksum')){if($contract -notmatch [regex]::Escape($marker)){throw "OPENAPI_SAFETY_MARKER_MISSING: $marker"}}
$sourceFiles=@('backend/SanSo.Api.V4/ProgramFixed.cs','backend/SanSo.Api.V4/V4AuthorizedWorkflowComposition.cs','backend/SanSo.Api/ModuleEndpoints.cs','backend/SanSo.Api/IdentityEndpoints.cs')
$sources=($sourceFiles|ForEach-Object{Get-Content -LiteralPath (Join-Path $workspace $_) -Raw})-join "`n"
foreach($path in $required){if($sources -notmatch [regex]::Escape($path)){throw "OPENAPI_PATH_NOT_MAPPED_IN_SOURCE: $path"}}
Write-Output "OPENAPI_V4_VERIFIED paths=$($required.Count) safety_markers=6 source_files=$($sourceFiles.Count)"
