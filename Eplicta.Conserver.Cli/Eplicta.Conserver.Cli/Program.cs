using System.IO.Compression;
using System.Xml.Linq;
using OfficeOpenXml;
using System.Diagnostics;
using System.Linq;

// ===== 1. KONTROLLERA DRAG-OCH-SLÄPP =====
if (args.Length == 0)
{
    Console.WriteLine("Dra en METS-zip på programmet.");
    return;
}

string zipPath = args[0];

// ===== 2. HITTA MALLEN AUTOMATISKT =====
string templatePath = Path.Combine(
    AppContext.BaseDirectory,
    "Templates",
    "mall_webb_2026.xlsx");

// ===== 3. RESULTATFIL VID SIDAN AV ZIP =====
string excelPath = Path.Combine(
    Path.GetDirectoryName(zipPath)!,
    "resultat.xlsx");

// ===== 4. EPPLUS LICENS =====
ExcelPackage.License.SetNonCommercialPersonal("METS Tool");

// ===== 5. LÄS sip.xml =====
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

// ===== 6. HÄMTA METADATA =====
string GetValue(string name) =>
    xmlDoc.Descendants()
          .FirstOrDefault(e => e.Name.LocalName == name)?.Value ?? "";

string title = GetValue("title");
string dateIssued = GetValue("dateIssued");
string identifier = GetValue("identifier");
string url = GetValue("url");
string systemName = GetValue("name");

// ===== 7. ÖPPNA EXCEL-MALL =====
using var package = new ExcelPackage(new FileInfo(templatePath));
var sheet = package.Workbook.Worksheets.First();

// ===== 8. HITTA RAD VIA RUBRIK =====
void SetValue(string rubric, string value)
{
    var row = sheet.Cells
        .Where(c => c.Start.Column == 1 && c.Text == rubric)
        .Select(c => c.Start.Row)
        .FirstOrDefault();

    if (row > 0)
        sheet.Cells[row, 2].Value = value;
}

// ===== 9. MAPPNING =====
SetValue("Beskrivning av leveransen", title);
SetValue("Uttagsdatum", dateIssued);
SetValue("Accessions-ID", identifier);
SetValue("Kommentar", url);
SetValue("Systemets namn", systemName);

// ===== 10. TA BORT GAMMAL FIL =====
if (File.Exists(excelPath))
{
    try { File.Delete(excelPath); }
    catch
    {
        Console.WriteLine("Stäng Excel-filen först:");
        Console.WriteLine(excelPath);
        return;
    }
}

// ===== 11. SPARA =====
package.SaveAs(new FileInfo(excelPath));

Console.WriteLine("Rapport skapad:");
Console.WriteLine(excelPath);

// ===== 12. ÖPPNA EXCEL =====
Process.Start(new ProcessStartInfo
{
    FileName = excelPath,
    UseShellExecute = true
});
