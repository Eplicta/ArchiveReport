namespace Eplicta.Conserver.ArchiveReport;

// Innehåller metadata för en arkivleverans samt dess filer.
public class ArchiveReport
{
    // Filer som ingår i leveransen.
    public List<FileEntry> Files { get; set; } = new();

    // Grundläggande arkivinformation
    public string Beskrivning { get; set; } = "";
    public string Arkivbildare { get; set; } = "";
    public string OrganisationsnummerArkivbildare { get; set; } = "";

    // Levererande organisation
    public string LevererandeMyndighet { get; set; } = "";
    public string OrganisationsnummerLevererande { get; set; } = "";

    // Ursprungssystem
    public string System { get; set; } = "";
    public string Systemnamn { get; set; } = "";

    // Kontaktuppgifter
    public string Kontaktperson { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string Epost { get; set; } = "";

    // Fakturainformation
    public string Kostnadsstalle { get; set; } = "";
    public string FakturaKontakt { get; set; } = "";

    // Arkivinformation
    public string ArkivetsNamn { get; set; } = "";
    public string Uttagsdatum { get; set; } = "";
    public string Kommentar { get; set; } = "";

    // Intern identifiering
    public string Projektkod { get; set; } = "";
    public string AccessionsId { get; set; } = "";
    public string BatchId { get; set; } = "";
}