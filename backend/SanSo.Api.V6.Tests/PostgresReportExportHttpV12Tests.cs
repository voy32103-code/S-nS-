extern alias apiv6;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SanSo.Api.Modules;
using Xunit;
using V12Program=apiv6::Program;

namespace SanSo.Api.V6.Tests;

public sealed class PostgresReportExportHttpV12Tests
{
    [Fact]
    public async Task ExportRequiresStepUpPreviewChecksumConfirmAndAuditedDownload()
    {
        var cs=Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");if(string.IsNullOrWhiteSpace(cs))return;var tenant=Guid.NewGuid();await SeedOrganization(cs,tenant);await using var factory=new WebApplicationFactory<V12Program>().WithWebHostBuilder(x=>{x.UseEnvironment("Development");x.UseSetting("ConnectionStrings:Postgres",cs);});using var client=factory.CreateClient();var identity=factory.Services.GetRequiredService<IdentityService>();var owner=await Login(identity,client,cs,tenant,OrgRole.Owner,true);var finance=await Login(identity,client,cs,tenant,OrgRole.Finance,false);
        var settlement=await UploadSettlement(client,owner.Token,tenant);var runId=settlement.GetProperty("runId").GetString()!;
        using(var denied=Json(HttpMethod.Post,"/api/reports/exports",new{runId},finance.Token,tenant)){using var response=await client.SendAsync(denied);Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);}
        using var catalogRequest=new HttpRequestMessage(HttpMethod.Get,"/api/reports");Auth(catalogRequest,owner.Token,tenant);using var catalog=await client.SendAsync(catalogRequest);Assert.Equal(HttpStatusCode.OK,catalog.StatusCode);Assert.Contains("RECONCILIATION_CSV",await catalog.Content.ReadAsStringAsync());
        JsonElement preview;using(var request=Json(HttpMethod.Post,"/api/reports/exports",new{runId},owner.Token,tenant)){using var response=await client.SendAsync(request);var text=await response.Content.ReadAsStringAsync();Assert.True(response.IsSuccessStatusCode,text);preview=JsonSerializer.Deserialize<JsonElement>(text);}var exportId=preview.GetProperty("exportId").GetString()!;var checksum=preview.GetProperty("contentChecksum").GetString()!;Assert.Equal("PREVIEWED",preview.GetProperty("status").GetString());
        using(var request=new HttpRequestMessage(HttpMethod.Get,$"/api/reports/exports/{exportId}/download")){Auth(request,owner.Token,tenant);using var response=await client.SendAsync(request);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Contains("EXPORT_NOT_READY_OR_EXPIRED",await response.Content.ReadAsStringAsync());}
        using(var wrong=Json(HttpMethod.Post,$"/api/reports/exports/{exportId}/confirm",new{contentChecksum=new string('0',64)},owner.Token,tenant)){using var response=await client.SendAsync(wrong);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);}
        using(var confirm=Json(HttpMethod.Post,$"/api/reports/exports/{exportId}/confirm",new{contentChecksum=checksum},owner.Token,tenant)){using var response=await client.SendAsync(confirm);Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Contains("READY",await response.Content.ReadAsStringAsync());}
        using(var metadataRequest=new HttpRequestMessage(HttpMethod.Get,$"/api/reports/exports/{exportId}")){Auth(metadataRequest,owner.Token,tenant);using var response=await client.SendAsync(metadataRequest);Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Contains(checksum,await response.Content.ReadAsStringAsync());}
        using(var download=new HttpRequestMessage(HttpMethod.Get,$"/api/reports/exports/{exportId}/download")){Auth(download,owner.Token,tenant);using var response=await client.SendAsync(download);Assert.Equal(HttpStatusCode.OK,response.StatusCode);var bytes=await response.Content.ReadAsByteArrayAsync();Assert.Equal(checksum,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());var text=Encoding.UTF8.GetString(bytes);Assert.StartsWith("# tenant=",text);Assert.Contains("input_checksum=",text);Assert.Contains("source_line_id,order_code,type",text);Assert.Contains("ln-fee",text);}
        await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText="SELECT status,download_count,(SELECT count(*) FROM audit_logs WHERE organization_id=$1 AND resource_type='EXPORT') FROM exports WHERE organization_id=$1 AND id=$2";q.Parameters.AddWithValue(tenant);q.Parameters.AddWithValue(Guid.Parse(exportId));await using var r=await q.ExecuteReaderAsync();Assert.True(await r.ReadAsync());Assert.Equal("READY",r.GetString(0));Assert.Equal(1,r.GetInt32(1));Assert.Equal(3,r.GetInt64(2));
    }
    private static async Task<(string Token,string UserId)> Login(IdentityService identity,HttpClient client,string cs,Guid tenant,OrgRole role,bool mfa){var secret=mfa?IdentityService.GenerateTotpSecret():null;var email=$"export-{role}-{Guid.NewGuid():N}@example.invalid";var password="Long-Safe-Export-Password-2026!";var user=identity.Register(email,$"Export {role}",password,secret);identity.AddMembership(tenant.ToString(),user.Id,role);await Execute(cs,"INSERT INTO users(id,email,display_name,password_hash,mfa_enabled)VALUES($1,$2,$3,'managed',$4)",Guid.Parse(user.Id),email,$"Export {role}",mfa);var login=await client.PostAsJsonAsync("/api/auth/login",new{email,password,tenantId=tenant.ToString(),totpCode=secret is null?null:IdentityService.CurrentTotp(secret)});var token=(await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;return(token,user.Id);}
    private static async Task<JsonElement> UploadSettlement(HttpClient client,string token,Guid tenant){var csv="settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount\nSET-EXPORT,2026-08-24,ln-sale,,SALE,1000,1000\nSET-EXPORT,2026-08-24,ln-fee,,PLATFORM_FEE,-100,-120";using var form=new MultipartFormDataContent();form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)){Headers={ContentType=new("text/csv")}},"file","settlements.csv");using var request=new HttpRequestMessage(HttpMethod.Post,"/api/imports/settlements"){Content=form};Auth(request,token,tenant);using var preview=await client.SendAsync(request);var previewText=await preview.Content.ReadAsStringAsync();Assert.True(preview.IsSuccessStatusCode,previewText);var p=JsonSerializer.Deserialize<JsonElement>(previewText);using var confirm=Json(HttpMethod.Post,"/api/imports/settlements/confirm",new{previewToken=p.GetProperty("previewToken").GetString(),checksum=p.GetProperty("checksum").GetString()},token,tenant);using var response=await client.SendAsync(confirm);var text=await response.Content.ReadAsStringAsync();Assert.True(response.IsSuccessStatusCode,text);return JsonSerializer.Deserialize<JsonElement>(text);}
    private static HttpRequestMessage Json(HttpMethod method,string path,object body,string token,Guid tenant){var request=new HttpRequestMessage(method,path){Content=JsonContent.Create(body)};Auth(request,token,tenant);return request;}
    private static void Auth(HttpRequestMessage r,string token,Guid tenant){r.Headers.Authorization=new("Bearer",token);r.Headers.Add("X-Tenant-Id",tenant.ToString());}
    private static async Task SeedOrganization(string cs,Guid tenant)=>await Execute(cs,"INSERT INTO organizations(id,slug,name)VALUES($1,$2,'Export HTTP')",tenant,$"export-http-{tenant:N}");
    private static async Task Execute(string cs,string sql,params object[] values){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText=sql;foreach(var value in values)q.Parameters.AddWithValue(value);await q.ExecuteNonQueryAsync();}
}
