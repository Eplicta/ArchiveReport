using System.IO.Compression;
using System.Xml.Linq;
using Eplicta.Conserver.ArchiveReport;
using ArchiveReportData = Eplicta.Conserver.ArchiveReport.ArchiveReport;

namespace Eplicta.Conserver.Cli;

public class MetsLoader
{
    public ArchiveReportData Read(string zipPath, string setDescription = null, string setArchiveCreator = null, string setSystemName = null, string setSystem = null, string setWithdrawalDate = null, string setComment = null)
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
        var files = new List<FileEntry>();

        var mods = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "mods");
        if (mods != null)
        {
            systemName = GetValue(mods, "identifier");
            description = GetValue(mods, "title");
            withdrawalDate = GetValue(mods, "dateIssued");
            comment = GetValue(mods, "url");
        }

        var archivist = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "agent" && x.Attribute("ROLE")?.Value == "ARCHIVIST");
        if (archivist != null)
        {
            archiveCreator = GetValue(archivist, "name");
            system = GetValue(archivist, "note");
        }

        try
        {
            var extractedRoot = FileScanner.ExtractNested(zipPath);
            files = FileScanner.Scan(extractedRoot);
        }
        catch (Exception ex)
        {
            comment += $" | File scanning failed: {ex.Message}";
        }

        if (!string.IsNullOrEmpty(setDescription)) description = setDescription;
        if (!string.IsNullOrEmpty(setArchiveCreator)) archiveCreator = setArchiveCreator;
        if (!string.IsNullOrEmpty(setComment)) comment = setComment;
        if (!string.IsNullOrEmpty(setSystemName)) systemName = setSystemName;
        if (!string.IsNullOrEmpty(setSystem)) system = setSystem;
        if (!string.IsNullOrEmpty(setWithdrawalDate)) withdrawalDate = setWithdrawalDate;

        return new ArchiveReportData
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

    private static ZipArchiveEntry FindSipXmlEntry(ZipArchive archive)
    {
        return archive.Entries.FirstOrDefault(e => e.FullName.EndsWith("sip.xml", StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException("Could not find sip.xml in the zip archive.");
    }

    private static XDocument LoadXmlDocument(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    private static string GetValue(XElement parent, string elementName)
    {
        return parent.Descendants().FirstOrDefault(x => x.Name.LocalName == elementName)?.Value.Trim() ?? "";
    }
}
