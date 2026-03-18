using System;
using System.IO;
using System.Reflection;
using OfficeOpenXml;

namespace Eplicta.Conserver.ArchiveReport
{
    // Manages the conversion of MetsData to Excel
    public record Renderer
    {
        // Runs the entire process and returns the Excel file as a byte array
        public byte[] GetBytes(ArchiveReport mets)
        {

            // Fetches the assembly
            var assembly = Assembly.GetExecutingAssembly();

            // Fetches the embedded Excel template as a stream
            using var templateStream =
            assembly.GetManifestResourceStream("Eplicta.Conserver.ArchiveReport.Templates.mall_webb_2026.xlsx")
                ?? throw new Exception("Kunde inte hitta den inbäddade Excel-mallen.");

            // Opens the excel template using EPPlus
            using var package = new ExcelPackage(templateStream);

            var allmantSheet = package.Workbook.Worksheets["Allmänt"]
                ?? throw new Exception("Kunde inte hitta bladet 'Allmänt'.");

            var filerSheet = package.Workbook.Worksheets["Filer"]
                ?? throw new Exception("Kunde inte hitta bladet 'Filer'.");

            // Fills in the metadata in the "Allmänt sheet
            allmantSheet.Cells["B4"].Value = mets.Description;
            allmantSheet.Cells["B5"].Value = mets.ArchiveBuilder;
            allmantSheet.Cells["B6"].Value = mets.OrganizationNumberArchiveBuilder;
            allmantSheet.Cells["B7"].Value = mets.DeliveringOrganization;
            allmantSheet.Cells["B8"].Value = mets.DeliveringOrganizationNumber;
            allmantSheet.Cells["B9"].Value = mets.System;
            allmantSheet.Cells["B10"].Value = mets.ContactPerson;
            allmantSheet.Cells["B11"].Value = mets.Telephone;
            allmantSheet.Cells["B12"].Value = mets.Email;
            allmantSheet.Cells["B13"].Value = mets.CostPlace;
            allmantSheet.Cells["B14"].Value = mets.InvoiceContact;
            allmantSheet.Cells["B15"].Value = mets.ArchiveName;
            allmantSheet.Cells["B16"].Value = mets.SystemName;
            allmantSheet.Cells["B17"].Value = mets.WithdrawalDate;
            allmantSheet.Cells["B18"].Value = mets.Comment;
            allmantSheet.Cells["B19"].Value = mets.ProjectCode;
            allmantSheet.Cells["B20"].Value = mets.AccessionsId;
            allmantSheet.Cells["B21"].Value = mets.BatchId;

            // Fills in the files in the "Filer" sheet, starting from row 2 (row 1 has headers)
            if (mets.Files != null && mets.Files.Count > 0)
            {
                int row = 2;

                foreach (var f in mets.Files)
                {
                    filerSheet.Cells[row, 1].Value = f.FileName;
                    filerSheet.Cells[row, 2].Value = f.FileType;
                    filerSheet.Cells[row, 3].Value = f.FileTypeVersion;
                    filerSheet.Cells[row, 4].Value = f.SizeBytes;
                    filerSheet.Cells[row, 5].Value = f.Charset;
                    filerSheet.Cells[row, 6].Value =
                        f.Duration.HasValue ? f.Duration.Value.ToString(@"hh\:mm\:ss") : "";
                    filerSheet.Cells[row, 7].Value = f.Path;
                    filerSheet.Cells[row, 8].Value = f.Secrecy;
                    filerSheet.Cells[row, 9].Value = f.PersonalDataHandling;
                    filerSheet.Cells[row, 10].Value = f.Comment;

                    row++;
                }
            }

            // Return excel as byte array
            return package.GetAsByteArray();
        }

        // Saves excel file 
        public void Save(ArchiveReport mets, string outputPath)
        {
            var excelBytes = GetBytes(mets);

            File.WriteAllBytes(outputPath, excelBytes);

            Console.WriteLine("Excel created:");
            Console.WriteLine(outputPath);
        }
    }
}