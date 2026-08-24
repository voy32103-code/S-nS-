$ErrorActionPreference='Stop'
$path='backend/SanSo.Api.V6.Tests/ProductionSettlementRouteV13Tests.cs'
$text=Get-Content -LiteralPath $path -Raw -Encoding UTF8
$text=$text.Replace('using System.Net;'+[Environment]::NewLine,'').Replace('using Microsoft.AspNetCore.Mvc.Testing;','using Microsoft.AspNetCore.Mvc.Testing;'+[Environment]::NewLine+'using Microsoft.AspNetCore.Routing;'+[Environment]::NewLine+'using Microsoft.Extensions.DependencyInjection;')
$old='using var client=factory.CreateClient();using var form=new MultipartFormDataContent();form.Add(new StringContent("ignored"),"file","settlements.csv");'+[Environment]::NewLine+'        using var response=await client.PostAsync("/api/imports/settlements/direct",form);'+[Environment]::NewLine+'        Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);'
$new='using var client=factory.CreateClient();await client.GetAsync("/health");var routes=factory.Services.GetServices<EndpointDataSource>().SelectMany(x=>x.Endpoints).OfType<RouteEndpoint>().Select(x=>x.RoutePattern.RawText).ToArray();'+[Environment]::NewLine+'        Assert.DoesNotContain("/api/imports/settlements/direct",routes);'
if(-not $text.Contains($old)){throw 'PRODUCTION_ROUTE_ASSERTION_ANCHOR_NOT_FOUND'}
$text=$text.Replace($old,$new)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) $path),$text,[System.Text.UTF8Encoding]::new($false))
'PRODUCTION_ROUTE_REGISTRATION_TEST_STRENGTHENED=1'
