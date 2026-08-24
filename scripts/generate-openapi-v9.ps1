param([string]$Source="docs/openapi-v8-canonical.json",[string]$Output="docs/openapi-v9-canonical.json")
$ErrorActionPreference="Stop"
$document=Get-Content -LiteralPath $Source -Raw -Encoding UTF8|ConvertFrom-Json
$document.info.title="SanSo API V9";$document.info.version="0.9.0-pilot";$document.info.description="Canonical V9 pilot contract. Durable encrypted onboarding, guarded import, inventory and notification inbox. No endpoint submits tax filings or tax payments."
$document.'x-sanso-canonical-entrypoint'="backend/SanSo.Api.V6/ProgramCanonicalV9.cs"
$tenant=@{'$ref'="#/components/parameters/TenantHeader"};$common=[ordered]@{'200'=@{description="Onboarding snapshot with explicit persisted flag"};'400'=@{'$ref'="#/components/responses/BadRequest"};'401'=@{'$ref'="#/components/responses/Unauthorized"};'403'=@{'$ref'="#/components/responses/Forbidden"};'503'=@{description="PostgreSQL is configured but field encryption key is absent"}}
$schemas=[ordered]@{
 BusinessProfileDraft=[ordered]@{type="object";required=@("subjectType","legalName","taxIdentifier","address");properties=[ordered]@{subjectType=@{type="string";enum=@("Individual","HouseholdBusiness","MicroEnterprise","Company","OtherNeedsReview")};legalName=@{type="string"};taxIdentifier=@{type="string";pattern="^(?:[0-9]{10}|[0-9]{13})$";writeOnly=$true};address=@{type="string";writeOnly=$true};currency=@{type="string";const="VND"};timeZone=@{type="string";const="Asia/Ho_Chi_Minh"}}}
 SourceSelection=[ordered]@{type="object";required=@("mode");properties=@{mode=@{type="string";enum=@("Demo","Csv","Shopee","TikTokShop")}}}
 BackfillSelection=[ordered]@{type="object";required=@("from");properties=@{from=@{type="string";format="date"}}}
 SkuMappingSelection=[ordered]@{type="object";required=@("mappedSkuCount");properties=@{mappedSkuCount=@{type="integer";minimum=0}}}
 OpeningBalancesSelection=[ordered]@{type="object";required=@("balances");properties=@{balances=@{type="array";items=@{type="object";required=@("canonicalSku","onHand");properties=@{canonicalSku=@{type="string"};onHand=@{type="integer";minimum=0};unitCostMinor=@{type=@("integer","null");minimum=0}}}}}}
 DisclaimerSelection=[ordered]@{type="object";required=@("version","explicitlyConfirmed");properties=@{version=@{type="string"};explicitlyConfirmed=@{type="boolean";const=$true}}}
 ActivationSelection=[ordered]@{type="object";required=@("reconciliationId","hasMatchedOrExplainedDiscrepancy");properties=@{reconciliationId=@{type="string";format="uuid"};hasMatchedOrExplainedDiscrepancy=@{type="boolean";const=$true}}}
}
foreach($p in $schemas.GetEnumerator()){$document.components.schemas|Add-Member $p.Key $p.Value -Force}
function Body($schema){return @{required=$true;content=@{'application/json'=@{schema=@{'$ref'="#/components/schemas/$schema"}}}}}
$document.paths.'/api/onboarding'.get.responses|Add-Member '503' @{description="FIELD_ENCRYPTION_NOT_CONFIGURED"} -Force
$routes=[ordered]@{
 '/api/onboarding/business-profile'='BusinessProfileDraft';'/api/onboarding/data-source'='SourceSelection';'/api/onboarding/backfill'='BackfillSelection';'/api/onboarding/sku-mapping'='SkuMappingSelection';'/api/onboarding/opening-balances'='OpeningBalancesSelection';'/api/onboarding/disclaimer'='DisclaimerSelection';'/api/onboarding/first-reconciliation'='ActivationSelection'
}
foreach($route in $routes.GetEnumerator()){$document.paths|Add-Member $route.Key ([ordered]@{post=[ordered]@{parameters=@($tenant);requestBody=(Body $route.Value);responses=$common}}) -Force}
$json=$document|ConvertTo-Json -Depth 100
[System.IO.File]::WriteAllText((Join-Path(Get-Location)$Output),$json+[Environment]::NewLine,[System.Text.UTF8Encoding]::new($false));"OPENAPI_V9_GENERATED=$Output"

