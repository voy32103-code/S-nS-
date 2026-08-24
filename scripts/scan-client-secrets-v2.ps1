param([string]$FrontendDist = "frontend/dist")

$ErrorActionPreference = "Stop"
$workspace = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$targets = New-Object System.Collections.Generic.List[string]
$distPath = Join-Path $workspace $FrontendDist
if (!(Test-Path -LiteralPath $distPath)) { throw "FRONTEND_DIST_NOT_FOUND: $distPath" }
Get-ChildItem -LiteralPath $distPath -Recurse -File | ForEach-Object { $targets.Add($_.FullName) }

foreach ($rootName in @("frontend", "e2e", "backend")) {
    $root = Join-Path $workspace $rootName
    Get-ChildItem -LiteralPath $root -Recurse -File |
        Where-Object {
            $_.Extension -in @('.log', '.out', '.err') -and
            $_.FullName -notmatch '[\\/](node_modules|bin|obj|test-results|playwright-report|dist)[\\/]'
        } |
        ForEach-Object { $targets.Add($_.FullName) }
}

$rules = [ordered]@{
    AwsAccessKey = 'AKIA[0-9A-Z]{16}'
    PrivateKey = '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----'
    Jwt = '(?<![A-Za-z0-9_-])eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}(?![A-Za-z0-9_-])'
    ConnectionStringPassword = '(?is)(Host|Server|Data Source)\s*=.{0,240}?(Password|Pwd)\s*=\s*[^;\s"'']{6,}'
    BearerToken = '(?i)Authorization["'']?\s*[:=]\s*["'']?Bearer\s+[A-Za-z0-9._~+/=-]{20,}'
    VietnamCitizenId = '(?<![0-9A-Fa-f-])0\d{11}(?![0-9A-Fa-f-])'
}

$findings = foreach ($file in $targets | Select-Object -Unique) {
    $content = Get-Content -LiteralPath $file -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }
    foreach ($rule in $rules.GetEnumerator()) {
        if ($content -match $rule.Value) { [pscustomobject]@{ Rule = $rule.Key; File = $file } }
    }
}

if ($findings) {
    $findings | Sort-Object Rule, File | Format-Table -AutoSize
    throw "SECRET_OR_PII_SCAN_FAILED: $(@($findings).Count) finding(s)"
}
Write-Output "SECRET_OR_PII_SCAN_PASSED files=$(@($targets | Select-Object -Unique).Count) rules=$($rules.Count)"
