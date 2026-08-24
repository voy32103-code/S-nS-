param([string]$Project = "backend/SanSo.Migrator.V5/SanSo.Migrator.V5.csproj")
$ErrorActionPreference = "Stop"
$xml = [xml](Get-Content -LiteralPath $Project -Raw -Encoding UTF8)
$items = @($xml.Project.ItemGroup.None | Where-Object { $_.Link -like "Migrations/*" })
$names = @($items | ForEach-Object { [string]$_.Link } | Sort-Object)
$expected = 1..12 | ForEach-Object { "Migrations/{0:D3}_" -f $_ }
if ($names.Count -ne 12) { throw "MIGRATION_COUNT_INVALID:$($names.Count)" }
for ($i=0; $i -lt 12; $i++) {
    if (-not $names[$i].StartsWith($expected[$i], [StringComparison]::Ordinal)) { throw "MIGRATION_SEQUENCE_INVALID:$($names[$i])" }
}
if (($names | Select-Object -Unique).Count -ne 12) { throw "MIGRATION_DUPLICATE" }
Write-Output "MIGRATION_MANIFEST_V8_VERIFIED count=12 first=$($names[0]) last=$($names[-1])"

