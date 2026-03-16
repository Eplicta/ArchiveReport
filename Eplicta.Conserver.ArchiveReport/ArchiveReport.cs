namespace Eplicta.Conserver.ArchiveReport;

// Contains metadata
public class ArchiveReport
{
    // Files that are included in the delivery
    public List<FileEntry> Files { get; init; } = new();

    // Basic archive information
    public string Description { get; init; } = "";
    public string ArchiveBuilder { get; init; } = "";
    public string OrganizationNumberArchiveBuilder { get; init; } = "";

    // Delivering organization
    public string DeliveringOrganization { get; init; } = "";
    public string DeliveringOrganizationNumber { get; init; } = "";

    // Original system information
    public string System { get; init; } = "";
    public string SystemName { get; init; } = "";

    // Contact information
    public string ContactPerson { get; init; } = "";
    public string Telephone { get; init; } = "";
    public string Email { get; init; } = "";

    // Invoice information
    public string CostPlace { get; init; } = "";
    public string InvoiceContact { get; init; } = "";

    // Archive information
    public string ArchiveName { get; init; } = "";
    public string WithdrawalDate { get; init; } = "";
    public string Comment { get; init; } = "";

    // Internal identification
    public string ProjectCode { get; init; } = "";
    public string AccessionsId { get; init; } = "";
    public string BatchId { get; init; } = "";
}