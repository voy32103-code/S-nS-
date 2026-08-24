param([string]$Configuration = "Debug")

$ErrorActionPreference = "Stop"
$workspace = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$migrationPath = Join-Path $workspace "backend/SanSo.Migrator/bin/$Configuration/net9.0/Migrations"
if (!(Test-Path -LiteralPath $migrationPath)) { throw "MIGRATION_OUTPUT_NOT_FOUND: build SanSo.Migrator first" }

$files = @(Get-ChildItem -LiteralPath $migrationPath -Filter *.sql -File | Sort-Object Name)
if ($files.Count -eq 0) { throw "MIGRATION_MANIFEST_EMPTY" }

$versions = @($files | ForEach-Object {
    if ($_.Name -notmatch '^([0-9]{3})_[a-z0-9_]+\.sql$') { throw "MIGRATION_NAME_INVALID: $($_.Name)" }
    [int]$Matches[1]
})

for ($index = 0; $index -lt $versions.Count; $index++) {
    $expected = $index + 1
    if ($versions[$index] -ne $expected) { throw "MIGRATION_SEQUENCE_GAP: expected $expected got $($versions[$index])" }
}

$required = @(
    @{ File = '002_tenant_guards.sql'; Pattern = 'FORCE ROW LEVEL SECURITY' },
    @{ File = '005_import_confirmation.sql'; Pattern = 'import_batches_tenant' },
    @{ File = '006_onboarding_profiles.sql'; Pattern = 'onboarding_profiles_tenant' },
    @{ File = '007_notification_delivery_guards.sql'; Pattern = 'notification_deliveries_tenant' }
)
foreach ($guard in $required) {
    $path = Join-Path $migrationPath $guard.File
    if (!(Test-Path -LiteralPath $path)) { throw "REQUIRED_MIGRATION_MISSING: $($guard.File)" }
    $sql = Get-Content -LiteralPath $path -Raw
    if ($sql -notmatch [regex]::Escape($guard.Pattern)) { throw "MIGRATION_GUARD_MISSING: $($guard.File) $($guard.Pattern)" }
}

Write-Output "MIGRATION_MANIFEST_VERIFIED count=$($files.Count) range=$($versions[0])-$($versions[-1])"
