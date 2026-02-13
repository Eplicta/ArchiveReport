using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using TagLib;

namespace Eplicta.Conserver.Cli;

public static class FileScanner
{
    public static string ExtractNested(string zipPath)
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempRoot);

        // nivå 1
        var level1 = Path.Combine(tempRoot, "level1");
        ZipFile.ExtractToDirectory(zipPath, level1);

        // hitta inre zip
        var innerZip = Directory.GetFiles(level1, "*.zip", SearchOption.AllDirectories)
                                .FirstOrDefault()
                       ?? throw new Exception("Hittade ingen inre zip.");

        // nivå 2
        var level2 = Path.Combine(tempRoot, "level2");
        ZipFile.ExtractToDirectory(innerZip, level2);

        return level2;
    }

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

            // audio/video duration
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
                // ignorera filer som inte stöds av TagLib
            }


            result.Add(entry);
        }

        return result;
    }
}
