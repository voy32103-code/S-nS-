$ErrorActionPreference='Stop'
$path='frontend/package.json'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$old='"test":"vitest run"'
$new='"test":"vitest run --config vitest.v8.config.ts"'
if(-not $text.Contains($old)){throw 'FRONTEND_TEST_SCRIPT_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'FRONTEND_CANONICAL_TEST_CONFIG_FIXED=1'
