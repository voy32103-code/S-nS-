using System.Text.RegularExpressions;

namespace SanSo.Api.V6;

public sealed class SafeProblemMiddlewareV2(RequestDelegate next,ILogger<SafeProblemMiddlewareV2> log)
{
    private static readonly Regex SafeCode=new("^[A-Z][A-Z0-9_:-]{1,99}$",RegexOptions.Compiled|RegexOptions.CultureInvariant);
    public async Task Invoke(HttpContext context)
    {
        try
        {
            context.Response.Headers["X-Content-Type-Options"]="nosniff";
            context.Response.Headers["X-Frame-Options"]="DENY";
            context.Response.Headers["Referrer-Policy"]="no-referrer";
            context.Response.Headers["Content-Security-Policy"]="default-src 'none'; frame-ancestors 'none'";
            await next(context);
        }
        catch(UnauthorizedAccessException){await Problem(context,401,"Unauthorized","Authentication or tenant membership is invalid.");}
        catch(ForbiddenException){await Problem(context,403,"Forbidden","You do not have permission for this action.");}
        catch(BadHttpRequestException){await Problem(context,400,"Invalid request","REQUEST_BODY_INVALID");}
        catch(ArgumentException e){await Problem(context,400,"Invalid request",Safe(e.Message,"REQUEST_VALIDATION_FAILED"));}
        catch(InvalidOperationException e){await Problem(context,409,"Conflict",Safe(e.Message,"RESOURCE_STATE_CONFLICT"));}
        catch(Exception e){var correlation=context.TraceIdentifier;log.LogError(e,"Unhandled request failure {CorrelationId}",correlation);await Problem(context,500,"Unexpected error",$"Request failed. Correlation: {correlation}");}
    }
    private static string Safe(string message,string fallback)=>SafeCode.IsMatch(message)?message:fallback;
    private static async Task Problem(HttpContext c,int status,string title,string detail){c.Response.StatusCode=status;c.Response.ContentType="application/problem+json";await c.Response.WriteAsJsonAsync(new{type=$"https://sanso.invalid/problems/{status}",title,status,detail,instance=c.Request.Path.Value,correlationId=c.TraceIdentifier});}
}

