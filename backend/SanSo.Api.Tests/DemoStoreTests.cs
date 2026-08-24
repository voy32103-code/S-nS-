using SanSo.Api;namespace SanSo.Api.Tests;
public class DemoStoreTests{
 [Fact]public void ReconciliationExplainsDifference(){var db=new DemoStore();var r=db.Reconcile("tenant-an-nhien");Assert.Equal(2_030_000,r.Gross);Assert.Equal(-182_700,r.Fees);Assert.Equal(-30_000,r.Difference);Assert.Equal("NEEDS_REVIEW",r.Status);Assert.Equal(4,r.Lines.Count);}
 [Fact]public void ReimportIsIdempotent(){var db=new DemoStore();var result=db.ImportDemo("tenant-an-nhien");Assert.Equal(0,result.Accepted);Assert.Equal(2,result.Duplicates);Assert.Equal(2,db.Orders("tenant-an-nhien").Count);}
 [Fact]public void TenantsAreIsolated(){var db=new DemoStore();db.ImportDemo("tenant-binh-minh");Assert.All(db.Orders("tenant-binh-minh"),x=>Assert.Equal("tenant-binh-minh",x.TenantId));}}
