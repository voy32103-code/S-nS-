using System.Collections.Concurrent;using System.Security.Cryptography;using System.Text;using System.Text.Json;
namespace SanSo.Api.Modules;
public record AuditEntry(string Id,string? TenantId,string ActorId,string Action,string ResourceType,string? ResourceId,string Reason,string CorrelationId,string? PreviousHash,string EntryHash,DateTimeOffset CreatedAt);
public sealed class AuditTrail
{
 private readonly ConcurrentQueue<AuditEntry> entries=new();private readonly object guard=new();
 public AuditEntry Append(string? tenant,string actor,string action,string resourceType,string? resourceId,string reason,string correlationId){if(string.IsNullOrWhiteSpace(reason)&&action is "PERIOD_REOPEN" or "SUPPORT_ACCESS" or "SENSITIVE_EXPORT")throw new ArgumentException("Reason is mandatory for sensitive action");lock(guard){var previous=entries.LastOrDefault()?.EntryHash;var at=DateTimeOffset.UtcNow;var canonical=JsonSerializer.Serialize(new{tenant,actor,action,resourceType,resourceId,reason,correlationId,previous,at});var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();var entry=new AuditEntry(Guid.NewGuid().ToString(),tenant,actor,action,resourceType,resourceId,reason,correlationId,previous,hash,at);entries.Enqueue(entry);return entry;}}
 public IReadOnlyList<AuditEntry> ForTenant(string tenant)=>entries.Where(x=>x.TenantId==tenant).ToList();
 public bool VerifyChain(){string? previous=null;foreach(var e in entries){if(e.PreviousHash!=previous)return false;previous=e.EntryHash;}return true;}
}
