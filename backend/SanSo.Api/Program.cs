using SanSo.Api;
var builder=WebApplication.CreateBuilder(args);builder.Services.AddSingleton<DemoStore>();builder.Services.AddCors(x=>x.AddDefaultPolicy(p=>p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));builder.Services.AddOpenApi();var app=builder.Build();app.UseCors();app.MapOpenApi();
static string Tenant(HttpRequest r)=>r.Headers["X-Tenant-Id"].FirstOrDefault()??"tenant-an-nhien";
app.MapGet("/health",()=>Results.Ok(new{status="ok"}));
app.MapGet("/api/dashboard",(HttpRequest r,DemoStore db)=>Results.Ok(db.Dashboard(Tenant(r))));
app.MapGet("/api/orders",(HttpRequest r,DemoStore db)=>Results.Ok(db.Orders(Tenant(r))));
app.MapGet("/api/reconciliations/current",(HttpRequest r,DemoStore db)=>Results.Ok(db.Reconcile(Tenant(r))));
app.MapPost("/api/imports/demo",(HttpRequest r,DemoStore db)=>Results.Ok(db.ImportDemo(Tenant(r))));app.Run();public partial class Program{}
