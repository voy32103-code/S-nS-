namespace SanSo.Api.Modules;
public record Plan(string Code,int ShopLimit,int MonthlyOrderLimit,bool Inventory,bool AdvancedExport);
public record Subscription(string TenantId,Plan Plan,string Status,DateTimeOffset? TrialEndsAt);
public sealed class EntitlementService
{
 public bool CanSync(Subscription s,int currentOrders)=>s.Status is "ACTIVE" or "TRIAL"&&currentOrders<s.Plan.MonthlyOrderLimit;
 public bool CanViewExistingData(Subscription s)=>true;
 public bool CanUse(Subscription s,string feature)=>s.Status is "ACTIVE" or "TRIAL"&&feature switch{"inventory"=>s.Plan.Inventory,"advanced_export"=>s.Plan.AdvancedExport,_=>true};
}
