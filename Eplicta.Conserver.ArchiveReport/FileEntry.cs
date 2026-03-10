namespace Eplicta.Conserver.ArchiveReport;

// Metadata för en fil i arkivleveransen.
public class FileEntry
{
    // Filens namn
    public string FileName { get; set; } = "";

    // Filformat (t.ex. PDF, XML)
    public string FileType { get; set; } = "";

    // Version av filformatet
    public string FileTypeVersion { get; set; } = "";

    // Filstorlek i bytes
    public long SizeBytes { get; set; }

    // Teckenkodning
    public string Charset { get; set; } = "";

    // Längd för mediafiler (valfri)
    public TimeSpan? Duration { get; set; }

    // Sökväg till filen
    public string Path { get; set; } = "";

    // Sekretessklassning
    public string Secrecy { get; set; } = "";

    // Hantering av personuppgifter
    public string PersonalDataHandling { get; set; } = "";

    // Övrig kommentar
    public string Comment { get; set; } = "";
}