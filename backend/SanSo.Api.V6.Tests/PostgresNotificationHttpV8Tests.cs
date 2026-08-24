extern alias apiv6;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SanSo.Api.Modules;
using Xunit;
using V8Program = apiv6::Program;

namespace SanSo.Api.V6.Tests;

public sealed class PostgresNotificationHttpV8Tests
{
    [Fact]
    public async Task AuthenticatedInAppFlowPersistsAndEmailFailsClosed()
    {
        var cs = Environment.GetEnvironmentVariable("SANSO_RUNTIME_POSTGRES");
        if (string.IsNullOrWhiteSpace(cs)) return;
        var tenant = Guid.NewGuid();
        await Seed(cs, tenant);
        await using var factory = new WebApplicationFactory<V8Program>().WithWebHostBuilder(x =>
        {
            x.UseEnvironment("Development");
            x.UseSetting("ConnectionStrings:Postgres", cs);
        });
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/notifications")).StatusCode);

        var identity = factory.Services.GetRequiredService<IdentityService>();
        var secret = IdentityService.GenerateTotpSecret();
        var email = $"notify-http-{Guid.NewGuid():N}@example.invalid";
        var user = identity.Register(email, "Notification owner", "Long-Safe-Notification-Password-2026!", secret);
        identity.AddMembership(tenant.ToString(), user.Id, OrgRole.Owner);
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Long-Safe-Notification-Password-2026!", tenantId = tenant.ToString(), totpCode = IdentityService.CurrentTotp(secret) });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;

        var inApp = new { type = "LowStock", channel = "InApp", recipient = "authorized-members", resourceRef = "sku:HTTP", windowStart = "2026-08-24T10:00:00Z" };
        var first = await Post(client, "/api/notifications", inApp, token, tenant);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains("\"persisted\":true", await first.Content.ReadAsStringAsync());
        var second = await Post(client, "/api/notifications", inApp, token, tenant);
        var firstId = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("delivery").GetProperty("id").GetString()!;
        var secondId = (await second.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("delivery").GetProperty("id").GetString()!;
        Assert.Equal(firstId, secondId);

        var list = await Get(client, "/api/notifications", token, tenant);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Single((await list.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("items").EnumerateArray());

        var ack = await Post(client, $"/api/notifications/{firstId}/acknowledge", new { }, token, tenant);
        Assert.Equal(HttpStatusCode.OK, ack.StatusCode);
        Assert.Contains("Acknowledged", await ack.Content.ReadAsStringAsync());

        var emailBody = new { type = "LowStock", channel = "Email", recipient = "owner@example.invalid", resourceRef = "sku:HTTP", windowStart = "2026-08-24T10:00:00Z" };
        var emailResponse = await Post(client, "/api/notifications", emailBody, token, tenant);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, emailResponse.StatusCode);
        Assert.Contains("EMAIL_PROVIDER_NOT_CONFIGURED", await emailResponse.Content.ReadAsStringAsync());
    }

    private static async Task<HttpResponseMessage> Get(HttpClient client, string path, string token, Guid tenant)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        Auth(request, token, tenant);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> Post(HttpClient client, string path, object body, string token, Guid tenant)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
        Auth(request, token, tenant);
        return await client.SendAsync(request);
    }

    private static void Auth(HttpRequestMessage request, string token, Guid tenant)
    {
        request.Headers.Authorization = new("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenant.ToString());
    }

    private static async Task Seed(string cs, Guid tenant)
    {
        await using var c = new NpgsqlConnection(cs);
        await c.OpenAsync();
        await using var q = c.CreateCommand();
        q.CommandText = "INSERT INTO organizations(id,slug,name) VALUES($1,$2,'Notification HTTP')";
        q.Parameters.AddWithValue(tenant);
        q.Parameters.AddWithValue($"notify-http-{tenant:N}");
        await q.ExecuteNonQueryAsync();
    }
}
