$ErrorActionPreference='Stop'
$workspace=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$path=Join-Path $workspace 'docs/openapi-v4-canonical.json'
$raw=Get-Content -LiteralPath $path -Raw
$document=$raw|ConvertFrom-Json -Depth 100
if($document.openapi -ne '3.1.0'){throw 'OPENAPI_VERSION_INVALID'}
$required=@('/health','/api/auth/login','/api/imports/preview','/api/imports/confirm','/api/dashboard','/api/orders','/api/reconciliations/current','/api/raw-events','/api/tax/periods/{period}/calculate','/api/refunds','/api/periods/{period}/lock','/api/inventory/{sku}/reserve','/api/team/invitations','/api/support/grants','/api/billing/trial','/api/copilot/explain','/api/onboarding','/api/onboarding/disclaimer','/api/notifications','/api/notifications/{id}/acknowledge','/api/exports/reconciliation.csv')
$paths=@($document.paths.PSObject.Properties.Name)
foreach($item in $required){if($item -notin $paths){throw "OPENAPI_REQUIRED_PATH_MISSING: $item"}}
if(!$document.components.parameters.TenantHeader){throw 'OPENAPI_TENANT_PARAMETER_MISSING'}
if(!$document.components.securitySchemes.bearerAuth){throw 'OPENAPI_BEARER_SECURITY_MISSING'}
$refs=[regex]::Matches($raw,'"\$ref"\s*:\s*"#/components/([^"/]+)/([^"/]+)"')
foreach($match in $refs){$group=$match.Groups[1].Value;$name=$match.Groups[2].Value;if(!$document.components.$group.$name){throw "OPENAPI_DANGLING_REF: $group/$name"}}
Write-Output "OPENAPI_V4_CANONICAL_VERIFIED paths=$($paths.Count) refs=$($refs.Count) version=$($document.info.version)"
