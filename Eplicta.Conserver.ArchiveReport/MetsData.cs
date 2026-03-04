namespace Eplicta.Conserver.ArchiveReport;

public class MetsData
{
    public List<FileEntry> Files { get; set; } = new();

    public string Beskrivning { get; set; } = "";
    public string Arkivbildare { get; set; } = "";
    public string OrganisationsnummerArkivbildare { get; set; } = "";
    public string LevererandeMyndighet { get; set; } = "";
    public string OrganisationsnummerLevererande { get; set; } = "";
    public string System { get; set; } = "";
    public string Kontaktperson { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string Epost { get; set; } = "";
    public string Kostnadsstalle { get; set; } = "";
    public string FakturaKontakt { get; set; } = "";
    public string ArkivetsNamn { get; set; } = "";
    public string Systemnamn { get; set; } = "";
    public string Uttagsdatum { get; set; } = "";
    public string Kommentar { get; set; } = "";
    public string Projektkod { get; set; } = "";
    public string AccessionsId { get; set; } = "";
    public string BatchId { get; set; } = "";
}
