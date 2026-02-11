using System;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Eplicta.Conserver.Cli;

public class MetsData
{
    public string Title { get; set; } = "";
    public string DateIssued { get; set; } = "";
    public string Identifier { get; set; } = "";
    public string SystemName { get; set; } = "";
    public string Url { get; set; } = "";
}

public static class MetsReader
{
    public static MetsData Read(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        // Försök hitta mets.xml först
        var entry = archive.Entries
            .FirstOrDefault(e => e.Name.Equals("mets.xml", StringComparison.OrdinalIgnoreCase))
            // annars ta första xml-filen som fallback
            ?? archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

        if (entry == null)
            throw new Exception("Ingen XML-fil hittades i zip-arkivet.");

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        // Namespaces som ofta används i METS/MODS
        XNamespace mets = "http://www.loc.gov/METS/";
        XNamespace mods = "http://www.loc.gov/mods/v3";

        // Hämta värden robust (tål saknade noder)
        string GetValue(XName name) =>
            doc.Descendants(name).FirstOrDefault()?.Value?.Trim() ?? "";

        // Bygg resultat
        return new MetsData
        {
            Title = GetValue(mods + "title"),
            DateIssued = GetValue(mods + "dateIssued"),
            Identifier = GetValue(mods + "identifier"),
            Url = GetValue(mods + "url"),

            // Försök hitta system/agent-namn i METS
            SystemName = doc
                .Descendants(mets + "agent")
                .Attributes("NAME")
                .FirstOrDefault()?.Value?.Trim() ?? ""
        };
    }
}
