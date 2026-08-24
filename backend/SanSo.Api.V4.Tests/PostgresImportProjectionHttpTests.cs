extern alias apiv4;

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
using V4Program=apiv4::Program;

namespace SanSo.Api.V4.Tests;

public sealed class PostgresImportProjectionHttpTests
{
    [Fact]
    public async Task AuthenticatedCsvConfirmPersistsRawOrderLedgerAndNoGuessTaxReview()
    {
        var connectionString=Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");
        if(string.IsNullOrWhiteSpace(connectionString))return;
        const string tenant="11111111-1111-1111-1111-111111111111";
        await using var factory=new WebApplicationFactory<V4Program>().WithWebHostBuilder(x=>{x.UseEnvironment("Development");x.UseSetting("ConnectionStrings:Postgres",connectionString);});using var client=factory.CreateClient();
        var identity=factory.Services.GetRequiredService<IdentityService>();var secret=IdentityService.GenerateTotpSecret();var email=$"pg-import-{Guid.NewGuid():N}@example.invalid";var user=identity.Register(email,"Postgres Import Owner","Long-Safe-Postgres-Test-Password-2026!",secret);identity.AddMembership(tenant,user.Id,OrgRole.Owner);
        await InsertDatabaseUser(connectionString,user.Id,email);
        var login=await client.PostAsJsonAsync("/api/auth/login",new{email,password="Long-Safe-Postgres-Test-Password-2026!",tenantId=tenant,totpCode=IdentityService.CurrentTotp(secret)});Assert.Equal(HttpStatusCode.OK,login.StatusCode);var token=(await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
        var orderCode=$"HTTP-PG-{Guid.NewGuid():N}";using var form=new MultipartFormDataContent();var file=new ByteArrayContent(Encoding.UTF8.GetBytes($"order_code,amount,occurred_at\n{orderCode},345000,2026-08-24T10:00:00+07:00"));file.Headers.ContentType=new("text/csv");form.Add(file,"file","orders.csv");using var previewRequest=new HttpRequestMessage(HttpMethod.Post,"/api/imports/preview"){Content=form};Authorize(previewRequest,token,tenant);var preview=await client.SendAsync(previewRequest);Assert.Equal(HttpStatusCode.OK,preview.StatusCode);var previewBody=await preview.Content.ReadFromJsonAsync<JsonElement>();Assert.True(previewBody.GetProperty("persisted").GetBoolean());
        using var confirmRequest=new HttpRequestMessage(HttpMethod.Post,"/api/imports/confirm"){Content=JsonContent.Create(new{previewToken=previewBody.GetProperty("previewToken").GetString(),checksum=previewBody.GetProperty("checksum").GetString()})};Authorize(confirmRequest,token,tenant);var confirmation=await client.SendAsync(confirmRequest);Assert.Equal(HttpStatusCode.OK,confirmation.StatusCode);Assert.True((await confirmation.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("persisted").GetBoolean());
        await AssertProjection(connectionString,tenant,orderCode);
    }

    private static void Authorize(HttpRequestMessage request,string token,string tenant){request.Headers.Authorization=new("Bearer",token);request.Headers.Add("X-Tenant-Id",tenant);}
    private static async Task InsertDatabaseUser(string cs,string id,string email){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText="INSERT INTO users(id,email,display_name,password_hash,mfa_enabled) VALUES($1::uuid,$2,'HTTP test identity','managed-in-test-process',true) ON CONFLICT(id) DO NOTHING";q.Parameters.AddWithValue(id);q.Parameters.AddWithValue(email);await q.ExecuteNonQueryAsync();}
    private static async Task AssertProjection(string cs,string tenant,string orderCode){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText="""
SELECT
 (SELECT count(*) FROM raw_events WHERE organization_id=$1::uuid AND payload->>'OrderCode'=$2),
 (SELECT count(*) FROM orders WHERE organization_id=$1::uuid AND code=$2 AND gross_amount=345000),
 (SELECT count(*) FROM ledger_lines ll JOIN orders o ON o.id=ll.order_id WHERE o.organization_id=$1::uuid AND o.code=$2 AND ll.type='SALE' AND ll.amount=345000),
 (SELECT count(*) FROM tax_calculations tc JOIN ledger_lines ll ON ll.id=tc.ledger_line_id JOIN orders o ON o.id=ll.order_id WHERE o.organization_id=$1::uuid AND o.code=$2 AND tc.status='NEEDS_REVIEW' AND tc.calculated_amount IS NULL AND tc.rule_version_id IS NULL)
""";q.Parameters.AddWithValue(tenant);q.Parameters.AddWithValue(orderCode);await using var reader=await q.ExecuteReaderAsync();Assert.True(await reader.ReadAsync());for(var i=0;i<4;i++)Assert.Equal(1,reader.GetInt64(i));}
}
