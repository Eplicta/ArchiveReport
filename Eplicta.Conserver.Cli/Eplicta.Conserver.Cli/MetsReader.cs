using System.IO.Compression;
using System.Xml.Linq;
using System.Linq;

namespace Eplicta.Conserver.Cli;

public static class MetsReader
{
    public static List<string> Read(string zipPath)
    {
        XDocument xmlDoc;

        using var archive = ZipFile.OpenRead(zipPath);

        var entry = archive.Entries
            .FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

        if (entry == null)
            throw new Exception("Ingen XML-fil hittades i zip.");

        using var stream = entry.Open();
        xmlDoc = XDocument.Load(stream);

        return xmlDoc.Descendants()
            .Select(e => e.Value.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Take(100)
            .ToList();
    }
}
