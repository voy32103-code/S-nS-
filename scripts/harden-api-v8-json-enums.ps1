param([string]$Path = "backend/SanSo.Api.V6/ProgramCanonicalV8.cs")

$ErrorActionPreference = "Stop"
$text = Get-Content -LiteralPath $Path -Raw
if (-not $text.Contains('using System.Text.Json.Serialization;')) {
    $text = $text.Replace('using System.Text;', 'using System.Text;' + [Environment]::NewLine + 'using System.Text.Json.Serialization;')
}
$anchor = 'var builder = WebApplication.CreateBuilder(args);'
$configuration = $anchor + [Environment]::NewLine + 'builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));'
if (-not $text.Contains('ConfigureHttpJsonOptions')) {
    if (-not $text.Contains($anchor)) { throw 'BUILDER_ANCHOR_NOT_FOUND' }
    $text = $text.Replace($anchor, $configuration)
}
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $Path), $text, [System.Text.UTF8Encoding]::new($false))
Write-Output "API_V8_STRING_ENUMS_ENABLED=$Path"
