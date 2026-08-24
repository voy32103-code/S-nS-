param([string]$Configuration = "Debug")

$ErrorActionPreference = "Stop"
$workspace = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$path = Join-Path $workspace "backend/SanSo.Migrator.V2/bin/$Configuration/net9.0/Migrations"
if (!(Test-Path -LiteralPath $path)) { throw "MIGRATOR_V2_OUTPUT_NOT_FOUND" }
$files = @(Get-ChildItem -LiteralPath $path -Filter *.sql -File | Sort-Object Name)
$expected = 1..8 | ForEach-Object { $_.ToString('000') }
$actual = @($files | ForEach-Object { if ($_.Name -notmatch '^([0-9]{3})_') { throw "MIGRATION_NAME_INVALID: $($_.Name)" }; $Matches[1] })
if (($actual -join ',') -ne ($expected -join ',')) { throw "MIGRATION_SEQUENCE_INVALID: $($actual -join ',')" }

$all = $files | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw }
$joined = $all -join "`n"
foreach ($required in @('import_staging_batches','REFERENCES reconciliation_runs(id)','notification_deliveries_tenant','FORCE ROW LEVEL SECURITY','tenant_raw_events_v2','tenant_audit_logs_v2')) {
    if ($joined -notmatch [regex]::Escape($required)) { throw "MIGRATION_V2_GUARD_MISSING: $required" }
}
$migration005 = Get-Content -LiteralPath (Join-Path $path '005_import_staging.sql') -Raw
if ($migration005 -match 'CREATE TABLE\s+import_batches\s*\(') { throw "MIGRATION_V2_LEGACY_TABLE_COLLISION" }
$migration006 = Get-Content -LiteralPath (Join-Path $path '006_onboarding_profiles.sql') -Raw
if ($migration006 -match 'REFERENCES\s+reconciliations\s*\(') { throw "MIGRATION_V2_INVALID_RECONCILIATION_FK" }

Write-Output "MIGRATION_MANIFEST_V2_VERIFIED count=$($files.Count) range=$($actual[0])-$($actual[-1])"
