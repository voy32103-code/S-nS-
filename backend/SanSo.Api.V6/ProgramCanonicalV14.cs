using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing;
using Npgsql;
using SanSo.Api;
using SanSo.Api.Modules;
using SanSo.Api.Security;
using SanSo.Api.V2;
using SanSo.Api.V5;
using SanSo.Import;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
var cs = builder.Configuration.GetConnectionString("Postgres") ?? Environment.GetEnvironmentVariable("SANSO_POSTGRES");
if (string.IsNullOrWhiteSpace(cs)) cs = null;
var fieldKeyBase64 = Environment.GetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_BASE64");
var fieldKeyVersion = Environment.GetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_VERSION");
if (!builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Production requires PostgreSQL");
if (!string.IsNullOrWhiteSpace(cs))
{
    builder.Services.AddSingleton(NpgsqlDataSource.Create(cs));
    builder.Services.AddSingleton<PostgresCommerceStore>();
    builder.Services.AddSingleton<PostgresLifecycleStore>();
    builder.Services.AddSingleton<PostgresImportStagingStoreV2>();
    builder.Services.AddSingleton<SanSo.Api.V6.PostgresNotificationStoreV1>();
    builder.Services.AddSingleton<SanSo.Api.V6.PostgresSettlementImportStoreV1>();
    builder.Services.AddSingleton<SanSo.Api.V6.PostgresReportExportStoreV2>();
    builder.Services.AddSingleton<SanSo.Api.V6.PostgresSettlementImportWorkflowV1>();
    if (!string.IsNullOrWhiteSpace(fieldKeyBase64) && !string.IsNullOrWhiteSpace(fieldKeyVersion))
    {
        byte[] fieldKey;
        try { fieldKey = Convert.FromBase64String(fieldKeyBase64); }
        catch (FormatException) { throw new InvalidOperationException("FIELD_ENCRYPTION_KEY_BASE64_INVALID"); }
        builder.Services.AddSingleton(new TenantFieldProtector(fieldKey, fieldKeyVersion));
        builder.Services.AddSingleton<SanSo.Api.V6.PostgresOnboardingStoreV1>();
    }
}
builder.Services.AddSingleton<DemoStore>();
builder.Services.AddSanSoModules();
builder.Services.AddSingleton<IdentityService>();
builder.Services.AddSingleton<AuditTrail>();
builder.Services.AddSingleton<ReliableSync>();
builder.Services.AddSingleton<FinancialLifecycle>();
builder.Services.AddSingleton<TeamAndSupport>();
builder.Services.AddSingleton<BillingLifecycle>();
builder.Services.AddSingleton<SafeCopilot>();
builder.Services.AddCors(x => x.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5174", "http://127.0.0.1:5176").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
SanSo.Api.V6.DevelopmentE2ESeedV2.Apply(app);
app.UseMiddleware<SanSo.Api.V6.SafeProblemMiddlewareV2>();
app.UseCors();
IdentityEndpoints.MapIdentity(app);
V5AuthorizedComposition.MapSanSoModules(app);
var importer = new CommerceFileImporter(new ImportRegistry());
var memoryImport = new ImportConfirmationWorkflow();
var memoryNotifications = new NotificationCenter();
var memoryOnboarding = new OnboardingWorkflow();

string Tenant(HttpRequest r) => r.HttpContext.Items["TenantId"] as string ?? r.Headers["X-Tenant-Id"].FirstOrDefault() ?? "tenant-an-nhien";
SessionPrincipal? Principal(HttpRequest r)
{
    if (r.HttpContext.Items["SessionPrincipal"] is SessionPrincipal p) return p;
    var tenant = r.Headers["X-Tenant-Id"].FirstOrDefault();
    var auth = r.Headers.Authorization.FirstOrDefault();
    if (tenant is null || auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) != true) return null;
    try { return app.Services.GetRequiredService<IdentityService>().Authenticate(auth[7..], tenant); }
    catch { return null; }
}
SessionPrincipal Require(HttpRequest r, string permission) => IdentityEndpoints.Require(r, app.Services.GetRequiredService<IdentityService>(), permission);
bool Db() => !string.IsNullOrWhiteSpace(cs);
PostgresCommerceStore Pg() => app.Services.GetRequiredService<PostgresCommerceStore>();
PostgresImportStagingStoreV2 Imports() => app.Services.GetRequiredService<PostgresImportStagingStoreV2>();
PostgresInventoryStoreV4 Inventory() => new(app.Services.GetRequiredService<NpgsqlDataSource>());
SanSo.Api.V6.PostgresNotificationStoreV1 NotificationStore() => app.Services.GetRequiredService<SanSo.Api.V6.PostgresNotificationStoreV1>();
SanSo.Api.V6.PostgresOnboardingStoreV1? OnboardingStore() => app.Services.GetService<SanSo.Api.V6.PostgresOnboardingStoreV1>();
SanSo.Api.V6.PostgresSettlementImportStoreV1 SettlementStore() => app.Services.GetRequiredService<SanSo.Api.V6.PostgresSettlementImportStoreV1>();
SanSo.Api.V6.PostgresReportExportStoreV2 ReportStore() => app.Services.GetRequiredService<SanSo.Api.V6.PostgresReportExportStoreV2>();
SanSo.Api.V6.PostgresSettlementImportWorkflowV1 SettlementWorkflow() => app.Services.GetRequiredService<SanSo.Api.V6.PostgresSettlementImportWorkflowV1>();
void Prefer(RouteHandlerBuilder route) => route.Add(endpoint => ((RouteEndpointBuilder)endpoint).Order = -1);

Prefer(app.MapGet("/api/inventory/{sku}", async (HttpRequest r, string sku, CancellationToken ct) =>
{
    Require(r, "finance.read");
    return Db() ? Results.Ok(await Inventory().Get(Tenant(r), sku, ct)) : Results.Json(new { code = "DATABASE_REQUIRED" }, statusCode: 503);
}));
Prefer(app.MapPost("/api/inventory/{sku}/reserve", async (HttpRequest r, string sku, V7InventoryBody b, CancellationToken ct) =>
{
    Require(r, "inventory.write");
    if (!Db()) return Results.Json(new { code = "DATABASE_REQUIRED" }, statusCode: 503);
    try { return Results.Ok(await Inventory().Reserve(Tenant(r), sku, b.Quantity, b.SourceKey, ct)); }
    catch (InventoryConflictException) { return Results.Conflict(new { code = "INSUFFICIENT_ATP" }); }
}));
Prefer(app.MapPost("/api/inventory/{sku}/release", async (HttpRequest r, string sku, V7InventoryBody b, CancellationToken ct) =>
{
    Require(r, "inventory.write");
    if (!Db()) return Results.Json(new { code = "DATABASE_REQUIRED" }, statusCode: 503);
    try { return Results.Ok(await Inventory().Release(Tenant(r), sku, b.Quantity, b.SourceKey, ct)); }
    catch (InvalidOperationException e) { return Results.BadRequest(new { code = e.Message }); }
}));

Prefer(app.MapGet("/api/reports",(HttpRequest r)=>{Require(r,"export.sensitive");return Results.Ok(new{items=SanSo.Api.V6.PostgresReportExportStoreV2.Catalog()});}));
Prefer(app.MapPost("/api/reports/exports",async(HttpRequest r,V13ExportRequest b,CancellationToken ct)=>{var p=Require(r,"export.sensitive");if(!p.StepUpVerified)throw new ForbiddenException();if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);try{return Results.Ok(await ReportStore().Preview(Tenant(r),b.RunId,p.UserId,b.Type,ct));}catch(KeyNotFoundException){return Results.NotFound(new{code="RECONCILIATION_NOT_FOUND"});}}));
Prefer(app.MapPost("/api/reports/exports/{id}/confirm",async(HttpRequest r,string id,V12ExportConfirm b,CancellationToken ct)=>{var p=Require(r,"export.sensitive");if(!p.StepUpVerified)throw new ForbiddenException();if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);return Results.Ok(await ReportStore().Confirm(Tenant(r),id,b.ContentChecksum,p.UserId,ct));}));
Prefer(app.MapGet("/api/reports/exports/{id}",async(HttpRequest r,string id,CancellationToken ct)=>{Require(r,"export.sensitive");if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);var item=await ReportStore().Get(Tenant(r),id,ct);return item is null?Results.NotFound(new{code="EXPORT_NOT_FOUND"}):Results.Ok(item);}));
Prefer(app.MapGet("/api/reports/exports/{id}/download",async(HttpRequest r,string id,CancellationToken ct)=>{var p=Require(r,"export.sensitive");if(!p.StepUpVerified)throw new ForbiddenException();if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);try{var file=await ReportStore().Download(Tenant(r),id,p.UserId,ct);return Results.File(file.Content,file.ContentType,file.FileName,enableRangeProcessing:false);}catch(KeyNotFoundException){return Results.NotFound(new{code="EXPORT_NOT_FOUND"});}}));
Prefer(app.MapPost("/api/imports/settlements",async(HttpRequest r,CancellationToken ct)=>
{
    Require(r,"tax.review");if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);if(!r.HasFormContentType)return Results.BadRequest(new{code="MULTIPART_REQUIRED"});var file=(await r.ReadFormAsync(ct)).Files.GetFile("file");if(file is null)return Results.BadRequest(new{code="FILE_REQUIRED"});if(file.Length>10*1024*1024)return Results.BadRequest(new{code="FILE_TOO_LARGE"});try{var bytes=SanSo.Api.V6.SettlementFileParserV1.Read(file.OpenReadStream());SanSo.Api.V6.SettlementImportV1 parsed;string format;var ext=Path.GetExtension(file.FileName).ToLowerInvariant();if(ext==".csv"&&file.ContentType is"text/csv"or"application/csv"or"application/vnd.ms-excel"or"application/octet-stream"){parsed=SanSo.Api.V6.SettlementFileParserV1.Csv(bytes);format="CSV";}else if(ext==".xlsx"&&file.ContentType is"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"or"application/octet-stream"){parsed=SanSo.Api.V6.SettlementFileParserV1.Xlsx(bytes);format="XLSX";}else return Results.BadRequest(new{code="UNSUPPORTED_FILE_TYPE"});return Results.Ok(await SettlementWorkflow().Stage(Tenant(r),parsed,format,ct));}catch(DecoderFallbackException){return Results.BadRequest(new{code="ENCODING_INVALID",hint="Save CSV as UTF-8"});}catch(InvalidDataException e){return Results.BadRequest(new{code=e.Message});}catch(OverflowException){return Results.BadRequest(new{code="AMOUNT_SUM_OVERFLOW"});}
}));
Prefer(app.MapPost("/api/imports/settlements/confirm",async(HttpRequest r,SanSo.Api.V6.SettlementConfirmRequestV1 b,CancellationToken ct)=>{var p=Require(r,"tax.review");if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);return Results.Ok(await SettlementWorkflow().Confirm(Tenant(r),b,p.UserId,ct));}));
if(app.Environment.IsDevelopment())Prefer(app.MapPost("/api/imports/settlements/direct", async (HttpRequest r, CancellationToken ct) =>
{
    var principal=Require(r,"tax.review");if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);if(!r.HasFormContentType)return Results.BadRequest(new{code="MULTIPART_REQUIRED"});var file=(await r.ReadFormAsync(ct)).Files.GetFile("file");if(file is null)return Results.BadRequest(new{code="FILE_REQUIRED"});if(file.Length>10*1024*1024)return Results.BadRequest(new{code="FILE_TOO_LARGE"});if(!Path.GetExtension(file.FileName).Equals(".csv",StringComparison.OrdinalIgnoreCase)||file.ContentType is not("text/csv"or"application/csv"or"application/vnd.ms-excel"or"application/octet-stream"))return Results.BadRequest(new{code="UNSUPPORTED_FILE_TYPE"});try{await using var stream=file.OpenReadStream();var parsed=SanSo.Api.V6.SettlementCsvParserV1.Parse(stream);return Results.Ok(await SettlementStore().Import(Tenant(r),parsed,principal.UserId,ct));}catch(DecoderFallbackException){return Results.BadRequest(new{code="ENCODING_INVALID",hint="Save CSV as UTF-8"});}catch(InvalidDataException e){return Results.BadRequest(new{code=e.Message});}catch(OverflowException){return Results.BadRequest(new{code="AMOUNT_SUM_OVERFLOW"});}
}));
Prefer(app.MapGet("/api/reconciliations/{runId}",async(HttpRequest r,string runId,CancellationToken ct)=>
{
    Require(r,"finance.read");if(!Db())return Results.Json(new{code="DATABASE_REQUIRED"},statusCode:503);if(!Guid.TryParse(runId,out _))return Results.BadRequest(new{code="RUN_ID_INVALID"});var result=await SettlementStore().GetRun(Tenant(r),runId,ct);return result is null?Results.NotFound(new{code="RECONCILIATION_NOT_FOUND"}):Results.Ok(result);
}));
Prefer(app.MapGet("/api/onboarding", async (HttpRequest r, CancellationToken ct) =>
{
    Require(r, "organization.manage");
    var store = OnboardingStore();
    if (store is not null) return Results.Ok(new { snapshot = await store.Start(Tenant(r), ct), persisted = true });
    if (Db()) return Results.Json(new { code = "FIELD_ENCRYPTION_NOT_CONFIGURED" }, statusCode: 503);
    return Results.Ok(new { snapshot = memoryOnboarding.Start(Tenant(r)), persisted = false });
}));
Prefer(app.MapPost("/api/onboarding/business-profile", async (HttpRequest r, BusinessProfileDraft b, CancellationToken ct) =>
{
    Require(r, "organization.manage");
    try { var store=OnboardingStore(); if(store is not null)return Results.Ok(new{snapshot=await store.SaveBusinessProfile(Tenant(r),b,ct),persisted=true});if(Db())return Results.Json(new{code="FIELD_ENCRYPTION_NOT_CONFIGURED"},statusCode:503);return Results.Ok(new{snapshot=memoryOnboarding.SaveBusinessProfile(Tenant(r),b),persisted=false}); } catch(OnboardingValidationException e){return Results.BadRequest(new{code=e.Message});}
}));
Prefer(app.MapPost("/api/onboarding/data-source", async (HttpRequest r, V5SourceBody b, CancellationToken ct) =>
{
    Require(r,"organization.manage");try{var store=OnboardingStore();if(store is not null)return Results.Ok(new{snapshot=await store.SelectSource(Tenant(r),b.Mode,ct),persisted=true});if(Db())return Results.Json(new{code="FIELD_ENCRYPTION_NOT_CONFIGURED"},statusCode:503);return Results.Ok(new{snapshot=memoryOnboarding.SelectDataSource(Tenant(r),b.Mode),persisted=false});}catch(OnboardingValidationException e){return Results.BadRequest(new{code=e.Message});}
}));
Prefer(app.MapPost("/api/onboarding/backfill", async (HttpRequest r, V5BackfillBody b, CancellationToken ct) =>
{
    Require(r,"organization.manage");try{var store=OnboardingStore();if(store is not null)return Results.Ok(new{snapshot=await store.SelectBackfill(Tenant(r),b.From,ct),persisted=true});if(Db())return Results.Json(new{code="FIELD_ENCRYPTION_NOT_CONFIGURED"},statusCode:503);return Results.Ok(new{snapshot=memoryOnboarding.SelectBackfill(Tenant(r),b.From),persisted=false});}catch(OnboardingValidationException e){return Results.BadRequest(new{code=e.Message});}
}));
Prefer(app.MapPost("/api/onboarding/sku-mapping", async (HttpRequest r, V5SkuBody b, CancellationToken ct) =>
{
    Require(r,"organization.manage");try{var store=OnboardingStore();if(store is not null)return Results.Ok(new{snapshot=await store.ConfirmSku(Tenant(r),b.MappedSkuCount,ct),persisted=true});if(Db())return Results.Json(new{code="FIELD_ENCRYPTION_NOT_CONFIGURED"},statusCode:503);return Results.Ok(new{snapshot=memoryOnboarding.ConfirmSkuMapping(Tenant(r),b.MappedSkuCount),persisted=false});}catch(OnboardingValidationException e){return Results.BadRequest(new{code=e.Message});}
}));
Prefer(app.MapPost("/api/onboarding/opening-balances", async (HttpRequest r, V5BalancesBody b, CancellationToken ct) =>
{
    Require(r,"organization.manage");try{var store=OnboardingStore();if(store is not null)return Results.Ok(new{snapshot=await store.OpeningBalances(Tenant(r),b.Balances,ct),persisted=true});if(Db())return Results.Json(new{code="FIELD_ENCRYPTION_NOT_CONFIGURED"},statusCode:503);return Results.Ok(new{snapshot=memoryOnboarding.SaveOpeningBalances(Tenant(r),b.Balances),persisted=false});}catch(OnboardingValidationException e){return Results.BadRequest(new{code=e.Message});}
}));
Prefer(app.MapPost("/api/onboarding/disclaimer", async (HttpRequest r, V5DisclaimerBody b, CancellationToken ct) =>
{
    var principal=Require(r,"organization.manage");try{var store=OnboardingStore();if(store is not null)return Results.Ok(new{snapshot=await store.Disclaimer(Tenant(r),b.Version,principal.UserId,b.ExplicitlyConfirmed,ct),persisted=true});if(Db())return Results.Json(new{code="FIELD_ENCRYPTION_NOT_CONFIGURED"},statusCode:503);return Results.Ok(new{snapshot=memoryOnboarding.ConfirmTaxDisclaimer(Tenant(r),b.Version,b.ExplicitlyConfirmed),persisted=false});}catch(OnboardingValidationException e){return Results.BadRequest(new{code=e.Message});}
}));
Prefer(app.MapPost("/api/onboarding/first-reconciliation", async (HttpRequest r, V5ActivationBody b, CancellationToken ct) =>
{
    Require(r,"organization.manage");try{var store=OnboardingStore();if(store is not null)return Results.Ok(new{snapshot=await store.Complete(Tenant(r),b.ReconciliationId,b.HasMatchedOrExplainedDiscrepancy,ct),persisted=true});if(Db())return Results.Json(new{code="FIELD_ENCRYPTION_NOT_CONFIGURED"},statusCode:503);return Results.Ok(new{snapshot=memoryOnboarding.CompleteFirstReconciliation(Tenant(r),b.ReconciliationId,b.HasMatchedOrExplainedDiscrepancy),persisted=false});}catch(OnboardingValidationException e){return Results.BadRequest(new{code=e.Message});}
}));
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
app.MapGet("/health", async (CancellationToken ct) =>
{
    if (!Db()) return Results.Ok(new { status = "degraded", database = "not-configured", api = "v14" });
    try
    {
        await using var c = await app.Services.GetRequiredService<NpgsqlDataSource>().OpenConnectionAsync(ct);
        await using var q = c.CreateCommand();
        q.CommandText = "SELECT 1";
        await q.ExecuteScalarAsync(ct);
        return Results.Ok(new { status = "ok", database = "postgresql", api = "v14" });
    }
    catch { return Results.Json(new { status = "unhealthy" }, statusCode: 503); }
});

