using System;
using System.IO;
using System.Reflection;
using OfficeOpenXml;

namespace Eplicta.Conserver.Cli
{
    public static class Runner
    {
        // Kör hela processen och returnerar Excel-filen som byte-array
        public static byte[] Run(string zipPath)
        {
            // Läser metadata från zip-filen
            var mets = MetsReader.Read(zipPath);

            // Hämtar den nuvarande assemblyn (programmet)
            var assembly = Assembly.GetExecutingAssembly();

            // Hämtar den inbäddade Excel-mallen från projektet
            using var templateStream =
                assembly.GetManifestResourceStream("Eplicta.Conserver.Cli.Templates.mall_webb_2026.xlsx")
                ?? throw new Exception("Kunde inte hitta den inbäddade Excel-mallen.");

            // Öppnar Excel-mallen
            using var package = new ExcelPackage(templateStream);

            // Hämtar bladet "Allmänt"
            var allmantSheet = package.Workbook.Worksheets["Allmänt"]
               ?? throw new Exception("Kunde inte hitta bladet 'Allmänt'.");

            // Hämtar bladet "Filer"
            var filerSheet = package.Workbook.Worksheets["Filer"]
                ?? throw new Exception("Kunde inte hitta bladet 'Filer'.");

            // Fyller i metadata i Excel-bladet "Allmänt"
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

            // Om det finns filer i listan
            if (mets.Files != null && mets.Files.Count > 0)
            {
                int row = 2; // Börjar skriva filer från rad 2

                // Loopar igenom alla filer
                foreach (var f in mets.Files)
                {
                    // Skriver filinformation i Excel-bladet "Filer"
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

                    row++; // Går till nästa rad
                }
            }

            // Returnerar Excel-filen som byte-array
            return package.GetAsByteArray();
        }

        // Kör processen och sparar Excel-filen på disk
        public static void RunAndSave(string zipPath)
        {
            // Kör Run-metoden och får Excel-filen
            var excelBytes = Run(zipPath);

            // Skapar sökväg för resultatfilen
            string excelPath = Path.Combine(
                Path.GetDirectoryName(zipPath)!,
                Path.GetFileNameWithoutExtension(zipPath) + "_resultat.xlsx");

            // Sparar Excel-filen
            File.WriteAllBytes(excelPath, excelBytes);

            // Skriver ut information i konsolen
            Console.WriteLine("Excel created:");
            Console.WriteLine(excelPath);
        }
    }
}