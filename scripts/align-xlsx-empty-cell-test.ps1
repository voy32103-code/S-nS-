$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6.Tests/SettlementFileParserV13Tests.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='Assert.Equal("",parsed.Rows[0].OrderCode);'
$new='Assert.True(string.IsNullOrEmpty(parsed.Rows[0].OrderCode));'
if(-not $text.Contains($old)){throw 'XLSX_TEST_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'XLSX_EMPTY_CELL_ASSERTION_ALIGNED=1'
