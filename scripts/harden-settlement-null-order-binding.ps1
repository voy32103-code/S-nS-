param([string]$Path="backend/SanSo.Api.V6/PostgresSettlementImportStoreV1.cs")
$ErrorActionPreference="Stop";$text=Get-Content -LiteralPath $Path -Raw -Encoding UTF8
$text=$text.Replace('VALUES($1,$2::uuid,$3::uuid,$4::uuid,$5,$6,$7,$8,$9,$10)','VALUES($1,NULLIF($2,'''')::uuid,$3::uuid,$4::uuid,$5,$6,$7,$8,$9,$10)')
$text=$text.Replace('(object?)orderId??DBNull.Value,rawId','orderId??"",rawId')
$text=$text.Replace('VALUES($1,$2::uuid,$3::uuid,$4,$5,$6,$7,$8,$9)','VALUES($1,$2::uuid,$3::uuid,$4,$5,$6,$7,$8,NULLIF($9,'''') )')
$text=$text.Replace('(object?)item.Row.OrderCode??DBNull.Value','item.Row.OrderCode??""')
[System.IO.File]::WriteAllText((Join-Path(Get-Location)$Path),$text,[System.Text.UTF8Encoding]::new($false));"SETTLEMENT_NULL_ORDER_BINDING_HARDENED=$Path"
