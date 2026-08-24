using SanSo.Api.Modules;
namespace SanSo.Api;
public sealed class ProductionTenantMiddleware(RequestDelegate next,IHostEnvironment environment)
{
 public async Task Invoke(HttpContext context,IdentityService identity){if(environment.IsDevelopment()||!context.Request.Path.StartsWithSegments("/api")||context.Request.Path.StartsWithSegments("/api/auth/login")){await next(context);return;}var tenant=context.Request.Headers["X-Tenant-Id"].FirstOrDefault()??throw new UnauthorizedAccessException();var auth=context.Request.Headers.Authorization.FirstOrDefault();if(auth?.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase)!=true)throw new UnauthorizedAccessException();var principal=identity.Authenticate(auth[7..],tenant);var permission=Permission(context.Request);if(permission is not null&&!IdentityService.Allows(principal.Role,permission))throw new ForbiddenException();if(permission=="export.sensitive"&&!principal.StepUpVerified)throw new ForbiddenException();context.Items["SessionPrincipal"]=principal;context.Items["TenantId"]=principal.TenantId;await next(context);}
 private static string? Permission(HttpRequest r){var path=r.Path.Value??"";if(path.StartsWith("/api/inventory",StringComparison.OrdinalIgnoreCase)&&r.Method!="GET")return"inventory.write";if(path.StartsWith("/api/tax",StringComparison.OrdinalIgnoreCase)&&r.Method!="GET")return"tax.review";if(path.StartsWith("/api/exports",StringComparison.OrdinalIgnoreCase))return"export.sensitive";if(path.StartsWith("/api/orders",StringComparison.OrdinalIgnoreCase)||path.StartsWith("/api/reconciliations",StringComparison.OrdinalIgnoreCase)||path.StartsWith("/api/dashboard",StringComparison.OrdinalIgnoreCase))return"finance.read";return null;}
}
public static class SecureCorsComposition
{
 public static WebApplication UseCors(this WebApplication app){Microsoft.AspNetCore.Builder.CorsMiddlewareExtensions.UseCors(app);app.UseMiddleware<ProductionTenantMiddleware>();return app;}
}
