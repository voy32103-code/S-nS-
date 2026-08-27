$ErrorActionPreference='Stop';$path='.github/workflows/ci-v14-canonical.yml';$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8;$anchor=@'
      - name: Backend live tests
        run: dotnet test SanSo.V10.slnx --no-build --configuration Release
'@;$insert=$anchor+[Environment]::NewLine+@'
      - name: Scan .NET dependencies
        shell: pwsh
        run: ./scripts/scan-dotnet-vulnerabilities-v14.ps1
'@;if(-not$text.Contains('Audit canonical .NET dependencies') -and -not$text.Contains('Scan .NET dependencies')){if(-not$text.Contains($anchor)){throw 'CI_BACKEND_TEST_ANCHOR_NOT_FOUND'};$text=$text.Replace($anchor,$insert)};$anchor=@'
      - name: Frontend unit and production build
'@;$insert=@'
      - name: Audit frontend dependencies
        working-directory: frontend
        run: npm audit --audit-level=high
      - name: Frontend unit and production build
'@;if(-not$text.Contains('Audit frontend dependencies')){if(-not$text.Contains($anchor)){throw 'CI_FRONTEND_BUILD_ANCHOR_NOT_FOUND'};$text=$text.Replace($anchor,$insert)};[System.IO.File]::WriteAllText((Join-Path(Get-Location)$path),$text,[System.Text.UTF8Encoding]::new($false));'CI_V14_DEPENDENCY_SCANS_ADDED=2'
