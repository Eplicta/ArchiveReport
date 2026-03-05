using System;
using System.IO;
using System.Reflection;
using OfficeOpenXml;

namespace Eplicta.Conserver.Cli
{
    public static class Runner
    {
        // Läser METS-metadata från ett ZIP-paket, fyller i en inbäddad Excel-mall och returnerar den ifyllda Excel-filen som en byte-array.
        public static byte[] Run(string zipPath)
        {
            var mets = MetsReader.Read(zipPath);

            var assembly = Assembly.GetExecutingAssembly();

            using var templateStream =
                assembly.GetManifestResourceStream("Eplicta.Conserver.Cli.Templates.mall_webb_2026.xlsx")
                ?? throw new Exception("Kunde inte hitta den inbäddade Excel-mallen.");

            using var package = new ExcelPackage(templateStream);

            var allmantSheet = package.Workbook.Worksheets["Allmänt"]
               ?? throw new Exception("Kunde inte hitta bladet 'Allmänt'.");

            var filerSheet = package.Workbook.Worksheets["Filer"]
                ?? throw new Exception("Kunde inte hitta bladet 'Filer'.");

            // Fyller i metadata på bladet "Allmänt" (B4–B21 = mallens layout)
            allmantSheet.Cells["B4"].Value = mets.Beskrivning;
            allmantSheet.Cells["B5"].Value = mets.Arkivbildare;
            allmantSheet.Cells["B6"].Value = mets.OrganisationsnummerArkivbildare;
            allmantSheet.Cells["B7"].Value = mets.LevererandeMyndighet;
            allmantSheet.Cells["B8"].Value = mets.OrganisationsnummerLevererande;
            allmantSheet.Cells["B9"].Value = mets.System;
            allmantSheet.Cells["B10"].Value = mets.Kontaktperson;
            allmantSheet.Cells["B11"].Value = mets.Telefon;
            allmantSheet.Cells["B12"].Value = mets.Epost;
            allmantSheet.Cells["B13"].Value = mets.Kostnadsstalle;
            allmantSheet.Cells["B14"].Value = mets.FakturaKontakt;
            allmantSheet.Cells["B15"].Value = mets.ArkivetsNamn;
            allmantSheet.Cells["B16"].Value = mets.Systemnamn;
            allmantSheet.Cells["B17"].Value = mets.Uttagsdatum;
            allmantSheet.Cells["B18"].Value = mets.Kommentar;
            allmantSheet.Cells["B19"].Value = mets.Projektkod;
            allmantSheet.Cells["B20"].Value = mets.AccessionsId;
            allmantSheet.Cells["B21"].Value = mets.BatchId;

            if (mets.Files != null && mets.Files.Count > 0)
            {
                int row = 2;

                foreach (var f in mets.Files)
                {
                    // Fyller i metadata på andra bladet
                    filerSheet.Cells[row, 1].Value = f.FileName;
                    filerSheet.Cells[row, 2].Value = f.FileType;
                    filerSheet.Cells[row, 3].Value = f.FileTypeVersion;
                    filerSheet.Cells[row, 4].Value = f.SizeBytes;
                    filerSheet.Cells[row, 5].Value = f.Charset;

                    // Duration formateras som HH:MM:SS eller lämnas tomt om värdet saknas
                    filerSheet.Cells[row, 6].Value =
                        f.Duration.HasValue ? f.Duration.Value.ToString(@"hh\:mm\:ss") : "";
                    filerSheet.Cells[row, 7].Value = f.Path;
                    filerSheet.Cells[row, 8].Value = f.Secrecy;
                    filerSheet.Cells[row, 9].Value = f.PersonalDataHandling;
                    filerSheet.Cells[row, 10].Value = f.Comment;

                    row++;
                }
            }

            return package.GetAsByteArray();
        }

        // Sparar resultatet
        public static void RunAndSave(string zipPath)
        {
            var excelBytes = Run(zipPath);

            string excelPath = Path.Combine(
                Path.GetDirectoryName(zipPath)!,
                Path.GetFileNameWithoutExtension(zipPath) + "_resultat.xlsx");

            File.WriteAllBytes(excelPath, excelBytes);

            Console.WriteLine("Excel created:");
            Console.WriteLine(excelPath);
        }
    }
}
