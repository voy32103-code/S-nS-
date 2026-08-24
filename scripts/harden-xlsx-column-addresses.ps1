$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6/SettlementImportWorkflowV1.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='var values=cells.Select(x=>Value(x,shared)).ToList();output.AppendLine(string.Join('','',values.Select(Escape)));'
$new='var values=new string[20];var last=-1;foreach(var cell in cells){var index=ColumnIndex(cell.CellReference?.Value);if(index<0||index>=20)throw new InvalidDataException("COLUMN_LIMIT_EXCEEDED");values[index]=Value(cell,shared);last=Math.Max(last,index);}output.AppendLine(string.Join('','',values.Take(last+1).Select(Escape)));'
if(-not $text.Contains($old)){throw 'XLSX_CELL_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
$anchor='    private static string Escape(string value)=>''"''+value.Replace("\"","\"\"")+''"'';'
$insert='    private static int ColumnIndex(string? reference){if(string.IsNullOrWhiteSpace(reference))return -1;var value=0;foreach(var ch in reference){if(!char.IsLetter(ch))break;value=checked(value*26+(char.ToUpperInvariant(ch)-''A''+1));}return value-1;}'
if(-not $text.Contains($anchor)){throw 'XLSX_METHOD_ANCHOR_NOT_FOUND'}
$text=$text.Replace($anchor,$insert+[Environment]::NewLine+$anchor)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'XLSX_COLUMN_ADDRESS_HARDENED=1'
