param([string]$Path = "backend/SanSo.Api.V6/PostgresNotificationStoreV1.cs")
$ErrorActionPreference = "Stop"
$text = Get-Content -LiteralPath $Path -Raw
$old = '        q.Parameters.AddWithValue(notificationId);' + [Environment]::NewLine + '        await using var r = await q.ExecuteReaderAsync(ct);'
$new = '        q.Parameters.AddWithValue(notificationId);' + [Environment]::NewLine + '        q.Parameters.AddWithValue(tenantId);' + [Environment]::NewLine + '        await using var r = await q.ExecuteReaderAsync(ct);'
if (-not $text.Contains($old)) { throw 'ACK_PARAMETER_ANCHOR_NOT_FOUND' }
$text = $text.Replace($old, $new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $Path), $text, [System.Text.UTF8Encoding]::new($false))
Write-Output "NOTIFICATION_ACK_TENANT_PARAMETER_ADDED=$Path"
