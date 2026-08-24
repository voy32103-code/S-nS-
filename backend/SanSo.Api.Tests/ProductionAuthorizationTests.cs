using System.Net;using Microsoft.AspNetCore.Hosting;using Microsoft.AspNetCore.Mvc.Testing;
namespace SanSo.Api.Tests;
public class ProductionAuthorizationTests
{
 [Fact]public async Task ProductionApiRejectsTenantHeaderWithoutBearer(){await using var factory=new WebApplicationFactory<Program>().WithWebHostBuilder(x=>x.UseEnvironment("Production"));using var client=factory.CreateClient();using var request=new HttpRequestMessage(HttpMethod.Get,"/api/dashboard");request.Headers.Add("X-Tenant-Id","tenant-an-nhien");var response=await client.SendAsync(request);Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);Assert.Equal("application/problem+json",response.Content.Headers.ContentType?.MediaType);}
 [Fact]public async Task DevelopmentDemoModeAllowsCredentiallessFirstReconciliation(){await using var factory=new WebApplicationFactory<Program>().WithWebHostBuilder(x=>x.UseEnvironment("Development"));using var client=factory.CreateClient();using var request=new HttpRequestMessage(HttpMethod.Get,"/api/reconciliations/current");request.Headers.Add("X-Tenant-Id","tenant-an-nhien");var response=await client.SendAsync(request);Assert.Equal(HttpStatusCode.OK,response.StatusCode);}
}
