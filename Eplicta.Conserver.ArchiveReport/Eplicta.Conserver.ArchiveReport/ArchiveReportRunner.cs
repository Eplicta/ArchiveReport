using ClosedXML.Excel;
using Eplicta.Mets.Entities;

namespace Eplicta.Conserver.ArchiveReport;

public class ArchiveReportRunner
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "html", "htm", "xhtml", "xml", "css", "xsl", "xsd", "dtd", "js", "txt", "sgml",
        "pdf", "jpg", "jpeg", "tiff", "tif", "png", "wav", "wma", "ogg", "mp3", "wmv",
        "mpeg", "mpg", "avi", "gml", "cals", "p7s", "p7m", "p7c"
    };

    public byte[] GetBytes(MetsData metsData)
    {
        using var stream = GetStream(metsData);
        return stream.ToArray();
    }

    public MemoryStream GetStream(MetsData metsData)
    {
        using var workbook = new XLWorkbook();
        BuildSheet1(workbook, metsData);
        BuildSheet2(workbook, metsData);
        BuildSheet3(workbook, metsData);

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static void BuildSheet1(XLWorkbook workbook, MetsData metsData)
    {
        var ws = workbook.Worksheets.Add("Flik 1");

        var mods = metsData.Mods;
        var agents = metsData.Agents ?? Array.Empty<MetsData.AgentData>();

        var creatorAgent = agents.FirstOrDefault(a => a.Role == MetsData.ERole.Creator && a.Type == MetsData.EType.Organization);
        var editorAgent = agents.FirstOrDefault(a => a.Role == MetsData.ERole.Editor);
        var custodianAgent = agents.FirstOrDefault(a => a.Role == MetsData.ERole.Custodian);

        var altRecords = metsData.AltRecords ?? Array.Empty<MetsData.AltRecord>();
        var deliverySpec = altRecords.FirstOrDefault(r => r.Type == MetsData.EAltRecordType.DeliverySpecification)?.InnerText;
        var submissionAgreement = altRecords.FirstOrDefault(r => r.Type == MetsData.EAltRecordType.SubmissionAgreement)?.InnerText;
        var packageNumber = altRecords.FirstOrDefault(r => r.Type == MetsData.EAltRecordType.PackageNumber)?.InnerText;
        var referenceCode = altRecords.FirstOrDefault(r => r.Type == MetsData.EAltRecordType.ReferenceCode)?.InnerText;

        var notes = mods?.Notes ?? Array.Empty<MetsData.ModsNote>();
        string? GetNote(MetsData.ENoteType type) => notes.FirstOrDefault(n => n.Type == type)?.InnerText;

        var rows = new List<(string Label, string? Value)>
        {
            ("Riksarkivets diarienummer leveransöverenskommelse", submissionAgreement),
            ("Riksarkivets diarienummer leverans", deliverySpec),
            ("Beskrivning av leveransen", mods?.ModsTitle),
            ("Arkivbildare", creatorAgent?.Name),
            ("Organisationsnummer arkivbildare", mods?.Identifier),
            ("Levererande myndighet", editorAgent?.Name ?? creatorAgent?.Name),
            ("Organisationsnummer levererande myndighet", mods?.Identifier),
            ("Servicebyrå/Konsult", custodianAgent?.Name),
            ("Kontaktperson för leverans", GetNote(MetsData.ENoteType.AuthorDisplayName)),
            ("Telefonnummer till kontaktperson", null),
            ("E-post-adress till kontaktperson", null),
            ("Kostnadsställe", null),
            ("Kontaktperson för e-fakturering", null),
            ("Arkivets namn", mods?.ModsTitleInfo),
            ("Systemets namn", mods?.Publisher),
            ("Uttagsdatum", mods == null || mods.DateIssued == default ? null : mods.DateIssued.ToString("yyyy-MM-dd")),
            ("Kommentar", null),
            ("Projektkod", null),
            ("Accessions-ID", packageNumber),
            ("Batch-ID", referenceCode),
        };

        ws.Column(1).Width = 50;
        ws.Column(2).Width = 40;
        ws.Cell(1, 1).Value = "Fält";
        ws.Cell(1, 2).Value = "Värde";
        ws.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < rows.Count; i++)
        {
            ws.Cell(i + 2, 1).Value = rows[i].Label;
            ws.Cell(i + 2, 2).Value = rows[i].Value ?? string.Empty;
        }
    }

    private static void BuildSheet2(XLWorkbook workbook, MetsData metsData)
    {
        var ws = workbook.Worksheets.Add("Flik 2");

        var headers = new[]
        {
            "Filnamn", "Filtyp", "Filtypsversion", "Storlek", "Teckenuppsättning",
            "Speltid", "Sökväg", "Sekretessgrad hos myndigheten",
            "Behandling av personuppgifter", "Kommentar"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Column(i + 1).Width = 20;
        }
        ws.Row(1).Style.Font.Bold = true;

        var files = metsData.Files ?? Array.Empty<MetsData.FileData>();
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var ext = Path.GetExtension(file.FileName)?.TrimStart('.') ?? string.Empty;
            var path = Path.GetDirectoryName(file.FileName)?.Replace('/', '\\') ?? string.Empty;

            ws.Cell(i + 2, 1).Value = Path.GetFileName(file.FileName);
            ws.Cell(i + 2, 2).Value = ext;
            ws.Cell(i + 2, 3).Value = string.Empty;
            ws.Cell(i + 2, 4).Value = file.Size;
            ws.Cell(i + 2, 5).Value = string.Empty;
            ws.Cell(i + 2, 6).Value = string.Empty;
            ws.Cell(i + 2, 7).Value = path;
            ws.Cell(i + 2, 8).Value = string.Empty;
            ws.Cell(i + 2, 9).Value = string.Empty;
            ws.Cell(i + 2, 10).Value = string.Empty;
        }
    }

    private static void BuildSheet3(XLWorkbook workbook, MetsData metsData)
    {
        var ws = workbook.Worksheets.Add("Flik 3");
        ws.Column(1).Width = 80;
        ws.Cell(1, 1).Value = "Fel";
        ws.Row(1).Style.Font.Bold = true;

        var files = metsData.Files ?? Array.Empty<MetsData.FileData>();
        int row = 2;

        // Check for disallowed file types
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName)?.TrimStart('.') ?? string.Empty;
            if (!string.IsNullOrEmpty(ext) && !AllowedExtensions.Contains(ext))
            {
                ws.Cell(row++, 1).Value =
                    $"ERROR vid otillåten filtyp ({Path.GetFileName(file.FileName)} - Filtyp '{ext}' är inte godkänd enligt RA-FS 2009:2)";
            }
        }

        // Check for duplicate filenames (case-insensitive)
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            var name = Path.GetFileName(file.FileName);
            if (!seen.Add(name))
                duplicates.Add(name);
        }

        foreach (var dup in duplicates)
        {
            ws.Cell(row++, 1).Value = $"ERROR om dubbelt filnamn detekterat (case-insensitive: {dup})";
        }
    }
}
