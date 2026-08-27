extern alias apiv6;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using V7Program = apiv6::Program;

namespace SanSo.Api.V6.Tests;

public sealed class ImportPreviewSecurityV7Tests
{
    private static WebApplicationFactory<V7Program> Factory() =>
        new WebApplicationFactory<V7Program>().WithWebHostBuilder(x => x.UseEnvironment("Development").UseSetting("ConnectionStrings:Postgres", string.Empty));

    [Fact]
    public async Task NonMultipartRequestIsRejected()
    {
        await using var factory = Factory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsync("/api/imports/preview", new StringContent("not multipart"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("MULTIPART_REQUIRED", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UnsupportedExtensionAndMimeAreRejected()
    {
        await using var factory = Factory();
        using var client = factory.CreateClient();
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("payload")) { Headers = { ContentType = new("application/pdf") } }, "file", "orders.pdf");
        using var response = await client.PostAsync("/api/imports/preview", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("UNSUPPORTED_FILE_TYPE", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task FileLargerThanTenMiBIsRejected()
    {
        await using var factory = Factory();
        using var client = factory.CreateClient();
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(new byte[10 * 1024 * 1024 + 1]) { Headers = { ContentType = new("text/csv") } }, "file", "orders.csv");
        using var response = await client.PostAsync("/api/imports/preview", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("FILE_TOO_LARGE", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ValidUtf8CsvReturnsPreviewWithoutPersistenceInDevelopment()
    {
        await using var factory = Factory();
        using var client = factory.CreateClient();
        using var form = new MultipartFormDataContent();
        var csv = "order_code,amount,occurred_at\no-v7,100,2026-08-24";
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)) { Headers = { ContentType = new("text/csv") } }, "file", "orders.csv");
        using var response = await client.PostAsync("/api/imports/preview", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("o-v7", body);
        Assert.Contains("\"persisted\":false", body);
    }
}
