param(
    [string]$Source = "backend/SanSo.Api.V6/ProgramCanonicalV7.cs",
    [string]$Output = "backend/SanSo.Api.V6/ProgramCanonicalV8.cs"
)

$ErrorActionPreference = "Stop"
$text = Get-Content -LiteralPath $Source -Raw
$text = $text.Replace('builder.Services.AddSingleton<PostgresImportStagingStoreV2>();', 'builder.Services.AddSingleton<PostgresImportStagingStoreV2>();' + [Environment]::NewLine + '    builder.Services.AddSingleton<SanSo.Api.V6.PostgresNotificationStoreV1>();')
$text = $text.Replace('var memoryImport = new ImportConfirmationWorkflow();', 'var memoryImport = new ImportConfirmationWorkflow();' + [Environment]::NewLine + 'var memoryNotifications = new NotificationCenter();')
$text = $text.Replace('PostgresInventoryStoreV4 Inventory() => new(app.Services.GetRequiredService<NpgsqlDataSource>());', 'PostgresInventoryStoreV4 Inventory() => new(app.Services.GetRequiredService<NpgsqlDataSource>());' + [Environment]::NewLine + 'SanSo.Api.V6.PostgresNotificationStoreV1 NotificationStore() => app.Services.GetRequiredService<SanSo.Api.V6.PostgresNotificationStoreV1>();')
$anchor = 'app.MapGet("/health", async (CancellationToken ct) =>'
$notificationRoutes = @'
Prefer(app.MapGet("/api/notifications", async (HttpRequest r, CancellationToken ct) =>
{
    Require(r, "finance.read");
    if (Db()) return Results.Ok(new { items = await NotificationStore().List(Tenant(r), ct), persisted = true });
    return Results.Ok(new { items = memoryNotifications.List(Tenant(r)), persisted = false });
}));
Prefer(app.MapPost("/api/notifications", async (HttpRequest r, V8NotificationBody b, CancellationToken ct) =>
{
    Require(r, "organization.manage");
    if (b.Channel != DeliveryChannel.InApp) return Results.Json(new { code = "EMAIL_PROVIDER_NOT_CONFIGURED" }, statusCode: 503);
    if (Db()) return Results.Ok(new { delivery = await NotificationStore().RaiseInApp(Tenant(r), b.Type, b.ResourceRef, b.WindowStart, ct), persisted = true });
    return Results.Ok(new { delivery = memoryNotifications.Raise(Tenant(r), b.Type, b.Channel, b.Recipient, b.ResourceRef, b.WindowStart), persisted = false });
}));
Prefer(app.MapPost("/api/notifications/{id}/acknowledge", async (HttpRequest r, string id, CancellationToken ct) =>
{
    Require(r, "finance.read");
    try
    {
        if (Db()) return Results.Ok(new { delivery = await NotificationStore().Acknowledge(Tenant(r), id, ct), persisted = true });
        return Results.Ok(new { delivery = memoryNotifications.Acknowledge(Tenant(r), id), persisted = false });
    }
    catch (KeyNotFoundException) { return Results.NotFound(new { code = "NOTIFICATION_NOT_FOUND" }); }
}));

'@
if (-not $text.Contains($anchor)) { throw "HEALTH_ANCHOR_NOT_FOUND" }
$text = $text.Replace($anchor, $notificationRoutes + $anchor)
$text = $text.Replace('record V7ConfirmBody(string PreviewToken, string Checksum);', 'record V7ConfirmBody(string PreviewToken, string Checksum);' + [Environment]::NewLine + 'record V8NotificationBody(NotificationType Type, DeliveryChannel Channel, string Recipient, string ResourceRef, DateTimeOffset WindowStart);')
$absoluteOutput = Join-Path (Get-Location) $Output
[System.IO.File]::WriteAllText($absoluteOutput, $text, [System.Text.UTF8Encoding]::new($false))
Write-Output "API_V8_ENTRYPOINT_GENERATED=$Output"

