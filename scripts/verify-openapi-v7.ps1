param([string]$Path = "docs/openapi-v7-canonical.json")

$ErrorActionPreference = "Stop"
$raw = Get-Content -LiteralPath $Path -Raw
$document = $raw | ConvertFrom-Json
if ($document.openapi -ne "3.1.0") { throw "OPENAPI_VERSION_INVALID" }
if ($document.info.version -ne "0.7.0-pilot") { throw "API_VERSION_INVALID" }
if ($document.'x-sanso-canonical-entrypoint' -ne "backend/SanSo.Api.V6/ProgramCanonicalV7.cs") { throw "ENTRYPOINT_INVALID" }

$required = @(
    "/health",
    "/api/auth/login",
    "/api/imports/preview",
    "/api/imports/confirm",
    "/api/dashboard",
    "/api/orders",
    "/api/reconciliations/current",
    "/api/inventory/{sku}",
    "/api/inventory/{sku}/reserve",
    "/api/inventory/{sku}/release"
)
foreach ($route in $required) {
    if ($null -eq $document.paths.$route) { throw "REQUIRED_ROUTE_MISSING:$route" }
}

$file = $document.paths.'/api/imports/preview'.post.requestBody.content.'multipart/form-data'.schema.properties.file
if ($file.maxLength -ne 10485760) { throw "IMPORT_SIZE_CONTRACT_MISSING" }
if ($null -eq $document.components.schemas.InventorySnapshot) { throw "INVENTORY_SCHEMA_MISSING" }
if ($null -eq $document.components.schemas.InventoryMutationRequest) { throw "INVENTORY_MUTATION_SCHEMA_MISSING" }

$refs = [regex]::Matches($raw, '"\$ref"\s*:\s*"#/components/([^/]+)/([^"/]+)"')
foreach ($match in $refs) {
    $section = $match.Groups[1].Value
    $name = $match.Groups[2].Value
    if ($null -eq $document.components.$section.$name) { throw "BROKEN_REF:$section/$name" }
}

Write-Output "OPENAPI_V7_VERIFIED paths=$($document.paths.PSObject.Properties.Count) refs=$($refs.Count) max_upload=$($file.maxLength)"
