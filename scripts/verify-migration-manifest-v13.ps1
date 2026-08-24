param([string]$Project='backend/SanSo.Migrator.V8/SanSo.Migrator.V8.csproj')
$ErrorActionPreference='Stop';$xml=[xml](Get-Content -LiteralPath $Project -Raw -Encoding UTF8);$names=@($xml.Project.ItemGroup.None|Where-Object{$_.Link-like'Migrations/*'}|ForEach-Object{[string]$_.Link}|Sort-Object)
if($names.Count-ne15){throw "MIGRATION_COUNT_INVALID:$($names.Count)"};for($i=0;$i-lt15;$i++){if(-not$names[$i].StartsWith(('Migrations/{0:D3}_'-f($i+1)),[StringComparison]::Ordinal)){throw "MIGRATION_SEQUENCE_INVALID:$($names[$i])"}}
"MIGRATION_MANIFEST_V13_VERIFIED count=15 first=$($names[0]) last=$($names[-1])"
