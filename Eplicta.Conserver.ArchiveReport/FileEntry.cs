namespace Eplicta.Conserver.ArchiveReport;

// Metadata for a file in the archive delivery.
public record FileEntry
{
    // The name of the file
    public string FileName { get; init; } = "";

    // Filetype (ex. PDF, XML)
    public string FileType { get; init; } = "";

    // Version of the filetype
    public string FileTypeVersion { get; init; } = "";

    // Filesize in bytes
    public long SizeBytes { get; init; }

    // Character encoding
    public string Charset { get; init; } = "";

    // Length of media files (if applicable)
    public TimeSpan? Duration { get; init; }

    // Searchpath to the file
    public string Path { get; init; } = "";

    // Security classification
    public string Secrecy { get; init; } = "";

    // Management of personal data
    public string PersonalDataHandling { get; init; } = "";

    // Additional comments
    public string Comment { get; init; } = "";
}