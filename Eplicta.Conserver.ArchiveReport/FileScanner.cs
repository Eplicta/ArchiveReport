using System.IO.Compression;
using TagLib;

namespace Eplicta.Conserver.ArchiveReport;

public static class FileScanner
{
    /// Extracts the outer zip and the nested zip inside it.
    /// Returns the path to the fully extracted directory.
    public static string ExtractNested(string zipPath)
    {
        var tempRoot = CreateTempDirectory();

        var level1Path = ExtractZip(zipPath, Path.Combine(tempRoot, "level1"));
        var innerZipPath = FindInnerZip(level1Path);
        var level2Path = ExtractZip(innerZipPath, Path.Combine(tempRoot, "level2"));

        return level2Path;
    }

    /// Scans all files in a directory and returns metadata entries.
    public static List<FileEntry> Scan(string rootDirectory)
    {
        var files = Directory.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories);

        var results = new List<FileEntry>();

        foreach (var filePath in files)
        {
            var entry = CreateFileEntry(filePath);
            results.Add(entry);
        }

        return results;
    }

    /// Creates a FileEntry object from a file on disk.
    private static FileEntry CreateFileEntry(string filePath)
    {
        var info = new FileInfo(filePath);

        var entry = new FileEntry
        {
            FileName = info.Name,
            FileType = info.Extension.ToLowerInvariant(),
            SizeBytes = info.Length,
            Path = filePath
        };

        entry.Duration = TryReadDuration(filePath);

        return entry;
    }

    /// Attempts to read media duration using TagLib.
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
            // Ignore unsupported files
        }

        return null;
    }

    /// Creates a temporary working directory.
    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    /// Extracts a zip file to a target directory.
    private static string ExtractZip(string zipPath, string destination)
    {
        Directory.CreateDirectory(destination);
        ZipFile.ExtractToDirectory(zipPath, destination);
        return destination;
    }

    /// Finds the first nested zip file in a directory.
    private static string FindInnerZip(string directory)
    {
        return Directory.GetFiles(directory, "*.zip", SearchOption.AllDirectories)
            .FirstOrDefault()
            ?? throw new Exception("No nested zip file found.");
    }
}