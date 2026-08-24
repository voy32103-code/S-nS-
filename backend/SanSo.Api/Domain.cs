namespace SanSo.Api;
public record Order(string Id,string TenantId,string Channel,string SourceKey,string Code,long Gross,string Status,DateTimeOffset OccurredAt);
public record LedgerLine(string Id,string TenantId,string OrderId,string Type,long Amount,string SourceKey,string Explanation);
public record Settlement(string Id,string TenantId,string Code,long Actual,DateTimeOffset PaidAt);
public record ReconciliationLine(string OrderCode,string Type,long Amount,string Explanation);
public record Reconciliation(string SettlementCode,long Gross,long Fees,long Refunds,long Expected,long Actual,long Difference,string Status,IReadOnlyList<ReconciliationLine> Lines);
public record Dashboard(long Gross,long ExpectedPayout,long ActualPayout,long Difference,int Matched,int NeedsReview,string DataFreshness,string TaxStatus);
public record ImportResult(string BatchId,int Accepted,int Duplicates,string Status);
