using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;

namespace SanSo.Api.Modules;

public sealed class OperationalTelemetry : IDisposable
{
    public const string MeterName = "SanSo.Operations";
    public const string ActivitySourceName = "SanSo.Operations.Sync";
    private static readonly HashSet<string> JobTypes = new(StringComparer.OrdinalIgnoreCase) { "order_sync", "settlement_sync", "inventory_sync", "file_import", "export" };
    private static readonly HashSet<string> Channels = new(StringComparer.OrdinalIgnoreCase) { "shopee", "tiktok_shop", "csv", "xlsx", "demo" };
    private static readonly HashSet<string> Outcomes = new(StringComparer.OrdinalIgnoreCase) { "success", "duplicate", "retry", "dead_letter", "paused", "rejected" };
    private readonly Meter meter = new(MeterName, "1.0.0");
    private readonly ActivitySource activities = new(ActivitySourceName, "1.0.0");
    private readonly Counter<long> attempts;
    private readonly Counter<long> rawEvents;
    private readonly Histogram<double> duration;
    private long queueDepth;

    public OperationalTelemetry()
    {
        attempts = meter.CreateCounter<long>("sanso.sync.attempts", unit: "{attempt}");
        rawEvents = meter.CreateCounter<long>("sanso.raw.events", unit: "{event}");
        duration = meter.CreateHistogram<double>("sanso.sync.duration", unit: "ms");
        meter.CreateObservableGauge("sanso.sync.queue.depth", () => Interlocked.Read(ref queueDepth), unit: "{item}");
    }

    public Activity? StartSync(string tenant, string correlationId, string jobType, string channel)
    {
        var activity = activities.StartActivity("sync.execute", ActivityKind.Internal);
        if (activity is null) return null;
        activity.SetTag("sanso.tenant_fingerprint", Fingerprint(tenant));
        activity.SetTag("sanso.correlation_id", SafeCorrelation(correlationId));
        activity.SetTag("sanso.job_type", Normalize(jobType, JobTypes));
        activity.SetTag("sanso.channel", Normalize(channel, Channels));
        return activity;
    }

    public void RecordSync(string jobType, string channel, string outcome, string? errorCode, TimeSpan elapsed)
    {
        var tags = new TagList
        {
            { "job_type", Normalize(jobType, JobTypes) },
            { "channel", Normalize(channel, Channels) },
            { "outcome", Normalize(outcome, Outcomes) },
            { "error_class", ErrorClass(errorCode) }
        };
        attempts.Add(1, tags);
        duration.Record(Math.Max(0, elapsed.TotalMilliseconds), tags);
    }

    public void RecordRaw(string source, bool duplicate, bool quarantined)
    {
        var tags = new TagList
        {
            { "source", Normalize(source, Channels) },
            { "result", quarantined ? "quarantined" : duplicate ? "duplicate" : "accepted" }
        };
        rawEvents.Add(1, tags);
    }

    public void SetQueueDepth(long value) => Interlocked.Exchange(ref queueDepth, Math.Max(0, value));

    public static string Fingerprint(string tenant)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(tenant));
        return Convert.ToHexString(hash.AsSpan(0, 6)).ToLowerInvariant();
    }

    private static string Normalize(string value, HashSet<string> allowed) => allowed.Contains(value) ? value.ToLowerInvariant() : "other";
    private static string SafeCorrelation(string value) => Guid.TryParse(value, out var id) ? id.ToString("N") : "invalid";
    private static string ErrorClass(string? value) => value switch
    {
        null or "" => "none",
        "TOKEN_REVOKED" or "TOKEN_EXPIRED" => "auth",
        "RATE_LIMITED" => "rate_limit",
        "TIMEOUT" or "NETWORK" => "transient",
        "SCHEMA_DRIFT" or "VALIDATION" => "data",
        _ => "other"
    };

    public void Dispose()
    {
        activities.Dispose();
        meter.Dispose();
    }
}
