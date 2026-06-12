using Eplicta.Conserver.ArchiveReport;

namespace Eplicta.Conserver.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var zipPath = args[0];
        if (!File.Exists(zipPath))
        {
            Console.WriteLine($"File not found: {zipPath}");
            return 1;
        }

        var options = ParseNamedArgs(args.Skip(1).ToArray());

        try
        {
            GenerateExcelReport(zipPath, options);
            Console.WriteLine("Done.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Details: {ex.InnerException.Message}");
            return 1;
        }
    }

    private static void GenerateExcelReport(string zipPath, Dictionary<string, string> options)
    {
        Console.WriteLine($"Generating metadata appendix from {zipPath}...");

        var report = new MetsLoader().Read(zipPath, options.GetValueOrDefault("description"), options.GetValueOrDefault("creator"), options.GetValueOrDefault("systemname"), options.GetValueOrDefault("system"), options.GetValueOrDefault("withdrawaldate"), options.GetValueOrDefault("comment"));

        var outputPath = Path.ChangeExtension(zipPath, ".xlsx");
        new Renderer().Save(report, outputPath);

        Console.WriteLine($"Excel report saved: {outputPath}");
    }

    private static Dictionary<string, string> ParseNamedArgs(string[] args)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var arg in args)
        {
            if (!arg.StartsWith("--")) continue;
            var parts = arg.Substring(2).Split('=', 2);
            if (parts.Length == 2) { dict[parts[0]] = parts[1]; }
        }

        return dict;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: archivereport.exe <zipfile> [options]");
        Console.WriteLine("Generates a Riksarkivet metadata appendix (.xlsx) next to the input zip.");
        Console.WriteLine();
        Console.WriteLine("Metadata override options:");
        Console.WriteLine("  --description=<text>     Description (optional)");
        Console.WriteLine("  --creator=<text>         Creator (optional)");
        Console.WriteLine("  --systemname=<text>      System name (optional)");
        Console.WriteLine("  --system=<text>          System (optional)");
        Console.WriteLine("  --withdrawaldate=<date>  Withdrawal date (optional)");
        Console.WriteLine("  --comment=<text>         Comment (optional)");
    }
}