app.MapPost("/api/imports/preview", async (HttpRequest r, CancellationToken ct) =>
{
    if (!r.HasFormContentType) return Results.BadRequest(new { code = "MULTIPART_REQUIRED" });
    var file = (await r.ReadFormAsync(ct)).Files.GetFile("file");
    if (file is null) return Results.BadRequest(new { code = "FILE_REQUIRED" });
    if (file.Length > 10 * 1024 * 1024) return Results.BadRequest(new { code = "FILE_TOO_LARGE" });
    await using var stream = file.OpenReadStream();
    ImportPreview preview;
    try
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext == ".csv" && file.ContentType is "text/csv" or "application/csv" or "application/vnd.ms-excel" or "application/octet-stream")
            preview = importer.PreviewCsv(Tenant(r), stream);
        else if (ext == ".xlsx" && file.ContentType is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" or "application/octet-stream")
            preview = importer.PreviewXlsx(Tenant(r), stream);
        else return Results.BadRequest(new { code = "UNSUPPORTED_FILE_TYPE" });
    }
    catch (DecoderFallbackException) { return Results.BadRequest(new { code = "ENCODING_INVALID", hint = "Save CSV as UTF-8" }); }
    catch (InvalidDataException e) { return Results.BadRequest(new { code = e.Message }); }
    if (Db())
    {
        var stored = await Imports().Stage(Tenant(r), preview, ct);
        return Results.Ok(new { stored.BatchId, stored.PreviewToken, stored.ExpiresAt, preview.Format, preview.TemplateVersion, preview.Checksum, preview.Delimiter, preview.Headers, preview.Rows, preview.Errors, preview.Duplicate, persisted = true });
    }
    var staged = memoryImport.Stage(Tenant(r), preview);
    return Results.Ok(new { batchId = (string?)null, staged.PreviewToken, staged.ExpiresAt, preview.Format, preview.TemplateVersion, preview.Checksum, preview.Delimiter, preview.Headers, preview.Rows, preview.Errors, preview.Duplicate, persisted = false });
});

