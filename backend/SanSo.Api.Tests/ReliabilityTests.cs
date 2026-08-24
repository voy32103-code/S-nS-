using SanSo.Api.Modules;
namespace SanSo.Api.Tests;
public class ReliabilityTests
{
 [Fact]public void TransientFailureSchedulesBoundedRetryWithoutLosingPayload(){var sync=new ReliableSync();var work=sync.Enqueue("t1","STOCK_PUSH","c1","payload-immutable");var failed=sync.RecordFailure("t1",work.Id,"shop-1","RATE_LIMIT",true);Assert.Equal(WorkStatus.RetryScheduled,failed.Status);Assert.Equal("payload-immutable",failed.Payload);Assert.True(failed.NextAttemptAt>DateTimeOffset.UtcNow);}
 [Fact]public void RevokedTokenPausesWriteBackAndCreatesAlert(){var sync=new ReliableSync();var work=sync.Enqueue("t1","STOCK_PUSH","c1","{}");var failed=sync.RecordFailure("t1",work.Id,"shop-1","TOKEN_REVOKED",false);Assert.Equal(WorkStatus.Paused,failed.Status);Assert.False(sync.Health("t1","shop-1").WriteBackEnabled);Assert.Contains(sync.Alerts("t1"),x=>x.Type=="TOKEN_REVOKED");}
 [Fact]public void TenantCannotMutateAnotherTenantWork(){var sync=new ReliableSync();var work=sync.Enqueue("t1","SYNC","c1","{}");Assert.Throws<KeyNotFoundException>(()=>sync.RecordSuccess("t2",work.Id,"shop"));}
 [Fact]public void ProfitBridgeUsesSignedIntegerMoney(){var result=Profitability.Calculate(new(1_000_000,-90_000,-20_000,-10_000,-30_000,-40_000,-50_000,-400_000,-20_000));Assert.Equal(760_000,result.NetRevenue);Assert.Equal(340_000,result.ContributionProfit);Assert.Equal(.34m,result.Margin);}
}
