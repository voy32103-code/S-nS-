$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6.Tests/PostgresSettlementImportHttpV10Tests.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='if(p.GetProperty("alreadyConfirmed").GetBoolean())return preview;preview.Dispose();'
$new='if(p.GetProperty("alreadyConfirmed").GetBoolean()){var confirmed=new HttpResponseMessage(preview.StatusCode){Content=JsonContent.Create(p)};preview.Dispose();return confirmed;}preview.Dispose();'
if(-not $text.Contains($old)){throw 'CONFIRMED_PREVIEW_RESPONSE_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'CONFIRMED_PREVIEW_TEST_RESPONSE_FIXED=1'
