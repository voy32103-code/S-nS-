param([string]$Path="backend/SanSo.Api.V6/PostgresSettlementImportStoreV1.cs")
$ErrorActionPreference="Stop";$text=Get-Content -LiteralPath $Path -Raw -Encoding UTF8
$text=$text.Replace('import.Currency,import.PaidAt);','import.Currency,import.PaidAt.ToUniversalTime());')
$text=$text.Replace('Hash(payload),import.PaidAt);','Hash(payload),import.PaidAt.ToUniversalTime());')
$text=$text.Replace('explanation,import.PaidAt);','explanation,import.PaidAt.ToUniversalTime());')
[System.IO.File]::WriteAllText((Join-Path(Get-Location)$Path),$text,[System.Text.UTF8Encoding]::new($false));"SETTLEMENT_TIMESTAMPTZ_UTC_FIXED=$Path"
