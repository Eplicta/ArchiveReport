using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using OfficeOpenXml;

namespace Eplicta.Conserver.Cli
{
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

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var files = (string[])e.Data!.GetData(DataFormats.FileDrop)!;
                string zipPath = files[0];

                Run(zipPath);

                MessageBox.Show("Klart! Excel skapad.", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fel");
            }
        }

        private void Run(string zipPath)
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mall_webb_2026.xlsx");
            string excelPath = Path.Combine(Path.GetDirectoryName(zipPath)!, "resultat.xlsx");

            // ==== Läs XML ur zip ====
            XDocument xmlDoc;

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                var entry = archive.Entries
                    .FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                    throw new Exception("Ingen XML-fil hittades i zip.");

                using var stream = entry.Open();
                xmlDoc = XDocument.Load(stream);
            }

            // ==== Öppna Excel-mall ====
            if (!File.Exists(templatePath))
                throw new Exception("Excel-mallen hittades inte:\n" + templatePath);

            using var package = new ExcelPackage(new FileInfo(templatePath));

            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null)
                throw new Exception("Excel-mallen innehåller inget blad.");

            // ==== Hämta XML-värden ====
            var values = xmlDoc.Descendants()
                .Select(e => e.Value?.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Take(100)
                .ToList();

            if (values.Count == 0)
                throw new Exception("Inga värden hittades i XML-filen.");

            // ==== Skriv till Excel ====
            for (int i = 0; i < values.Count; i++)
                sheet.Cells[i + 2, 2].Value = values[i];

            package.SaveAs(new FileInfo(excelPath));

            // ==== Öppna resultat ====
            Process.Start(new ProcessStartInfo
            {
                FileName = excelPath,
                UseShellExecute = true
            });
        }
    }
}