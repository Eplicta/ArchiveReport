using System.IO.Compression;

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
            var fileData = new List<(string filename, byte[] data)>();
            foreach (var file in files)
            {
                var fullPath = Path.Combine(extractedPath, file.Path);
                if (File.Exists(fullPath))
                {
                    var data = File.ReadAllBytes(fullPath);
                    fileData.Add((file.FileName, data));
                }
            }

            // Create the ZIP archive with the pre-read files
            using var outputStream = new MemoryStream();
            using var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create);

            // Create METS metadata XML
            var metsXml = GenerateMetsXml(archiveId, sipData);
            var metsEntry = zipArchive.CreateEntry("aip.xml");
            using (var streamWriter = new StreamWriter(metsEntry.Open()))
            {
                streamWriter.Write(metsXml);
            }

            // Add all extracted files to the archive
            foreach (var (filename, data) in fileData)
            {
                var fileEntry = zipArchive.CreateEntry(filename);
                using (var entryStream = fileEntry.Open())
                {
                    entryStream.Write(data, 0, data.Length);
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

    private static string GenerateMetsXml(string archiveId, ArchiveReport sipData)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<mets xmlns=""http://www.loc.gov/METS/"" OBJID=""{archiveId}"">
    <metsHdr CREATEDATE=""{timestamp}"">
        <agent ROLE=""CREATOR"" TYPE=""ORGANIZATION"">
            <name>{System.Security.SecurityElement.Escape(sipData.ArchiveBuilder ?? "Unknown")}</name>
        </agent>
        <agent ROLE=""ARCHIVIST"" TYPE=""ORGANIZATION"">
            <name>Eplicta Archive</name>
        </agent>
    </metsHdr>
    <dmdSec ID=""dmd1"">
        <mdWrap MDTYPE=""MODS"">
            <xmlData>
                <mods xmlns=""http://www.loc.gov/mods/v3"">
                    <identifier>{System.Security.SecurityElement.Escape(sipData.SystemName ?? archiveId)}</identifier>
                    <titleInfo>
                        <title>{System.Security.SecurityElement.Escape(sipData.Description ?? "Archived Package")}</title>
                    </titleInfo>
                    <originInfo>
                        <dateIssued encoding=""w3cdtf"">{DateTime.UtcNow:yyyy-MM-dd}</dateIssued>
                        <publisher>{System.Security.SecurityElement.Escape(sipData.DeliveringOrganization ?? sipData.ArchiveBuilder ?? "Unknown")}</publisher>
                    </originInfo>
                    <accessCondition>PuL</accessCondition>
                </mods>
            </xmlData>
        </mdWrap>
    </dmdSec>
</mets>";
        return xml;
    }
}
