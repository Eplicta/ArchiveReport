#nullable enable
using System.IO.Compression;
using System.Xml.Linq;

namespace Eplicta.Conserver.ArchiveReport;

public record MetsLoader
{
    /// Reads a zip archive and extracts metadata from sip.xml.
    public ArchiveReport Read(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        var sipEntry = FindSipXmlEntry(archive);
        var document = LoadXmlDocument(sipEntry);

        var description = "";
        var archiveCreator = "";
        var systemName = "";
        var system = "";
        var withdrawalDate = "";
        var comment = "";
        List<FileEntry> files = new();

        // Parse MODS metadata
        var mods = document.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "mods");
        if (mods != null)
        {
            systemName = GetValue(mods, "identifier");
            description = GetValue(mods, "title");
            withdrawalDate = GetValue(mods, "dateIssued");
            comment = GetValue(mods, "url");
        }

        // Parse archivist metadata
        var archivist = document.Descendants()
            .FirstOrDefault(x =>
                x.Name.LocalName == "agent" &&
                x.Attribute("ROLE")?.Value == "ARCHIVIST");
        if (archivist != null)
        {
            archiveCreator = GetValue(archivist, "name");
            system = GetValue(archivist, "note");
        }

        // Scan files
        try
        {
            var extractedRoot = FileScanner.ExtractNested(zipPath);
            files = FileScanner.Scan(extractedRoot);
        }
        catch (Exception ex)
        {
            comment += $" | File scanning failed: {ex.Message}";
        }

        return new ArchiveReport
        {
            Files = files,
            Description = description,
            ArchiveBuilder = archiveCreator,
            SystemName = systemName,
            System = system,
            WithdrawalDate = withdrawalDate,
            Comment = comment
        };
    }

    /// Finds the sip.xml entry inside the zip archive.
    private ZipArchiveEntry FindSipXmlEntry(ZipArchive archive)
    {
        return archive.Entries
            .FirstOrDefault(e => e.FullName.EndsWith("sip.xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception("Could not find sip.xml in the zip archive.");
    }

    /// Loads the XML document from a zip entry.
    private XDocument LoadXmlDocument(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    /// Retrieves the value of an XML element by local name.
    private string GetValue(XElement parent, string elementName)
    {
        return parent.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == elementName)?
            .Value.Trim() ?? "";
    }
}