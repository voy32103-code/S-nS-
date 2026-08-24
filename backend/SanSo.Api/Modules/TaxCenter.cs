using System.Collections.Concurrent;
namespace SanSo.Api.Modules;
public enum TaxPeriodStatus{Draft,NeedsReview,Reviewed,ReadyToExport,Exported,Locked,Amended}
public record TaxRuleVersion(string Code,int Version,DateOnly EffectiveFrom,DateOnly? EffectiveTo,string LegalSource,string Status,decimal? Rate);
public record TaxCalculation(string Id,string TenantId,string PeriodKey,string Status,string? RuleCode,int? RuleVersion,string? LegalSource,DateOnly EffectiveDate,long BasisAmount,long? CalculatedAmount,long? PlatformWithheldAmount,long? Difference,string Explanation);
public sealed class TaxCenter
{
 private readonly ConcurrentDictionary<string,TaxPeriodStatus> periods=new();private readonly List<TaxRuleVersion> rules=[];private readonly List<TaxCalculation> calculations=[];
 public void AddRule(TaxRuleVersion rule){if(rule.Status=="APPROVED"&&(rule.Rate is null||string.IsNullOrWhiteSpace(rule.LegalSource)))throw new InvalidOperationException("Approved rule requires expert-approved rate and legal source");rules.Add(rule);}
 public TaxCalculation Calculate(string tenant,string period,DateOnly effectiveDate,long basis,long? platformWithheld,bool hasRequiredProfile,string? category){
  var rule=rules.Where(x=>x.Status=="APPROVED"&&x.EffectiveFrom<=effectiveDate&&(x.EffectiveTo is null||x.EffectiveTo>=effectiveDate)).OrderByDescending(x=>x.Version).FirstOrDefault();
  var review=!hasRequiredProfile||string.IsNullOrWhiteSpace(category)||rule is null;
  long? amount=review?null:checked((long)Math.Round(basis*rule!.Rate!.Value,0,MidpointRounding.AwayFromZero));
  var result=new TaxCalculation(Guid.NewGuid().ToString(),tenant,period,review?"NEEDS_REVIEW":"CALCULATED",rule?.Code,rule?.Version,rule?.LegalSource,effectiveDate,basis,amount,platformWithheld,amount is null||platformWithheld is null?null:platformWithheld-amount,review?"Thiếu profile/category hoặc chưa có rule được chuyên gia phê duyệt.":"Tính deterministic từ input snapshot và rule version đã phê duyệt.");
  calculations.Add(result);periods[$"{tenant}|{period}"]=review?TaxPeriodStatus.NeedsReview:TaxPeriodStatus.Draft;return result;
 }
 public TaxPeriodStatus Status(string tenant,string period)=>periods.GetValueOrDefault($"{tenant}|{period}",TaxPeriodStatus.Draft);
 public void Transition(string tenant,string period,TaxPeriodStatus next,string? reason=null){var key=$"{tenant}|{period}";var current=Status(tenant,period);var allowed=(current,next) switch{(TaxPeriodStatus.Draft,TaxPeriodStatus.Reviewed)=>true,(TaxPeriodStatus.NeedsReview,TaxPeriodStatus.Reviewed)=>true,(TaxPeriodStatus.Reviewed,TaxPeriodStatus.ReadyToExport)=>true,(TaxPeriodStatus.ReadyToExport,TaxPeriodStatus.Exported)=>true,(TaxPeriodStatus.Exported,TaxPeriodStatus.Locked)=>true,(TaxPeriodStatus.Locked,TaxPeriodStatus.Amended)=>!string.IsNullOrWhiteSpace(reason),_=>false};if(!allowed)throw new InvalidOperationException($"Invalid tax period transition {current} -> {next}");periods[key]=next;}
 public void AssertMutable(string tenant,string period){if(Status(tenant,period)==TaxPeriodStatus.Locked)throw new InvalidOperationException("Locked period is immutable; create amendment or reopen with reason");}
}
