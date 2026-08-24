param([string]$Path="docs/openapi-v8-canonical.json",[string]$EntryPoint="backend/SanSo.Api.V6/ProgramCanonicalV8.cs")
$ErrorActionPreference="Stop"
$raw=Get-Content -LiteralPath $Path -Raw -Encoding UTF8
$source=Get-Content -LiteralPath $EntryPoint -Raw -Encoding UTF8
$document=$raw|ConvertFrom-Json
if($document.openapi-ne"3.1.0"){throw"OPENAPI_VERSION_INVALID"}
if($document.info.version-ne"0.8.0-pilot"){throw"API_VERSION_INVALID"}
if($document.'x-sanso-canonical-entrypoint'-ne$EntryPoint){throw"ENTRYPOINT_INVALID"}
if($raw.Contains("SĂ")){throw"MOJIBAKE_DETECTED"}
$required=@("/health","/api/auth/login","/api/imports/preview","/api/imports/confirm","/api/dashboard","/api/orders","/api/reconciliations/current","/api/inventory/{sku}","/api/inventory/{sku}/reserve","/api/inventory/{sku}/release","/api/notifications","/api/notifications/{id}/acknowledge")
foreach($route in $required){if($null-eq$document.paths.$route){throw"ROUTE_MISSING:$route"};if($route-ne"/api/auth/login"-and-not$source.Contains($route)){throw"ROUTE_NOT_IN_SOURCE:$route"}}
if($document.paths.'/api/imports/preview'.post.requestBody.content.'multipart/form-data'.schema.properties.file.maxLength-ne10485760){throw"UPLOAD_LIMIT_MISSING"}
foreach($schema in @("InventorySnapshot","InventoryMutationRequest","NotificationRequest","NotificationDelivery")){if($null-eq$document.components.schemas.$schema){throw"SCHEMA_MISSING:$schema"}}
$refs=[regex]::Matches($raw,'"\$ref"\s*:\s*"#/components/([^/]+)/([^"/]+)"')
foreach($m in $refs){if($null-eq$document.components.($m.Groups[1].Value).($m.Groups[2].Value)){throw"BROKEN_REF:$($m.Value)"}}
Write-Output "OPENAPI_V8_VERIFIED paths=$(@($document.paths.PSObject.Properties).Count) refs=$($refs.Count) routes=$($required.Count) utf8=true"
