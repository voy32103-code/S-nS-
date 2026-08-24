$ErrorActionPreference='Stop'
$config='frontend/tsconfig.json';$text=Get-Content -LiteralPath $config -Raw -Encoding UTF8;$old='"include":["src"]}';$new='"include":["src"],"exclude":["src/**/*.test.ts","src/**/*.test.tsx"]}'
if(-not $text.Contains($old)){throw 'TSCONFIG_EXCLUDE_ANCHOR_NOT_FOUND'};$text=$text.Replace($old,$new);[System.IO.File]::WriteAllText((Join-Path(Get-Location)$config),$text,[System.Text.UTF8Encoding]::new($false))
foreach($path in @('frontend/src/OnboardingV5.tsx','frontend/src/OnboardingV6.tsx')){$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8;$old='useEffect(load,[session]);';$new='useEffect(()=>{void load()},[session]);';if(-not$text.Contains($old)){throw "ONBOARDING_EFFECT_ANCHOR_NOT_FOUND:$path"};$text=$text.Replace($old,$new);[System.IO.File]::WriteAllText((Join-Path(Get-Location)$path),$text,[System.Text.UTF8Encoding]::new($false))}
'FRONTEND_PRODUCTION_TYPES_FIXED=3'
