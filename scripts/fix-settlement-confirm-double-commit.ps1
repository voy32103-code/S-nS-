$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6/SettlementImportWorkflowV1.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='if(status=="CONFIRMED"&&confirmedRun is not null){await tx.CommitAsync(ct);var existing=await importer.GetRun(tenant,confirmedRun,ct);if(existing is null)throw new InvalidOperationException("CONFIRMED_RUN_NOT_FOUND");}'
$new='if(status=="CONFIRMED"&&confirmedRun is null)throw new InvalidOperationException("CONFIRMED_RUN_NOT_FOUND");'
if(-not $text.Contains($old)){throw 'SETTLEMENT_DOUBLE_COMMIT_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'SETTLEMENT_CONFIRM_DOUBLE_COMMIT_FIXED=1'
