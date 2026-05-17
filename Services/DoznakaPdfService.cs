using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using TeddyBearExport.Model;
using TeddyBearExport.Model.Doznaka;
using TeddyBearExport.Helpers;
using System.Globalization;

namespace TeddyBearExport.Services
{
    public class DoznakaPdfService
    {
        private readonly IConverter _converter = new SynchronizedConverter(new PdfTools());
        private readonly string _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private readonly string _outputFolder;
        private readonly string _templatePath;



        public DoznakaPdfService()
        {
            _outputFolder = Path.Combine(_documentsPath, "DoznakaPDF");
            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
            _templatePath = Path.Combine(AppContext.BaseDirectory, "PdfUtils", "doznaka_template.html");

        }

        public DokumentDoznaka? ReadJson(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var dokument = JsonSerializer.Deserialize<DokumentDoznaka>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });
                if (!dokument.Tablice.Any())
                {
                    MessageBox.Show("JSON ne sadrži obavezne podatke za Doznaka dokument.");
                    return null;
                }

                return dokument;
            }
            catch
            {
                return null;
            }
        }

        public async Task ConvertToPdfAsync(string path)
        {
            try
            {
                var dokument = ReadJson(path);
                if (dokument == null)
                    throw new Exception("Nevalidan JSON dokument.");

                await Task.Run(() =>
                {
                    string html;
                    try
                    {
                        html = GenerateHtmlAsync(dokument).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText("C:\\Users\\Public\\doznaka_error.log", $"[HTML ERROR] {ex}");
                        throw;
                    }

                    string fileName = $"Doznaka_{dokument.Doznacar}_{dokument.GazJedinica}_{dokument.BrOdeljenja}_{dokument.Odsek}.pdf";
                    string outputPath = Path.Combine(_outputFolder, fileName);

                    try
                    {
                        var doc = new HtmlToPdfDocument
                        {
                            GlobalSettings = {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Portrait,
                        Out = outputPath
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
                    catch (Exception ex)
                    {
                        File.WriteAllText("C:\\Users\\Public\\doznaka_error.log", $"[PDF ERROR] {ex}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Doznaka PDF greška: {ex.Message}");
                throw;
            }
        }


        private async Task<string> GenerateHtmlAsync(DokumentDoznaka dokument)
        {
            string html = await File.ReadAllTextAsync(_templatePath);

            html = html.Replace("{{GazJed}}", dokument.GazJedinica.ToString());
            html = html.Replace("{{Odeljenje}}", dokument.BrOdeljenja.ToString());
            html = html.Replace("{{Odsek}}", dokument.Odsek);
            html = html.Replace("{{TipDoznake}}", dokument.TipDoznake?.ToString() ?? "");
            html = html.Replace("{{Doznacar}}", dokument.Doznacar);
            html = html.Replace("{{Povrsina}}", dokument.PovrsinaDoznake.ToString("F2"));
            html = html.Replace("{{VrstaPrinosa}}", dokument.VrstaPrinosa.ToString());
            html = html.Replace("{{VrstaSece}}", dokument.VrstaSece.ToString());

            // ✅ Conditional rendering
            if (dokument.TipDoznake == TipDoznake.STABLIMICNA)
            {
                // --- Replace TabliceGrid section with a Stabla table ---
                var sbStabla = new StringBuilder();

                sbStabla.AppendLine("<h2>Stabla</h2>");
                sbStabla.AppendLine("<table>");
                sbStabla.AppendLine("<tr><th>R.br</th><th>Vrsta</th><th>Prečnik (cm)</th><th>Procena Tehnike(%)</th><th>Bruto Zapremina(m³)</th><th>Tehnika</th><th>Radni dan</th></tr>");

                int rbr = 1;

                foreach (var s in dokument.Stabla.OrderBy(x => x.Rbr))
                {
                    sbStabla.AppendLine($"<tr><td>{rbr++}</td><td>{s.Vrsta}</td><td>{s.Precnik:F1}</td><td>{(int)(s.Tehnika*100)}</td><td>{s.Zapremina}</td><td>{s.Tehnika*s.Zapremina}</td><td>{s.RadniDan:dd.MM.yyyy}</td></tr>");
                }

                sbStabla.AppendLine("</table>");
                html = html.Replace("{{Stabla}}", sbStabla.ToString());
            }
            else
            {
                html = html.Replace("{{Stabla}}", "");
            }

            // --- Tablice cards (default layout) ---
            // --- Tablice cards (table layout for PDF compatibility) ---
            var sbTablice = new StringBuilder();
            int i = 0;
            sbTablice.AppendLine("<table style='width:100%; border-collapse:collapse;'>");

            foreach (var t in dokument.Tablice)
            {
                if (i % 3 == 0) sbTablice.AppendLine("<tr>"); // new row every 3 cards

                string vrstaName;
                if (Enum.IsDefined(typeof(VrstaDrvo), t.Vrsta))
                {
                    var enumValue = (VrstaDrvo)t.Vrsta;
                    vrstaName = enumValue.GetDescription();
                }
                else
                {
                    vrstaName = $"Nepoznata ({t.Vrsta})"; // fallback for invalid codes
                }

                sbTablice.AppendLine("<td style='vertical-align:top; width:33%; padding:8px;'>");
                sbTablice.AppendLine("<div style='border:2px solid #ccc; border-radius:10px; padding:10px; background:#f8f8f8;'>");
                sbTablice.AppendLine($"<h4 style='text-align:center; border-bottom:1px solid #ddd; padding-bottom:4px;'>{vrstaName}</h4>");
                sbTablice.AppendLine($"<p><b>Vrsta:</b> {t.Vrsta}</p>");
                sbTablice.AppendLine($"<p><b>Tarifa:</b> {t.Tarifa}</p>");
                sbTablice.AppendLine($"<p><b>Plan Zapremina:</b> {t.PlanZapremina:F2} m³</p>");
                sbTablice.AppendLine($"<p><b>Izmerena Zapremina:</b> {t.TrenutnaZapremina:F2} m³</p>");
                if(t.PlanZapremina > 0)
                {
                   sbTablice.AppendLine($"<p><b>Uradjeno (%):</b> {(int)(SafeDiv(t.TrenutnaZapremina, t.PlanZapremina) * 100)}%</p>");
                }
                sbTablice.AppendLine($"<p><b>Tehnika zapremina:</b> {t.TehnikaZapremina:F2} m³</p>");

                // --- DebStepeni section inside each card ---
                if (dokument.TipDoznake == TipDoznake.DEBLJINSKI_STEPEN && t.DebStepeni.Any())
                {
                    sbTablice.AppendLine("<div style='margin-top:10px; padding:6px 10px; background-color:#f1f3f4; border-left:4px solid #007bff; border-radius:6px;'>");
                    sbTablice.AppendLine("<b>Deb Stepeni:</b><ul style='list-style:none; padding-left:0; margin:0;'>");
                    foreach (var ds in t.DebStepeni.OrderBy(x => x.DebStepen))
                    sbTablice.AppendLine($"<li style='border-bottom:1px solid #e0e0e0; padding:2px 0;'>{ds.DebStepen}: {ds.Kolicina}</li>");
                    sbTablice.AppendLine("</ul></div>");
                }

                if(dokument.TipDoznake == TipDoznake.DEBLJINSKI_STEPEN)
                {
                    sbTablice.AppendLine($"<p><b>Ukupno Izmereno:</b> {t.DebStepeni.Sum(x => x.Kolicina)} stabala</p>");
                }
                else
                {
                    sbTablice.AppendLine($"<p><b>Ukupno Izmereno:</b> {dokument.Stabla.Count(x => x.Vrsta == t.Vrsta)} stabala</p>");
                }

                sbTablice.AppendLine("</div>");
                sbTablice.AppendLine("</td>");

                i++;
                if (i % 3 == 0) sbTablice.AppendLine("</tr>"); // close row after 3 cards
            }

            // Close last row if not full
            if (i % 3 != 0) sbTablice.AppendLine("</tr>");
            sbTablice.AppendLine("</table>");

            html = html.Replace("{{TabliceGrid}}", sbTablice.ToString());



            // Helper for safe division
            double SafeDiv(double numerator, double denominator) => denominator == 0 ? 0 : numerator / denominator;

            CultureInfo sr = new CultureInfo("sr-Latn-RS");

            // --- Radni Dani (kept the same) ---
            var sbDani = new StringBuilder();
            foreach (var d in dokument.RadniDani)
            {
                double dozPoStBudVal = d.StBuducnosti > 0 ? SafeDiv(d.UkupnoDoznaceni, d.StBuducnosti) : 0;
                double stBudPoPovrsiniVal = d.DozPovrsina > 0 ? SafeDiv(d.StBuducnosti, d.DozPovrsina) : 0;
                sbDani.AppendLine($"<tr><td>{(d.Dan.HasValue ? d.Dan.Value.ToString("dd/MM/yyyy") +
                    "<div style='height:2px;background:black;'></div>" +
                    d.Dan.Value.ToString("dddd", sr).ToUpper() : "")}</td>" +
                    $"<td>{d.UkupnoDoznaceni}</td>" +
                    $"<td>{d.UkupnoZapremina:F2}</td>" +
                    $"<td>{d.StBuducnosti}</td>" +
                    $"<td>{d.DozPovrsina:F2}</td>" +
                    $"<td>{dozPoStBudVal:F2}</td>" +
                    $"<td>{stBudPoPovrsiniVal:F2}</td>" +
                    $"</tr>");
            }

            // Calculate both averages safely
            double dozPoStBud = dokument.RadniDani.Count > 0
                ? dokument.RadniDani.Average(x => SafeDiv(x.UkupnoDoznaceni, x.StBuducnosti))
                : 0;

            double stBudPoPovrsini = dokument.RadniDani.Count > 0
                ? dokument.RadniDani.Average(x => SafeDiv(x.StBuducnosti, x.DozPovrsina))
                : 0; // If your second column should be based on another formula, change this line

            // Build the summary row
            sbDani.AppendLine("<tr class='summary'>");
            sbDani.AppendLine("<td>UKUPNO</td>");
            sbDani.AppendLine($"<td>{dokument.RadniDani.Sum(x => x.UkupnoDoznaceni)}</td>");
            sbDani.AppendLine($"<td>{dokument.RadniDani.Sum(x => x.UkupnoZapremina):F2}</td>");
            sbDani.AppendLine($"<td>{dokument.RadniDani.Sum(x => x.StBuducnosti)}</td>");
            sbDani.AppendLine($"<td>{dokument.RadniDani.Sum(x => x.DozPovrsina):F2}</td>");
            sbDani.AppendLine($"<td>{dozPoStBud:F2}</td>");
            sbDani.AppendLine($"<td>{stBudPoPovrsini:F2}</td>");
            sbDani.AppendLine("</tr>");

            html = html.Replace("{{RadniDaniRows}}", sbDani.ToString());

            return html;
        }

    }
}
