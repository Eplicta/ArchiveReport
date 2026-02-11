using OfficeOpenXml;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eplicta.Conserver.Cli
{
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
                var zipFiles = files
                    .Where(f => f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    .ToList();

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
            // ===== Läs METS =====
            var mets = MetsReader.Read(zipPath);

            // ===== Ladda inbäddad Excel-mall =====
            var assembly = Assembly.GetExecutingAssembly();
            using var templateStream =
                assembly.GetManifestResourceStream("Eplicta.Conserver.Cli.Templates.mall_webb_2026.xlsx")
                ?? throw new Exception("Kunde inte hitta inbyggd Excel-mall.");

            using var package = new ExcelPackage(templateStream);
            var sheet = package.Workbook.Worksheets.FirstOrDefault()
                ?? throw new Exception("Excel-mallen innehåller inget blad.");

            // ===== Fyll rätt celler enligt mallen =====
            sheet.Cells["B4"].Value = mets.Beskrivning;
            sheet.Cells["B5"].Value = mets.Arkivbildare;
            sheet.Cells["B6"].Value = mets.OrganisationsnummerArkivbildare;
            sheet.Cells["B7"].Value = mets.LevererandeMyndighet;
            sheet.Cells["B8"].Value = mets.OrganisationsnummerLevererande;
            sheet.Cells["B9"].Value = mets.System;
            sheet.Cells["B10"].Value = mets.Kontaktperson;
            sheet.Cells["B11"].Value = mets.Telefon;
            sheet.Cells["B12"].Value = mets.Epost;
            sheet.Cells["B13"].Value = mets.Kostnadsstalle;
            sheet.Cells["B14"].Value = mets.FakturaKontakt;
            sheet.Cells["B15"].Value = mets.ArkivetsNamn;
            sheet.Cells["B16"].Value = mets.Systemnamn;
            sheet.Cells["B17"].Value = mets.Uttagsdatum;
            sheet.Cells["B18"].Value = mets.Kommentar;
            sheet.Cells["B19"].Value = mets.Projektkod;
            sheet.Cells["B20"].Value = mets.AccessionsId;
            sheet.Cells["B21"].Value = mets.BatchId;

            // ===== Spara Excel bredvid zip =====
            string excelPath = Path.Combine(
                Path.GetDirectoryName(zipPath)!,
                Path.GetFileNameWithoutExtension(zipPath) + "_resultat.xlsx");

            package.SaveAs(new FileInfo(excelPath));

            // ===== Öppna Excel =====
            Process.Start(new ProcessStartInfo
            {
                FileName = excelPath,
                UseShellExecute = true
            });
        }
    }
}
