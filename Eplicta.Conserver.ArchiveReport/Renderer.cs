using System;
using System.IO;
using System.Reflection;
using OfficeOpenXml;

namespace Eplicta.Conserver.ArchiveReport
{
    /// Responsible for generating an Excel report from a zip archive and its METS metadata.
    public static class Renderer
    {
        private const string TemplateResource =
            "Eplicta.Conserver.ArchiveReport.Templates.mall_webb_2026.xlsx";

        private const string SheetGeneral = "Allmänt";
        private const string SheetFiles = "Filer";

        /// Generates an Excel report and returns it as a byte array.
        public static byte[] GetBytes(string zipPath)
        {
            var metsData = MetsReader.Read(zipPath);

            using var templateStream = GetTemplateStream();
            using var package = new ExcelPackage(templateStream);

            var generalSheet = GetWorksheet(package, SheetGeneral);
            var filesSheet = GetWorksheet(package, SheetFiles);

            FillGeneralMetadata(generalSheet, metsData);
            FillFileList(filesSheet, metsData);

            return package.GetAsByteArray();
        }

        /// Saves the generated Excel report to disk.
        public static void Save(string zipPath)
        {
            var excelBytes = GetBytes(zipPath);

            var outputPath = Path.Combine(
                Path.GetDirectoryName(zipPath)!,
                Path.GetFileNameWithoutExtension(zipPath) + ".xlsx");

            File.WriteAllBytes(outputPath, excelBytes);

            Console.WriteLine("Excel created:");
            Console.WriteLine(outputPath);
        }

        /// Retrieves the embedded Excel template from the assembly.
        private static Stream GetTemplateStream()
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream(TemplateResource)
                ?? throw new Exception("Could not find embedded Excel template.");
        }

        /// Retrieves a worksheet by name from the Excel package.
        private static ExcelWorksheet GetWorksheet(ExcelPackage package, string name)
        {
            return package.Workbook.Worksheets[name]
                ?? throw new Exception($"Worksheet '{name}' was not found in the template.");
        }

        /// Populates the "Allmänt" worksheet with metadata.
        private static void FillGeneralMetadata(ExcelWorksheet sheet, dynamic mets)
        {
            sheet.Cells["B4"].Value = mets.Beskrivning;
            sheet.Cells["B5"].Value = mets.Arkivbildare;
            sheet.Cells["B6"].Value = mets.OrganisationsnummerArkivbildare;
            sheet.Cells["B7"].Value = mets.LevererandeMyndighet;
            sheet.Cells["B8"].Value = mets.OrganisationsnummerLevererande;
            sheet.Cells["B9"].Value = mets.System;
            sheet.Cells["B10"].Value = mets.Kontaktperson;
            sheet.Cells["B11"].Value = mets.Telefon;
            sheet.Cells["B12"].Value = mets.Epost;
            sheet.Cells["B13"].Value = mets.Kostnadsstalle;
            sheet.Cells["B14"].Value = mets.FakturaKontakt;
            sheet.Cells["B15"].Value = mets.ArkivetsNamn;
            sheet.Cells["B16"].Value = mets.Systemnamn;
            sheet.Cells["B17"].Value = mets.Uttagsdatum;
            sheet.Cells["B18"].Value = mets.Kommentar;
            sheet.Cells["B19"].Value = mets.Projektkod;
            sheet.Cells["B20"].Value = mets.AccessionsId;
            sheet.Cells["B21"].Value = mets.BatchId;
        }

        /// Populates the "Filer" worksheet with file information.
        private static void FillFileList(ExcelWorksheet sheet, dynamic mets)
        {
            if (mets.Files == null || mets.Files.Count == 0)
                return;

            int row = 2;

            foreach (var file in mets.Files)
            {
                sheet.Cells[row, 1].Value = file.FileName;
                sheet.Cells[row, 2].Value = file.FileType;
                sheet.Cells[row, 3].Value = file.FileTypeVersion;
                sheet.Cells[row, 4].Value = file.SizeBytes;
                sheet.Cells[row, 5].Value = file.Charset;
                sheet.Cells[row, 6].Value = file.Duration.HasValue
                    ? file.Duration.Value.ToString(@"hh\:mm\:ss")
                    : "";
                sheet.Cells[row, 7].Value = file.Path;
                sheet.Cells[row, 8].Value = file.Secrecy;
                sheet.Cells[row, 9].Value = file.PersonalDataHandling;
                sheet.Cells[row, 10].Value = file.Comment;

                row++;
            }
        }
    }
}