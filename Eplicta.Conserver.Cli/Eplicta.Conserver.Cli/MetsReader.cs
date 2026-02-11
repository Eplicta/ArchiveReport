using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Eplicta.Conserver.Cli;

public class MetsData
{
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

public static class MetsReader
{
    public static MetsData Read(string zipPath)
    {
        if (!File.Exists(zipPath))
            throw new Exception("ZIP-filen finns inte.");

        XDocument xmlDoc;

        using (var archive = ZipFile.OpenRead(zipPath))
        {
            var entry = archive.Entries
                .FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            if (entry == null)
                throw new Exception("Ingen XML hittades i ZIP-filen.");

            using var stream = entry.Open();
            xmlDoc = XDocument.Load(stream);
        }

        var data = new MetsData();

        // Hjälpfunktion för att hitta första element med visst namn
        string Get(string name) =>
            xmlDoc.Descendants()
                  .FirstOrDefault(e => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))
                  ?.Value.Trim() ?? "";

        // 🔹 Här mappar vi METS → Excel-fält
        data.Beskrivning = Get("abstract");
        data.Arkivbildare = Get("creator");
        data.OrganisationsnummerArkivbildare = Get("organizationNumber");
        data.LevererandeMyndighet = Get("publisher");
        data.OrganisationsnummerLevererande = Get("publisherOrgNumber");
        data.System = Get("system");
        data.Kontaktperson = Get("contactPerson");
        data.Telefon = Get("telephone");
        data.Epost = Get("email");
        data.Kostnadsstalle = Get("costCenter");
        data.FakturaKontakt = Get("invoiceContact");
        data.ArkivetsNamn = Get("title");
        data.Systemnamn = Get("systemName");
        data.Uttagsdatum = Get("created");
        data.Kommentar = Get("note");
        data.Projektkod = Get("projectCode");
        data.AccessionsId = Get("accessionId");
        data.BatchId = Get("batchId");

        return data;
    }
}
