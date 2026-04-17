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

        private static void ConfigureExcelLicense()
        {
            ExcelPackage.License.SetNonCommercialPersonal(ExcelLicenseName);
        }

        private static bool ValidateArguments(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: archivereport.exe <zipfile> [--type=sip|aip|all] [options]\n");
                Console.WriteLine("Default behavior (no --type specified): Generates Excel and AIP\n");
                Console.WriteLine("Options:");
                Console.WriteLine("  --type=all (default)     Generate Excel report and AIP");
                Console.WriteLine("  --type=sip               Generate only Excel report from SIP");
                Console.WriteLine("  --type=aip               Convert only SIP to AIP\n");

                Console.WriteLine("Conversion options:");
                Console.WriteLine("  --archiveid=<id>         Archive ID for AIP conversion (optional)\n");

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

        private static void ProcessZipFiles(string[] args)
        {
            var zipPath = args[0];
            var options = ParseNamedArgs(args.Skip(1).ToArray());

            ProcessSingleZip(zipPath, options);
        }

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
                    case "sip":
                        GenerateExcelReport(zipPath, options);
                        break;
                    case "all":
                    default:
                        Console.WriteLine("Generating Excel and AIP...\n");
                        GenerateExcelReport(zipPath, options);
                        ConvertToAip(zipPath, options);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Details: {ex.InnerException?.Message}");
            }
        }

        private static void GenerateExcelReport(string zipPath, Dictionary<string, string> options)
        {
            Console.WriteLine("[1/2] Generating Excel report from SIP...");

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

            WriteFileWithRetry(outputPath, excelBytes);
            Console.WriteLine($"✓ Excel report saved: {outputPath}\n");
        }

        private static string ConvertToAip(string sipZipPath, Dictionary<string, string> options)
        {
            var archiveId = options.GetValueOrDefault("archiveid");

            Console.WriteLine("[2/2] Converting SIP to AIP...");

            var aipBytes = Eplicta.Conserver.ArchiveReport.AipConverter.ConvertSipToAip(sipZipPath, archiveId);

            var outputPath = Path.GetDirectoryName(sipZipPath);
            var fileName = Path.GetFileNameWithoutExtension(sipZipPath);
            var aipOutputPath = Path.Combine(outputPath, $"{fileName}_aip.zip");

            WriteFileWithRetry(aipOutputPath, aipBytes);
            Console.WriteLine($"✓ AIP saved: {aipOutputPath}\n");

            return aipOutputPath;
        }

        private static void WriteFileWithRetry(string filePath, byte[] data)
        {
            int maxRetries = 3;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    File.WriteAllBytes(filePath, data);
                    return;
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    Console.WriteLine($"File is in use, retrying in 500ms... ({i + 1}/{maxRetries})");
                    System.Threading.Thread.Sleep(500);
                }
            }

            File.WriteAllBytes(filePath, data);
        }
    }
}