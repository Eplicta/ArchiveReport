using System.IO.Compression;
using System.Xml.Linq;
using OfficeOpenXml;
using System.Diagnostics;
using System.Linq;

// ===== SÖKVÄGAR =====
string zipPath = @"C:\Users\Lenovo\DEV\ZipTest\2025-12-18 (1) (1).zip";
string templatePath = @"C:\Users\Lenovo\Downloads\mall_webb_2026.xlsx";
string excelPath = @"C:\Users\Lenovo\DEV\ZipTest\resultat.xlsx";

// ===== EPPLUS LICENS =====
ExcelPackage.License.SetNonCommercialPersonal("Lenovo User");

// ===== 1. LÄS sip.xml =====
XDocument xmlDoc;

using (ZipArchive archive = ZipFile.OpenRead(zipPath))
{
    var xmlEntry = archive.GetEntry("sip.xml");
    if (xmlEntry == null)
    {
        Console.WriteLine("sip.xml saknas i zippen.");
        return;
    }

            using var stream = xmlEntry.Open();
            xmlDoc = XDocument.Load(stream);
        }

// ===== 2. HÄMTA VIKTIG METADATA =====
string GetValue(string name) =>
    xmlDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == name)?.Value ?? "";

// Fält från sip.xml
string title = GetValue("title");
string dateIssued = GetValue("dateIssued");
string identifier = GetValue("identifier");
string url = GetValue("url");
string systemName = GetValue("name");

// ===== 3. ÖPPNA EXCEL-MALL =====
using var package = new ExcelPackage(new FileInfo(templatePath));
var sheet = package.Workbook.Worksheets.First();

// ===== 4. SKRIV TILL RÄTT RADER =====
void SetValue(string rubric, string value)
{
    var row = sheet.Cells
        .Where(c => c.Start.Column == 1 && c.Text == rubric)
        .Select(c => c.Start.Row)
        .FirstOrDefault();

    if (row > 0)
        sheet.Cells[row, 2].Value = value;
}

// Mappning
SetValue("Beskrivning av leveransen", title);
SetValue("Uttagsdatum", dateIssued);
SetValue("Accessions-ID", identifier);
SetValue("Kommentar", url);
SetValue("Systemets namn", systemName);

// ===== 5. SPARA =====
package.SaveAs(new FileInfo(excelPath));

Console.WriteLine("Excel skapad:");
Console.WriteLine(excelPath);

// ===== 6. ÖPPNA EXCEL =====
Process.Start(new ProcessStartInfo
{
    FileName = excelPath,
    UseShellExecute = true
});



