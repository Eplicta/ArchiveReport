namespace Eplicta.Conserver.ArchiveReport;

// Metadata for a file in the archive delivery.
public class FileEntry
{
    // The name of the file
    public string FileName { get; set; } = "";

    // Filetype (ex. PDF, XML)
    public string FileType { get; set; } = "";

    // Version of the filetype
    public string FileTypeVersion { get; set; } = "";

    // Filesize in bytes
    public long SizeBytes { get; set; }

    // Character encoding
    public string Charset { get; set; } = "";

    // Length of media files (if applicable)
    public TimeSpan? Duration { get; set; }

    // Searchpath to the file
    public string Path { get; set; } = "";

    // Security classification
    public string Secrecy { get; set; } = "";

    // Management of personal data
    public string PersonalDataHandling { get; set; } = "";

    // Additional comments
    public string Comment { get; set; } = "";
}