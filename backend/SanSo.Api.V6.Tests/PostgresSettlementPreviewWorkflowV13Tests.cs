extern alias apiv6;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Xunit;
using Confirm=apiv6::SanSo.Api.V6.SettlementConfirmRequestV1;
using Importer=apiv6::SanSo.Api.V6.PostgresSettlementImportStoreV1;
using Parser=apiv6::SanSo.Api.V6.SettlementFileParserV1;
using Workflow=apiv6::SanSo.Api.V6.PostgresSettlementImportWorkflowV1;

namespace SanSo.Api.V6.Tests;

public sealed class PostgresSettlementPreviewWorkflowV13Tests
{
    [Fact]
    public async Task PreviewStoresOnlyTokenHashAndDoesNotWriteAccountingBeforeConfirm()
    {
        var cs=Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");if(string.IsNullOrWhiteSpace(cs))return;
        var tenant=Guid.NewGuid();var actor=Guid.NewGuid();await Execute(cs,"INSERT INTO organizations(id,slug,name)VALUES($1,$2,'Preview workflow')",tenant,$"preview-{tenant:N}");
        await Execute(cs,"INSERT INTO orders(organization_id,channel,source_key,code,status,gross_amount,occurred_at)VALUES($1,'CSV',$2,'ORD-PREVIEW','COMPLETED',1000,now())",tenant,$"order-{tenant:N}");await Execute(cs,"INSERT INTO users(id,email,display_name,password_hash,mfa_enabled)VALUES($1,$2,'Preview actor','managed',true)",actor,$"preview-{actor:N}@example.invalid");
        var parsed=Parser.Csv(Encoding.UTF8.GetBytes(Csv()));await using var source=NpgsqlDataSource.Create(cs);var importer=new Importer(source);var workflow=new Workflow(source,importer);
        var preview=await workflow.Stage(tenant.ToString(),parsed,"CSV",default);
        Assert.False(preview.AlreadyConfirmed);Assert.Equal(64,preview.PreviewToken.Length);
        Assert.Equal((0,0,0,0,0),await AccountingCounts(cs,tenant));
        var stored=await Scalar<string>(cs,"SELECT token_hash FROM settlement_import_previews WHERE organization_id=$1 AND id=$2::uuid",tenant,preview.PreviewId);
        var expected=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(preview.PreviewToken))).ToLowerInvariant();
        Assert.Equal(expected,stored);Assert.DoesNotContain(preview.PreviewToken,stored,StringComparison.Ordinal);
        var invalid=await Assert.ThrowsAsync<InvalidOperationException>(()=>workflow.Confirm(tenant.ToString(),new Confirm(preview.PreviewToken+"x",preview.Checksum),actor.ToString(),default));
        Assert.Equal("SETTLEMENT_PREVIEW_INVALID_OR_EXPIRED",invalid.Message);Assert.Equal((0,0,0,0,0),await AccountingCounts(cs,tenant));
        var first=await workflow.Confirm(tenant.ToString(),new Confirm(preview.PreviewToken,preview.Checksum),actor.ToString(),default);Assert.False(first.Duplicate);
        var retry=await workflow.Confirm(tenant.ToString(),new Confirm(preview.PreviewToken,preview.Checksum),actor.ToString(),default);Assert.True(retry.Duplicate);Assert.Equal(first.RunId,retry.RunId);
        Assert.Equal((1,1,3,3,1),await AccountingCounts(cs,tenant));
    }

    private static string Csv()=>"settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount\nSET-PREVIEW,2026-08-24,ln-sale,ORD-PREVIEW,SALE,1000,1000\nSET-PREVIEW,2026-08-24,ln-fee,ORD-PREVIEW,PLATFORM_FEE,-100,-120\nSET-PREVIEW,2026-08-24,ln-adjust,,ADJUSTMENT,20,20";
    private static async Task<(int,int,int,int,int)> AccountingCounts(string cs,Guid tenant){var values=new List<int>();foreach(var table in new[]{"import_batches","settlements","raw_events","ledger_lines","reconciliation_runs"})values.Add(await Scalar<int>(cs,$"SELECT count(*)::int FROM {table} WHERE organization_id=$1",tenant));return(values[0],values[1],values[2],values[3],values[4]);}
    private static async Task<T> Scalar<T>(string cs,string sql,params object[] values){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText=sql;foreach(var value in values)q.Parameters.AddWithValue(value);return (T)(await q.ExecuteScalarAsync()??throw new InvalidOperationException("SCALAR_EMPTY"));}
    private static async Task Execute(string cs,string sql,params object[] values){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText=sql;foreach(var value in values)q.Parameters.AddWithValue(value);await q.ExecuteNonQueryAsync();}
}
