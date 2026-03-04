using System;
using System.Collections.Generic;
using System.Text;

namespace Eplicta.Conserver.ArchiveReport;

public class FileEntry
{
    public string FileName { get; set; } = "";
    public string FileType { get; set; } = "";
    public string FileTypeVersion { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Charset { get; set; } = "";
    public TimeSpan? Duration { get; set; }
    public string Path { get; set; } = "";
    public string Secrecy { get; set; } = "";
    public string PersonalDataHandling { get; set; } = "";
    public string Comment { get; set; } = "";
}

