using System.IO.Compression;
using TagLib;

namespace Eplicta.Conserver.ArchiveReport;

public static class FileScanner
{
    // Packar upp zip → hittar en inre zip → packar upp den också
    public static string ExtractNested(string zipPath)
    {
        // Skapa temporär arbetsmapp
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempRoot);

        // Packa upp första zip
        var level1 = Path.Combine(tempRoot, "level1");
        ZipFile.ExtractToDirectory(zipPath, level1);

        // Hitta den inre zip-filen
        var innerZip = Directory.GetFiles(level1, "*.zip", SearchOption.AllDirectories)
                                .FirstOrDefault()
                       ?? throw new Exception("Hittade ingen inre zip.");

        // Packa upp den inre zip-filen
        var level2 = Path.Combine(tempRoot, "level2");
        ZipFile.ExtractToDirectory(innerZip, level2);

        return level2;
    }

    // Skannar alla filer i en mapp och skapar FileEntry-objekt
    public static List<FileEntry> Scan(string root)
    {
        var result = new List<FileEntry>();

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var info = new FileInfo(file);

            var entry = new FileEntry
            {
                FileName = info.Name,
                FileType = info.Extension.ToLowerInvariant(),
                SizeBytes = info.Length,
                Path = file
            };

            // Försök läsa duration för audio/video
            try
            {
                using var tag = TagLib.File.Create(file);

                if (tag?.Properties != null && tag.Properties.Duration.TotalSeconds > 0)
                {
                    entry.Duration = tag.Properties.Duration;
                }
            }
            catch
            {
                // Ignorera filer som inte stöds
            }

            result.Add(entry);
        }

        return result;
    }
}