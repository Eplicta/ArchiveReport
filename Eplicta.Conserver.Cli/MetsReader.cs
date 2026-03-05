using Eplicta.Conserver.ArchiveReport;
using System.IO.Compression;
using System.Xml.Linq;

namespace Eplicta.Conserver.Cli;

public static class MetsReader
{
    // Läser metadata från sip.xml i zip-filen
    public static MetsData Read(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        // Hitta sip.xml i arkivet
        var entry = archive.Entries
            .FirstOrDefault(e => e.FullName.EndsWith("sip.xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception("Kunde inte hitta sip.xml i zip-filen.");

        // Läs filen som bytes
        byte[] xmlBytes;
        using (var stream = entry.Open())
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            xmlBytes = ms.ToArray();
        }

        // Ladda XML från bytes
        var doc = XDocument.Parse(System.Text.Encoding.UTF8.GetString(xmlBytes));

        var mets = new MetsData();

        // Läs MODS-data (titel, id, datum, url)
        var mods = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "mods");

        if (mods != null)
        {
            mets.Systemnamn = Get(mods, "identifier");
            mets.Beskrivning = Get(mods, "title");
            mets.Uttagsdatum = Get(mods, "dateIssued");
            mets.Kommentar = Get(mods, "url");
        }

        // Läs METS header (arkivbildare och systeminfo)
        var archivist = doc.Descendants()
            .FirstOrDefault(x =>
                x.Name.LocalName == "agent" &&
                (string?)x.Attribute("ROLE") == "ARCHIVIST");

        if (archivist != null)
        {
            mets.Arkivbildare = Get(archivist, "name");
            mets.System = Get(archivist, "note");
        }

        // Extrahera webarchive och skanna filer
        try
        {
            var extractedRoot = FileScanner.ExtractNested(zipPath);
            mets.Files = FileScanner.Scan(extractedRoot);
        }
        catch (Exception ex)
        {
            mets.Kommentar += $" | Filskanning misslyckades: {ex.Message}";
        }

        return mets;
    }

    // Hämtar värde från XML-element
    private static string Get(XElement parent, string local) =>
        parent.Descendants()
              .FirstOrDefault(x => x.Name.LocalName == local)?
              .Value.Trim() ?? "";
}