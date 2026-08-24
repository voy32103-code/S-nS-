extern alias apiv6;
using System.Security.Cryptography;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Xunit;
using Parser=apiv6::SanSo.Api.V6.SettlementFileParserV1;

namespace SanSo.Api.V6.Tests;

public sealed class SettlementFileParserV13Tests
{
    [Fact]
    public void XlsxPreservesSparseColumnsAndHashesOriginalFile()
    {
        var bytes=Workbook(false);
        var parsed=Parser.Xlsx(bytes);
        Assert.Equal("SET-XLSX",parsed.SettlementCode);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),parsed.Checksum);
        Assert.Single(parsed.Rows);
        Assert.True(string.IsNullOrEmpty(parsed.Rows[0].OrderCode));
        Assert.Equal("ADJUSTMENT",parsed.Rows[0].LineType);
        Assert.Equal(20,parsed.Rows[0].ExpectedAmount);
    }

    [Fact]
    public void XlsxRejectsFormulaCells()
    {
        var error=Assert.Throws<InvalidDataException>(()=>Parser.Xlsx(Workbook(true)));
        Assert.Equal("FORMULA_NOT_ALLOWED",error.Message);
    }

    private static byte[] Workbook(bool formula)
    {
        using var stream=new MemoryStream();
        using(var document=SpreadsheetDocument.Create(stream,SpreadsheetDocumentType.Workbook,true))
        {
            var workbook=document.AddWorkbookPart();workbook.Workbook=new Workbook();
            var worksheet=workbook.AddNewPart<WorksheetPart>();
            var data=new SheetData();worksheet.Worksheet=new Worksheet(data);
            data.Append(Row(1,("A1","settlement_code"),("B1","paid_at"),("C1","source_line_id"),("D1","order_code"),("E1","line_type"),("F1","expected_amount"),("G1","actual_amount")));
            var row=Row(2,("A2","SET-XLSX"),("B2","2026-08-24"),("C2","ln-xlsx"),("E2","ADJUSTMENT"),("F2","20"),("G2","20"));
            if(formula)row.Elements<Cell>().First(x=>x.CellReference=="F2").CellFormula=new CellFormula("10+10");
            data.Append(row);
            var sheets=workbook.Workbook.AppendChild(new Sheets());sheets.Append(new Sheet{Id=workbook.GetIdOfPart(worksheet),SheetId=1,Name="Settlements"});
            workbook.Workbook.Save();
        }
        return stream.ToArray();
    }

    private static Row Row(uint number,params (string Reference,string Value)[] values)
    {
        var row=new Row{RowIndex=number};
        foreach(var value in values)row.Append(new Cell{CellReference=value.Reference,DataType=CellValues.String,CellValue=new CellValue(value.Value)});
        return row;
    }
}
