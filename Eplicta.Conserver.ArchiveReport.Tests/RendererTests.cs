using ClosedXML.Excel;
using Xunit;

namespace Eplicta.Conserver.ArchiveReport.Tests;

public class RendererTests
{
    private const string GeneralSheetName = "Allmänt";
    private const string FilesSheetName = "Filer";

    [Fact]
    public void GetBytes_returns_a_readable_xlsx_workbook()
    {
        var bytes = new Renderer().GetBytes(new ArchiveReport());

        using var workbook = OpenWorkbook(bytes);
        Assert.True(workbook.TryGetWorksheet(GeneralSheetName, out _));
        Assert.True(workbook.TryGetWorksheet(FilesSheetName, out _));
    }

    [Fact]
    public void GetBytes_writes_general_metadata_into_the_allmant_sheet()
    {
        var report = new ArchiveReport
        {
            Description = "A delivery",
            ArchiveBuilder = "Builder AB",
            ArchiveName = "The archive",
            BatchId = "B-42"
        };

        var bytes = new Renderer().GetBytes(report);

        using var workbook = OpenWorkbook(bytes);
        var sheet = workbook.Worksheet(GeneralSheetName);
        Assert.Equal("A delivery", sheet.Cell("B4").GetString());
        Assert.Equal("Builder AB", sheet.Cell("B5").GetString());
        Assert.Equal("The archive", sheet.Cell("B15").GetString());
        Assert.Equal("B-42", sheet.Cell("B21").GetString());
    }

    [Fact]
    public void GetBytes_writes_one_row_per_file_starting_at_row_two()
    {
        var report = new ArchiveReport
        {
            Files =
            [
                new FileEntry { FileName = "index.html", FileType = "text/html", SizeBytes = 1234, Charset = "utf-8", Path = "https://example.org/", Secrecy = "Öppen", PersonalDataHandling = "Nej" },
                new FileEntry { FileName = "clip.mp4", FileType = "video/mp4", SizeBytes = 5678, Duration = TimeSpan.FromSeconds(90) }
            ]
        };

        var bytes = new Renderer().GetBytes(report);

        using var workbook = OpenWorkbook(bytes);
        var sheet = workbook.Worksheet(FilesSheetName);

        Assert.Equal("index.html", sheet.Cell(2, 1).GetString());
        Assert.Equal("text/html", sheet.Cell(2, 2).GetString());
        Assert.Equal(1234, sheet.Cell(2, 4).GetValue<long>());
        Assert.Equal("utf-8", sheet.Cell(2, 5).GetString());
        Assert.Equal("https://example.org/", sheet.Cell(2, 7).GetString());
        Assert.Equal("Öppen", sheet.Cell(2, 8).GetString());

        Assert.Equal("clip.mp4", sheet.Cell(3, 1).GetString());
        Assert.Equal("00:01:30", sheet.Cell(3, 6).GetString());
    }

    private static XLWorkbook OpenWorkbook(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new XLWorkbook(stream);
    }
}
