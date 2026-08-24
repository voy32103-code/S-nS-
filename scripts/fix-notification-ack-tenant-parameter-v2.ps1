param([string]$Path = "backend/SanSo.Api.V6/PostgresNotificationStoreV1.cs")
$ErrorActionPreference = "Stop"
$text = Get-Content -LiteralPath $Path -Raw
$pattern = '(q\.Parameters\.AddWithValue\(notificationId\);\r?\n)(\s*await using var r = await q\.ExecuteReaderAsync\(ct\);)'
$replacement = '$1        q.Parameters.AddWithValue(tenantId);' + [Environment]::NewLine + '$2'
$updated = [regex]::Replace($text, $pattern, $replacement, 1)
if ($updated -eq $text) { throw 'ACK_PARAMETER_REGEX_NOT_MATCHED' }
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $Path), $updated, [System.Text.UTF8Encoding]::new($false))
Write-Output "NOTIFICATION_ACK_TENANT_PARAMETER_ADDED=$Path"
