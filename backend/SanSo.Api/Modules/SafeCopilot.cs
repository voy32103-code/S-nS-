namespace SanSo.Api.Modules;
public record EvidenceItem(string SourceId,string Label,string Text);
public record CopilotAnswer(string Status,string Answer,IReadOnlyList<string> Citations);
public sealed class SafeCopilot
{
 public CopilotAnswer Explain(string tenant,string question,IReadOnlyList<EvidenceItem> tenantEvidence){if(question.Contains("thuế suất",StringComparison.OrdinalIgnoreCase)||question.Contains("quyết định số thuế",StringComparison.OrdinalIgnoreCase))return new("REFUSED","Tôi không thể chọn thuế suất hoặc quyết định số thuế. Hãy dùng rule version đã được chuyên gia phê duyệt; nếu thiếu dữ liệu, giữ NEEDS_REVIEW.",[]);if(tenantEvidence.Count==0)return new("NEEDS_EVIDENCE","Chưa có bằng chứng trong tenant để giải thích.",[]);var citations=tenantEvidence.Take(3).Select(x=>x.SourceId).ToList();return new("EXPLAINED",$"Tóm tắt dựa trên {citations.Count} nguồn: "+string.Join("; ",tenantEvidence.Take(3).Select(x=>x.Label+": "+x.Text)),citations);}
}
