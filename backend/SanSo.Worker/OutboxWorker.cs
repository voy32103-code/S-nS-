using Npgsql;

namespace SanSo.Worker;

public sealed record OutboxEnvelope(Guid Id,string TenantId,string Type,string Payload,string CorrelationId,int Attempt,DateTimeOffset CreatedAt);
public sealed record WorkerFailure(string Code,bool Transient);
public interface IOutboxStore
{
    Task<int> RecoverExpiredLeases(CancellationToken ct);
    Task<OutboxEnvelope?> Claim(TimeSpan lease,CancellationToken ct);
    Task Complete(Guid id,CancellationToken ct);
    Task Fail(Guid id,WorkerFailure failure,TimeSpan delay,bool deadLetter,CancellationToken ct);
}
public interface IOutboxHandler{Task Handle(OutboxEnvelope work,CancellationToken ct);}
public sealed class OutboxHandlingException(string code,bool transient):Exception(code){public string Code{get;}=code;public bool Transient{get;}=transient;}

public sealed class OutboxProcessor(IOutboxStore store,IOutboxHandler handler,TimeProvider? timeProvider=null)
{
    private readonly TimeProvider clock=timeProvider??TimeProvider.System;
    public Task<int> Recover(CancellationToken ct=default)=>store.RecoverExpiredLeases(ct);
    public async Task<bool> Tick(CancellationToken ct=default)
    {
        var work=await store.Claim(TimeSpan.FromMinutes(2),ct);if(work is null)return false;
        try{await handler.Handle(work,ct);await store.Complete(work.Id,ct);return true;}
        catch(OutboxHandlingException error){var attempt=work.Attempt;var dead=!error.Transient||attempt>=5;var delay=TimeSpan.FromSeconds(Math.Min(900,Math.Pow(2,attempt)*5));await store.Fail(work.Id,new(error.Code,error.Transient),delay,dead,ct);return true;}
        catch(OperationCanceledException)when(ct.IsCancellationRequested){throw;}
        catch(Exception){var attempt=work.Attempt;var dead=attempt>=5;var delay=TimeSpan.FromSeconds(Math.Min(900,Math.Pow(2,attempt)*5));await store.Fail(work.Id,new("UNEXPECTED_HANDLER_FAILURE",true),delay,dead,ct);return true;}
    }
}

public sealed class PostgresOutboxStore(NpgsqlDataSource dataSource,string tenant):IOutboxStore
{
    public async Task<int> RecoverExpiredLeases(CancellationToken ct){await using var c=await Open(ct);await using var q=c.CreateCommand();q.CommandText="UPDATE outbox_messages SET status='RETRY_SCHEDULED',next_attempt_at=now() WHERE organization_id=$1::uuid AND status='PROCESSING' AND next_attempt_at<=now()";q.Parameters.AddWithValue(tenant);return await q.ExecuteNonQueryAsync(ct);}
    public async Task<OutboxEnvelope?> Claim(TimeSpan lease,CancellationToken ct)
    {
        await using var c=await Open(ct);await using var tx=await c.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted,ct);await using var q=c.CreateCommand();q.Transaction=tx;q.CommandText="""
WITH candidate AS (
 SELECT id FROM outbox_messages WHERE organization_id=$1::uuid AND status IN('PENDING','RETRY_SCHEDULED') AND next_attempt_at<=now()
 ORDER BY created_at,id FOR UPDATE SKIP LOCKED LIMIT 1
)
UPDATE outbox_messages o SET status='PROCESSING',attempt=o.attempt+1,next_attempt_at=now()+$2::interval
FROM candidate WHERE o.id=candidate.id
RETURNING o.id,o.organization_id::text,o.type,o.payload::text,o.correlation_id,o.attempt,o.created_at
""";q.Parameters.AddWithValue(tenant);q.Parameters.AddWithValue(lease);await using var r=await q.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct)){await tx.CommitAsync(ct);return null;}var item=new OutboxEnvelope(r.GetGuid(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetInt32(5),r.GetFieldValue<DateTimeOffset>(6));await r.CloseAsync();await tx.CommitAsync(ct);return item;
    }
    public async Task Complete(Guid id,CancellationToken ct){await Update(id,"COMPLETED",null,TimeSpan.Zero,ct);}
    public async Task Fail(Guid id,WorkerFailure failure,TimeSpan delay,bool deadLetter,CancellationToken ct){await Update(id,deadLetter?"DEAD_LETTER":"RETRY_SCHEDULED",failure.Code,delay,ct);}
    private async Task Update(Guid id,string status,string? error,TimeSpan delay,CancellationToken ct){await using var c=await Open(ct);await using var q=c.CreateCommand();q.CommandText="UPDATE outbox_messages SET status=$1,error_code=$2,next_attempt_at=now()+$3::interval WHERE organization_id=$4::uuid AND id=$5 AND status='PROCESSING'";q.Parameters.AddWithValue(status);q.Parameters.AddWithValue((object?)error??DBNull.Value);q.Parameters.AddWithValue(delay);q.Parameters.AddWithValue(tenant);q.Parameters.AddWithValue(id);if(await q.ExecuteNonQueryAsync(ct)!=1)throw new InvalidOperationException("OUTBOX_LEASE_LOST");}
    private async Task<NpgsqlConnection> Open(CancellationToken ct){var c=await dataSource.OpenConnectionAsync(ct);await using var q=c.CreateCommand();q.CommandText="SELECT set_config('app.current_organization_id',$1,false)";q.Parameters.AddWithValue(tenant);await q.ExecuteNonQueryAsync(ct);return c;}
}

public sealed class PilotOutboxHandler:IOutboxHandler
{
    public Task Handle(OutboxEnvelope work,CancellationToken ct)=>work.Type switch
    {
        "NOOP_AUDIT"=>Task.CompletedTask,
        "SHOPEE_SYNC" or "TIKTOK_SYNC"=>throw new OutboxHandlingException("PARTNER_ADAPTER_NOT_CONFIGURED",false),
        "EMAIL_DELIVERY"=>throw new OutboxHandlingException("EMAIL_PROVIDER_NOT_CONFIGURED",false),
        _=>throw new OutboxHandlingException("OUTBOX_TYPE_UNSUPPORTED",false)
    };
}
