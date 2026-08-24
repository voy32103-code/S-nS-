$ErrorActionPreference='Stop';$path='.github/workflows/ci-v14-canonical.yml';$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8;$old=@'
      - name: Verify OpenAPI and client artifacts
        shell: pwsh
        run: |
          ./scripts/verify-openapi-v14.ps1
          ./scripts/scan-client-secrets-v2.ps1
'@;$new=@'
      - name: Verify OpenAPI
        shell: pwsh
        run: ./scripts/verify-openapi-v14.ps1
'@;if(-not$text.Contains($old)){throw 'CI_OPENAPI_SCAN_ANCHOR_NOT_FOUND'};$text=$text.Replace($old,$new);$anchor=@'
      - name: Install Playwright Chromium
'@;$insert=@'
      - name: Scan production client artifact
        shell: pwsh
        run: ./scripts/scan-client-secrets-v2.ps1
      - name: Install Playwright Chromium
'@;if(-not$text.Contains($anchor)){throw 'CI_PLAYWRIGHT_ANCHOR_NOT_FOUND'};$text=$text.Replace($anchor,$insert);[System.IO.File]::WriteAllText((Join-Path(Get-Location)$path),$text,[System.Text.UTF8Encoding]::new($false));'CI_V14_CLIENT_SCAN_ORDERED=2'
