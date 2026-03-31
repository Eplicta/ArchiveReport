using System.IO.Compression;
using Eplicta.Mets;

namespace Eplicta.Conserver.ArchiveReport;

public static class AipConverter
{
    public static byte[] ConvertSipToAip(string sipZipPath, string? archiveId = null)
    {
        /*
        archiveId ??= Guid.NewGuid().ToString();

        var loader = new MetsLoader();
        var sipData = loader.Read(sipZipPath);

        var extractedPath = FileScanner.ExtractNested(sipZipPath);
        var files = FileScanner.Scan(extractedPath);

        try
        {
            var builder = new Builder();

            builder.SetMetsAttributes(new[]
            {
                new MetsData.MetsAttribute
                {
                    Name = MetsData.EMetsAttributeName.ObjId,
                    Value = archiveId
                }
            });

            if (!string.IsNullOrEmpty(sipData.ArchiveBuilder))
            {
                builder.AddAgent(new MetsData.AgentData
                {
                    Role = MetsData.ERole.CREATOR,
                    Type = MetsData.EType.ORGANIZATION,
                    Name = sipData.ArchiveBuilder,
                    Note = sipData.System
                });
            }

            builder.AddAgent(new MetsData.AgentData
            {
                Role = MetsData.ERole.ARCHIVIST,
                Type = MetsData.EType.ORGANIZATION,
                Name = "Eplicta Archive",
                Note = "Long-term preservation policy"
            });

            builder.SetModsSection(new MetsData.ModsSectionData
            {
                Identifier = sipData.SystemName ?? "unknown",
                ModsTitle = sipData.Description ?? "Archived Package",
                DateIssued = DateTime.UtcNow,
                Publisher = sipData.DeliveringOrganization ?? sipData.ArchiveBuilder,
                AccessCondition = "PuL"
            });

            foreach (var file in files)
            {
                if (File.Exists(file.Path))
                {
                    var data = File.ReadAllBytes(file.Path);
                    builder.AddFile(new DataFileSource
                    {
                        Name = Path.GetFileName(file.Path),
                        Data = data,
                        ChecksumType = EChecksumType.SHA256
                    });
                }
            }

            var metsData = builder.Build();
            var renderer = new Renderer(metsData);
            var archive = renderer.GetArchiveStream(
                ArchiveFormat.Zip,
                metsFileName: "aip.xml",
                prettify: true
            );

            return archive.Stream.ToArray();
        }
        finally
        {
            try { Directory.Delete(extractedPath, true); }
            catch { }
        }
        */
        throw new NotImplementedException("AIP conversion not yet enabled");
    }
}
