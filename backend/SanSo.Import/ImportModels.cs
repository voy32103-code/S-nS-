namespace SanSo.Import;
public record ImportRow(int RowNumber,string? OrderCode,long? Amount,DateTimeOffset? OccurredAt,IReadOnlyDictionary<string,string> Raw,IReadOnlyList<string> Errors);
public record ImportPreview(string Format,string TemplateVersion,string Checksum,char? Delimiter,IReadOnlyList<string> Headers,IReadOnlyList<ImportRow> Rows,IReadOnlyList<string> Errors,bool Duplicate);
public sealed class ImportRegistry{private readonly HashSet<string> checksums=[];private readonly object gate=new();public bool Mark(string tenant,string checksum){lock(gate)return!checksums.Add($"{tenant}|{checksum}");}}
