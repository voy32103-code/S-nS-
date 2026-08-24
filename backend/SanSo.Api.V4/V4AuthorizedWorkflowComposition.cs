using SanSo.Api;
using SanSo.Api.Modules;
using SanSo.Api.V2;

namespace SanSo.Api.V4;

public static class V4AuthorizedWorkflowComposition
{
    private static readonly OnboardingWorkflow Onboarding=new();
    private static readonly NotificationCenter Notifications=new();

    public static WebApplication MapSanSoModules(this WebApplication app)
    {
        SanSo.Api.ModuleEndpoints.MapSanSoModules(app);
        string Tenant(HttpRequest request)=>request.Headers["X-Tenant-Id"].FirstOrDefault()??throw new UnauthorizedAccessException();
        SessionPrincipal Require(HttpRequest request,string permission)=>IdentityEndpoints.Require(request,app.Services.GetRequiredService<IdentityService>(),permission);
        bool Db()=>app.Services.GetService<Npgsql.NpgsqlDataSource>() is not null;
        PostgresLifecycleStore Life()=>app.Services.GetRequiredService<PostgresLifecycleStore>();

        app.MapPost("/api/refunds",async(HttpRequest request,V4RefundBody body,FinancialLifecycle memory,CancellationToken ct)=>{Require(request,"tax.review");return Db()?Results.Ok(await Life().Refund(Tenant(request),body.OrderId,body.Amount,body.SourceRefundId,body.OriginalPeriod,body.CurrentPeriod,ct)):Results.Ok(memory.Refund(Tenant(request),body.OrderId,body.Amount,body.SourceRefundId,body.OriginalPeriod,body.CurrentPeriod));});
        app.MapPost("/api/periods/{period}/freeze",async(HttpRequest request,string period,FinancialLifecycle memory,CancellationToken ct)=>{Require(request,"tax.review");return Db()?Results.Ok(await Life().FreezePeriod(Tenant(request),period,ct)):Results.Ok(memory.Freeze(Tenant(request),period));});
        app.MapPost("/api/periods/{period}/lock",async(HttpRequest request,string period,FinancialLifecycle memory,CancellationToken ct)=>{var principal=Require(request,"tax.review");if(Db()){await Life().LockPeriod(Tenant(request),period,principal.UserId,ct);return Results.NoContent();}return Results.Ok(memory.Lock(Tenant(request),period));});
        app.MapPost("/api/team/invitations",async(HttpRequest request,V4InviteBody body,TeamAndSupport memory,CancellationToken ct)=>{var principal=Require(request,"organization.manage");return Db()?Results.Ok(await Life().Invite(Tenant(request),body.Email,body.Role.ToString().ToUpperInvariant(),principal.UserId,body.ExpiresAt,ct)):Results.Ok(memory.Invite(Tenant(request),body.Email,body.Role,body.ExpiresAt));});
        app.MapPost("/api/support/grants",(HttpRequest request,V4SupportBody body,TeamAndSupport support,AuditTrail audit)=>{var principal=Require(request,"organization.manage");var grant=support.GrantSupport(Tenant(request),body.SupportUserId,body.Reason,body.ExpiresAt,principal.UserId,principal.StepUpVerified);audit.Append(Tenant(request),principal.UserId,"SUPPORT_ACCESS","SUPPORT_GRANT",grant.Id,body.Reason,request.HttpContext.TraceIdentifier);return Results.Ok(grant);});
        app.MapPost("/api/billing/trial",(HttpRequest request,V4TrialBody body,BillingLifecycle billing)=>{Require(request,"billing.manage");return Results.Ok(billing.StartTrial(Tenant(request),body.Plan,body.EndsAt));});
        app.MapPost("/api/billing/transition",async(HttpRequest request,V4BillingBody body,BillingLifecycle billing,CancellationToken ct)=>{Require(request,"billing.manage");if(Db()){await Life().SetSubscription(Tenant(request),body.Status,body.PeriodEnd,ct);return Results.NoContent();}return Results.Ok(billing.Transition(Tenant(request),body.Status,body.PeriodEnd));});
        app.MapPost("/api/copilot/explain",(HttpRequest request,V4CopilotBody body,SafeCopilot copilot)=>{Require(request,"finance.read");return Results.Ok(copilot.Explain(Tenant(request),body.Question,body.Evidence));});

        app.MapGet("/api/onboarding",(HttpRequest request)=>{Require(request,"organization.manage");return Results.Ok(new{snapshot=Onboarding.Start(Tenant(request)),persisted=false});});
        app.MapPost("/api/onboarding/business-profile",(HttpRequest request,BusinessProfileDraft body)=>AuthorizedOnboarding(request,()=>Onboarding.SaveBusinessProfile(Tenant(request),body)));
        app.MapPost("/api/onboarding/data-source",(HttpRequest request,V4SourceBody body)=>AuthorizedOnboarding(request,()=>Onboarding.SelectDataSource(Tenant(request),body.Mode)));
        app.MapPost("/api/onboarding/backfill",(HttpRequest request,V4BackfillBody body)=>AuthorizedOnboarding(request,()=>Onboarding.SelectBackfill(Tenant(request),body.From)));
        app.MapPost("/api/onboarding/sku-mapping",(HttpRequest request,V4SkuBody body)=>AuthorizedOnboarding(request,()=>Onboarding.ConfirmSkuMapping(Tenant(request),body.MappedSkuCount)));
        app.MapPost("/api/onboarding/opening-balances",(HttpRequest request,V4BalancesBody body)=>AuthorizedOnboarding(request,()=>Onboarding.SaveOpeningBalances(Tenant(request),body.Balances)));
        app.MapPost("/api/onboarding/disclaimer",(HttpRequest request,V4DisclaimerBody body)=>AuthorizedOnboarding(request,()=>Onboarding.ConfirmTaxDisclaimer(Tenant(request),body.Version,body.ExplicitlyConfirmed)));
        app.MapPost("/api/onboarding/first-reconciliation",(HttpRequest request,V4ActivationBody body)=>AuthorizedOnboarding(request,()=>Onboarding.CompleteFirstReconciliation(Tenant(request),body.ReconciliationId,body.HasMatchedOrExplainedDiscrepancy)));

        app.MapPost("/api/notifications",(HttpRequest request,V4NotificationBody body)=>{Require(request,"organization.manage");if(!app.Environment.IsDevelopment())return Results.Json(new{code="NOTIFICATION_PERSISTENCE_REQUIRED"},statusCode:503);return Results.Ok(new{delivery=Notifications.Raise(Tenant(request),body.Type,body.Channel,body.Recipient,body.ResourceRef,body.WindowStart),persisted=false});});
        app.MapGet("/api/notifications",(HttpRequest request)=>{Require(request,"finance.read");return Results.Ok(new{items=Notifications.List(Tenant(request)),persisted=false});});
        app.MapPost("/api/notifications/{id}/acknowledge",(HttpRequest request,string id)=>{Require(request,"finance.read");try{return Results.Ok(new{delivery=Notifications.Acknowledge(Tenant(request),id),persisted=false});}catch(KeyNotFoundException){return Results.NotFound();}catch(InvalidOperationException error){return Results.BadRequest(new{code=error.Message});}});
        return app;

        IResult AuthorizedOnboarding(HttpRequest request,Func<OnboardingSnapshot> action){Require(request,"organization.manage");try{return Results.Ok(new{snapshot=action(),persisted=false});}catch(OnboardingValidationException error){return Results.BadRequest(new{code=error.Message});}}
    }
}

public sealed record V4RefundBody(string OrderId,long Amount,string SourceRefundId,string OriginalPeriod,string CurrentPeriod);
public sealed record V4InviteBody(string Email,OrgRole Role,DateTimeOffset ExpiresAt);public sealed record V4SupportBody(string SupportUserId,string Reason,DateTimeOffset ExpiresAt);
public sealed record V4TrialBody(string Plan,DateTimeOffset EndsAt);public sealed record V4BillingBody(string Status,DateTimeOffset PeriodEnd);public sealed record V4CopilotBody(string Question,List<EvidenceItem> Evidence);
public sealed record V4SourceBody(SourceMode Mode);public sealed record V4BackfillBody(DateOnly From);public sealed record V4SkuBody(int MappedSkuCount);public sealed record V4BalancesBody(IReadOnlyList<OpeningBalanceDraft> Balances);public sealed record V4DisclaimerBody(string Version,bool ExplicitlyConfirmed);public sealed record V4ActivationBody(string ReconciliationId,bool HasMatchedOrExplainedDiscrepancy);
public sealed record V4NotificationBody(NotificationType Type,DeliveryChannel Channel,string Recipient,string ResourceRef,DateTimeOffset WindowStart);
