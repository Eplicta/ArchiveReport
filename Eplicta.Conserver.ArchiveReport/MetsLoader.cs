#nullable enable
using System.IO.Compression;
using System.Xml.Linq;

namespace Eplicta.Conserver.ArchiveReport;

public static class MetsLoader
{
    /// Reads a zip archive and extracts metadata from sip.xml.
    public static ArchiveReport Read(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        var sipEntry = FindSipXmlEntry(archive);
        var document = LoadXmlDocument(sipEntry);

        var mets = new ArchiveReport();

        ParseModsMetadata(document, mets);
        ParseArchivistMetadata(document, mets);
        ScanFiles(zipPath, mets);

        return mets;
    }

    /// Finds the sip.xml entry inside the zip archive.
    private static ZipArchiveEntry FindSipXmlEntry(ZipArchive archive)
    {
        return archive.Entries
            .FirstOrDefault(e => e.FullName.EndsWith("sip.xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception("Could not find sip.xml in the zip archive.");
    }

    /// Loads the XML document from a zip entry.
    private static XDocument LoadXmlDocument(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    /// Extracts metadata from the MODS section.
    private static void ParseModsMetadata(XDocument doc, ArchiveReport mets)
    {
        var mods = doc.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "mods");

        if (mods == null)
            return;

        mets.Systemnamn = GetValue(mods, "identifier");
        mets.Beskrivning = GetValue(mods, "title");
        mets.Uttagsdatum = GetValue(mods, "dateIssued");
        mets.Kommentar = GetValue(mods, "url");
    }

    /// Extracts metadata about the archivist/system.
    private static void ParseArchivistMetadata(XDocument doc, ArchiveReport mets)
    {
        var archivist = doc.Descendants()
            .FirstOrDefault(x =>
                x.Name.LocalName == "agent" &&
                x.Attribute("ROLE")?.Value == "ARCHIVIST");

        if (archivist == null)
            return;

        mets.Arkivbildare = GetValue(archivist, "name");
        mets.System = GetValue(archivist, "note");
    }

    /// Scans files in the archive and attaches them to the MetsData object.
    private static void ScanFiles(string zipPath, ArchiveReport mets)
    {
        try
        {
            var extractedRoot = FileScanner.ExtractNested(zipPath);
            mets.Files = FileScanner.Scan(extractedRoot);
        }
        catch (Exception ex)
        {
            mets.Kommentar += $" | File scanning failed: {ex.Message}";
        }
    }

    /// Retrieves the value of an XML element by local name.
    private static string GetValue(XElement parent, string elementName)
    {
        return parent.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == elementName)?
            .Value.Trim() ?? "";
    }
}