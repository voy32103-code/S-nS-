extern alias apiv6;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SanSo.Api.Modules;
using Xunit;
using V10Program=apiv6::Program;

namespace SanSo.Api.V6.Tests;

public sealed class PostgresSettlementImportHttpV10Tests
{
    [Fact]
    public async Task CsvImportCreatesTraceableLineReconciliationAndIsIdempotent()
    {
        var cs=Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");if(string.IsNullOrWhiteSpace(cs))return;var tenant=Guid.NewGuid();await Seed(cs,tenant);await using var factory=new WebApplicationFactory<V10Program>().WithWebHostBuilder(x=>{x.UseEnvironment("Development");x.UseSetting("ConnectionStrings:Postgres",cs);});using var client=factory.CreateClient();Assert.Equal(HttpStatusCode.Unauthorized,(await Upload(client,Csv(),null,tenant)).StatusCode);var identity=factory.Services.GetRequiredService<IdentityService>();var secret=IdentityService.GenerateTotpSecret();var email=$"settlement-{Guid.NewGuid():N}@example.invalid";var user=identity.Register(email,"Settlement finance","Long-Safe-Settlement-Password-2026!",secret);identity.AddMembership(tenant.ToString(),user.Id,OrgRole.Finance);await SeedUser(cs,Guid.Parse(user.Id),email);var login=await client.PostAsJsonAsync("/api/auth/login",new{email,password="Long-Safe-Settlement-Password-2026!",tenantId=tenant.ToString(),totpCode=IdentityService.CurrentTotp(secret)});var token=(await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
        var first=await Upload(client,Csv(),token,tenant);var firstText=await first.Content.ReadAsStringAsync();Assert.True(first.IsSuccessStatusCode,$"IMPORT_FAILED {(int)first.StatusCode}: {firstText}");var body=JsonSerializer.Deserialize<JsonElement>(firstText);Assert.Equal(920,body.GetProperty("expectedAmount").GetInt64());Assert.Equal(900,body.GetProperty("actualAmount").GetInt64());Assert.Equal(-20,body.GetProperty("difference").GetInt64());Assert.Equal("NEEDS_REVIEW",body.GetProperty("status").GetString());Assert.False(body.GetProperty("duplicate").GetBoolean());var runId=body.GetProperty("runId").GetString()!;
        var retry=await Upload(client,Csv(),token,tenant);var retryBody=await retry.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal(runId,retryBody.GetProperty("confirmedRunId").GetString());Assert.True(retryBody.GetProperty("alreadyConfirmed").GetBoolean());
        using var get=new HttpRequestMessage(HttpMethod.Get,$"/api/reconciliations/{runId}");Auth(get,token,tenant);using var detail=await client.SendAsync(get);var detailText=await detail.Content.ReadAsStringAsync();Assert.True(detail.IsSuccessStatusCode,$"DETAIL_FAILED {(int)detail.StatusCode}: {detailText}");var detailBody=JsonSerializer.Deserialize<JsonElement>(detailText);var lines=detailBody.GetProperty("lines").EnumerateArray().ToArray();Assert.Equal(3,lines.Length);Assert.Contains(lines,x=>x.GetProperty("reasonCode").GetString()=="LINE_AMOUNT_MISMATCH");Assert.Contains(lines,x=>x.GetProperty("reasonCode").GetString()=="ORDER_REFERENCE_MISSING");Assert.All(lines,x=>Assert.StartsWith("settlement:",x.GetProperty("rawSourceEventId").GetString()));
        var conflict=await Upload(client,Csv().Replace("-100,-120","-100,-110"),token,tenant);Assert.Equal(HttpStatusCode.Conflict,conflict.StatusCode);Assert.Contains("SETTLEMENT_CODE_CONFLICT",await conflict.Content.ReadAsStringAsync());var counts=await Counts(cs,tenant);Assert.Equal((1,1,3,3,1,3,1),counts);
    }
    private static string Csv()=>"settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount\nSET-HTTP,2026-08-24,ln-sale,ORD-HTTP,SALE,1000,1000\nSET-HTTP,2026-08-24,ln-fee,ORD-HTTP,PLATFORM_FEE,-100,-120\nSET-HTTP,2026-08-24,ln-adjust,,ADJUSTMENT,20,20";
    private static async Task<HttpResponseMessage> Upload(HttpClient c,string csv,string? token,Guid tenant){using var form=new MultipartFormDataContent();form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)){Headers={ContentType=new("text/csv")}},"file","settlements.csv");using var request=new HttpRequestMessage(HttpMethod.Post,"/api/imports/settlements"){Content=form};if(token is not null)Auth(request,token,tenant);var preview=await c.SendAsync(request);if(token is null||!preview.IsSuccessStatusCode)return preview;var p=await preview.Content.ReadFromJsonAsync<JsonElement>();if(p.GetProperty("alreadyConfirmed").GetBoolean()){var confirmed=new HttpResponseMessage(preview.StatusCode){Content=JsonContent.Create(p)};preview.Dispose();return confirmed;}preview.Dispose();using var confirm=JsonContent.Create(new{previewToken=p.GetProperty("previewToken").GetString(),checksum=p.GetProperty("checksum").GetString()});using var confirmRequest=new HttpRequestMessage(HttpMethod.Post,"/api/imports/settlements/confirm"){Content=confirm};Auth(confirmRequest,token,tenant);return await c.SendAsync(confirmRequest);}
    private static void Auth(HttpRequestMessage r,string token,Guid tenant){r.Headers.Authorization=new("Bearer",token);r.Headers.Add("X-Tenant-Id",tenant.ToString());}
    private static async Task Seed(string cs,Guid tenant){await Execute(cs,"INSERT INTO organizations(id,slug,name)VALUES($1,$2,'Settlement HTTP')",tenant,$"settle-http-{tenant:N}");await Execute(cs,"INSERT INTO orders(organization_id,channel,source_key,code,status,gross_amount,occurred_at)VALUES($1,'CSV',$2,'ORD-HTTP','COMPLETED',1000,now())",tenant,$"order-{tenant:N}");}
    private static async Task SeedUser(string cs,Guid id,string email)=>await Execute(cs,"INSERT INTO users(id,email,display_name,password_hash,mfa_enabled)VALUES($1,$2,'Finance','managed',true)",id,email);
    private static async Task<(int,int,int,int,int,int,int)> Counts(string cs,Guid tenant){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();var values=new List<int>();foreach(var table in new[]{"import_batches","settlements","raw_events","ledger_lines","reconciliation_runs","reconciliation_lines","audit_logs"}){await using var q=c.CreateCommand();q.CommandText=$"SELECT count(*)::int FROM {table} WHERE organization_id=$1";q.Parameters.AddWithValue(tenant);values.Add((int)(await q.ExecuteScalarAsync()??0));}return(values[0],values[1],values[2],values[3],values[4],values[5],values[6]);}
    private static async Task Execute(string cs,string sql,params object[] values){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText=sql;foreach(var value in values)q.Parameters.AddWithValue(value);await q.ExecuteNonQueryAsync();}
}
