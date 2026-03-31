using System.IO.Compression;
using Eplicta.Mets;

namespace Eplicta.Conserver.ArchiveReport;

public static class DipConverter
{
    public static byte[] ConvertAipToDip(string aipZipPath, string? distributionId = null)
    {
        /*
        distributionId ??= Guid.NewGuid().ToString();

        var dipFiles = new List<(string filename, byte[] data)>();
        
        using (var zipStream = File.OpenRead(aipZipPath))
        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            foreach (var entry in zipArchive.Entries)
            {
                if (!entry.Name.Equals("aip.xml", StringComparison.OrdinalIgnoreCase) &&
                    !entry.Name.Equals("metadata.xml", StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = entry.Open())
                    using (var memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        dipFiles.Add((entry.Name, memory.ToArray()));
                    }
                }
            }
        }

        var aipMetadata = ExtractMetadataFromAip(aipZipPath);

        try
        {
            var builder = new Builder();

            builder.SetMetsAttributes(new[]
            {
                new MetsData.MetsAttribute
                {
                    Name = MetsData.EMetsAttributeName.ObjId,
                    Value = distributionId
                }
            });

            builder.AddAgent(new MetsData.AgentData
            {
                Role = MetsData.ERole.CREATOR,
                Type = MetsData.EType.ORGANIZATION,
                Name = aipMetadata.Creator ?? "Unknown Submitter",
                Note = "Original submitter information"
            });

            builder.AddAgent(new MetsData.AgentData
            {
                Role = MetsData.ERole.OTHER,
                Type = MetsData.EType.ORGANIZATION,
                Name = "Eplicta Archive",
                Note = "This is a distribution copy for user access"
            });

            builder.SetModsSection(new MetsData.ModsSectionData
            {
                Identifier = aipMetadata.Identifier ?? distributionId,
                ModsTitle = aipMetadata.Title ?? "Distribution Package",
                DateIssued = DateTime.UtcNow,
                Publisher = aipMetadata.Publisher ?? "Eplicta Archive",
                AccessCondition = aipMetadata.AccessCondition ?? "Open"
            });

            foreach (var (filename, filedata) in dipFiles)
            {
                builder.AddFile(new DataFileSource
                {
                    Name = filename,
                    Data = filedata,
                    ChecksumType = EChecksumType.SHA256
                });
            }

            var metsData = builder.Build();
            var renderer = new Renderer(metsData);
            var archive = renderer.GetArchiveStream(
                ArchiveFormat.Zip,
                metsFileName: "dip.xml",
                prettify: true
            );

            return archive.Stream.ToArray();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to convert AIP '{aipZipPath}' to DIP: {ex.Message}",
                ex
            );
        }
        */
        throw new NotImplementedException("DIP conversion not yet enabled");
    }

    private static AipMetadata ExtractMetadataFromAip(string aipZipPath)
    {
        /*
        var metadata = new AipMetadata();

        try
        {
            using (var zipStream = File.OpenRead(aipZipPath))
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                var aipXmlEntry = zipArchive.Entries
                    .FirstOrDefault(e => e.Name.Equals("aip.xml", StringComparison.OrdinalIgnoreCase));

                if (aipXmlEntry != null)
                {
                    using (var stream = aipXmlEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        metadata = ParseMetadataFromXml(xmlContent);
                    }
                }
            }
        }
        catch
        {
        }

        return metadata;
        */
        return new AipMetadata();
    }

    private static AipMetadata ParseMetadataFromXml(string xmlContent)
    {
        /*
        var metadata = new AipMetadata();

        try
        {
            if (xmlContent.Contains("identifier"))
                metadata.Identifier = ExtractXmlValue(xmlContent, "identifier");

            if (xmlContent.Contains("title"))
                metadata.Title = ExtractXmlValue(xmlContent, "title");

            if (xmlContent.Contains("publisher"))
                metadata.Publisher = ExtractXmlValue(xmlContent, "publisher");

            var creatorMatch = System.Text.RegularExpressions.Regex.Match(
                xmlContent, 
                @"<name>([^<]+)</name>"
            );
            if (creatorMatch.Success)
                metadata.Creator = creatorMatch.Groups[1].Value;

            if (xmlContent.Contains("accessCondition"))
                metadata.AccessCondition = ExtractXmlValue(xmlContent, "accessCondition");
        }
        catch
        {
        }

        return metadata;
        */
        return new AipMetadata();
    }

    private static string? ExtractXmlValue(string xml, string elementName)
    {
        /*
        var pattern = $@"<{elementName}[^>]*>([^<]+)</{elementName}>";
        var match = System.Text.RegularExpressions.Regex.Match(xml, pattern);
        return match.Success ? match.Groups[1].Value : null;
        */
        return null;
    }

    private class AipMetadata
    {
        public string? Identifier { get; set; }
        public string? Title { get; set; }
        public string? Creator { get; set; }
        public string? Publisher { get; set; }
        public string? AccessCondition { get; set; }
    }
}
