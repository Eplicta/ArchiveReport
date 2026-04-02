using System.IO.Compression;
using System.Security.Cryptography;

namespace Eplicta.Conserver.ArchiveReport;

public static class AipConverter
{
    public static byte[] ConvertSipToAip(string sipZipPath, string archiveId = null)
    {
        archiveId ??= Guid.NewGuid().ToString();

        var loader = new MetsLoader();
        var sipData = loader.Read(sipZipPath);

        var extractedPath = FileScanner.ExtractNested(sipZipPath);
        var files = FileScanner.Scan(extractedPath);

        try
        {
            // Read all files BEFORE deleting the temporary directory
            var fileData = new List<(string filename, byte[] data, string checksum)>();
            foreach (var file in files)
            {
                var fullPath = Path.Combine(extractedPath, file.Path);
                if (File.Exists(fullPath))
                {
                    var data = File.ReadAllBytes(fullPath);
                    var checksum = ComputeSha256(data);
                    fileData.Add((file.FileName, data, checksum));
                }
            }

            // Create the ZIP archive with the pre-read files
            using var outputStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                // Create METS metadata XML with AIP structure
                var metsXml = GenerateMetsXml(archiveId, sipData, fileData);
                var metsEntry = zipArchive.CreateEntry("aip.xml");
                using (var streamWriter = new StreamWriter(metsEntry.Open()))
                {
                    streamWriter.Write(metsXml);
                }

                // Create preservation metadata
                var preservationXml = GeneratePreservationMetadata(archiveId, sipData);
                var preservationEntry = zipArchive.CreateEntry("preservation_metadata.xml");
                using (var streamWriter = new StreamWriter(preservationEntry.Open()))
                {
                    streamWriter.Write(preservationXml);
                }

                // Create provenance metadata
                var provenanceXml = GenerateProvenanceMetadata(sipData);
                var provenanceEntry = zipArchive.CreateEntry("provenance_metadata.xml");
                using (var streamWriter = new StreamWriter(provenanceEntry.Open()))
                {
                    streamWriter.Write(provenanceXml);
                }

                // Create checksums file
                var checksumsXml = GenerateChecksumsFile(fileData);
                var checksumsEntry = zipArchive.CreateEntry("checksums.xml");
                using (var streamWriter = new StreamWriter(checksumsEntry.Open()))
                {
                    streamWriter.Write(checksumsXml);
                }

                // Create event history
                var eventXml = GenerateEventHistory(archiveId);
                var eventEntry = zipArchive.CreateEntry("event_history.xml");
                using (var streamWriter = new StreamWriter(eventEntry.Open()))
                {
                    streamWriter.Write(eventXml);
                }

                // Create archive structure manifest
                var manifestXml = GenerateManifest(fileData);
                var manifestEntry = zipArchive.CreateEntry("manifest.xml");
                using (var streamWriter = new StreamWriter(manifestEntry.Open()))
                {
                    streamWriter.Write(manifestXml);
                }

                // Add all extracted files to the archive under "data" folder
                foreach (var (filename, data, _) in fileData)
                {
                    var fileEntry = zipArchive.CreateEntry($"data/{filename}");
                    using (var entryStream = fileEntry.Open())
                    {
                        entryStream.Write(data, 0, data.Length);
                    }
                }
            }

            return outputStream.ToArray();
        }
        finally
        {
            try { Directory.Delete(extractedPath, true); }
            catch { }
        }
    }

    private static string GenerateMetsXml(string archiveId, ArchiveReport sipData, List<(string, byte[], string)> fileData)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<mets xmlns=""http://www.loc.gov/METS/"" OBJID=""{archiveId}"">
    <metsHdr CREATEDATE=""{timestamp}"">
        <agent ROLE=""CREATOR"" TYPE=""ORGANIZATION"">
            <name>{System.Security.SecurityElement.Escape(sipData.ArchiveBuilder ?? "Unknown")}</name>
        </agent>
        <agent ROLE=""PRESERVATION"" TYPE=""ORGANIZATION"">
            <name>Eplicta Archive</name>
            <note>Long-term preservation package</note>
        </agent>
    </metsHdr>
    <dmdSec ID=""dmd1"">
        <mdWrap MDTYPE=""MODS"">
            <xmlData>
                <mods xmlns=""http://www.loc.gov/mods/v3"">
                    <identifier>{System.Security.SecurityElement.Escape(sipData.SystemName ?? archiveId)}</identifier>
                    <titleInfo>
                        <title>{System.Security.SecurityElement.Escape(sipData.Description ?? "Archive Package")}</title>
                    </titleInfo>
                    <originInfo>
                        <dateIssued encoding=""w3cdtf"">{DateTime.UtcNow:yyyy-MM-dd}</dateIssued>
                        <publisher>{System.Security.SecurityElement.Escape(sipData.DeliveringOrganization ?? sipData.ArchiveBuilder ?? "Unknown")}</publisher>
                    </originInfo>
                    <note type=""preservation"">This is an Archive Information Package (AIP) for long-term preservation</note>
                </mods>
            </xmlData>
        </mdWrap>
    </dmdSec>
    <fileSec>
        <fileGrp USE=""archive data"">
            {string.Join("\n            ", fileData.Select(f => $"<file ID=\"{System.Security.SecurityElement.Escape(f.Item1)}\" CHECKSUM=\"{f.Item3}\" CHECKSUMTYPE=\"SHA256\" SIZE=\"{f.Item2.Length}\"><FLocat LOCTYPE=\"URL\" xlink:href=\"data/{System.Security.SecurityElement.Escape(f.Item1)}\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"/></file>"))}
        </fileGrp>
    </fileSec>
</mets>";
        return xml;
    }

    private static string GeneratePreservationMetadata(string archiveId, ArchiveReport sipData)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<preservation xmlns=""http://www.loc.gov/premis/v3"">
    <object>
        <objectIdentifier>
            <objectIdentifierType>UUID</objectIdentifierType>
            <objectIdentifierValue>{archiveId}</objectIdentifierValue>
        </objectIdentifier>
        <objectCategory>file</objectCategory>
        <preservationLevel>
            <preservationLevelType>full</preservationLevelType>
            <preservationLevelNote>Complete preservation with all metadata and checksums</preservationLevelNote>
        </preservationLevel>
        <creationDate>{timestamp}</creationDate>
    </object>
    <agent>
        <agentIdentifier>
            <agentIdentifierType>organization</agentIdentifierType>
            <agentIdentifierValue>Eplicta Archive</agentIdentifierValue>
        </agentIdentifier>
        <agentName>Eplicta Archive</agentName>
        <agentType>organization</agentType>
        <note>Preservation system operator</note>
    </agent>
</preservation>";
    }

    private static string GenerateProvenanceMetadata(ArchiveReport sipData)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<provenance>
    <originatingOrganization>
        <name>{System.Security.SecurityElement.Escape(sipData.DeliveringOrganization ?? "Unknown")}</name>
        <organizationNumber>{System.Security.SecurityElement.Escape(sipData.DeliveringOrganizationNumber ?? "N/A")}</organizationNumber>
    </originatingOrganization>
    <submittingOrganization>
        <name>{System.Security.SecurityElement.Escape(sipData.ArchiveBuilder ?? "Unknown")}</name>
        <organizationNumber>{System.Security.SecurityElement.Escape(sipData.OrganizationNumberArchiveBuilder ?? "N/A")}</organizationNumber>
        <contactPerson>{System.Security.SecurityElement.Escape(sipData.ContactPerson ?? "N/A")}</contactPerson>
    </submittingOrganization>
    <submissionDate>{timestamp}</submissionDate>
    <system>{System.Security.SecurityElement.Escape(sipData.System ?? "N/A")}</system>
    <systemName>{System.Security.SecurityElement.Escape(sipData.SystemName ?? "N/A")}</systemName>
</provenance>";
    }

    private static string GenerateChecksumsFile(List<(string filename, byte[] data, string checksum)> fileData)
    {
        var checksums = string.Join("\n        ", fileData.Select(f => 
            $"<file name=\"{System.Security.SecurityElement.Escape(f.filename)}\" sha256=\"{f.checksum}\" size=\"{f.data.Length}\"/>"));

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<checksums timestamp=""{DateTime.UtcNow:o}"">
        {checksums}
</checksums>";
    }

    private static string GenerateEventHistory(string archiveId)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<events>
    <event>
        <eventID>{Guid.NewGuid()}</eventID>
        <eventType>CREATION</eventType>
        <eventDateTime>{timestamp}</eventDateTime>
        <eventDetail>AIP package created from SIP</eventDetail>
        <eventOutcomeInformation>success</eventOutcomeInformation>
        <linkingAgentIdentifier>Eplicta Archive</linkingAgentIdentifier>
        <linkingObjectIdentifier>{archiveId}</linkingObjectIdentifier>
    </event>
</events>";
    }

    private static string GenerateManifest(List<(string filename, byte[] data, string checksum)> fileData)
    {
        var files = string.Join("\n        ", fileData.Select(f => 
            $"<file path=\"data/{System.Security.SecurityElement.Escape(f.filename)}\" size=\"{f.data.Length}\" checksum=\"{f.checksum}\"/>"));

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<manifest timestamp=""{DateTime.UtcNow:o}"">
    <metadata>
        <file path=""aip.xml"" type=""METS""/>
        <file path=""preservation_metadata.xml"" type=""PREMIS""/>
        <file path=""provenance_metadata.xml"" type=""Provenance""/>
        <file path=""checksums.xml"" type=""Checksums""/>
        <file path=""event_history.xml"" type=""Events""/>
    </metadata>
    <data>
        {files}
    </data>
</manifest>";
    }

    private static string ComputeSha256(byte[] data)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
