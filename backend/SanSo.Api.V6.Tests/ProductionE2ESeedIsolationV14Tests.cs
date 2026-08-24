extern alias apiv6;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SanSo.Api.Modules;
using Xunit;
using Seed=apiv6::SanSo.Api.V6.DevelopmentE2ESeedV2;

namespace SanSo.Api.V6.Tests;

public sealed class ProductionE2ESeedIsolationV14Tests
{
    [Fact]
    public void ProductionIgnoresCompleteE2ESeedEnvironment()
    {
        var names=new[]{"SANSO_E2E_EMAIL","SANSO_E2E_PASSWORD","SANSO_E2E_TENANT","SANSO_E2E_TOTP_SECRET"};var previous=names.ToDictionary(x=>x,Environment.GetEnvironmentVariable);
        var email=$"production-seed-{Guid.NewGuid():N}@example.invalid";
        try
        {
            Environment.SetEnvironmentVariable(names[0],email);Environment.SetEnvironmentVariable(names[1],"Long-Safe-Production-Seed-Password-2026!");Environment.SetEnvironmentVariable(names[2],Guid.NewGuid().ToString());Environment.SetEnvironmentVariable(names[3],"3132333435363738393031323334353637383930");
            var builder=WebApplication.CreateBuilder(new WebApplicationOptions{EnvironmentName="Production"});builder.Services.AddSingleton<IdentityService>();using var app=builder.Build();Seed.Apply(app);
            var identity=app.Services.GetRequiredService<IdentityService>();Assert.Throws<UnauthorizedAccessException>(()=>identity.Login(email,"Long-Safe-Production-Seed-Password-2026!",Environment.GetEnvironmentVariable(names[2])!,"000000"));
        }
        finally{foreach(var name in names)Environment.SetEnvironmentVariable(name,previous[name]);}
    }
}
