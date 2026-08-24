extern alias apiv6;
using System.Text;
using Xunit;
using Parser=apiv6::SanSo.Api.V6.SettlementCsvParserV1;

namespace SanSo.Api.V6.Tests;

public sealed class SettlementCsvParserV1Tests
{
    [Fact]public void ParsesSignedAmountsQuotedFieldsAndMissingOrder(){var csv="settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount\nSET-1,2026-08-24,ln-1,ORD-1,SALE,1000,1000\nSET-1,2026-08-24,ln-2,,PLATFORM_FEE,-100,-120";var parsed=Parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(csv)));Assert.Equal("SET-1",parsed.SettlementCode);Assert.Equal(2,parsed.Rows.Count);Assert.Null(parsed.Rows[1].OrderCode);Assert.Equal(-120,parsed.Rows[1].ActualAmount);}
    [Fact]public void RejectsDuplicateSourceLine(){var csv="settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount\nSET-1,2026-08-24,ln-1,O,SALE,1,1\nSET-1,2026-08-24,ln-1,O,SALE,1,1";var e=Assert.Throws<InvalidDataException>(()=>Parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(csv))));Assert.Equal("SOURCE_LINE_ID_DUPLICATE",e.Message);}
    [Fact]public void RejectsFormulaInTextIdentity(){var csv="settlement_code,paid_at,source_line_id,order_code,line_type,expected_amount,actual_amount\n=cmd,2026-08-24,ln-1,O,SALE,1,1";var e=Assert.Throws<InvalidDataException>(()=>Parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(csv))));Assert.Contains("FORMULA_NOT_ALLOWED",e.Message);}
}

