param(
    [string]$Source = "docs/openapi-v4-canonical.json",
    [string]$Output = "docs/openapi-v8-canonical.json"
)
$ErrorActionPreference = "Stop"
$document = Get-Content -LiteralPath $Source -Raw -Encoding UTF8 | ConvertFrom-Json
$document.info.title = "SanSo API V8"
$document.info.version = "0.8.0-pilot"
$document.info.description = "Canonical V8 pilot contract. Import and notification persistence are guarded. No endpoint submits tax filings or tax payments. Tax results may be NEEDS_REVIEW."
$document | Add-Member -NotePropertyName "x-sanso-canonical-entrypoint" -NotePropertyValue "backend/SanSo.Api.V6/ProgramCanonicalV8.cs" -Force

$file = $document.paths.'/api/imports/preview'.post.requestBody.content.'multipart/form-data'.schema.properties.file
$file | Add-Member maxLength 10485760 -Force
$file | Add-Member description "CSV or XLSX allowlisted by extension and MIME; maximum 10 MiB; CSV must be UTF-8." -Force
$document.paths.'/api/imports/preview'.post.responses | Add-Member '400' @{ description = "Safe import validation error" } -Force

$tenant = @{ '$ref' = "#/components/parameters/TenantHeader" }
$sku = @{ name = "sku"; in = "path"; required = $true; schema = @{ type = "string" } }
$inventory = [ordered]@{ type="object"; required=@("sku","onHand","reserved","quarantine","available","version"); properties=[ordered]@{ sku=@{type="string"};onHand=@{type="integer"};reserved=@{type="integer"};quarantine=@{type="integer"};available=@{type="integer"};version=@{type="integer";format="int64"} } }
$mutation = [ordered]@{ type="object";required=@("quantity","sourceKey");properties=[ordered]@{quantity=@{type="integer";minimum=1};sourceKey=@{type="string";minLength=1}} }
$document.components.schemas | Add-Member InventorySnapshot $inventory -Force
$document.components.schemas | Add-Member InventoryMutationRequest $mutation -Force
$inventoryResponse = @{description="Durable inventory snapshot";content=@{'application/json'=@{schema=@{'$ref'="#/components/schemas/InventorySnapshot"}}}}
$mutationBody = @{required=$true;content=@{'application/json'=@{schema=@{'$ref'="#/components/schemas/InventoryMutationRequest"}}}}
$document.paths | Add-Member '/api/inventory/{sku}' ([ordered]@{get=[ordered]@{parameters=@($tenant,$sku);responses=[ordered]@{'200'=$inventoryResponse;'401'=@{'$ref'="#/components/responses/Unauthorized"};'403'=@{'$ref'="#/components/responses/Forbidden"};'503'=@{'$ref'="#/components/responses/ServiceUnavailable"}}}}) -Force
$reserve=$document.paths.'/api/inventory/{sku}/reserve'.post
$reserve | Add-Member requestBody $mutationBody -Force
$reserve.responses | Add-Member '200' $inventoryResponse -Force
$reserve.responses | Add-Member '401' @{'$ref'="#/components/responses/Unauthorized"} -Force
$reserve.responses | Add-Member '403' @{'$ref'="#/components/responses/Forbidden"} -Force
$reserve.responses | Add-Member '503' @{'$ref'="#/components/responses/ServiceUnavailable"} -Force
$document.paths | Add-Member '/api/inventory/{sku}/release' ([ordered]@{post=[ordered]@{parameters=@($tenant,$sku);requestBody=$mutationBody;responses=[ordered]@{'200'=$inventoryResponse;'400'=@{'$ref'="#/components/responses/BadRequest"};'401'=@{'$ref'="#/components/responses/Unauthorized"};'403'=@{'$ref'="#/components/responses/Forbidden"};'503'=@{'$ref'="#/components/responses/ServiceUnavailable"}}}}) -Force

$notificationRequest=[ordered]@{type="object";required=@("type","channel","recipient","resourceRef","windowStart");properties=[ordered]@{type=@{type="string";enum=@("SyncFailure","LargeDiscrepancy","LowStock","PeriodNeedsReview")};channel=@{type="string";enum=@("InApp","Email")};recipient=@{type="string"};resourceRef=@{type="string";maxLength=100};windowStart=@{type="string";format="date-time"}}}
$notification=[ordered]@{type="object";required=@("id","type","channel","recipientMasked","resourceRef","dedupeKey","title","body","status","attempt","createdAt");properties=[ordered]@{id=@{type="string";format="uuid"};type=$notificationRequest.properties.type;channel=$notificationRequest.properties.channel;recipientMasked=@{type="string"};resourceRef=@{type="string"};dedupeKey=@{type="string"};title=@{type="string"};body=@{type="string"};status=@{type="string";enum=@("Pending","Delivered","RetryScheduled","DeadLetter","Acknowledged")};attempt=@{type="integer"};createdAt=@{type="string";format="date-time"}}}
$document.components.schemas | Add-Member NotificationRequest $notificationRequest -Force
$document.components.schemas | Add-Member NotificationDelivery $notification -Force
$notificationPost=$document.paths.'/api/notifications'.post
$notificationPost | Add-Member requestBody @{required=$true;content=@{'application/json'=@{schema=@{'$ref'="#/components/schemas/NotificationRequest"}}}} -Force
$notificationPost.responses | Add-Member '503' @{description="Email provider or production dependency is not configured"} -Force

$json=$document|ConvertTo-Json -Depth 100
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $Output),$json+[Environment]::NewLine,[System.Text.UTF8Encoding]::new($false))
Write-Output "OPENAPI_V8_GENERATED=$Output"

