param([string]$Path="backend/SanSo.Api.V6.Tests/PostgresSettlementImportHttpV10Tests.cs")
$ErrorActionPreference="Stop";$text=Get-Content -LiteralPath $Path -Raw -Encoding UTF8
$old='using var detail=await client.SendAsync(get);var detailBody=await detail.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal(HttpStatusCode.OK,detail.StatusCode);'
$new='using var detail=await client.SendAsync(get);var detailText=await detail.Content.ReadAsStringAsync();Assert.True(detail.IsSuccessStatusCode,$"DETAIL_FAILED {(int)detail.StatusCode}: {detailText}");var detailBody=JsonSerializer.Deserialize<JsonElement>(detailText);'
if(-not$text.Contains($old)){throw'DETAIL_ASSERTION_ANCHOR_NOT_FOUND'};$text=$text.Replace($old,$new);[System.IO.File]::WriteAllText((Join-Path(Get-Location)$Path),$text,[System.Text.UTF8Encoding]::new($false));"SETTLEMENT_DETAIL_DIAGNOSTICS_ENABLED=$Path"
