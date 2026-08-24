param(
    [string]$Source = "docs/openapi-v4-canonical.json",
    [string]$Output = "docs/openapi-v7-canonical.json"
)

$ErrorActionPreference = "Stop"
$document = Get-Content -LiteralPath $Source -Raw | ConvertFrom-Json
$document.info.title = "SànSổ API V7"
$document.info.version = "0.7.0-pilot"
$document.info.description = "Canonical V7 pilot contract. Import preview is size/MIME/UTF-8 guarded. No endpoint submits tax filings or tax payments. Tax results may be NEEDS_REVIEW."
$document | Add-Member -NotePropertyName "x-sanso-canonical-entrypoint" -NotePropertyValue "backend/SanSo.Api.V6/ProgramCanonicalV7.cs" -Force

$fileSchema = $document.paths.'/api/imports/preview'.post.requestBody.content.'multipart/form-data'.schema.properties.file
$fileSchema | Add-Member -NotePropertyName maxLength -NotePropertyValue 10485760 -Force
$fileSchema | Add-Member -NotePropertyName description -NotePropertyValue "CSV or XLSX allowlisted by extension and MIME; maximum 10 MiB; CSV must be UTF-8." -Force
$document.paths.'/api/imports/preview'.post.responses | Add-Member -NotePropertyName '400' -NotePropertyValue @{ description = "MULTIPART_REQUIRED, FILE_REQUIRED, FILE_TOO_LARGE, UNSUPPORTED_FILE_TYPE, ENCODING_INVALID, or safe validation error" } -Force

$inventorySchema = [ordered]@{ type = "object"; required = @("sku", "onHand", "reserved", "quarantine", "available", "version"); properties = [ordered]@{ sku = @{ type = "string" }; onHand = @{ type = "integer" }; reserved = @{ type = "integer" }; quarantine = @{ type = "integer" }; available = @{ type = "integer"; description = "onHand - reserved - quarantine" }; version = @{ type = "integer"; format = "int64" } } }
$mutationSchema = [ordered]@{ type = "object"; required = @("quantity", "sourceKey"); properties = [ordered]@{ quantity = @{ type = "integer"; minimum = 1 }; sourceKey = @{ type = "string"; minLength = 1; description = "Idempotency key scoped to tenant/SKU/operation" } } }
$document.components.schemas | Add-Member -NotePropertyName InventorySnapshot -NotePropertyValue $inventorySchema -Force
$document.components.schemas | Add-Member -NotePropertyName InventoryMutationRequest -NotePropertyValue $mutationSchema -Force

$tenant = @{ '$ref' = "#/components/parameters/TenantHeader" }
$sku = @{ name = "sku"; in = "path"; required = $true; schema = @{ type = "string" } }
$inventoryResponse = @{ description = "Durable inventory snapshot"; content = @{ 'application/json' = @{ schema = @{ '$ref' = "#/components/schemas/InventorySnapshot" } } } }
$mutationBody = @{ required = $true; content = @{ 'application/json' = @{ schema = @{ '$ref' = "#/components/schemas/InventoryMutationRequest" } } } }

$document.paths | Add-Member -NotePropertyName '/api/inventory/{sku}' -NotePropertyValue ([ordered]@{ get = [ordered]@{ parameters = @($tenant, $sku); responses = [ordered]@{ '200' = $inventoryResponse; '401' = @{ '$ref' = "#/components/responses/Unauthorized" }; '403' = @{ '$ref' = "#/components/responses/Forbidden" }; '503' = @{ '$ref' = "#/components/responses/ServiceUnavailable" } } } }) -Force

$reserve = $document.paths.'/api/inventory/{sku}/reserve'.post
$reserve | Add-Member -NotePropertyName requestBody -NotePropertyValue $mutationBody -Force
$reserve.responses | Add-Member -NotePropertyName '200' -NotePropertyValue $inventoryResponse -Force
$reserve.responses | Add-Member -NotePropertyName '401' -NotePropertyValue @{ '$ref' = "#/components/responses/Unauthorized" } -Force
$reserve.responses | Add-Member -NotePropertyName '403' -NotePropertyValue @{ '$ref' = "#/components/responses/Forbidden" } -Force
$reserve.responses | Add-Member -NotePropertyName '503' -NotePropertyValue @{ '$ref' = "#/components/responses/ServiceUnavailable" } -Force

$document.paths | Add-Member -NotePropertyName '/api/inventory/{sku}/release' -NotePropertyValue ([ordered]@{ post = [ordered]@{ parameters = @($tenant, $sku); requestBody = $mutationBody; responses = [ordered]@{ '200' = $inventoryResponse; '400' = @{ '$ref' = "#/components/responses/BadRequest" }; '401' = @{ '$ref' = "#/components/responses/Unauthorized" }; '403' = @{ '$ref' = "#/components/responses/Forbidden" }; '503' = @{ '$ref' = "#/components/responses/ServiceUnavailable" } } } }) -Force

$json = $document | ConvertTo-Json -Depth 100
$absoluteOutput = Join-Path (Get-Location) $Output
[System.IO.File]::WriteAllText($absoluteOutput, $json + [Environment]::NewLine, [System.Text.UTF8Encoding]::new($false))
Write-Output "OPENAPI_V7_GENERATED=$Output"
