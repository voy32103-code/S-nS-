extern alias apiv6;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SanSo.Api.Modules;
using Xunit;
using V9Program = apiv6::Program;

namespace SanSo.Api.V6.Tests;

public sealed class PostgresOnboardingHttpV9Tests
{
    [Fact]
    public async Task DatabaseRequiresFieldKeyAndConfiguredFlowPersistsEncryptedSevenSteps()
    {
        var cs = Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");
        if (string.IsNullOrWhiteSpace(cs)) return;
        var oldKey = Environment.GetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_BASE64");
        var oldVersion = Environment.GetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_VERSION");
        try
        {
            Environment.SetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_BASE64", null);
            Environment.SetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_VERSION", null);
            var tenantMissing = Guid.NewGuid();
            await SeedOrganization(cs, tenantMissing);
            await using (var missingFactory = Factory(cs))
            {
                var (client, token, _) = await Login(missingFactory, cs, tenantMissing);
                using (client)
                {
                    var response = await Get(client, "/api/onboarding", token, tenantMissing);
                    Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
                    Assert.Contains("FIELD_ENCRYPTION_NOT_CONFIGURED", await response.Content.ReadAsStringAsync());
                }
            }

            var key = RandomNumberGenerator.GetBytes(32);
            Environment.SetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_BASE64", Convert.ToBase64String(key));
            Environment.SetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_VERSION", "onboarding-test-v1");
            var tenant = Guid.NewGuid();
            var reconciliation = Guid.NewGuid();
            await SeedOrganizationAndReconciliation(cs, tenant, reconciliation);
            await using (var factory = Factory(cs))
            {
                var (client, token, userId) = await Login(factory, cs, tenant);
                using (client)
                {
                    Assert.Equal(HttpStatusCode.OK, (await Get(client, "/api/onboarding", token, tenant)).StatusCode);
                    await Ok(client, "/api/onboarding/business-profile", new { subjectType="Company",legalName="Cong ty HTTP",taxIdentifier="0123456789",address="123 Duong Bao Mat",currency="VND",timeZone="Asia/Ho_Chi_Minh" }, token, tenant);
                    await Ok(client, "/api/onboarding/data-source", new { mode="Csv" }, token, tenant);
                    await Ok(client, "/api/onboarding/backfill", new { from=DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)) }, token, tenant);
                    await Ok(client, "/api/onboarding/sku-mapping", new { mappedSkuCount=1 }, token, tenant);
                    await Ok(client, "/api/onboarding/opening-balances", new { balances=new[]{new{canonicalSku="SKU-HTTP",onHand=2,unitCostMinor=15000L}} }, token, tenant);
                    await Ok(client, "/api/onboarding/disclaimer", new { version="pilot-v1",explicitlyConfirmed=true }, token, tenant);
                    var completed = await Ok(client, "/api/onboarding/first-reconciliation", new { reconciliationId=reconciliation,hasMatchedOrExplainedDiscrepancy=true }, token, tenant);
                    Assert.Contains("\"currentStep\":8", completed);
                    Assert.Contains("\"persisted\":true", completed);
                    Assert.DoesNotContain("0123456789", completed);
                    Assert.Equal(Guid.Parse(userId), await DisclaimerActor(cs, tenant));
                }
            }

            await using (var restarted = Factory(cs))
            {
                var (client, token, _) = await Login(restarted, cs, tenant);
                using (client)
                {
                    var response = await Get(client, "/api/onboarding", token, tenant);
                    var body = await response.Content.ReadAsStringAsync();
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Contains("\"currentStep\":8", body);
                    Assert.Contains("******6789", body);
                    Assert.Contains("123 Duong Bao Mat", body);
                }
            }

