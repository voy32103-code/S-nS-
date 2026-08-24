using System.Collections.Concurrent;
namespace SanSo.Api.Modules;
public record SubscriptionState(string TenantId,string PlanCode,string Status,DateTimeOffset CurrentPeriodEnd,long OrdersUsed,int ShopsUsed);
public sealed class BillingLifecycle
{
 private readonly ConcurrentDictionary<string,SubscriptionState> states=new();
 public SubscriptionState StartTrial(string tenant,string plan,DateTimeOffset end)=>states[tenant]=new(tenant,plan,"TRIAL",end,0,0);
 public SubscriptionState RecordUsage(string tenant,long orders,int shops){var x=Get(tenant);if(orders<0||shops<0)throw new ArgumentOutOfRangeException();var updated=x with{OrdersUsed=checked(x.OrdersUsed+orders),ShopsUsed=Math.Max(x.ShopsUsed,shops)};states[tenant]=updated;return updated;}
 public SubscriptionState Transition(string tenant,string next,DateTimeOffset end){var x=Get(tenant);var allowed=(x.Status,next) switch{("TRIAL","ACTIVE")=>true,("TRIAL","EXPIRED")=>true,("ACTIVE","PAST_DUE")=>true,("PAST_DUE","ACTIVE")=>true,("PAST_DUE","EXPIRED")=>true,("ACTIVE","CANCEL_AT_PERIOD_END")=>true,("CANCEL_AT_PERIOD_END","EXPIRED")=>true,_=>false};if(!allowed)throw new InvalidOperationException($"Invalid subscription transition {x.Status}->{next}");return states[tenant]=x with{Status=next,CurrentPeriodEnd=end};}
 public bool AllowsNewSync(string tenant,long planOrderLimit)=>Get(tenant) is var x&&x.Status is"TRIAL"or"ACTIVE"&&x.CurrentPeriodEnd>DateTimeOffset.UtcNow&&x.OrdersUsed<planOrderLimit;public bool AllowsExistingRead(string tenant)=>states.ContainsKey(tenant);public SubscriptionState Get(string tenant)=>states.TryGetValue(tenant,out var x)?x:throw new KeyNotFoundException();
}
