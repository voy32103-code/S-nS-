using System.Diagnostics;

namespace SanSo.Api.Modules;

public sealed record SyncExecutionResult(OutboxWork Work, IntegrationHealth Health);
public sealed class SyncExecutionException(string code, bool transient, string safeMessage) : Exception(safeMessage)
{
    public string Code { get; } = code;
    public bool Transient { get; } = transient;
}

public sealed class InstrumentedSyncExecutor(ReliableSync sync, OperationalTelemetry telemetry)
{
    public async Task<SyncExecutionResult> Execute(
        string tenant,
        string connection,
        string jobType,
        string channel,
        string correlationId,
        string payload,
        Func<OutboxWork, CancellationToken, Task> handler,
        CancellationToken ct = default)
    {
        var work = sync.Enqueue(tenant, jobType, correlationId, payload);
        var started = Stopwatch.GetTimestamp();
        using var activity = telemetry.StartSync(tenant, correlationId, jobType, channel);
        try
        {
            await handler(work, ct);
            var completed = sync.RecordSuccess(tenant, work.Id, connection);
            telemetry.RecordSync(jobType, channel, "success", null, Stopwatch.GetElapsedTime(started));
            activity?.SetStatus(ActivityStatusCode.Ok);
            return new(completed, sync.Health(tenant, connection));
        }
        catch (SyncExecutionException error)
        {
            var failed = sync.RecordFailure(tenant, work.Id, connection, error.Code, error.Transient);
            var outcome = failed.Status switch
            {
                WorkStatus.RetryScheduled => "retry",
                WorkStatus.DeadLetter => "dead_letter",
                WorkStatus.Paused => "paused",
                _ => "rejected"
            };
            telemetry.RecordSync(jobType, channel, outcome, error.Code, Stopwatch.GetElapsedTime(started));
            activity?.SetTag("sanso.error_class", SafeErrorClass(error.Code));
            activity?.SetStatus(ActivityStatusCode.Error, error.Code);
            return new(failed, sync.Health(tenant, connection));
        }
    }

    private static string SafeErrorClass(string code) => code switch
    {
        "TOKEN_REVOKED" or "TOKEN_EXPIRED" => "auth",
        "RATE_LIMITED" => "rate_limit",
        "TIMEOUT" or "NETWORK" => "transient",
        "SCHEMA_DRIFT" or "VALIDATION" => "data",
        _ => "other"
    };
}
