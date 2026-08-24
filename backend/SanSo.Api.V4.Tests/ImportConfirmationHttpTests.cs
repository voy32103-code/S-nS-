extern alias apiv4;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using V4Program=apiv4::Program;

namespace SanSo.Api.V4.Tests;

public sealed class ImportConfirmationHttpTests
{
    [Fact]
    public async Task PreviewThenConfirmIsExplicitAndOneTime()
    {
        await using var factory=Development();using var client=factory.CreateClient();
        var preview=await Preview(client,"tenant-http-a","Mã đơn;Số tiền;Ngày đơn\nHTTP-001;125000;24/08/2026");
        Assert.Equal(HttpStatusCode.OK,preview.StatusCode);var body=await preview.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("persisted").GetBoolean());Assert.Equal("CSV",body.GetProperty("format").GetString());
        var token=body.GetProperty("previewToken").GetString()!;var checksum=body.GetProperty("checksum").GetString()!;
        var confirmed=await Confirm(client,"tenant-http-a",token,checksum);Assert.Equal(HttpStatusCode.OK,confirmed.StatusCode);
        var result=await confirmed.Content.ReadFromJsonAsync<JsonElement>();Assert.Equal(1,result.GetProperty("acceptedRows").GetInt32());Assert.False(result.GetProperty("persisted").GetBoolean());
        Assert.Equal(HttpStatusCode.BadRequest,(await Confirm(client,"tenant-http-a",token,checksum)).StatusCode);
    }

    [Fact]
    public async Task ChecksumTamperDoesNotConsumeToken()
    {
        await using var factory=Development();using var client=factory.CreateClient();var preview=await Preview(client,"tenant-http-b","order_code,amount,occurred_at\nHTTP-002,99000,2026-08-24");var body=await preview.Content.ReadFromJsonAsync<JsonElement>();var token=body.GetProperty("previewToken").GetString()!;var checksum=body.GetProperty("checksum").GetString()!;
        var tampered=await Confirm(client,"tenant-http-b",token,"bad-checksum");Assert.Equal(HttpStatusCode.BadRequest,tampered.StatusCode);Assert.Contains("CHECKSUM_MISMATCH",await tampered.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK,(await Confirm(client,"tenant-http-b",token,checksum)).StatusCode);
    }

    [Fact]
    public async Task AnotherTenantCannotConfirmPreview()
    {
        await using var factory=Development();using var client=factory.CreateClient();var preview=await Preview(client,"tenant-http-c","order_code,amount,occurred_at\nHTTP-003,99000,2026-08-24");var body=await preview.Content.ReadFromJsonAsync<JsonElement>();
        var response=await Confirm(client,"tenant-http-d",body.GetProperty("previewToken").GetString()!,body.GetProperty("checksum").GetString()!);
        Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
    }

    [Fact]
    public async Task ProductionRejectsAnonymousImportBeforeDatabaseAccess()
    {
        await using var factory=new WebApplicationFactory<V4Program>().WithWebHostBuilder(x=>{x.UseEnvironment("Production");x.UseSetting("ConnectionStrings:Postgres","Host=127.0.0.1;Port=1;Database=sanso;Username=x;Password=x;Timeout=1");});using var client=factory.CreateClient();
        var response=await Preview(client,"11111111-1111-1111-1111-111111111111","order_code,amount,occurred_at\nHTTP-004,1,2026-08-24");
        Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
    }

    private static WebApplicationFactory<V4Program> Development()=>new WebApplicationFactory<V4Program>().WithWebHostBuilder(x=>x.UseEnvironment("Development"));
    private static async Task<HttpResponseMessage> Preview(HttpClient client,string tenant,string csv){using var form=new MultipartFormDataContent();var file=new ByteArrayContent(Encoding.UTF8.GetBytes(csv));file.Headers.ContentType=new("text/csv");form.Add(file,"file","orders.csv");using var request=new HttpRequestMessage(HttpMethod.Post,"/api/imports/preview"){Content=form};request.Headers.Add("X-Tenant-Id",tenant);return await client.SendAsync(request);}
    private static async Task<HttpResponseMessage> Confirm(HttpClient client,string tenant,string token,string checksum){using var request=new HttpRequestMessage(HttpMethod.Post,"/api/imports/confirm"){Content=JsonContent.Create(new{previewToken=token,checksum})};request.Headers.Add("X-Tenant-Id",tenant);return await client.SendAsync(request);}
}
