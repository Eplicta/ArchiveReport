using Eplicta.Conserver.ArchiveReport;
using OfficeOpenXml;

namespace Eplicta.Conserver.Cli
{
    internal static class Program
    {
        private const string ExcelLicenseName = "Lenovo User";

        static int Main(string[] args)
        {
            ConfigureExcelLicense();

            if (!ValidateArguments(args))
                return 1;

            ProcessZipFiles(args);

            Console.WriteLine("Done.");
            return 0;
        }

        // Configures the EPPlus license required for Excel generation.
        private static void ConfigureExcelLicense()
        {
            ExcelPackage.License.SetNonCommercialPersonal(ExcelLicenseName);
        }


        // Validates that at least one file argument was provided.
        private static bool ValidateArguments(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No file provided.");
                return false;
            }

            return true;
        }

        // Processes all provided zip files.
        private static void ProcessZipFiles(string[] zipPaths)
        {
            foreach (var zipPath in zipPaths)
            {
                ProcessSingleZip(zipPath);
            }
        }

        // Processes a single zip archive and generates an Excel report.
        private static void ProcessSingleZip(string zipPath)
        {
            Console.WriteLine($"Processing: {zipPath}");

            if (!File.Exists(zipPath))
            {
                Console.WriteLine("File not found!");
                return;
            }

            // Read METS data from the archive
            var mets = MetsReader.Read(zipPath);

            // Generate Excel report as byte array
            var excelBytes = Renderer.GetBytes(zipPath);

            // Save the Excel file next to the zip archive
            var outputPath = zipPath + ".xlsx";
            File.WriteAllBytes(outputPath, excelBytes);
        }
    }
}