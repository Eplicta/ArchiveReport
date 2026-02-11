using OfficeOpenXml;
using System.Diagnostics;
using System.Windows.Forms;

namespace Eplicta.Conserver.Cli;

public class AppForm : Form
{
    private readonly Label _statusLabel;
    private readonly ProgressBar _progressBar;

    public AppForm()
    {
        Text = "METS → Excel";
        Width = 420;
        Height = 220;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var instruction = new Label
        {
            Text = "Dra in en eller flera METS-zippar här",
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Font = new System.Drawing.Font("Segoe UI", 12)
        };

        _statusLabel = new Label
        {
            Text = "Väntar på fil…",
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Top,
            Height = 25,
            Minimum = 0,
            Maximum = 100
        };

        Controls.Add(_progressBar);
        Controls.Add(_statusLabel);
        Controls.Add(instruction);

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

    private async void OnDragDrop(object? sender, DragEventArgs e)
    {
        try
        {
            var files = (string[])e.Data!.GetData(DataFormats.FileDrop)!;
            var zipFiles = files.Where(f => f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).ToList();

            if (zipFiles.Count == 0)
                throw new Exception("Ingen zip-fil hittades.");

            _progressBar.Value = 0;
            _progressBar.Maximum = zipFiles.Count;

            for (int i = 0; i < zipFiles.Count; i++)
            {
                string zip = zipFiles[i];

                _statusLabel.Text = $"Bearbetar: {Path.GetFileName(zip)}";

                await Task.Run(() => Run(zip));

                _progressBar.Value = i + 1;
            }

            _statusLabel.Text = "Klart!";
            MessageBox.Show("Alla filer är behandlade.", "Färdig");
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Fel uppstod";
            MessageBox.Show(ex.Message, "Fel");
        }
    }

    private void Run(string zipPath)
    {
        var values = MetsReader.Read(zipPath);

        string excelPath = Path.Combine(Path.GetDirectoryName(zipPath)!, "resultat.xlsx");

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Data");

        for (int i = 0; i < values.Count; i++)
            sheet.Cells[i + 1, 1].Value = values[i];

        package.SaveAs(new FileInfo(excelPath));

        Process.Start(new ProcessStartInfo
        {
            FileName = excelPath,
            UseShellExecute = true
        });
    }
}
