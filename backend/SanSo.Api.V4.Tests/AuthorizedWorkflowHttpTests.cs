extern alias apiv4;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SanSo.Api.Modules;
using Xunit;
using V4Program=apiv4::Program;

namespace SanSo.Api.V4.Tests;

public sealed class AuthorizedWorkflowHttpTests
{
    [Fact]
    public async Task MfaOwnerCompletesOnboardingHttpStateMachine()
    {
        await using var factory=Development();using var client=factory.CreateClient();var token=await OwnerToken(factory,client,"tenant-onboard-http");
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/onboarding")).StatusCode);
        await Ok(Post(client,"/api/onboarding/business-profile",new{subjectType=1,legalName="Hộ kinh doanh Demo",taxIdentifier="0123456789",address="Địa chỉ giả",currency="VND",timeZone="Asia/Ho_Chi_Minh"},token,"tenant-onboard-http"));
        await Ok(Post(client,"/api/onboarding/data-source",new{mode=0},token,"tenant-onboard-http"));
        await Ok(Post(client,"/api/onboarding/backfill",new{from="2026-08-01"},token,"tenant-onboard-http"));
        await Ok(Post(client,"/api/onboarding/sku-mapping",new{mappedSkuCount=1},token,"tenant-onboard-http"));
        await Ok(Post(client,"/api/onboarding/opening-balances",new{balances=new[]{new{canonicalSku="SKU-HTTP",onHand=10,unitCostMinor=120000}}},token,"tenant-onboard-http"));
        await Ok(Post(client,"/api/onboarding/disclaimer",new{version="tax-support-v1",explicitlyConfirmed=true},token,"tenant-onboard-http"));
        var completed=await Post(client,"/api/onboarding/first-reconciliation",new{reconciliationId="rec-http-1",hasMatchedOrExplainedDiscrepancy=true},token,"tenant-onboard-http");await Ok(completed);var body=await completed.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal(8,body.GetProperty("snapshot").GetProperty("currentStep").GetInt32());Assert.False(body.GetProperty("persisted").GetBoolean());
    }

    [Fact]
    public async Task ViewerCannotManageOnboardingOrNotifications()
    {
        await using var factory=Development();using var client=factory.CreateClient();var identity=factory.Services.GetRequiredService<IdentityService>();var user=identity.Register("viewer-v4@example.invalid","Viewer","Long-Safe-Viewer-Password-2026!");identity.AddMembership("tenant-viewer-v4",user.Id,OrgRole.Viewer);var login=await client.PostAsJsonAsync("/api/auth/login",new{email=user.Email,password="Long-Safe-Viewer-Password-2026!",tenantId="tenant-viewer-v4"});var token=(await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
        Assert.Equal(HttpStatusCode.Forbidden,(await Send(client,HttpMethod.Get,"/api/onboarding",null,token,"tenant-viewer-v4")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await Post(client,"/api/notifications",new{type=0,channel=0,recipient="ignored",resourceRef="connection-1",windowStart=DateTimeOffset.UtcNow},token,"tenant-viewer-v4")).StatusCode);
    }

    [Fact]
    public async Task AuthorizedNotificationMasksEmailAndCopilotRefusesTaxDecision()
    {
        await using var factory=Development();using var client=factory.CreateClient();var token=await OwnerToken(factory,client,"tenant-workflow-v4");
        var notification=await Post(client,"/api/notifications",new{type=0,channel=1,recipient="owner@example.invalid",resourceRef="connection-1",windowStart=new DateTimeOffset(2026,8,24,9,0,0,TimeSpan.FromHours(7))},token,"tenant-workflow-v4");await Ok(notification);var n=await notification.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal("o***@example.invalid",n.GetProperty("delivery").GetProperty("recipientMasked").GetString());Assert.False(n.GetProperty("persisted").GetBoolean());
        var copilot=await Post(client,"/api/copilot/explain",new{question="Hãy chọn thuế suất",evidence=Array.Empty<object>()},token,"tenant-workflow-v4");await Ok(copilot);Assert.Contains("REFUSED",await copilot.Content.ReadAsStringAsync());
    }

    private static WebApplicationFactory<V4Program> Development()=>new WebApplicationFactory<V4Program>().WithWebHostBuilder(x=>x.UseEnvironment("Development"));
    private static async Task<string> OwnerToken(WebApplicationFactory<V4Program> factory,HttpClient client,string tenant){var identity=factory.Services.GetRequiredService<IdentityService>();var secret=IdentityService.GenerateTotpSecret();var user=identity.Register($"owner-{tenant}@example.invalid","Owner","Long-Safe-Owner-Password-2026!",secret);identity.AddMembership(tenant,user.Id,OrgRole.Owner);var login=await client.PostAsJsonAsync("/api/auth/login",new{email=user.Email,password="Long-Safe-Owner-Password-2026!",tenantId=tenant,totpCode=IdentityService.CurrentTotp(secret)});Assert.Equal(HttpStatusCode.OK,login.StatusCode);return(await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;}
    private static Task<HttpResponseMessage> Post(HttpClient client,string path,object body,string token,string tenant)=>Send(client,HttpMethod.Post,path,body,token,tenant);
    private static async Task<HttpResponseMessage> Send(HttpClient client,HttpMethod method,string path,object? body,string token,string tenant){using var request=new HttpRequestMessage(method,path);if(body is not null)request.Content=JsonContent.Create(body);request.Headers.Authorization=new("Bearer",token);request.Headers.Add("X-Tenant-Id",tenant);return await client.SendAsync(request);}
    private static async Task Ok(Task<HttpResponseMessage> responseTask)=>await Ok(await responseTask);private static async Task Ok(HttpResponseMessage response){if(response.StatusCode!=HttpStatusCode.OK)throw new Xunit.Sdk.XunitException($"Expected 200, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");}
}
