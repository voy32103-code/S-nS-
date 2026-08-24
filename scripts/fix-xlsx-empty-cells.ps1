$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6/SettlementImportWorkflowV1.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='var values=new string[20];var last=-1;'
$new='var values=Enumerable.Repeat("",20).ToArray();var last=-1;'
if(-not $text.Contains($old)){throw 'XLSX_EMPTY_CELL_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'XLSX_EMPTY_CELLS_FIXED=1'
