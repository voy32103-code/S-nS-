using System.Collections.Concurrent;using System.Security.Cryptography;using System.Text;
namespace SanSo.Api.Modules;
public record RawEnvelope(string Id,string TenantId,string Source,string EventId,string EventType,string SchemaVersion,string Payload,string Checksum,DateTimeOffset ReceivedAt);
public record IngestOutcome(RawEnvelope Event,bool Duplicate);
public sealed class RawIngestion
{
 private readonly ConcurrentDictionary<string,RawEnvelope> events=new();
 public IngestOutcome Accept(string tenant,string source,string eventId,string eventType,string payload,string schemaVersion="1"){
  if(string.IsNullOrWhiteSpace(tenant)||string.IsNullOrWhiteSpace(eventId))throw new ArgumentException("tenant and eventId are required");
  var key=$"{tenant}|{source}|{eventId}";var checksum=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
  var created=new RawEnvelope(Guid.NewGuid().ToString(),tenant,source,eventId,eventType,schemaVersion,payload,checksum,DateTimeOffset.UtcNow);
  var stored=events.GetOrAdd(key,created);return new(stored,!ReferenceEquals(stored,created));
 }
 public RawEnvelope? Find(string tenant,string id)=>events.Values.SingleOrDefault(x=>x.TenantId==tenant&&x.Id==id);
}
