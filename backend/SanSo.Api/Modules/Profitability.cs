namespace SanSo.Api.Modules;
public record ProfitInput(long Gross,long PlatformFees,long SellerVouchers,long Shipping,long Affiliate,long Ads,long Refunds,long CostOfGoods,long TaxCost);
public record ProfitResult(long Gross,long NetRevenue,long ContributionProfit,decimal Margin,IReadOnlyDictionary<string,long> Bridge);
public static class Profitability
{
 public static ProfitResult Calculate(ProfitInput i){var net=checked(i.Gross+i.PlatformFees+i.SellerVouchers+i.Shipping+i.Affiliate+i.Ads+i.Refunds);var contribution=checked(net+i.CostOfGoods+i.TaxCost);var margin=i.Gross==0?0m:decimal.Round((decimal)contribution/i.Gross,4,MidpointRounding.AwayFromZero);return new(i.Gross,net,contribution,margin,new Dictionary<string,long>{{"platform_fees",i.PlatformFees},{"seller_vouchers",i.SellerVouchers},{"shipping",i.Shipping},{"affiliate",i.Affiliate},{"ads",i.Ads},{"refunds",i.Refunds},{"cogs",i.CostOfGoods},{"tax_cost",i.TaxCost}});}
}
