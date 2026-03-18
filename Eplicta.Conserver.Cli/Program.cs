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

        // Processes only the first zip file, logs the rest.
        private static void ProcessZipFiles(string[] args)
        {
            var zipPath = args[0];
            var overrides = args.Skip(1).ToArray();

            ProcessSingleZip(zipPath, overrides);

            // Still log them if you want
            foreach (var o in overrides)
            {
                Console.WriteLine($"Override: {o}");
            }
        }

        // Processes a single zip archive and generates an Excel report.
        private static void ProcessSingleZip(string zipPath, string[] overrides)
        {
            Console.WriteLine($"Processing: {zipPath}");

            if (!File.Exists(zipPath))
            {
                Console.WriteLine("File not found!");
                return;
            }

            // Map overrides safely (avoid index errors)
            string description = overrides.Length > 0 ? overrides[0] : null;
            string archiveCreator = overrides.Length > 1 ? overrides[1] : null;
            string systemName = overrides.Length > 2 ? overrides[2] : null;
            string system = overrides.Length > 3 ? overrides[3] : null;
            string withdrawalDate = overrides.Length > 4 ? overrides[4] : null;
            string comment = overrides.Length > 5 ? overrides[5] : null;

            var mets = new Eplicta.Conserver.ArchiveReport.MetsLoader().Read(
                zipPath,
                description,
                archiveCreator,
                systemName,
                system,
                withdrawalDate,
                comment
            );

            var excelBytes = new Eplicta.Conserver.ArchiveReport.Renderer().GetBytes(mets);

            var outputPath = zipPath + ".exe";
            File.WriteAllBytes(outputPath, excelBytes);
        }
    }
}