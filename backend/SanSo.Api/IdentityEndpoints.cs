using SanSo.Api.Modules;
namespace SanSo.Api;
public static class IdentityEndpoints
{
 public static IEndpointRouteBuilder MapIdentity(this IEndpointRouteBuilder app){
  app.MapPost("/api/auth/login",(LoginRequest b,IdentityService identity)=>{try{return Results.Ok(identity.Login(b.Email,b.Password,b.TenantId,b.TotpCode));}catch(UnauthorizedAccessException e){return Results.Json(new{type="https://sanso.invalid/problems/unauthorized",title=e.Message,status=401},statusCode:401);}}).AllowAnonymous();
  app.MapPost("/api/auth/logout",(HttpRequest r,IdentityService identity)=>{var token=Bearer(r);if(token is null)return Results.Unauthorized();identity.Revoke(token);return Results.NoContent();});
  app.MapGet("/api/auth/me",(HttpRequest r,IdentityService identity)=>{var tenant=r.Headers["X-Tenant-Id"].FirstOrDefault();var token=Bearer(r);if(tenant is null||token is null)return Results.Unauthorized();try{return Results.Ok(identity.Authenticate(token,tenant));}catch(UnauthorizedAccessException){return Results.Unauthorized();}});
  return app;}
 public static SessionPrincipal Require(HttpRequest r,IdentityService identity,string permission){var tenant=r.Headers["X-Tenant-Id"].FirstOrDefault()??throw new UnauthorizedAccessException();var token=Bearer(r)??throw new UnauthorizedAccessException();var principal=identity.Authenticate(token,tenant);if(!IdentityService.Allows(principal.Role,permission))throw new ForbiddenException();return principal;}
 private static string? Bearer(HttpRequest r){var value=r.Headers.Authorization.FirstOrDefault();return value?.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase)==true?value[7..]:null;}
}
public record LoginRequest(string Email,string Password,string TenantId,string? TotpCode);public sealed class ForbiddenException:Exception;
