using SanSo.Api.Modules;

namespace SanSo.Api.V6;

public static class DevelopmentE2ESeedV2
{
    public static void Apply(WebApplication app)
    {
        if(!app.Environment.IsDevelopment())return;
        var identity=app.Services.GetRequiredService<IdentityService>();var tenant=Environment.GetEnvironmentVariable("SANSO_E2E_TENANT");
        Add(identity,tenant,Environment.GetEnvironmentVariable("SANSO_E2E_EMAIL"),Environment.GetEnvironmentVariable("SANSO_E2E_PASSWORD"),Environment.GetEnvironmentVariable("SANSO_E2E_TOTP_SECRET"),OrgRole.Owner,"E2E Owner");
        Add(identity,tenant,Environment.GetEnvironmentVariable("SANSO_E2E_VIEWER_EMAIL"),Environment.GetEnvironmentVariable("SANSO_E2E_VIEWER_PASSWORD"),null,OrgRole.Viewer,"E2E Viewer");
    }
    private static void Add(IdentityService identity,string? tenant,string? email,string? password,string? secret,OrgRole role,string name)
    {
        if(string.IsNullOrWhiteSpace(tenant)||string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(password)||(role is OrgRole.Owner or OrgRole.Admin&&string.IsNullOrWhiteSpace(secret)))return;
        try{var user=identity.Register(email,name,password,secret);identity.AddMembership(tenant,user.Id,role);}catch(InvalidOperationException e)when(e.Message=="Email already exists"){}
    }
}
