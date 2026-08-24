using System.Text.Json;using SanSo.Api.Modules;
namespace SanSo.Api.Tests;
public class GoldenTaxRegressionTests
{
 [Fact]public void SafeNegativeGoldenCasesNeverGuessRateOrAmount(){var root=FindWorkspace();var json=File.ReadAllText(Path.Combine(root,"backend","SanSo.Api.Tests","GoldenTax","negative-cases.json"));var set=JsonSerializer.Deserialize<GoldenSet>(json,new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;Assert.Equal("SAFE_NEGATIVE_CASES_NO_RATE_REQUIRED",set.ApprovalStatus);foreach(var c in set.Cases){var tax=new TaxCenter();var result=tax.Calculate("golden-tenant","2026-08",DateOnly.Parse(c.EffectiveDate),c.Basis,c.PlatformWithheld,c.HasProfile,c.Category);Assert.Equal(c.ExpectedStatus,result.Status);Assert.Null(result.RuleVersion);Assert.Null(result.CalculatedAmount);Assert.Contains("Thiếu profile/category hoặc chưa có rule",result.Explanation);}}
 private static string FindWorkspace(){var d=new DirectoryInfo(AppContext.BaseDirectory);while(d is not null&&!File.Exists(Path.Combine(d.FullName,"SanSo.sln")))d=d.Parent;return d?.FullName??throw new DirectoryNotFoundException("Workspace root not found");}
 private record GoldenSet(string ApprovalStatus,List<GoldenCase> Cases);private record GoldenCase(string Id,string EffectiveDate,long Basis,bool HasProfile,string? Category,long? PlatformWithheld,string ExpectedStatus,string ExpectedReason);
}
