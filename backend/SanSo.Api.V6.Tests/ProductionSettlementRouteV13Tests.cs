extern alias apiv6;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using V13Program=apiv6::Program;

namespace SanSo.Api.V6.Tests;

public sealed class ProductionSettlementRouteV13Tests
{
    [Fact]
    public async Task DirectSettlementImportIsNotDiscoverableInProduction()
    {
        var cs=Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");if(string.IsNullOrWhiteSpace(cs))return;
        await using var factory=new WebApplicationFactory<V13Program>().WithWebHostBuilder(x=>{x.UseEnvironment("Production");x.UseSetting("ConnectionStrings:Postgres",cs);});
        using var client=factory.CreateClient();await client.GetAsync("/health");var routes=factory.Services.GetServices<EndpointDataSource>().SelectMany(x=>x.Endpoints).OfType<RouteEndpoint>().Select(x=>x.RoutePattern.RawText).ToArray();
        Assert.DoesNotContain("/api/imports/settlements/direct",routes);
    }
}