            await AssertEncryptedAtRest(cs, tenant);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_BASE64", oldKey);
            Environment.SetEnvironmentVariable("SANSO_FIELD_ENCRYPTION_KEY_VERSION", oldVersion);
        }
    }

    private static WebApplicationFactory<V9Program> Factory(string cs) => new WebApplicationFactory<V9Program>().WithWebHostBuilder(x => { x.UseEnvironment("Development"); x.UseSetting("ConnectionStrings:Postgres", cs); });
    private static async Task<(HttpClient Client,string Token,string UserId)> Login(WebApplicationFactory<V9Program> factory,string cs,Guid tenant)
    {
        var client=factory.CreateClient();var identity=factory.Services.GetRequiredService<IdentityService>();var secret=IdentityService.GenerateTotpSecret();var email=$"onboarding-http-{Guid.NewGuid():N}@example.invalid";var user=identity.Register(email,"Onboarding owner","Long-Safe-Onboarding-Password-2026!",secret);identity.AddMembership(tenant.ToString(),user.Id,OrgRole.Owner);await SeedUser(cs,Guid.Parse(user.Id),email);var login=await client.PostAsJsonAsync("/api/auth/login",new{email,password="Long-Safe-Onboarding-Password-2026!",tenantId=tenant.ToString(),totpCode=IdentityService.CurrentTotp(secret)});var token=(await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;return(client,token,user.Id);
    }
    private static async Task<HttpResponseMessage> Get(HttpClient c,string path,string token,Guid tenant){using var r=new HttpRequestMessage(HttpMethod.Get,path);Auth(r,token,tenant);return await c.SendAsync(r);}
    private static async Task<string> Ok(HttpClient c,string path,object body,string token,Guid tenant){using var r=new HttpRequestMessage(HttpMethod.Post,path){Content=JsonContent.Create(body)};Auth(r,token,tenant);using var response=await c.SendAsync(r);var text=await response.Content.ReadAsStringAsync();Assert.True(response.IsSuccessStatusCode,$"{path}: {(int)response.StatusCode} {text}");return text;}
    private static void Auth(HttpRequestMessage r,string token,Guid tenant){r.Headers.Authorization=new("Bearer",token);r.Headers.Add("X-Tenant-Id",tenant.ToString());}
    private static async Task SeedOrganization(string cs,Guid tenant){await Execute(cs,"INSERT INTO organizations(id,slug,name)VALUES($1,$2,'Onboarding HTTP')",tenant,$"onboard-http-{tenant:N}");}
    private static async Task SeedOrganizationAndReconciliation(string cs,Guid tenant,Guid reconciliation){await SeedOrganization(cs,tenant);var settlement=Guid.NewGuid();await Execute(cs,"INSERT INTO settlements(id,organization_id,code,actual_amount,paid_at)VALUES($1,$2,$3,0,now())",settlement,tenant,$"SET-{settlement:N}");await Execute(cs,"INSERT INTO reconciliation_runs(id,organization_id,settlement_id,status,expected_amount,actual_amount,difference,mapping_version,input_checksum)VALUES($1,$2,$3,'MATCHED',0,0,0,1,'http-test')",reconciliation,tenant,settlement);}
    private static async Task SeedUser(string cs,Guid id,string email){await Execute(cs,"INSERT INTO users(id,email,display_name,password_hash,mfa_enabled)VALUES($1,$2,'HTTP actor','managed',true) ON CONFLICT(id)DO NOTHING",id,email);}
    private static async Task<Guid> DisclaimerActor(string cs,Guid tenant){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText="SELECT disclaimer_confirmed_by FROM onboarding_profiles WHERE organization_id=$1";q.Parameters.AddWithValue(tenant);return(Guid)(await q.ExecuteScalarAsync()??throw new InvalidOperationException());}
    private static async Task AssertEncryptedAtRest(string cs,Guid tenant){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText="SELECT tax_identifier_protected,address_protected,field_key_version FROM onboarding_profiles WHERE organization_id=$1";q.Parameters.AddWithValue(tenant);await using var r=await q.ExecuteReaderAsync();Assert.True(await r.ReadAsync());Assert.DoesNotContain("0123456789",r.GetString(0));Assert.DoesNotContain("123 Duong Bao Mat",r.GetString(1));Assert.Equal("onboarding-test-v1",r.GetString(2));}
    private static async Task Execute(string cs,string sql,params object[] values){await using var c=new NpgsqlConnection(cs);await c.OpenAsync();await using var q=c.CreateCommand();q.CommandText=sql;foreach(var value in values)q.Parameters.AddWithValue(value);await q.ExecuteNonQueryAsync();}
}
