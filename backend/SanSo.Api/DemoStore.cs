using System.Collections.Concurrent;
namespace SanSo.Api;
public sealed class DemoStore
{
 private readonly ConcurrentDictionary<string,Order> orders=new();
 private readonly ConcurrentDictionary<string,LedgerLine> ledger=new();
 private readonly ConcurrentDictionary<string,Settlement> settlements=new();
 public DemoStore()=>Seed("tenant-an-nhien");
 public void Seed(string tenant){AddOrder(new("ord-001",tenant,"Shopee","shopee:order:001","SP-240801",1_250_000,"COMPLETED",DateTimeOffset.Parse("2026-08-20T09:00:00+07:00")),-112_500);AddOrder(new("ord-002",tenant,"TikTok Shop","tiktok:order:002","TT-240802",780_000,"COMPLETED",DateTimeOffset.Parse("2026-08-20T10:00:00+07:00")),-70_200);settlements.TryAdd(Key(tenant,"stl-001"),new("stl-001",tenant,"SET-2026-08-01",1_817_300,DateTimeOffset.Parse("2026-08-22T15:00:00+07:00")));}
 private void AddOrder(Order o,long fee){if(!orders.TryAdd(Key(o.TenantId,o.SourceKey),o))return;ledger.TryAdd(Key(o.TenantId,$"{o.SourceKey}:gross"),new($"led-{o.Id}-g",o.TenantId,o.Id,"SALE",o.Gross,$"{o.SourceKey}:gross","Giá trị hàng hóa từ đơn nguồn"));ledger.TryAdd(Key(o.TenantId,$"{o.SourceKey}:fee"),new($"led-{o.Id}-f",o.TenantId,o.Id,"PLATFORM_FEE",fee,$"{o.SourceKey}:fee","Phí sàn từ dòng giao dịch nguồn"));}
 public ImportResult ImportDemo(string tenant){var before=orders.Count(x=>x.Value.TenantId==tenant);Seed(tenant);var accepted=orders.Count(x=>x.Value.TenantId==tenant)-before;return new($"batch-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",accepted,2-accepted,"COMPLETED");}
 public IReadOnlyList<Order> Orders(string tenant)=>orders.Values.Where(x=>x.TenantId==tenant).OrderByDescending(x=>x.OccurredAt).ToList();
 public IReadOnlyList<LedgerLine> Ledger(string tenant)=>ledger.Values.Where(x=>x.TenantId==tenant).ToList();
 public Reconciliation Reconcile(string tenant){var ls=Ledger(tenant);var s=settlements.Values.Single(x=>x.TenantId==tenant);var gross=ls.Where(x=>x.Type=="SALE").Sum(x=>x.Amount);var fees=ls.Where(x=>x.Type=="PLATFORM_FEE").Sum(x=>x.Amount);var expected=gross+fees;var diff=s.Actual-expected;var lines=ls.Select(x=>new ReconciliationLine(Orders(tenant).Single(o=>o.Id==x.OrderId).Code,x.Type,x.Amount,x.Explanation)).ToList();return new(s.Code,gross,fees,0,expected,s.Actual,diff,diff==0?"MATCHED":"NEEDS_REVIEW",lines);}
 public Dashboard Dashboard(string tenant){var r=Reconcile(tenant);return new(r.Gross,r.Expected,r.Actual,r.Difference,r.Status=="MATCHED"?1:0,r.Status=="NEEDS_REVIEW"?1:0,"Đồng bộ 2 phút trước","NEEDS_REVIEW — chưa có rule được phê duyệt");}
 private static string Key(string tenant,string id)=>$"{tenant}|{id}";
}
