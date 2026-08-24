using SanSo.Api.Modules;
using Xunit;

namespace SanSo.Api.Tests;

public sealed class InstrumentedSyncExecutorTests
{
    [Fact]
    public async Task SuccessfulJobUpdatesHealthWithoutChangingPayload()
    {
        var sync = new ReliableSync();
        using var telemetry = new OperationalTelemetry();
        var executor = new InstrumentedSyncExecutor(sync, telemetry);
        OutboxWork? observed = null;
        var result = await executor.Execute("tenant-a", "shop-a", "order_sync", "shopee", Guid.NewGuid().ToString(), "{\"secret\":\"kept-in-work-only\"}", (work, _) => { observed = work; return Task.CompletedTask; });
        Assert.Equal(WorkStatus.Completed, result.Work.Status);
        Assert.Equal("HEALTHY", result.Health.Status);
        Assert.Equal("{\"secret\":\"kept-in-work-only\"}", observed!.Payload);
    }

    [Fact]
    public async Task TransientFailureSchedulesRetryAndKeepsWriteBackEnabled()
    {
        var sync = new ReliableSync();
        using var telemetry = new OperationalTelemetry();
        var executor = new InstrumentedSyncExecutor(sync, telemetry);
        var result = await executor.Execute("tenant-a", "shop-a", "settlement_sync", "tiktok_shop", Guid.NewGuid().ToString(), "{}", (_, _) => throw new SyncExecutionException("TIMEOUT", true, "Safe timeout"));
        Assert.Equal(WorkStatus.RetryScheduled, result.Work.Status);
        Assert.True(result.Health.WriteBackEnabled);
        Assert.Equal("RETRYING", result.Health.Status);
    }

    [Fact]
    public async Task RevokedTokenPausesJobAndDisablesWriteBack()
    {
        var sync = new ReliableSync();
        using var telemetry = new OperationalTelemetry();
        var executor = new InstrumentedSyncExecutor(sync, telemetry);
        var result = await executor.Execute("tenant-a", "shop-a", "inventory_sync", "shopee", Guid.NewGuid().ToString(), "{}", (_, _) => throw new SyncExecutionException("TOKEN_REVOKED", false, "Reauthentication required"));
        Assert.Equal(WorkStatus.Paused, result.Work.Status);
        Assert.False(result.Health.WriteBackEnabled);
        Assert.Contains(sync.Alerts("tenant-a"), alert => alert.Type == "TOKEN_REVOKED");
    }
}
