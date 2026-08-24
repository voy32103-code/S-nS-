param([string]$Path="backend/SanSo.Api.V6/PostgresSettlementImportStoreV1.cs")
$ErrorActionPreference="Stop";$text=Get-Content -LiteralPath $Path -Raw -Encoding UTF8
$old='RawSourceEventId=lr.GetString(9)});await tx.CommitAsync(ct);'
$new='RawSourceEventId=lr.GetString(9)});await lr.CloseAsync();await tx.CommitAsync(ct);'
if(-not$text.Contains($old)){throw'READER_LIFETIME_ANCHOR_NOT_FOUND'};$text=$text.Replace($old,$new);[System.IO.File]::WriteAllText((Join-Path(Get-Location)$Path),$text,[System.Text.UTF8Encoding]::new($false));"SETTLEMENT_DETAIL_READER_LIFETIME_FIXED=$Path"
