using System.Reflection;
using ClosedXML.Excel;

namespace Eplicta.Conserver.ArchiveReport;

public class Renderer
{
    private const string TemplateResourceName = "Eplicta.Conserver.ArchiveReport.Templates.mall_webb_2026.xlsx";
    private const string GeneralSheetName = "Allmänt";
    private const string FilesSheetName = "Filer";
    private const string DurationFormat = @"hh\:mm\:ss";
    private const int FirstFileRow = 2;
    private const int FileColumnCount = 10;
    private const string GeneralValueRange = "B2:B21";

    public byte[] GetBytes(ArchiveReport report)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var templateStream = assembly.GetManifestResourceStream(TemplateResourceName) ?? throw new InvalidOperationException($"Could not find the embedded Excel template '{TemplateResourceName}'.");
        using var workbook = new XLWorkbook(templateStream);

        if (!workbook.TryGetWorksheet(GeneralSheetName, out var generalSheet)) throw new InvalidOperationException($"Could not find the worksheet '{GeneralSheetName}'.");
        if (!workbook.TryGetWorksheet(FilesSheetName, out var filesSheet)) throw new InvalidOperationException($"Could not find the worksheet '{FilesSheetName}'.");

        FillGeneralSheet(generalSheet, report);
        FillFilesSheet(filesSheet, report);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public void Save(ArchiveReport report, string outputPath)
    {
        File.WriteAllBytes(outputPath, GetBytes(report));
    }

    private static void FillGeneralSheet(IXLWorksheet sheet, ArchiveReport report)
    {
        sheet.Range(GeneralValueRange).Clear(XLClearOptions.Contents);
        sheet.Cell("B4").Value = report.Description;
        sheet.Cell("B5").Value = report.ArchiveBuilder;
        sheet.Cell("B6").Value = report.OrganizationNumberArchiveBuilder;
        sheet.Cell("B7").Value = report.DeliveringOrganization;
        sheet.Cell("B8").Value = report.DeliveringOrganizationNumber;
        sheet.Cell("B9").Value = report.System;
        sheet.Cell("B10").Value = report.ContactPerson;
        sheet.Cell("B11").Value = report.Telephone;
        sheet.Cell("B12").Value = report.Email;
        sheet.Cell("B13").Value = report.CostPlace;
        sheet.Cell("B14").Value = report.InvoiceContact;
        sheet.Cell("B15").Value = report.ArchiveName;
        sheet.Cell("B16").Value = report.SystemName;
        sheet.Cell("B17").Value = report.WithdrawalDate;
        sheet.Cell("B18").Value = report.Comment;
        sheet.Cell("B19").Value = report.ProjectCode;
        sheet.Cell("B20").Value = report.AccessionsId;
        sheet.Cell("B21").Value = report.BatchId;
    }

    private static void FillFilesSheet(IXLWorksheet sheet, ArchiveReport report)
    {
        ClearDataRows(sheet);

        var row = FirstFileRow;
        foreach (var file in report.Files)
        {
            sheet.Cell(row, 1).Value = file.FileName;
            sheet.Cell(row, 2).Value = file.FileType;
            sheet.Cell(row, 3).Value = file.FileTypeVersion;
            sheet.Cell(row, 4).Value = file.SizeBytes;
            sheet.Cell(row, 5).Value = file.Charset;
            sheet.Cell(row, 6).Value = file.Duration.HasValue ? file.Duration.Value.ToString(DurationFormat) : "";
            sheet.Cell(row, 7).Value = file.Path;
            sheet.Cell(row, 8).Value = file.Secrecy;
            sheet.Cell(row, 9).Value = file.PersonalDataHandling;
            sheet.Cell(row, 10).Value = file.Comment;
            row++;
        }
    }

    private static void ClearDataRows(IXLWorksheet sheet)
    {
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow < FirstFileRow) return;
        var lastColumn = Math.Max(sheet.LastColumnUsed()?.ColumnNumber() ?? FileColumnCount, FileColumnCount);
        sheet.Range(FirstFileRow, 1, lastRow, lastColumn).Clear(XLClearOptions.Contents);
    }
}