app.MapPost("/api/imports/confirm", async (HttpRequest r, V7ConfirmBody b, CancellationToken ct) =>
{
    if (Db())
    {
        var actor = Principal(r)?.UserId;
        if (actor is null) return Results.Unauthorized();
        try
        {
            var x = await Imports().Confirm(Tenant(r), b.PreviewToken, b.Checksum, actor, ct);
            return Results.Ok(new { x.BatchId, x.Checksum, x.AcceptedRows, x.RejectedRows, x.Duplicate, persisted = true });
        }
        catch (InvalidOperationException e) { return Results.BadRequest(new { code = e.Message }); }
    }
    var m = memoryImport.Confirm(Tenant(r), b.PreviewToken, b.Checksum);
    return Results.Ok(new { m.Checksum, m.AcceptedRows, m.RejectedRows, m.Duplicate, persisted = false });
});
Prefer(app.MapGet("/api/dashboard", async (HttpRequest r, DemoStore demo, CancellationToken ct) => Db() ? Results.Ok(await Pg().Dashboard(Tenant(r), ct)) : Results.Ok(demo.Dashboard(Tenant(r)))));
Prefer(app.MapGet("/api/orders", async (HttpRequest r, DemoStore demo, CancellationToken ct) => Db() ? Results.Ok(await Pg().Orders(Tenant(r), ct)) : Results.Ok(demo.Orders(Tenant(r)))));
Prefer(app.MapGet("/api/reconciliations/current", async (HttpRequest r, DemoStore demo, CancellationToken ct) => Db() ? Results.Ok(await Pg().CurrentReconciliation(Tenant(r), ct)) : Results.Ok(demo.Reconcile(Tenant(r)))));
app.Run();

public partial class Program { }
record V7InventoryBody(int Quantity, string SourceKey);
record V7ConfirmBody(string PreviewToken, string Checksum);
record V8NotificationBody(NotificationType Type, DeliveryChannel Channel, string Recipient, string ResourceRef, DateTimeOffset WindowStart);
record V13ExportRequest(string RunId,string? Type);
record V12ExportConfirm(string ContentChecksum);
