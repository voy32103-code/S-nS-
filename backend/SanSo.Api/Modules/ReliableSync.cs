using System.Collections.Concurrent;
namespace SanSo.Api.Modules;
public enum WorkStatus{Pending,Processing,RetryScheduled,Completed,DeadLetter,Paused}
public record OutboxWork(string Id,string TenantId,string Type,string CorrelationId,string Payload,int Attempt,DateTimeOffset NextAttemptAt,WorkStatus Status,string? LastError=null);
public record IntegrationHealth(string TenantId,string ConnectionId,string Status,DateTimeOffset? LastSuccessAt,string? ErrorCode,bool WriteBackEnabled);
public record AppAlert(string Id,string TenantId,string Type,string Severity,string Title,string Details,DateTimeOffset CreatedAt,bool Acknowledged=false);
public sealed class ReliableSync
{
 private readonly ConcurrentDictionary<string,OutboxWork> work=new();private readonly ConcurrentDictionary<string,IntegrationHealth> health=new();private readonly ConcurrentQueue<AppAlert>alerts=new();
 public OutboxWork Enqueue(string tenant,string type,string correlation,string payload){var x=new OutboxWork(Guid.NewGuid().ToString(),tenant,type,correlation,payload,0,DateTimeOffset.UtcNow,WorkStatus.Pending);work[x.Id]=x;return x;}
 public OutboxWork RecordSuccess(string tenant,string id,string connection){var current=Owned(tenant,id);var done=current with{Attempt=current.Attempt+1,Status=WorkStatus.Completed,LastError=null};work[id]=done;health[$"{tenant}|{connection}"]=new(tenant,connection,"HEALTHY",DateTimeOffset.UtcNow,null,true);return done;}
 public OutboxWork RecordFailure(string tenant,string id,string connection,string code,bool transient){var current=Owned(tenant,id);var attempt=current.Attempt+1;var revoked=code is "TOKEN_REVOKED" or "TOKEN_EXPIRED";var dead=!transient||attempt>=5||revoked;var status=revoked?WorkStatus.Paused:dead?WorkStatus.DeadLetter:WorkStatus.RetryScheduled;var delay=TimeSpan.FromSeconds(Math.Min(900,Math.Pow(2,attempt)*5));var failed=current with{Attempt=attempt,Status=status,NextAttemptAt=DateTimeOffset.UtcNow+delay,LastError=code};work[id]=failed;health[$"{tenant}|{connection}"]=new(tenant,connection,revoked?"DEGRADED_AUTH":dead?"DEGRADED_FAILURE":"RETRYING",null,code,!revoked&&!dead);if(revoked||dead)alerts.Enqueue(new(Guid.NewGuid().ToString(),tenant,revoked?"TOKEN_REVOKED":"SYNC_FAILED","HIGH",revoked?"Kết nối cần xác thực lại":"Đồng bộ thất bại",code,DateTimeOffset.UtcNow));return failed;}
 public IntegrationHealth Health(string tenant,string connection)=>health.GetValueOrDefault($"{tenant}|{connection}",new(tenant,connection,"NEVER_SYNCED",null,null,false));
 public IReadOnlyList<AppAlert> Alerts(string tenant)=>alerts.Where(x=>x.TenantId==tenant).ToList();
 private OutboxWork Owned(string tenant,string id)=>work.TryGetValue(id,out var x)&&x.TenantId==tenant?x:throw new KeyNotFoundException("Work item not found");
}
