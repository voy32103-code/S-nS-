extern alias apiv6;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Middleware=apiv6::SanSo.Api.V6.SafeProblemMiddlewareV2;

namespace SanSo.Api.V6.Tests;

public sealed class SafeProblemMiddlewareV2Tests
{
    [Fact]public async Task LibraryArgumentMessageIsNotExposed(){var raw="Cannot write DateTimeOffset with Offset=07:00 to PostgreSQL (Parameter 'value')";var result=await Invoke(new ArgumentException(raw));Assert.Equal(400,result.Status);Assert.Equal("REQUEST_VALIDATION_FAILED",result.Detail);Assert.DoesNotContain("PostgreSQL",result.Body);}
    [Fact]public async Task DomainCodeIsPreserved(){var result=await Invoke(new InvalidOperationException("SETTLEMENT_CODE_CONFLICT"));Assert.Equal(409,result.Status);Assert.Equal("SETTLEMENT_CODE_CONFLICT",result.Detail);}
    [Fact]public async Task UnexpectedErrorReturnsOnlyCorrelation(){var result=await Invoke(new Exception("database secret detail"));Assert.Equal(500,result.Status);Assert.Contains("correlation-test",result.Detail);Assert.DoesNotContain("secret",result.Body);}
    private static async Task<(int Status,string Detail,string Body)> Invoke(Exception error){var context=new DefaultHttpContext();context.TraceIdentifier="correlation-test";context.Request.Path="/api/test";context.Response.Body=new MemoryStream();var middleware=new Middleware(_=>throw error,NullLogger<Middleware>.Instance);await middleware.Invoke(context);context.Response.Body.Position=0;var body=await new StreamReader(context.Response.Body).ReadToEndAsync();var json=JsonSerializer.Deserialize<JsonElement>(body);return(context.Response.StatusCode,json.GetProperty("detail").GetString()!,body);}
}
