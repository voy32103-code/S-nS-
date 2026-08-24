param([string]$Path = "backend/SanSo.Api.V6/PostgresNotificationStoreV1.cs")

$ErrorActionPreference = "Stop"
$text = Get-Content -LiteralPath $Path -Raw
$listOld = @'
        q.CommandText = """
            SELECT id::text,notification_type,channel,recipient_masked,resource_ref,dedupe_key,title,body,status,attempt,next_attempt_at,last_error_code,created_at
            FROM notification_deliveries WHERE channel='IN_APP' ORDER BY created_at DESC
            """;
        await using var r = await q.ExecuteReaderAsync(ct);
'@
$listNew = @'
        q.CommandText = """
            SELECT id::text,notification_type,channel,recipient_masked,resource_ref,dedupe_key,title,body,status,attempt,next_attempt_at,last_error_code,created_at
            FROM notification_deliveries WHERE organization_id=$1 AND channel='IN_APP' ORDER BY created_at DESC
            """;
        q.Parameters.AddWithValue(tenantId);
        await using var r = await q.ExecuteReaderAsync(ct);
'@
if ($text.Contains($listOld)) { $text = $text.Replace($listOld, $listNew) }
$text = $text.Replace("WHERE id=`$1 AND channel='IN_APP'", "WHERE organization_id=`$2 AND id=`$1 AND channel='IN_APP'")
$ackOld = '        q.Parameters.AddWithValue(notificationId);' + [Environment]::NewLine + '        await using var r = await q.ExecuteReaderAsync(ct);'
$ackNew = '        q.Parameters.AddWithValue(notificationId);' + [Environment]::NewLine + '        q.Parameters.AddWithValue(tenantId);' + [Environment]::NewLine + '        await using var r = await q.ExecuteReaderAsync(ct);'
if ($text.Contains($ackOld) -and -not $text.Contains('q.Parameters.AddWithValue(tenantId);' + [Environment]::NewLine + '        await using var r = await q.ExecuteReaderAsync(ct);')) { $text = $text.Replace($ackOld, $ackNew) }
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $Path), $text, [System.Text.UTF8Encoding]::new($false))
Write-Output "NOTIFICATION_TENANT_FILTER_HARDENED=$Path"
