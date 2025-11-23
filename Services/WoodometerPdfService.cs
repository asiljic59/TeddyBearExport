using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DinkToPdf;
using TeddyBearExport.Model.Woodometer;
using TeddyBearExport.Model;
using System.Windows;
using System.Globalization;

namespace TeddyBearExport.Services
{
    internal class WoodometerPdfService
    {
        private readonly SynchronizedConverter _converter = new SynchronizedConverter(new PdfTools());
        private readonly string _templatePath;
        private readonly string _outputRoot;

        public WoodometerPdfService()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _outputRoot = Path.Combine(documentsPath, "KrugoviPDF");
            _templatePath = Path.Combine(AppContext.BaseDirectory, "PdfUtils", "template.html");
        }

        private Dokument? dokument;

        public Dokument? ReadJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            dokument = JsonSerializer.Deserialize<Dokument>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!dokument.Krugovi.Any())
            {
                MessageBox.Show("JSON ne sadrži obavezne podatke za Woodometer dokument.");
                return null;
            }
            return dokument;
        }

        public async Task ConvertToPdfAsync(string selectedFilePath)
        {
            if (dokument == null)
                throw new InvalidOperationException("Dokument nije učitan.");

            string originalFileNameWithoutExt = Path.GetFileNameWithoutExtension(selectedFilePath);

            await Task.Run(() =>
            {
                foreach (Krug krug in dokument.Krugovi)
                {
                    string odsek = InsertUnderscore(originalFileNameWithoutExt.Split("_")[0]);
                    string odsekFolderPath = Path.Combine(_outputRoot, odsek);
                    string pdfFileName = $"{krug.BrKruga}.pdf";
                    string pdfFilePath = Path.Combine(odsekFolderPath, pdfFileName);

                    if (!Directory.Exists(odsekFolderPath))
                    {
                        Directory.CreateDirectory(odsekFolderPath);
                    }

                    string html = FormatPdf(krug);

                    var doc = new HtmlToPdfDocument
                    {
                        GlobalSettings = {
                            ColorMode = ColorMode.Color,
                            Orientation = Orientation.Portrait,
                            PaperSize = PaperKind.A4,
                            Out = pdfFilePath
                        },
                        Objects = {
                            new ObjectSettings
                            {
                                HtmlContent = html,
                                WebSettings = { DefaultEncoding = "utf-8" }
                            }
                        }
                    };

                    _converter.Convert(doc);
                }
            });
        }

        private string FormatPdf(Krug krug)
        {
            string html = File.ReadAllText(_templatePath);
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
            html = html.Replace("{{Napomena}}", krug.Napomena.ToString());
            html = html.Replace("{{PocetnoVreme}}", krug.StartTime);
            html = html.Replace("{{ZavrsnoVreme}}", krug.EndTime);
            html = html.Replace("{{PeriodRada}}", getPeriod(krug));
            html = GenerateTreeRows(html, krug);
            html = GenerateDeadTreeRows(html, krug);
            html = GenerateBiodiversity(html, krug);

            return html;
        }

        private string getPeriod(Krug krug)
        {
            DateTime startTime = DateTime.ParseExact(krug.StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.ParseExact(krug.EndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            TimeSpan period = endTime - startTime;
            return $"{(int)period.TotalMinutes:D2} minuta, {period.Seconds:D2} sekundi";
        }

        private string GenerateBiodiversity(string html, Krug krug)
        {
            Biodiverzitet b = krug.Biodiverzitet;
            html = html.Replace("{{dubeca}}", b.Dubeca.ToString());
            html = html.Replace("{{polomljena}}", b.OsteceniVrh.ToString());
            html = html.Replace("{{ispucalaKora}}", b.OstecenaKora.ToString());
            html = html.Replace("{{gnezda}}", b.Gnezda.ToString());
            html = html.Replace("{{supljine}}", b.Supljine.ToString());
            html = html.Replace("{{lisajevi}}", b.Lisajevi.ToString());
            html = html.Replace("{{mahovine}}", b.Mahovine.ToString());
            html = html.Replace("{{gljive}}", b.Gljive.ToString());
            html = html.Replace("{{izuzetnaDimenzija}}", b.IzuzetnaDimenzija.ToString());
            html = html.Replace("{{usamljena}}", b.VelikaUsamljena.ToString());
            return html;
        }

        private string GenerateTreeRows(string html, Krug krug)
        {
            var sb = new StringBuilder();
            foreach (var s in krug.Stabla)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{s.Rbr}</td>");
                sb.AppendLine($"<td>{s.Vrsta}</td>");
                sb.AppendLine($"<td>{s.Precnik}</td>");
                sb.AppendLine($"<td>{s.Visina}</td>");
                sb.AppendLine($"<td>{s.DuzDebla}</td>");
                sb.AppendLine($"<td>{s.StepSusenja}</td>");
                sb.AppendLine($"<td>{s.SocStatus}</td>");
                sb.AppendLine($"<td>{s.TehKlasa}</td>");
                sb.AppendLine($"<td>{s.ProbDoznaka}</td>");
                sb.AppendLine($"<td>{s.Razdaljina}</td>");
                sb.AppendLine($"<td>{s.Azimut}</td>");
                sb.AppendLine("</tr>");
            }
            return html.Replace("{{tree_rows}}", sb.ToString());
        }

        private string GenerateDeadTreeRows(string html, Krug krug)
        {
            var sb = new StringBuilder();
            foreach (var t in krug.MrtvaStabla)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{t.Rbr}</td>");
                sb.AppendLine($"<td>{t.Vrsta}</td>");
                sb.AppendLine($"<td>{t.Polozaj}</td>");
                sb.AppendLine($"<td>{t.Precnik}</td>");
                sb.AppendLine($"<td>{t.Visina}</td>");
                sb.AppendLine("</tr>");
            }
            return html.Replace("{{dead_tree_rows}}", sb.ToString());
        }

        private static string InsertUnderscore(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= 4)
                return input;

            return input.Insert(4, "_");
        }
    
    }
}
