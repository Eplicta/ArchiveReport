using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using OfficeOpenXml;

namespace Eplicta.Conserver.Cli;

public class AppForm : Form
{
    public AppForm()
    {
        Text = "METS → Excel";
        Width = 400;
        Height = 200;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var label = new Label
        {
            Text = "Dra in en METS-zip här",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Font = new System.Drawing.Font("Segoe UI", 12)
        };

        Controls.Add(label);

        AllowDrop = true;
        DragEnter += OnDragEnter;
        DragDrop += OnDragDrop;

        ExcelPackage.License.SetNonCommercialPersonal("Lenovo User");
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        try
        {
            var files = (string[])e.Data!.GetData(DataFormats.FileDrop)!;
            string zipPath = files[0];

            Run(zipPath);

            MessageBox.Show("Klart! Excel skapad.", "Färdig");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Fel");
        }
    }

    private void Run(string zipPath)
    {
        // ===== Läs METS =====
        var mets = MetsReader.Read(zipPath);

        // ===== Ladda inbakad Excel-mall =====
        var assembly = typeof(AppForm).Assembly;
        using var templateStream =
            assembly.GetManifestResourceStream("Eplicta.Conserver.Cli.Templates.mall_webb_2026.xlsx");

        if (templateStream == null)
            throw new Exception("Kunde inte hitta inbyggd Excel-mall.");

        using var package = new ExcelPackage(templateStream);
        var sheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new Exception("Excel-mallen saknar blad.");

        // ===== Skriv data till rätt celler =====
        sheet.Cells["B2"].Value = mets.Title;
        sheet.Cells["B3"].Value = mets.DateIssued;
        sheet.Cells["B4"].Value = mets.Identifier;
        sheet.Cells["B5"].Value = mets.SystemName;
        sheet.Cells["B6"].Value = mets.Url;

        // ===== Spara bredvid zip =====
        string excelPath = Path.Combine(Path.GetDirectoryName(zipPath)!, "resultat.xlsx");
        package.SaveAs(new FileInfo(excelPath));

        // ===== Öppna Excel =====
        Process.Start(new ProcessStartInfo
        {
            FileName = excelPath,
            UseShellExecute = true
        });
    }
}
