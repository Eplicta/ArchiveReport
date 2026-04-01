using System.IO.Compression;

namespace Eplicta.Conserver.ArchiveReport;

public static class DipConverter
{
    public static byte[] ConvertAipToDip(string aipZipPath, string distributionId = null)
    {
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
            using var outputStream = new MemoryStream();
            using var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create);

            // Create METS metadata XML for DIP
            var metsXml = GenerateDipMetsXml(distributionId, aipMetadata);
            var metsEntry = zipArchive.CreateEntry("dip.xml");
            using (var streamWriter = new StreamWriter(metsEntry.Open()))
            {
                streamWriter.Write(metsXml);
            }

            // Add all files to the distribution package
            foreach (var (filename, filedata) in dipFiles)
            {
                var fileEntry = zipArchive.CreateEntry(filename);
                using (var entryStream = fileEntry.Open())
                {
                    entryStream.Write(filedata, 0, filedata.Length);
                }
            }

            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to convert AIP ''{aipZipPath}'' to DIP: {ex.Message}",
                ex
            );
        }
    }

    private static string GenerateDipMetsXml(string distributionId, AipMetadata metadata)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<mets xmlns=""http://www.loc.gov/METS/"" OBJID=""{distributionId}"">
    <metsHdr CREATEDATE=""{timestamp}"">
        <agent ROLE=""CREATOR"" TYPE=""ORGANIZATION"">
            <name>{System.Security.SecurityElement.Escape(metadata.Creator ?? "Unknown Submitter")}</name>
            <note>Original submitter information</note>
        </agent>
        <agent ROLE=""OTHER"" TYPE=""ORGANIZATION"">
            <name>Eplicta Archive</name>
            <note>This is a distribution copy for user access</note>
        </agent>
    </metsHdr>
    <dmdSec ID=""dmd1"">
        <mdWrap MDTYPE=""MODS"">
            <xmlData>
                <mods xmlns=""http://www.loc.gov/mods/v3"">
                    <identifier>{System.Security.SecurityElement.Escape(metadata.Identifier ?? distributionId)}</identifier>
                    <titleInfo>
                        <title>{System.Security.SecurityElement.Escape(metadata.Title ?? "Distribution Package")}</title>
                    </titleInfo>
                    <originInfo>
                        <dateIssued encoding=""w3cdtf"">{DateTime.UtcNow:yyyy-MM-dd}</dateIssued>
                        <publisher>{System.Security.SecurityElement.Escape(metadata.Publisher ?? "Eplicta Archive")}</publisher>
                    </originInfo>
                    <accessCondition>{System.Security.SecurityElement.Escape(metadata.AccessCondition ?? "Open")}</accessCondition>
                </mods>
            </xmlData>
        </mdWrap>
    </dmdSec>
</mets>";
        return xml;
    }

    private static AipMetadata ExtractMetadataFromAip(string aipZipPath)
    {
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
    }

    private static AipMetadata ParseMetadataFromXml(string xmlContent)
    {
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
    }

    private static string ExtractXmlValue(string xml, string elementName)
    {
        var pattern = $@"<{elementName}[^>]*>([^<]+)</{elementName}>";
        var match = System.Text.RegularExpressions.Regex.Match(xml, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }

    private class AipMetadata
    {
        public string Identifier { get; set; }
        public string Title { get; set; }
        public string Creator { get; set; }
        public string Publisher { get; set; }
        public string AccessCondition { get; set; }
    }
}