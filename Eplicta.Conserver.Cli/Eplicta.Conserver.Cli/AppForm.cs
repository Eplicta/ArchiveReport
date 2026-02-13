using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OfficeOpenXml;


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

                _progressBar.Value = 0;

                foreach (var file in files)
                {
                    _statusLabel.Text = $"Bearbetar: {Path.GetFileName(file)}";
                    await Task.Run(() => Run(file));
                }

                _statusLabel.Text = "Klart!";
                _progressBar.Value = 100;
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
                ?? throw new Exception("Kunde inte hitta den inbäddade Excel-mallen.");


            using var package = new OfficeOpenXml.ExcelPackage(templateStream);





            // ===== Fyll metadata =====
            // ===== Hämta blad =====
            var allmantSheet = package.Workbook.Worksheets["Allmänt"]
               ?? throw new Exception("Kunde inte hitta bladet 'Allmänt'.");

            var filerSheet = package.Workbook.Worksheets["Filer"]
                ?? throw new Exception("Kunde inte hitta bladet 'Filer'.");



            // ===== Fyll metadata (ska ligga i Allmänt) =====
            allmantSheet.Cells["B4"].Value = mets.Beskrivning;
            allmantSheet.Cells["B5"].Value = mets.Arkivbildare;
            allmantSheet.Cells["B6"].Value = mets.OrganisationsnummerArkivbildare;
            allmantSheet.Cells["B7"].Value = mets.LevererandeMyndighet;
            allmantSheet.Cells["B8"].Value = mets.OrganisationsnummerLevererande;
            allmantSheet.Cells["B9"].Value = mets.System;
            allmantSheet.Cells["B10"].Value = mets.Kontaktperson;
            allmantSheet.Cells["B11"].Value = mets.Telefon;
            allmantSheet.Cells["B12"].Value = mets.Epost;
            allmantSheet.Cells["B13"].Value = mets.Kostnadsstalle;
            allmantSheet.Cells["B14"].Value = mets.FakturaKontakt;
            allmantSheet.Cells["B15"].Value = mets.ArkivetsNamn;
            allmantSheet.Cells["B16"].Value = mets.Systemnamn;
            allmantSheet.Cells["B17"].Value = mets.Uttagsdatum;
            allmantSheet.Cells["B18"].Value = mets.Kommentar;
            allmantSheet.Cells["B19"].Value = mets.Projektkod;
            allmantSheet.Cells["B20"].Value = mets.AccessionsId;
            allmantSheet.Cells["B21"].Value = mets.BatchId;


            // ===== Skriv filförteckning (ska ligga i Filer-bladet) =====
            if (mets.Files != null && mets.Files.Count > 0)
            {
                int row = 2; // ← oftast börjar tabellen på rad 2 i bladet "Filer"

                foreach (var f in mets.Files)
                {
                    filerSheet.Cells[row, 1].Value = f.FileName;
                    filerSheet.Cells[row, 2].Value = f.FileType;
                    filerSheet.Cells[row, 3].Value = f.FileTypeVersion;
                    filerSheet.Cells[row, 4].Value = f.SizeBytes;
                    filerSheet.Cells[row, 5].Value = f.Charset;
                    filerSheet.Cells[row, 6].Value =
                        f.Duration.HasValue ? f.Duration.Value.ToString(@"hh\:mm\:ss") : "";
                    filerSheet.Cells[row, 7].Value = f.Path;
                    filerSheet.Cells[row, 8].Value = f.Secrecy;
                    filerSheet.Cells[row, 9].Value = f.PersonalDataHandling;
                    filerSheet.Cells[row, 10].Value = f.Comment;

                    row++;
                }
            }


            // ===== Spara Excel =====
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
