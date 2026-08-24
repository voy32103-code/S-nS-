extern alias apiv2;

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using V2Program = apiv2::Program;

namespace SanSo.Api.V2.Tests;

public sealed class ImportPreviewApiTests
{
    [Fact]
    public async Task CsvPreviewNormalizesVietnameseHeadersAndValues()
    {
        await using var factory = DevelopmentFactory();
        using var client = factory.CreateClient();
        var csv = "Mã đơn;Số tiền;Ngày đơn\nVN-API-001;125000;24/08/2026 09:30:00";

        using var response = await Upload(client, "orders.csv", "text/csv", csv);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CSV", body.GetProperty("sourceType").GetString());
        Assert.Equal(";", body.GetProperty("delimiter").GetString());
        var row = body.GetProperty("rows")[0];
        Assert.Equal("VN-API-001", row.GetProperty("orderCode").GetString());
        Assert.Equal(125000, row.GetProperty("amount").GetInt64());
        Assert.Equal(7, row.GetProperty("occurredAt").GetDateTimeOffset().Offset.Hours);
        Assert.Empty(body.GetProperty("globalErrors").EnumerateArray());
    }

    [Fact]
    public async Task SameCsvIsReportedAsDuplicateForTheSameTenant()
    {
        await using var factory = DevelopmentFactory();
        using var client = factory.CreateClient();
        var csv = "order_code,amount,occurred_at\nVN-DUP-991,88000,2026-08-24";

        using var first = await Upload(client, "orders.csv", "text/csv", csv);
        using var second = await Upload(client, "orders.csv", "text/csv", csv);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        var secondBody = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(firstBody.GetProperty("duplicate").GetBoolean());
        Assert.True(secondBody.GetProperty("duplicate").GetBoolean());
        Assert.Equal(
            firstBody.GetProperty("checksum").GetString(),
            secondBody.GetProperty("checksum").GetString());
    }

    [Fact]
    public async Task UnsupportedExtensionIsRejectedBeforeParsing()
    {
        await using var factory = DevelopmentFactory();
        using var client = factory.CreateClient();

        using var response = await Upload(client, "orders.exe", "application/octet-stream", "not executable");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("UNSUPPORTED_FILE_TYPE", await response.Content.ReadAsStringAsync());
    }

    private static WebApplicationFactory<V2Program> DevelopmentFactory() =>
        new WebApplicationFactory<V2Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Development"));

    private static async Task<HttpResponseMessage> Upload(
        HttpClient client,
        string fileName,
        string contentType,
        string contents)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(contents));
        file.Headers.ContentType = new(contentType);
        form.Add(file, "file", fileName);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/imports/preview") { Content = form };
        request.Headers.Add("X-Tenant-Id", "tenant-import-api-tests");
        return await client.SendAsync(request);
    }
}
