using System.Diagnostics;
using System.Diagnostics.Metrics;
using SanSo.Api.Modules;
using Xunit;

namespace SanSo.Api.Tests;

public sealed class OperationalTelemetryTests
{
    [Fact]
    public void MetricTagsNeverContainTenantOrUnboundedInput()
    {
        var measurements = new List<KeyValuePair<string, object?>>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == OperationalTelemetry.MeterName) meterListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((_, _, tags, _) => measurements.AddRange(tags.ToArray()));
        listener.Start();
        using var telemetry = new OperationalTelemetry();
        telemetry.RecordSync("TENANT-A-PRIVATE", "unknown-shop-123456", "made-up-result", "raw vendor secret detail", TimeSpan.FromMilliseconds(50));
        Assert.DoesNotContain(measurements, tag => tag.Value?.ToString()?.Contains("TENANT-A", StringComparison.OrdinalIgnoreCase) == true);
        Assert.Contains(measurements, tag => tag.Key == "job_type" && Equals(tag.Value, "other"));
        Assert.Contains(measurements, tag => tag.Key == "channel" && Equals(tag.Value, "other"));
        Assert.Contains(measurements, tag => tag.Key == "error_class" && Equals(tag.Value, "other"));
    }

    [Fact]
    public void TraceUsesTenantFingerprintAndRejectsFreeFormCorrelation()
    {
        Activity? captured = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == OperationalTelemetry.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => captured = activity
        };
        ActivitySource.AddActivityListener(listener);
        using var telemetry = new OperationalTelemetry();
        using (telemetry.StartSync("tenant-sensitive-name", "customer@example.com", "order_sync", "shopee")) { }
        Assert.NotNull(captured);
        Assert.Equal(12, captured!.GetTagItem("sanso.tenant_fingerprint")!.ToString()!.Length);
        Assert.NotEqual("tenant-sensitive-name", captured.GetTagItem("sanso.tenant_fingerprint"));
        Assert.Equal("invalid", captured.GetTagItem("sanso.correlation_id"));
    }

    [Fact]
    public void FingerprintIsStableWithoutRevealingTenant()
    {
        var first = OperationalTelemetry.Fingerprint("tenant-a");
        Assert.Equal(first, OperationalTelemetry.Fingerprint("tenant-a"));
        Assert.NotEqual(first, OperationalTelemetry.Fingerprint("tenant-b"));
        Assert.DoesNotContain("tenant", first);
    }
}
