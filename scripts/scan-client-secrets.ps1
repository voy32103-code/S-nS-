param(
    [string]$FrontendDist = "frontend/dist-v3",
    [string[]]$LogRoots = @("frontend", "e2e", "backend")
)

$ErrorActionPreference = "Stop"
$workspace = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$targets = New-Object System.Collections.Generic.List[string]

$distPath = Join-Path $workspace $FrontendDist
if (Test-Path -LiteralPath $distPath) {
    Get-ChildItem -LiteralPath $distPath -Recurse -File | ForEach-Object { $targets.Add($_.FullName) }
}

foreach ($root in $LogRoots) {
    $path = Join-Path $workspace $root
    if (Test-Path -LiteralPath $path) {
        Get-ChildItem -LiteralPath $path -Recurse -File -Include *.log,*.out,*.err |
            Where-Object { $_.FullName -notmatch '[\\/](node_modules|bin|obj|test-results|playwright-report)[\\/]' } |
            ForEach-Object { $targets.Add($_.FullName) }
    }
}

$rules = [ordered]@{
    AwsAccessKey = 'AKIA[0-9A-Z]{16}'
    PrivateKey = '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----'
    Jwt = 'eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}'
    ConnectionPassword = '(?i)(password|pwd)\s*[=:]\s*[^;\s"'']{6,}'
    BearerToken = '(?i)bearer\s+[A-Za-z0-9._~+/=-]{20,}'
    VietnamCitizenId = '(?<!\d)(0\d{11})(?!\d)'
}

$findings = New-Object System.Collections.Generic.List[object]
foreach ($file in $targets | Select-Object -Unique) {
    $content = Get-Content -LiteralPath $file -Raw -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }
    foreach ($rule in $rules.GetEnumerator()) {
        if ($content -match $rule.Value) {
            $findings.Add([pscustomobject]@{ Rule = $rule.Key; File = $file })
        }
    }
}

if ($findings.Count -gt 0) {
    $findings | Sort-Object Rule, File | Format-Table -AutoSize
    throw "SECRET_OR_PII_SCAN_FAILED: $($findings.Count) finding(s)"
}

Write-Output "SECRET_OR_PII_SCAN_PASSED files=$($targets.Count) rules=$($rules.Count)"
