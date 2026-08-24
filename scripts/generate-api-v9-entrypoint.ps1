param(
    [string]$Source = "backend/SanSo.Api.V6/ProgramCanonicalV8.cs",
    [string]$Output = "backend/SanSo.Api.V6/ProgramCanonicalV9.cs",
    [string]$Targets = "backend/SanSo.Api.V6/Directory.Build.targets"
)
$ErrorActionPreference = "Stop"
$text = Get-Content -LiteralPath $Source -Raw -Encoding UTF8
if (-not $text.Contains('using SanSo.Api.Security;')) { $text = $text.Replace('using SanSo.Api.Modules;', 'using SanSo.Api.Modules;' + [Environment]::NewLine + 'using SanSo.Api.Security;') }
$csAnchor = 'var cs = builder.Configuration.GetConnectionString("Postgres") ?? Environment.GetEnvironmentVariable("SANSO_POSTGRES");'
$csReplacement = $csAnchor + [Environment]::NewLine + 'var fieldKeyBase64 = Environment.GetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_BASE64");' + [Environment]::NewLine + 'var fieldKeyVersion = Environment.GetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_VERSION");'
$text = $text.Replace($csAnchor, $csReplacement)
$registrationAnchor = '    builder.Services.AddSingleton<SanSo.Api.V6.PostgresNotificationStoreV1>();'
$registration = $registrationAnchor + @'

    if (!string.IsNullOrWhiteSpace(fieldKeyBase64) && !string.IsNullOrWhiteSpace(fieldKeyVersion))
    {
        byte[] fieldKey;
        try { fieldKey = Convert.FromBase64String(fieldKeyBase64); }
        catch (FormatException) { throw new InvalidOperationException("FIELD_ENCRYPTION_KEY_BASE64_INVALID"); }
        builder.Services.AddSingleton(new TenantFieldProtector(fieldKey, fieldKeyVersion));
        builder.Services.AddSingleton<SanSo.Api.V6.PostgresOnboardingStoreV1>();
    }
'@
$text = $text.Replace($registrationAnchor, $registration)
$text = $text.Replace('var memoryNotifications = new NotificationCenter();', 'var memoryNotifications = new NotificationCenter();' + [Environment]::NewLine + 'var memoryOnboarding = new OnboardingWorkflow();')
$helperAnchor = 'SanSo.Api.V6.PostgresNotificationStoreV1 NotificationStore() => app.Services.GetRequiredService<SanSo.Api.V6.PostgresNotificationStoreV1>();'
$helper = $helperAnchor + [Environment]::NewLine + 'SanSo.Api.V6.PostgresOnboardingStoreV1? OnboardingStore() => app.Services.GetService<SanSo.Api.V6.PostgresOnboardingStoreV1>();'
$text = $text.Replace($helperAnchor, $helper)
$routeAnchor = 'Prefer(app.MapGet("/api/notifications", async (HttpRequest r, CancellationToken ct) =>'
$routes = @'
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

'@
if (-not $text.Contains($routeAnchor)) { throw 'NOTIFICATION_ROUTE_ANCHOR_NOT_FOUND' }
$text = $text.Replace($routeAnchor, $routes + $routeAnchor)
$text = $text.Replace('api = "v7"', 'api = "v9"')
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $Output),$text,[System.Text.UTF8Encoding]::new($false))
$targetText = Get-Content -LiteralPath $Targets -Raw -Encoding UTF8
$targetText = $targetText.Replace('ProgramCanonicalV8.cs','ProgramCanonicalV9.cs')
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $Targets),$targetText,[System.Text.UTF8Encoding]::new($false))
Write-Output "API_V9_ENTRYPOINT_GENERATED=$Output"
