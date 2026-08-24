$ErrorActionPreference='Stop'
$program='backend/SanSo.Api.V6/ProgramCanonicalV13.cs'
$text=Get-Content -LiteralPath $program -Raw -Encoding UTF8
$old='Prefer(app.MapPost("/api/imports/settlements/direct", async'
$new='if(app.Environment.IsDevelopment())Prefer(app.MapPost("/api/imports/settlements/direct", async'
if(-not $text.Contains($old)){throw 'DIRECT_PROGRAM_MAP_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new).Replace('if(!app.Environment.IsDevelopment())return Results.NotFound();var principal=Require(r,"tax.review");','var principal=Require(r,"tax.review");')
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $program),$text,[System.Text.UTF8Encoding]::new($false))
$generator='scripts/generate-api-v13-entrypoint.ps1'
$text=Get-Content -LiteralPath $generator -Raw -Encoding UTF8
$old='$text=$text.Replace($directAnchor,''    if(!app.Environment.IsDevelopment())return Results.NotFound();var principal=Require(r,"tax.review");if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);'')'
$new='$text=$text.Replace($directAnchor,$directAnchor);$text=$text.Replace(''Prefer(app.MapPost("/api/imports/settlements/direct"'',''if(app.Environment.IsDevelopment())Prefer(app.MapPost("/api/imports/settlements/direct"'')'
if(-not $text.Contains($old)){throw 'DIRECT_GENERATOR_HARDENING_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
$text=$text.Replace('$anchor=''Prefer(app.MapPost("/api/imports/settlements/direct", async (HttpRequest r, CancellationToken ct) =>''','$anchor=''if(app.Environment.IsDevelopment())Prefer(app.MapPost("/api/imports/settlements/direct", async (HttpRequest r, CancellationToken ct) =>''')
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $generator),$text,[System.Text.UTF8Encoding]::new($false))
'DIRECT_ROUTE_PRODUCTION_MAPPING_DISABLED=2'
