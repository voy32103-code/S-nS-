using System.Collections.Concurrent;
namespace SanSo.Api.Modules;
public record InventoryBalance(string TenantId,string Sku,int OnHand,int Reserved,int Quarantine,long Version){public int Available=>OnHand-Reserved-Quarantine;}
public record InventoryMovement(string Id,string TenantId,string Sku,string Type,int Quantity,string SourceKey,DateTimeOffset OccurredAt);
public sealed class InventoryService
{
 private readonly ConcurrentDictionary<string,InventoryBalance> balances=new();private readonly ConcurrentDictionary<string,InventoryMovement> movements=new();private readonly ConcurrentDictionary<string,object> guards=new();private static string Key(string tenant,string sku)=>$"{tenant}|{sku}";
 public InventoryBalance Seed(string tenant,string sku,int onHand){var b=new InventoryBalance(tenant,sku,onHand,0,0,0);balances[Key(tenant,sku)]=b;return b;}
 public bool Reserve(string tenant,string sku,int qty,string sourceKey){if(qty<=0)throw new ArgumentOutOfRangeException(nameof(qty));var movementKey=$"{tenant}|{sourceKey}";if(movements.ContainsKey(movementKey))return true;lock(guards.GetOrAdd(Key(tenant,sku),_=>new())){if(movements.ContainsKey(movementKey))return true;var current=Get(tenant,sku);if(current.Available<qty)return false;balances[Key(tenant,sku)]=current with{Reserved=current.Reserved+qty,Version=current.Version+1};movements[movementKey]=new(Guid.NewGuid().ToString(),tenant,sku,"RESERVE",qty,sourceKey,DateTimeOffset.UtcNow);return true;}}
 public void Release(string tenant,string sku,int qty,string sourceKey){lock(guards.GetOrAdd(Key(tenant,sku),_=>new())){var current=Get(tenant,sku);if(current.Reserved<qty)throw new InvalidOperationException("Cannot release more than reserved");if(!movements.TryAdd($"{tenant}|{sourceKey}",new(Guid.NewGuid().ToString(),tenant,sku,"RELEASE",qty,sourceKey,DateTimeOffset.UtcNow)))return;balances[Key(tenant,sku)]=current with{Reserved=current.Reserved-qty,Version=current.Version+1};}}
 public void ReceiveReturnToQuarantine(string tenant,string sku,int qty,string sourceKey){lock(guards.GetOrAdd(Key(tenant,sku),_=>new())){var current=Get(tenant,sku);if(!movements.TryAdd($"{tenant}|{sourceKey}",new(Guid.NewGuid().ToString(),tenant,sku,"RETURN_QUARANTINE",qty,sourceKey,DateTimeOffset.UtcNow)))return;balances[Key(tenant,sku)]=current with{OnHand=current.OnHand+qty,Quarantine=current.Quarantine+qty,Version=current.Version+1};}}
 public InventoryBalance Get(string tenant,string sku)=>balances.TryGetValue(Key(tenant,sku),out var b)?b:throw new KeyNotFoundException("Inventory balance not found");
}
