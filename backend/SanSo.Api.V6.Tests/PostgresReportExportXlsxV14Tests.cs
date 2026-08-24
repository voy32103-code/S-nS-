extern alias apiv6;
using System.Security.Cryptography;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Npgsql;
using Xunit;
using ExportStore=apiv6::SanSo.Api.V6.PostgresReportExportStoreV2;
using Importer=apiv6::SanSo.Api.V6.PostgresSettlementImportStoreV1;
using Parser=apiv6::SanSo.Api.V6.SettlementFileParserV1;

namespace SanSo.Api.V6.Tests;

public sealed class PostgresReportExportXlsxV14Tests
{
    [Fact]
    public async Task XlsxPreviewConfirmDownloadIsTraceableAndFormulaFree()
    {
        var cs=Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");if(string.IsNullOrWhiteSpace(cs))return;var tenant=Guid.NewGuid();var actor=Guid.NewGuid();await Execute(cs,"INSERT INTO organizations(id,slug,name)VALUES($1,$2,'XLSX export')",tenant,$"xlsx-{tenant:N}");await Execute(cs,"INSERT INTO users(id,email,display_name,password_hash,mfa_enabled)VALUES($1,$2,'XLSX actor','managed',true)",actor,$"xlsx-{actor:N}@example.invalid");await Execute(cs,"INSERT INTO orders(organization_id,channel,source_key,code,status,gross_amount,occurred_at)VALUES($1,'CSV',$2,'ORD-XLSX','COMPLETED',1000,now())",tenant,$"order-{tenant:N}");
        await using var source=NpgsqlDataSource.Create(cs);var importer=new Importer(source);var parsed=Parser.Csv(Encoding.UTF8.GetBytes("settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount\nSET-XLSX-EXPORT,2026-08-24,ln-safe,ORD-XLSX,SALE,1000,1000\nSET-XLSX-EXPORT,2026-08-24,ln-fee,ORD-XLSX,PLATFORM_FEE,-100,-120"));var imported=await importer.Import(tenant.ToString(),parsed,actor.ToString(),default);
        var store=new ExportStore(source);var catalog=ExportStore.Catalog();Assert.Contains(catalog,x=>x.Code=="RECONCILIATION_XLSX"&&x.Format=="XLSX");var preview=await store.Preview(tenant.ToString(),imported.RunId,actor.ToString(),"RECONCILIATION_XLSX",default);Assert.Equal("PREVIEWED",preview.Status);Assert.EndsWith(".xlsx",preview.FileName);Assert.Equal(2,preview.LineCount);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>store.Download(tenant.ToString(),preview.ExportId,actor.ToString(),default));var ready=await store.Confirm(tenant.ToString(),preview.ExportId,preview.ContentChecksum,actor.ToString(),default);Assert.Equal("READY",ready.Status);var file=await store.Download(tenant.ToString(),preview.ExportId,actor.ToString(),default);Assert.Equal(preview.ContentChecksum,Convert.ToHexString(SHA256.HashData(file.Content)).ToLowerInvariant());
        using var memory=new MemoryStream(file.Content);using var document=SpreadsheetDocument.Open(memory,false);var sheets=document.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().Select(x=>x.Name!.Value).ToArray();Assert.Equal(new[]{"Metadata","Reconciliation"},sheets);Assert.Empty(document.WorkbookPart.WorksheetParts.SelectMany(x=>x.Worksheet.Descendants<CellFormula>()));Assert.Contains(document.WorkbookPart.WorksheetParts.SelectMany(x=>x.Worksheet.Descendants<Text>()),x=>x.Text=="ln-safe");
    }
    private static async Task Execute(string cs,string sql,params object[] values){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText=sql;foreach(var value in values)q.Parameters.AddWithValue(value);await q.ExecuteNonQueryAsync();}
}
