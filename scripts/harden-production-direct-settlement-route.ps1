$ErrorActionPreference='Stop'
$program='backend/SanSo.Api.V6/ProgramCanonicalV13.cs'
$text=Get-Content -LiteralPath $program -Raw -Encoding UTF8
$old='var principal=Require(r,"tax.review");if(!app.Environment.IsDevelopment())return Results.NotFound();'
$new='if(!app.Environment.IsDevelopment())return Results.NotFound();var principal=Require(r,"tax.review");'
if(-not $text.Contains($old)){throw 'DIRECT_ROUTE_PROGRAM_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $program),$text,[System.Text.UTF8Encoding]::new($false))
$generator='scripts/generate-api-v13-entrypoint.ps1'
$text=Get-Content -LiteralPath $generator -Raw -Encoding UTF8
$old='var principal=Require(r,"tax.review");if(!app.Environment.IsDevelopment())return Results.NotFound();'
$new='if(!app.Environment.IsDevelopment())return Results.NotFound();var principal=Require(r,"tax.review");'
if(-not $text.Contains($old)){throw 'DIRECT_ROUTE_GENERATOR_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $generator),$text,[System.Text.UTF8Encoding]::new($false))
'PRODUCTION_DIRECT_ROUTE_HARDENED=2'
