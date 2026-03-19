using OfficeOpenXml;

namespace Eplicta.Conserver.Cli
{

    internal static class Program
    {
        private static Dictionary<string, string> ParseNamedArgs(string[] args)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var arg in args)
            {
                if (!arg.StartsWith("--")) continue;

                var parts = arg.Substring(2).Split('=', 2);
                if (parts.Length == 2)
                {
                    dict[parts[0]] = parts[1];
                }
            }

            return dict;
        }
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

            // Parse named arguments from remaining args
            var options = ParseNamedArgs(args.Skip(1).ToArray());

            ProcessSingleZip(zipPath, options);
        }

        // Processes a single zip archive and generates an Excel report.
        private static void ProcessSingleZip(string zipPath, Dictionary<string, string> options)
        {
            Console.WriteLine($"Processing: {zipPath}");

            if (!File.Exists(zipPath))
            {
                Console.WriteLine("File not found!");
                return;
            }

            var mets = new Eplicta.Conserver.ArchiveReport.MetsLoader().Read(
                zipPath,
                options.GetValueOrDefault("description"),
                options.GetValueOrDefault("creator"),
                options.GetValueOrDefault("systemname"),
                options.GetValueOrDefault("system"),
                options.GetValueOrDefault("withdrawaldate"),
                options.GetValueOrDefault("comment")
            );

            var excelBytes = new Eplicta.Conserver.ArchiveReport.Renderer().GetBytes(mets);

            var outputPath = Path.ChangeExtension(zipPath, ".xlsx");
            File.WriteAllBytes(outputPath, excelBytes);
        }

    }


}