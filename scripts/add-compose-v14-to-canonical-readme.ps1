$ErrorActionPreference='Stop';$path='README.md';$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8;$anchor='## Security-sensitive configuration';$section=@'
## Docker Compose V14

```powershell
Copy-Item .env.example .env
# thay local passwords; không dùng local values ở production
docker compose config
docker compose up --build
```

Compose gồm PostgreSQL, Redis, migrator V8, API V14 và frontend V8. Static config đã pass; runtime trên máy kiểm chứng hiện bị chặn vì Docker Desktop daemon không start. Xem `docs/phase-42-docker-compose-v14.md`.

'@;if(-not$text.Contains($anchor)){throw 'README_SECURITY_ANCHOR_NOT_FOUND'};if(-not$text.Contains('## Docker Compose V14')){$text=$text.Replace($anchor,$section+$anchor)};[System.IO.File]::WriteAllText((Join-Path(Get-Location)$path),$text,[System.Text.UTF8Encoding]::new($false));'CANONICAL_README_COMPOSE_V14_ADDED=1'
