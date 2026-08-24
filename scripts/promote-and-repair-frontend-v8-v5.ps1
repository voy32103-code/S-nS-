$ErrorActionPreference='Stop'
$cp1258=[Text.Encoding]::GetEncoding(1258);$cp1252=[Text.Encoding]::GetEncoding(1252);$utf8=[Text.Encoding]::UTF8
$chars=[Collections.Generic.HashSet[char]]::new()
foreach($n in 0x00C0..0x00FF){$ch=[char]$n;if([Globalization.CharUnicodeInfo]::GetUnicodeCategory($ch)-match '^UppercaseLetter$|^LowercaseLetter$'){$null=$chars.Add($ch)}}
foreach($n in @(0x0102,0x0103,0x0110,0x0111,0x0128,0x0129,0x0168,0x0169,0x01A0,0x01A1,0x01AF,0x01B0)+@(0x1EA0..0x1EF9)+@(0x00B7,0x2013,0x2014,0x2026,0x2192)){$null=$chars.Add([char]$n)}
$map=@{};foreach($ch in $chars){foreach($cp in @($cp1258,$cp1252)){$bad=$cp.GetString($utf8.GetBytes([string]$ch));if($bad-ne[string]$ch-and-not$bad.Contains([char]0xFFFD)){$map[$bad]=[string]$ch}}}
$keys=@($map.Keys|Sort-Object Length -Descending)
$files=@('frontend/index-v8.html','frontend/src/AppV8.tsx','frontend/src/ImportV8.tsx','frontend/src/OnboardingV8.tsx','frontend/src/WorkflowV9.tsx','frontend/src/AppV8.test.tsx')
foreach($path in $files){$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8;foreach($key in $keys){$text=$text.Replace($key,$map[$key])};[System.IO.File]::WriteAllText((Join-Path(Get-Location)$path),$text,[System.Text.UTF8Encoding]::new($false))}
$app=Get-Content -LiteralPath 'frontend/src/AppV8.tsx' -Raw -Encoding UTF8;$brand='S'+[char]0x00E0+'nS'+[char]0x1ED5;$badA=[string][char]0x0102;$badTone=([string][char]0x00E1)+([char]0x00BB);if(-not$app.Contains($brand)-or$app.Contains($badA)-or$app.Contains($badTone)){throw 'V8_VIETNAMESE_REPAIR_ASSERTION_FAILED'}
$package='frontend/package.json';$text=Get-Content -LiteralPath $package -Raw -Encoding UTF8;$text=$text.Replace('vite --config vite.v3.config.js','vite --config vite.v8.config.js').Replace('vite build --config vite.v3.config.js','vite build --config vite.v8.config.js');if(-not$text.Contains('vite.v8.config.js')){throw 'V8_PACKAGE_PROMOTION_FAILED'};[System.IO.File]::WriteAllText((Join-Path(Get-Location)$package),$text,[System.Text.UTF8Encoding]::new($false))
'FRONTEND_V8_PROMOTED_AND_REPAIRED_V5 files=6 scripts=2'
