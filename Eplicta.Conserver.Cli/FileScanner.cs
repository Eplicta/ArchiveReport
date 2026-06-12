using System.IO.Compression;
using Eplicta.Conserver.ArchiveReport;

namespace Eplicta.Conserver.Cli;

public static class FileScanner
{
    public static string ExtractNested(string zipPath)
    {
        var tempRoot = CreateTempDirectory();

        var level1Path = ExtractZip(zipPath, Path.Combine(tempRoot, "level1"));
        var innerZipPath = FindInnerZip(level1Path);
        var level2Path = ExtractZip(innerZipPath, Path.Combine(tempRoot, "level2"));

        return level2Path;
    }

    public static List<FileEntry> Scan(string rootDirectory)
    {
        var files = Directory.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories);

        var results = new List<FileEntry>();
        foreach (var filePath in files)
        {
            results.Add(CreateFileEntry(filePath, rootDirectory));
        }

        return results;
    }

    private static FileEntry CreateFileEntry(string filePath, string rootDirectory)
    {
        var info = new FileInfo(filePath);
        var relativePath = Path.GetRelativePath(rootDirectory, filePath);

        return new FileEntry
        {
            FileName = info.Name,
            FileType = info.Extension.ToLowerInvariant(),
            SizeBytes = info.Length,
            Path = relativePath,
            Duration = TryReadDuration(filePath)
        };
    }

    private static TimeSpan? TryReadDuration(string filePath)
    {
        try
        {
            using var tag = TagLib.File.Create(filePath);
            if (tag?.Properties != null && tag.Properties.Duration.TotalSeconds > 0)
            {
                return tag.Properties.Duration;
            }
        }
        catch
        {
            // Ignore files TagLib cannot read
        }

        return null;
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ExtractZip(string zipPath, string destination)
    {
        Directory.CreateDirectory(destination);
        ZipFile.ExtractToDirectory(zipPath, destination);
        return destination;
    }

    private static string FindInnerZip(string directory)
    {
        return Directory.GetFiles(directory, "*.zip", SearchOption.AllDirectories).FirstOrDefault() ?? throw new InvalidOperationException("No nested zip file found.");
    }
}
