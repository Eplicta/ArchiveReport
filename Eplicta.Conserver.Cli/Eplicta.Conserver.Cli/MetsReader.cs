using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Eplicta.Conserver.Cli;

public static class MetsReader
{
    public static MetsData Read(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        // === Hitta sip.xml ===
        var entry = archive.Entries
            .FirstOrDefault(e => e.FullName.EndsWith("sip.xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception("Kunde inte hitta sip.xml i zip-filen.");

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var mets = new MetsData();

        // ============================================================
        // MODS (titel, id, datum, url)
        // ============================================================
        var mods = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "mods");

        if (mods != null)
        {
            mets.Systemnamn = Get(mods, "identifier");
            mets.Beskrivning = Get(mods, "title");
            mets.Uttagsdatum = Get(mods, "dateIssued");

            // URL som kommentar
            mets.Kommentar = Get(mods, "url");
        }

        // ============================================================
        // METS header → arkivbildare / systeminfo
        // ============================================================
        var archivist = doc.Descendants()
            .FirstOrDefault(x =>
                x.Name.LocalName == "agent" &&
                (string?)x.Attribute("ROLE") == "ARCHIVIST");

        if (archivist != null)
        {
            mets.Arkivbildare = Get(archivist, "name");
            mets.System = Get(archivist, "note");
        }

        // ============================================================
        // NYTT: Extrahera webarchive + skanna filer
        // ============================================================
        try
        {
            var extractedRoot = FileScanner.ExtractNested(zipPath);
            mets.Files = FileScanner.Scan(extractedRoot);
        }
        catch (Exception ex)
        {
            // Vi vill inte krascha hela körningen om filscanning misslyckas
            mets.Kommentar += $" | Filskanning misslyckades: {ex.Message}";
        }

        return mets;
    }

    // ================================================================
    // Hjälpmetod för XML-värden
    // ================================================================
    private static string Get(XElement parent, string local) =>
        parent.Descendants()
              .FirstOrDefault(x => x.Name.LocalName == local)?
              .Value.Trim() ?? "";
}
