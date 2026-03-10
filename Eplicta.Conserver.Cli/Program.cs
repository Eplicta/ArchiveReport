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
            // Sätter licens för EPPlus (krävs för att skapa Excel)
            ExcelPackage.License.SetNonCommercialPersonal("Lenovo User");

            // Kontrollera att minst en fil skickats in
            if (args.Length == 0)
            {
                Console.WriteLine("No file provided.");
                return 1;
            }

                // Loopa igenom alla zip-filer som skickats som argument
                foreach (var zipPath in args)
                {
                    Console.WriteLine("Processing: " + zipPath);

                    // Kontrollera att filen finns
                    if (!File.Exists(zipPath))
                    {
                        Console.WriteLine("File not found!");
                        continue;
                    }

                    // Läs METS-data från zip
                    var mets = MetsReader.Read(zipPath);

                    // Generera Excel-data
                    var bytes = Renderer.GetBytes(zipPath);

                    // Spara Excel-fil
                    System.IO.File.WriteAllBytes(zipPath + ".xlsx", bytes);

                }

            Console.WriteLine("Done.");
            return 0;
            }
        }
    }
