using System.IO.Compression;
using System.Xml;
using Eplicta.Conserver.ArchiveReport;
using Eplicta.Mets;
using Eplicta.Mets.Entities;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Eplicta.Conserver.Cli <path-to-zip>");
    return 1;
}

var zipPath = args[0];

if (!File.Exists(zipPath))
{
    Console.Error.WriteLine($"File not found: {zipPath}");
    return 1;
}

string metsXml;
try
{
    using var zipStream = File.OpenRead(zipPath);
    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

    // Find the mets.xml entry (case-insensitive)
    var metsEntry = archive.Entries.FirstOrDefault(e =>
        e.Name.Equals("mets.xml", StringComparison.OrdinalIgnoreCase));

    if (metsEntry == null)
    {
        Console.Error.WriteLine("No mets.xml found in the zip archive.");
        return 1;
    }

    using var metsStream = metsEntry.Open();
    using var reader = new StreamReader(metsStream);
    metsXml = await reader.ReadToEndAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to read zip: {ex.Message}");
    return 1;
}

// Build MetsData using Parser (primary approach)
var parser = new Parser();
var (metsData, parseError) = parser.GetMetsData(metsXml);

if (!string.IsNullOrEmpty(parseError))
{
    Console.Error.WriteLine($"Warning: METS parsing warning: {parseError}");
}

if (metsData == null)
{
    Console.Error.WriteLine("Failed to parse METS data.");
    return 1;
}

// If Parser returned no files, supplement with Serializer
if (metsData.Files == null || metsData.Files.Length == 0)
{
    try
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(metsXml);
        var serializer = new Serializer();
        var deserialized = serializer.Deserialize(xmlDoc);
        var rawFiles = deserialized?.FileSec?.FileGrp?.Files;

        if (rawFiles != null && rawFiles.Length > 0)
        {
            metsData = metsData with
            {
                Files = rawFiles.Select(f => new MetsData.FileData
                {
                    Id = f.Id ?? string.Empty,
                    FileName = f.FLocat?.Href ?? f.Id ?? string.Empty,
                    MimeType = f.MimeType ?? string.Empty,
                    Size = f.Size,
                    Checksum = f.CheckSum ?? string.Empty,
                    ChecksumType = Enum.TryParse<MetsData.EChecksumType>(
                        f.CheckSumType.ToString().Replace("-", "_"), out var csType) ? csType : MetsData.EChecksumType.MD5,
                    LocType = MetsData.ELocType.Url
                }).ToArray()
            };
        }

        // Also supplement Agents if missing
        if ((metsData.Agents == null || metsData.Agents.Length == 0)
            && deserialized?.MetsHdr?.Agents != null)
        {
            metsData = metsData with
            {
                Agents = deserialized.MetsHdr.Agents
                    .Select(a => new MetsData.AgentData
                    {
                        Name = a.Name ?? string.Empty,
                        Role = Enum.TryParse<MetsData.ERole>(a.Role, true, out var role) ? role : MetsData.ERole.Other,
                        Type = Enum.TryParse<MetsData.EType>(a.Type, true, out var agentType) ? agentType : MetsData.EType.Other
                    }).ToArray()
            };
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Warning: Serializer fallback failed: {ex.Message}");
    }
}

var runner = new ArchiveReportRunner();
var excelBytes = runner.GetBytes(metsData);

var outputPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Downloads",
    "resultat.xlsx");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllBytesAsync(outputPath, excelBytes);

Console.WriteLine($"Report saved to: {outputPath}");
return 0;
