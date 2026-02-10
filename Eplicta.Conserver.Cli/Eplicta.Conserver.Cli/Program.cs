using System.IO.Compression;
using System.Xml.Linq;
using OfficeOpenXml;
using System.Diagnostics;

// Sökvägar
string zipPath = @"C:\Users\Lenovo\DEV\ZipTest\2025-12-18 (1) (1).zip";
string excelPath = @"C:\Users\Lenovo\DEV\ZipTest\mets.xlsx";

// EPPlus licens (för privat/icke-kommersiell användning)
ExcelPackage.License.SetNonCommercialPersonal("Lenovo User");

// 1. Läs sip.xml ur zip
XDocument xmlDoc;

using (ZipArchive archive = ZipFile.OpenRead(zipPath))
{
    var xmlEntry = archive.GetEntry("sip.xml");

    if (xmlEntry == null)
    {
        Console.WriteLine("Kunde inte hitta sip.xml i zip-filen.");
        return;
    }

    using var stream = xmlEntry.Open();
    xmlDoc = XDocument.Load(stream);
}

// 2. Skapa Excel
using var package = new ExcelPackage();
var sheet = package.Workbook.Worksheets.Add("XML-data");

// 3. Skriv alla XML-element till Excel
int row = 1;

foreach (var element in xmlDoc.Descendants())
{
    sheet.Cells[row, 1].Value = element.Name.LocalName;
    sheet.Cells[row, 2].Value = element.Value;
    row++;
}

// 4. Spara Excel-filen
package.SaveAs(new FileInfo(excelPath));

Console.WriteLine("Klart! Excel skapad:");
Console.WriteLine(excelPath);

// 5. Öppna Excel-filen automatiskt
Process.Start(new ProcessStartInfo
{
    FileName = excelPath,
    UseShellExecute = true
});