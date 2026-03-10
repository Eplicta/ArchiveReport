#nullable enable
using System.IO.Compression;
using System.Xml.Linq;
using System.Text;

namespace Eplicta.Conserver.ArchiveReport;

public static class MetsReader
{
    // Läser en zip-fil och extraherar metadata från sip.xml
    public static MetsData Read(string zipPath)
    {
        // Läser hela zip-filen som bytes
        byte[] zipBytes = File.ReadAllBytes(zipPath);

        // Öppnar zip-arkivet i läsläge
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);

        // Hittar filen sip.xml i zipen
        var entry = archive.Entries
            .FirstOrDefault(e => e.FullName.EndsWith("sip.xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception("Kunde inte hitta sip.xml i zip-filen.");

        byte[] xmlBytes;

        // Läser innehållet i sip.xml till en byte-array
        using (var entryStream = entry.Open())
        using (var ms = new MemoryStream())
        {
            entryStream.CopyTo(ms);
            xmlBytes = ms.ToArray();
        }

        // Parsar XML-innehållet
        var doc = XDocument.Parse(Encoding.UTF8.GetString(xmlBytes));

        // Skapar objektet som ska fyllas med data
        var mets = new MetsData();

        // Letar efter mods-elementet i XML
        var mods = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "mods");

        if (mods != null)
        {
            // Hämtar metadata från mods
            mets.Systemnamn = Get(mods, "identifier");
            mets.Beskrivning = Get(mods, "title");
            mets.Uttagsdatum = Get(mods, "dateIssued");
            mets.Kommentar = Get(mods, "url");
        }

        // Letar efter agent med rollen ARCHIVIST
        var archivist = doc.Descendants()
            .FirstOrDefault(x =>
                x.Name.LocalName == "agent" &&
                (x.Attribute("ROLE")?.Value == "ARCHIVIST"));

        if (archivist != null)
        {
            // Hämtar information om arkivbildare och system
            mets.Arkivbildare = Get(archivist, "name");
            mets.System = Get(archivist, "note");
        }

        try
        {
            // Extraherar eventuella zip-filer inuti zipen
            var extractedRoot = FileScanner.ExtractNested(zipPath);

            // Skannar filer och lägger till i MetsData
            mets.Files = FileScanner.Scan(extractedRoot);
        }
        catch (Exception ex)
        {
            // Lägger till felmeddelande i kommentar om filskanning misslyckas
            mets.Kommentar += $" | Filskanning misslyckades: {ex.Message}";
        }

        // Returnerar det färdiga objektet
        return mets;
    }

    // Hjälpmetod för att hämta värdet från ett XML-element
    private static string Get(XElement parent, string local) =>
        parent.Descendants()
              .FirstOrDefault(x => x.Name.LocalName == local)?
              .Value.Trim() ?? "";
}