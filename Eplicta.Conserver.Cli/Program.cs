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
                Console.WriteLine("Usage: archivereport.exe <zipfile> [--type=sip|aip|dip|all] [options]\n");
                Console.WriteLine("Default behavior (no --type specified): Generates all outputs (Excel, AIP, and DIP)\n");
                Console.WriteLine("Options:");
                Console.WriteLine("  --type=all (default)     Generate Excel report, AIP, and DIP");
                Console.WriteLine("  --type=sip               Generate only Excel report from SIP");
                Console.WriteLine("  --type=aip               Convert only SIP to AIP");
                Console.WriteLine("  --type=dip               Convert only AIP to DIP\n");
                Console.WriteLine("Conversion options:");
                Console.WriteLine("  --archiveid=<id>         Archive ID for AIP conversion (optional)");
                Console.WriteLine("  --distributionid=<id>    Distribution ID for DIP conversion (optional)\n");
                Console.WriteLine("SIP metadata options:");
                Console.WriteLine("  --description=<text>     Description (optional)");
                Console.WriteLine("  --creator=<text>         Creator (optional)");
                Console.WriteLine("  --systemname=<text>      System name (optional)");
                Console.WriteLine("  --system=<text>          System (optional)");
                Console.WriteLine("  --withdrawaldate=<date>  Withdrawal date (optional)");
                Console.WriteLine("  --comment=<text>         Comment (optional)\n");
                Console.WriteLine("Examples:");
                Console.WriteLine("  archivereport.exe archive.zip");
                Console.WriteLine("  archivereport.exe archive.zip --type=sip");
                Console.WriteLine("  archivereport.exe archive.zip --archiveid=my-archive-123");
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

        // Processes a single zip archive and generates an Excel report or converts to AIP/DIP.
        private static void ProcessSingleZip(string zipPath, Dictionary<string, string> options)
        {
            var type = options.GetValueOrDefault("type", "all").ToLowerInvariant();

            Console.WriteLine($"Processing: {zipPath}");

            if (!File.Exists(zipPath))
            {
                Console.WriteLine("File not found!");
                return;
            }

            try
            {
                switch (type)
                {
                    case "aip":
                        ConvertToAip(zipPath, options);
                        break;
                    case "dip":
                        ConvertToDip(zipPath, options);
                        break;
                    case "sip":
                        GenerateExcelReport(zipPath, options);
                        break;
                    case "all":
                    default:
                        // Generate all: Excel, AIP, and DIP
                        Console.WriteLine("Generating all outputs...\n");
                        GenerateExcelReport(zipPath, options);
                        var aipPath = ConvertToAip(zipPath, options);
                        if (!string.IsNullOrEmpty(aipPath))
                        {
                            ConvertToDip(aipPath, options);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Details: {ex.InnerException?.Message}");
            }
        }

        // Generates an Excel report from SIP
        private static void GenerateExcelReport(string zipPath, Dictionary<string, string> options)
        {
            Console.WriteLine("[1/3] Generating Excel report from SIP...");
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
            Console.WriteLine($"✓ Excel report saved: {outputPath}\n");
        }

        // Converts SIP to AIP and returns the AIP file path
        private static string ConvertToAip(string sipZipPath, Dictionary<string, string> options)
        {
            var archiveId = options.GetValueOrDefault("archiveid");

            Console.WriteLine("[2/3] Converting SIP to AIP...");
            var aipBytes = Eplicta.Conserver.ArchiveReport.AipConverter.ConvertSipToAip(sipZipPath, archiveId);

            var outputPath = Path.GetDirectoryName(sipZipPath);
            var fileName = Path.GetFileNameWithoutExtension(sipZipPath);
            var aipOutputPath = Path.Combine(outputPath, $"{fileName}_aip.zip");

            File.WriteAllBytes(aipOutputPath, aipBytes);
            Console.WriteLine($"✓ AIP saved: {aipOutputPath}\n");

            return aipOutputPath;
        }

        // Converts AIP to DIP
        private static void ConvertToDip(string aipZipPath, Dictionary<string, string> options)
        {
            var distributionId = options.GetValueOrDefault("distributionid");

            Console.WriteLine("[3/3] Converting AIP to DIP...");
            var dipBytes = Eplicta.Conserver.ArchiveReport.DipConverter.ConvertAipToDip(aipZipPath, distributionId);

            var outputPath = Path.GetDirectoryName(aipZipPath);
            var fileName = Path.GetFileNameWithoutExtension(aipZipPath);
            var dipOutputPath = Path.Combine(outputPath, $"{fileName}_dip.zip");

            File.WriteAllBytes(dipOutputPath, dipBytes);
            Console.WriteLine($"✓ DIP saved: {dipOutputPath}\n");
        }

    }


}