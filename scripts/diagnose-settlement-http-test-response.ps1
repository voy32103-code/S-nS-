param([string]$Path="backend/SanSo.Api.V6.Tests/PostgresSettlementImportHttpV10Tests.cs")
$ErrorActionPreference="Stop";$text=Get-Content -LiteralPath $Path -Raw -Encoding UTF8
$old='var first=await Upload(client,Csv(),token,tenant);var body=await first.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal(HttpStatusCode.OK,first.StatusCode);'
$new='var first=await Upload(client,Csv(),token,tenant);var firstText=await first.Content.ReadAsStringAsync();Assert.True(first.IsSuccessStatusCode,$"IMPORT_FAILED {(int)first.StatusCode}: {firstText}");var body=JsonSerializer.Deserialize<JsonElement>(firstText);'
if(-not$text.Contains($old)){throw'ASSERTION_ANCHOR_NOT_FOUND'};$text=$text.Replace($old,$new);[System.IO.File]::WriteAllText((Join-Path(Get-Location)$Path),$text,[System.Text.UTF8Encoding]::new($false));"SETTLEMENT_TEST_DIAGNOSTICS_ENABLED=$Path"
