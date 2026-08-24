using System.Security.Cryptography;using System.Text;
namespace SanSo.Api.Modules;
public record ExportArtifact(string FileName,string Content,string InputChecksum,string GeneratedAt,string TenantId,string RuleVersions);
public static class TraceableExport
{
 public static ExportArtifact ReconciliationCsv(string tenant,Reconciliation r,string ruleVersions="none"){
  static string Safe(string value){if(value.Length>0&&"=+-@".Contains(value[0]))value="'"+value;return '"'+value.Replace("\"","\"\"")+'"';}
  var body=new StringBuilder("order_code,type,amount,explanation\r\n");foreach(var line in r.Lines)body.AppendLine($"{Safe(line.OrderCode)},{Safe(line.Type)},{line.Amount},{Safe(line.Explanation)}");
  var checksum=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body.ToString()))).ToLowerInvariant();
  var metadata=$"# tenant={tenant}; settlement={r.SettlementCode}; generated_at={DateTimeOffset.UtcNow:O}; input_checksum={checksum}; rule_versions={ruleVersions}\r\n";
  return new($"reconciliation-{r.SettlementCode}.csv",metadata+body,checksum,DateTimeOffset.UtcNow.ToString("O"),tenant,ruleVersions);
 }
}
