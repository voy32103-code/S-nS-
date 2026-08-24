using SanSo.Api.Modules;using System.Text;
namespace SanSo.Api;
public static class ModuleEndpoints
{
 public static IServiceCollection AddSanSoModules(this IServiceCollection services)=>services.AddSingleton<RawIngestion>().AddSingleton<TaxCenter>().AddSingleton<InventoryService>().AddSingleton<EntitlementService>();
 public static IEndpointRouteBuilder MapSanSoModules(this IEndpointRouteBuilder app){static string Tenant(HttpRequest r)=>r.Headers["X-Tenant-Id"].FirstOrDefault()??"tenant-an-nhien";
  app.MapPost("/api/raw-events",(HttpRequest r,RawEventRequest b,RawIngestion s)=>Results.Ok(s.Accept(Tenant(r),b.Source,b.EventId,b.EventType,b.Payload,b.SchemaVersion)));
  app.MapGet("/api/raw-events/{id}",(HttpRequest r,string id,RawIngestion s)=>s.Find(Tenant(r),id) is{} x?Results.Ok(x):Results.NotFound());
  app.MapPost("/api/tax/periods/{period}/calculate",(HttpRequest r,string period,TaxCalculationRequest b,TaxCenter s)=>Results.Ok(s.Calculate(Tenant(r),period,b.EffectiveDate,b.BasisAmount,b.PlatformWithheldAmount,b.HasRequiredProfile,b.Category)));
  app.MapPost("/api/tax/periods/{period}/transition",(HttpRequest r,string period,TaxTransitionRequest b,TaxCenter s)=>{s.Transition(Tenant(r),period,b.Next,b.Reason);return Results.Ok(new{status=s.Status(Tenant(r),period).ToString()});});
  app.MapPost("/api/inventory/{sku}/seed",(HttpRequest r,string sku,InventorySeedRequest b,InventoryService s)=>Results.Ok(s.Seed(Tenant(r),sku,b.OnHand)));
  app.MapPost("/api/inventory/{sku}/reserve",(HttpRequest r,string sku,InventoryCommand b,InventoryService s)=>s.Reserve(Tenant(r),sku,b.Quantity,b.SourceKey)?Results.Ok(s.Get(Tenant(r),sku)):Results.Conflict(new{code="INSUFFICIENT_ATP"}));
  app.MapPost("/api/inventory/{sku}/release",(HttpRequest r,string sku,InventoryCommand b,InventoryService s)=>{s.Release(Tenant(r),sku,b.Quantity,b.SourceKey);return Results.Ok(s.Get(Tenant(r),sku));});
  app.MapGet("/api/inventory/{sku}",(HttpRequest r,string sku,InventoryService s)=>Results.Ok(s.Get(Tenant(r),sku)));
  app.MapGet("/api/exports/reconciliation.csv",(HttpRequest r,DemoStore db)=>{var x=TraceableExport.ReconciliationCsv(Tenant(r),db.Reconcile(Tenant(r)));return Results.Text(x.Content,"text/csv",Encoding.UTF8);});return app;}
}
public record RawEventRequest(string Source,string EventId,string EventType,string Payload,string SchemaVersion="1");public record TaxCalculationRequest(DateOnly EffectiveDate,long BasisAmount,long? PlatformWithheldAmount,bool HasRequiredProfile,string? Category);public record TaxTransitionRequest(TaxPeriodStatus Next,string? Reason);public record InventorySeedRequest(int OnHand);public record InventoryCommand(int Quantity,string SourceKey);
