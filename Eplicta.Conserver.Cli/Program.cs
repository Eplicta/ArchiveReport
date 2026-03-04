using System;
using System.IO;
using Eplicta.Conserver.ArchiveReport;
using OfficeOpenXml;

namespace Eplicta.Conserver.Cli
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Lenovo User");

            if (args.Length == 0)
            {
                Console.WriteLine("No file provided.");
                return 1;
            }

            foreach (var zipPath in args)
            {
                Console.WriteLine("Processing: " + zipPath);

                if (!File.Exists(zipPath))
                {
                    Console.WriteLine("File not found!");
                    continue;
                }
                var mets = MetsReader.Read(zipPath);
                var bytes = Renderer.GetBytes(mets);
                System.IO.File.WriteAllBytes(zipPath + ".xlsx", bytes);
                //Runner.Ru+n(zipPath);
            }

            Console.WriteLine("Done.");
            return 0;
        }
    }
}
