using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DinkToPdf;
using Microsoft.Win32;
using TeddyBearExport.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TeddyBearExport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static Dokument dokument = new Dokument();

        string templatePath = System.IO.Path.Combine(AppContext.BaseDirectory, "PdfUtils", "template.html");
        string selectedFilePath;
        private static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string folderPath = System.IO.Path.Combine(documentsPath, "KrugoviPDF");

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.Title = "Select a JSON file";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }



                selectedFilePath = openFileDialog.FileName;

                readJsonContent(selectedFilePath);
            }

        }

        private void convertToPdf(string pdfFilePath, string html)
        {
            var converter = new SynchronizedConverter(new PdfTools());

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                ColorMode = ColorMode.Grayscale,
                Orientation = DinkToPdf.Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Out = pdfFilePath,
            },
                Objects = {
                new ObjectSettings()
                {
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
            };

            converter.Convert(doc);
        }

        private string formatPdf(Krug krug)
        {
            string html = File.ReadAllText(templatePath);

            var permanent = krug.Permanentna == true ? "Da" : "Ne";

            html = html.Replace("{{GazJed}}", dokument.GazJedinica.ToString());
            html = html.Replace("{{Odeljenje}}", dokument.BrOdeljenja.ToString());
            html = html.Replace("{{Odsek}}", dokument.Odsek.ToString());
            html = html.Replace("{{BrKrug}}", krug.BrKruga.ToString());
            html = html.Replace("{{UzgojnaGrupa}}", krug.UzgojnaGrupa.ToString());
            html = html.Replace("{{GazTip}}", krug.GazTip.ToString());
            html = html.Replace("{{Permanentan}}", permanent);
            html = html.Replace("{{ID}}", krug.Permanentna == true ? krug.IdBroj.ToString() : "0");
            html = html.Replace("{{Nagib}}", krug.Nagib.ToString());
            html = generateTreeRows(html, krug);
            html = generateDeadTreeRows(html, krug);
            html = generateBiodiversity(html, krug);

            return html;

        }

        private string generateBiodiversity(string htmlTemplate, Krug krug)
        {
            Biodiverzitet biodiverzitet = krug.Biodiverzitet;
            htmlTemplate = htmlTemplate.Replace("{{dubeca}}", biodiverzitet.Dubeca.ToString());
            htmlTemplate = htmlTemplate.Replace("{{polomljena}}", biodiverzitet.OsteceniVrh.ToString());
            htmlTemplate = htmlTemplate.Replace("{{ispucalaKora}}", biodiverzitet.OstecenaKora.ToString());
            htmlTemplate = htmlTemplate.Replace("{{gnezda}}", biodiverzitet.Gnezda.ToString());
            htmlTemplate = htmlTemplate.Replace("{{supljine}}", biodiverzitet.Supljine.ToString());
            htmlTemplate = htmlTemplate.Replace("{{lisajevi}}", biodiverzitet.Lisajevi.ToString());
            htmlTemplate = htmlTemplate.Replace("{{mahovine}}", biodiverzitet.Mahovine.ToString());
            htmlTemplate = htmlTemplate.Replace("{{gljive}}", biodiverzitet.Gljive.ToString());
            htmlTemplate = htmlTemplate.Replace("{{izuzetnaDimenzija}}", biodiverzitet.IzuzetnaDimenzija.ToString());
            htmlTemplate = htmlTemplate.Replace("{{usamljena}}", biodiverzitet.VelikaUsamljena.ToString());

            return htmlTemplate;
        }

        private string generateDeadTreeRows(string html, Krug krug)
        {
            StringBuilder treeRows = new StringBuilder();
            foreach (var tree in krug.MrtvaStabla)
            {
                treeRows.AppendLine("<tr>");
                treeRows.AppendLine($"<td>{tree.Rbr}</td>");
                treeRows.AppendLine($"<td>{tree.Vrsta}</td>");
                treeRows.AppendLine($"<td>{tree.Polozaj}</td>");
                treeRows.AppendLine($"<td>{tree.Precnik}</td>");
                treeRows.AppendLine($"<td>{tree.Visina}</td>");
                treeRows.AppendLine("</tr>");
            }
            return html.Replace("{{dead_tree_rows}}", treeRows.ToString());

        }

        private string generateTreeRows(string html, Krug krug)
        {
            StringBuilder treeRows = new StringBuilder();
            foreach (Stablo stablo in krug.Stabla)
            {
                treeRows.AppendLine("<tr>");
                treeRows.AppendLine($"<td>{stablo.Rbr}</td>");
                treeRows.AppendLine($"<td>{stablo.Vrsta}</td>");
                treeRows.AppendLine($"<td>{stablo.Precnik}</td>");
                treeRows.AppendLine($"<td>{stablo.Visina}</td>");
                treeRows.AppendLine($"<td>{stablo.DuzDebla}</td>");
                treeRows.AppendLine($"<td>{stablo.StepSusenja}</td>");
                treeRows.AppendLine($"<td>{stablo.SocStatus}</td>");
                treeRows.AppendLine($"<td>{stablo.TehKlasa}</td>");
                treeRows.AppendLine($"<td>{stablo.ProbDoznaka}</td>");
                treeRows.AppendLine($"<td>{stablo.Razdaljina}</td>");
                treeRows.AppendLine($"<td>{stablo.Azimut}</td>");
                treeRows.AppendLine("</tr>");
            }
            return html.Replace("{{tree_rows}}", treeRows.ToString());

        }

        private void readJsonContent(string selectedFilePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(selectedFilePath);

                // Now deserialize
                dokument = JsonSerializer.Deserialize<Dokument>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dokument != null)
                {
                    // Success - krug is populated from JSON!
                    MessageBox.Show($"Dokument učitan!");
                }
                else
                {
                    MessageBox.Show("Neuspešno učitavanje dokumenta!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JSON: {ex.Message}");
            }
        }

        private void ConverButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // New PDF file name
                string originalFileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(selectedFilePath);

               
                foreach (Krug krug in dokument.Krugovi)
                {
                    string pdfFileName = string.Format("{0}_Krug{1}.pdf", originalFileNameWithoutExt, krug.BrKruga);
                    string pdfFilePath = System.IO.Path.Combine(folderPath, pdfFileName);
                    string html = formatPdf(krug);

                    convertToPdf(pdfFilePath, html);
                }

                MessageBox.Show("Uspešno prebacivanje dokumenta u pdf!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

        }
    }
}