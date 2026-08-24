$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6.Tests/PostgresSettlementImportHttpV10Tests.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='var retry=await Upload(client,Csv(),token,tenant);var retryBody=await retry.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal(runId,retryBody.GetProperty("runId").GetString());Assert.True(retryBody.GetProperty("duplicate").GetBoolean());'
$new='var retry=await Upload(client,Csv(),token,tenant);var retryBody=await retry.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal(runId,retryBody.GetProperty("confirmedRunId").GetString());Assert.True(retryBody.GetProperty("alreadyConfirmed").GetBoolean());'
if(-not $text.Contains($old)){throw 'SETTLEMENT_RETRY_ASSERTION_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
$old='if(token is null||!preview.IsSuccessStatusCode)return preview;var p=await preview.Content.ReadFromJsonAsync<JsonElement>();preview.Dispose();using var confirm='
$new='if(token is null||!preview.IsSuccessStatusCode)return preview;var p=await preview.Content.ReadFromJsonAsync<JsonElement>();if(p.GetProperty("alreadyConfirmed").GetBoolean())return preview;preview.Dispose();using var confirm='
if(-not $text.Contains($old)){throw 'SETTLEMENT_PREVIEW_HELPER_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'SETTLEMENT_CONFIRMED_PREVIEW_TEST_ALIGNED=1'
