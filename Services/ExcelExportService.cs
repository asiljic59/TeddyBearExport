using System;
using System.Globalization;
using System.Linq;
using TeddyBearExport.Helpers;
using TeddyBearExport.Model;
using TeddyBearExport.Model.Doznaka;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2019.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TeddyBearExport.Services
{
    public static class ExcelExportService
    {
        //cuva se vrsta, i za svaku vrstu se cuva 11-30 zapremina, 31,50 zapremina i >50 zapremina
        public static Dictionary<int, Dictionary<String, float>> cumSumZapremine;
        /// <summary>
        /// Create an Excel workbook with three sheets that visually match the provided templates.
        /// This method writes generic placeholders only; data population is intentionally minimal.
        /// </summary>
        public static void ExportDoznakaToExcel(DokumentDoznaka doc, string path)
        {
            // doc may be null in this mode; we only create the exact template layout
            using var wb = new XLWorkbook();

            cumSumZapremine = new Dictionary<int, Dictionary<string, float>>();

            CreateKoricaSheet(wb,doc);
            var tablice = doc?.Tablice?.OrderBy(t => t.Vrsta).ToList() ?? new List<Tablica>();
            int sheetIndex = 1;
            for (int i = 0; i < tablice.Count; i += 3)
            {
                var group = tablice.Skip(i).Take(3).ToList();
                CreateInnerTabliceSheet(wb, group, sheetIndex++);
            }
            CreateRekapitulacijaSheet(wb, doc);


            wb.SaveAs(path);
        }

        private static void CreateKoricaSheet(XLWorkbook wb,DokumentDoznaka dokument)
        {
            var ws = wb.Worksheets.Add("Korica");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 1);
            ws.PageSetup.CenterHorizontally = true;
            ws.PageSetup.CenterVertically = true;
            ws.PageSetup.Scale = 100;
            ws.PageSetup.Margins.Top = 0.25;
            ws.PageSetup.Margins.Bottom = 0.25;
            ws.PageSetup.Margins.Left = 0.35;
            ws.PageSetup.Margins.Right = 0.3;

            // Define a wide area to work with
            for (int c = 1; c <= 8; c++) ws.Column(c).Width = 11;
            for (int r = 1; r <= 40; r++) ws.Row(r).Height = 20;

            ws.Column(1).Width = 14;

            // Outer border like the picture
            var outer = ws.Range(1, 1, 40, 8);
            outer.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Title
            var title = ws.Range(2, 1, 2, 8);
            title.Merge().Value = "DOZNAČNA KNJIGA ZA ŠUME U DRŽAVNOJ SVOJINI";
            title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            title.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            title.Style.Font.Bold = true;
            title.Style.Font.FontSize = 16;

            // PREDUZECE label and input
            var preduzeceLabel = ws.Range(5, 2, 5, 7);
            preduzeceLabel.Merge().Value = "PREDUZEĆE";
            preduzeceLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            preduzeceLabel.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            preduzeceLabel.Style.Font.Bold = true;

 

            var preduzeceInput = ws.Range(6, 2, 6, 7);
            preduzeceInput.Merge();
            preduzeceInput.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            preduzeceInput.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            preduzeceInput.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
           

            // GAZDINSKA JEDINICA label and input
            var gazLabel = ws.Range(8, 2, 8, 7);
            gazLabel.Merge().Value = "GAZDINSKA JEDINICA";
            gazLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            gazLabel.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            gazLabel.Style.Font.Bold = true;
           

            var gazInput = ws.Range(9, 2, 9, 7);
            gazInput.Merge();
            gazInput.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            gazInput.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            gazInput.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            gazInput.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            gazInput.Value = dokument.GazJedinica;

            // Odeljenje and Odsek with underline
            ws.Cell(12, 1).Value = "ODELJENJE";
            var odel = ws.Range(12, 2, 12, 3);
            odel.Merge();
            odel.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            odel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            odel.Value = dokument.BrOdeljenja;

            ws.Cell(12, 5).Value = "ODSEK";
            var odsek = ws.Range(12, 6, 12, 7);
            odsek.Merge();
            odsek.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            odsek.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            odsek.Value = dokument.Odsek;

            // Povrsina
            ws.Cell(14, 1).Value = "POVRŠINA (ha)";
            var pov = ws.Range(14, 2, 14, 3);
            pov.Merge();
            pov.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            pov.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            pov.Value = dokument.PovrsinaDoznake;


            // Central lines for book number and date
            var book = ws.Range(18, 2, 18, 7);
            book.Merge().Value = "DOZNAČNA KNJIGA BROJ ________ ZA 20__ . GODINU";
            book.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var period = ws.Range(21, 1, 21, 8);
            period.Merge().Value = "DOZNAKU IZVRŠIO U VREMENU OD ________ DO ________ 20__ . GODINE";
            period.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Name/Prezime fields bottom-left
            var nameLabel = ws.Range(35, 1, 35, 3);
            nameLabel.Merge().Value = "IME I PREZIME";
            nameLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            nameLabel.Style.Font.Bold = true;

            var name = ws.Range(36, 1, 36, 3);
            name.Merge();
            name.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            name.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            name.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            name.Value = dokument.Doznacar;

            

            ws.Rows().AdjustToContents();

            // Set font to Times New Roman
            ws.Style.Font.FontName = "Times New Roman";
            ws.Style.Font.FontSize = 12;
        }

      


        private static void CreateInnerTabliceSheet(XLWorkbook wb, List<Tablica> tablice, int sheetIndex)
        {
            var ws = wb.Worksheets.Add($"Tablice (unutra){(sheetIndex > 1 ? $" {sheetIndex}" : "")}");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 1);
            ws.PageSetup.CenterHorizontally = true;
            ws.PageSetup.CenterVertically = true;
            ws.PageSetup.Margins.Top = 0.12;
            ws.PageSetup.Margins.Bottom = 0.12;
            ws.PageSetup.Margins.Left = 0.12;
            ws.PageSetup.Margins.Right = 0.12;

            // =====================================================
            // COLUMN WIDTHS
            // =====================================================

            ws.Column(1).Width = 5.0; // No
            ws.Column(2).Width = 7.0; // cm

            // 3 bloka × 3 kolone
            // [Vrsta drveta] [Broj stab.] [Zapremina]

            for (int c = 3; c <= 11; c++)
            {
                int mod = (c - 3) % 3;

                if (mod == 0)
                    ws.Column(c).Width = 10.0;
                else if (mod == 1)
                    ws.Column(c).Width = 6.5;
                else
                    ws.Column(c).Width = 6.5;
            }

            // =====================================================
            // ROW HEIGHTS
            // =====================================================

            for (int r = 1; r <= 45; r++)
                ws.Row(r).Height = 19;

            ws.Row(1).Height = 28;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 26;
            ws.Row(4).Height = 24;
            ws.Row(5).Height = 30;

            // =====================================================
            // TOP BOXES
            // =====================================================

            ws.Range("A1:C1").Merge().Value = "GJ";
            ws.Range("D1:E1").Merge().Value = "Odeljenje";
            ws.Range("F1:G1").Merge().Value = "Odsek";
            ws.Range("H1:I1").Merge().Value = "Namenska\ncelina";
            ws.Range("J1:J1").Value = "Gazd. klasa";
            ws.Range("K1:K1").Value = "Površina\n(ha)";

            ws.Range("A2:C2").Merge();
            ws.Range("D2:E2").Merge();
            ws.Range("F2:G2").Merge();
            ws.Range("H2:I2").Merge();
            ws.Range("J2:J2").Merge();
            ws.Range("K2:K2").Merge();

            var top = ws.Range("A1:K2");

            top.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            top.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            top.Style.Alignment.WrapText = true;
            top.Style.Font.Bold = true;
            top.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            top.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // =====================================================
            // MAIN HEADER
            // =====================================================

            ws.Range("A3:B4").Merge().Value = "DEBLJ.\nSTEPEN";

            ws.Range("C3:K3").Merge().Value =
                "VRSTA DRVETA I BONITET ILI TARIFNI NIZ";

            ws.Range("A3:K4").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Range("A3:K4").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            ws.Range("A3:K4").Style.Font.Bold = true;
            ws.Range("A3:K4").Style.Alignment.WrapText = true;

            // =====================================================
            // SUBHEADERS
            // =====================================================

            ws.Cell("A5").Value = "No";
            ws.Cell("B5").Value = "cm";

            int col = 3;

            for (int i = 0; i < 3; i++)
            {
                ws.Cell(5, col).Value = "";
                ws.Cell(5, col + 1).Value = "Broj\nstab.";
                ws.Cell(5, col + 2).Value = "Zapr.\n(m3)";

                col += 3;
            }

            ws.Range("A5:K5").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Range("A5:K5").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            ws.Range("A5:K5").Style.Alignment.WrapText = true;
            ws.Range("A5:K5").Style.Font.Bold = true;

            FillInnerTabliceData(ws, tablice);

            // =====================================================
            // GRID
            // =====================================================

            int startRow = 6;

            var diameterValues = new[]
            {
                "7,5", "12,5", "17,5", "22,5", "27,5",
                "32,5", "37,5", "42,5", "47,5",
                "52,5", "57,5", "62,5", "67,5",
                "72,5", "77,5", "82,5", "87,5",
                "92,5", "97,5"
            };

            int no = 1;

            // I grupa
            for (int i = 0; i < 5; i++)
            {
                int r = startRow + i;

                ws.Cell(r, 1).Value = no++;
                ws.Cell(r, 2).Value = diameterValues[i];
            }

            ws.Range($"A{startRow + 5}:B{startRow + 5}")
                .Merge()
                .Value = "I";
            ws.Range($"A{startRow + 6}:B{startRow + 6}")
                .Merge()
                .Value = "11 - 30";

            ws.Range($"A{startRow + 5}:B{startRow + 5}")
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range($"A{startRow + 6}:B{startRow + 6}")
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // II grupa
            for (int i = 5; i < 9; i++)
            {
                int r = startRow + 2 + i;

                ws.Cell(r, 1).Value = no++;
                ws.Cell(r, 2).Value = diameterValues[i];
            }

            ws.Range($"A{startRow + 11}:B{startRow + 11}")
                .Merge()
                .Value = "II";
            ws.Range($"A{startRow + 12}:B{startRow + 12}")
                .Merge()
                .Value = "31 - 50";

            ws.Range($"A{startRow + 11}:B{startRow + 11}")
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range($"A{startRow + 12}:B{startRow + 12}")
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // III grupa
            for (int i = 9; i < diameterValues.Length; i++)
            {
                int r = startRow + 4 + i;

                ws.Cell(r, 1).Value = no++;
                ws.Cell(r, 2).Value = diameterValues[i];
            }

            ws.Range($"A{startRow + 23}:B{startRow + 23}")
                .Merge()
                .Value = "III";
            ws.Range($"A{startRow + 24}:B{startRow + 24}")
                .Merge()
                .Value = "51 +";

            ws.Range($"A{startRow + 23}:B{startRow + 23}")
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range($"A{startRow + 24}:B{startRow + 24}")
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // =====================================================
            // COMPLETE TABLE BORDERING
            // =====================================================

            var full = ws.Range("A1:K30");

            full.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            full.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            full.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range("A5:A28").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // =====================================================
            // FONT
            // =====================================================

            ws.Style.Font.FontName = "Times New Roman";
            ws.Style.Font.FontSize = 10;
        }

        private static void FillInnerTabliceData(IXLWorksheet ws, List<Tablica> tablice)
        {

            MeasurementService _service = new MeasurementService(new Repository.TarifeRepository());

            ws.Style.Font.FontName = "Times New Roman";
            ws.Style.Font.FontSize = 11;
            ws.Style.Font.Bold = true;

            if (tablice == null || !tablice.Any())
                return;

            var diameterValues = new[]
            {
                7.5f, 12.5f, 17.5f, 22.5f, 27.5f,
                32.5f, 37.5f, 42.5f, 47.5f,
                52.5f, 57.5f, 62.5f, 67.5f,
                72.5f, 77.5f, 82.5f, 87.5f,
                92.5f, 97.5f
            };

            for (int groupIndex = 0; groupIndex < tablice.Count; groupIndex++)
            {
                int colBase = 3 + groupIndex * 3;
                var tablica = tablice[groupIndex];

                var dataMap = tablica.DebStepeni?
                    .GroupBy(ds => Math.Round(ds.DebStepen, 1))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Kolicina))
                    ?? new Dictionary<double, int>();

                string vrstaName;
                if (Enum.IsDefined(typeof(VrstaDrvo), tablica.Vrsta))
                {
                    vrstaName = ((VrstaDrvo)tablica.Vrsta).GetDescription();
                }
                else
                {
                    vrstaName = $"Vrsta {tablica.Vrsta}";
                }

                ws.Cell(4, colBase).Value = $"{vrstaName}/{tablica.Tarifa}";
                ws.Range(4, colBase, 4, colBase + 2).Merge();
                ws.Range(4, colBase, 4, colBase + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(4, colBase, 4, colBase + 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(4, colBase, 4, colBase + 2).Style.Alignment.WrapText = true;
                ws.Range(4, colBase, 4, colBase + 2).Style.Font.Bold = true;


                int row = 5;
                for (int i = 0; i < diameterValues.Length; i++)
                {

                    row += 1;
                    float diameter = diameterValues[i];

                    if (dataMap.TryGetValue(diameter, out int count))
                    {
                        ws.Cell(row, colBase + 1).Value = count == 0 ? "" : count;
                        ws.Cell(row, colBase + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        if (count > 0)
                        {
                            float zapremina = _service.GetZapremina(
                                    tablica.Tarifa,
                                    tablica.TarifniNiz,
                                    (int)diameter,
                                    diameter
                            );

                            AddToCumSum(tablica.Vrsta, zapremina * count, diameter);

                            //AKo imamo kolicinu onda dodaje zapreminu hehe 
                            ws.Cell(row, colBase + 2).Value = count * zapremina;
                            ws.Cell(row, colBase + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }


                        // If you also have volume per deb stepen,
                        // add a property such as DebStepeni.Zapremina and write it here:
                        // ws.Cell(row, colBase + 2).Value = tablica.DebStepeni
                        //     .Where(ds => Math.Abs(ds.DebStepen - diameter) < 0.1)
                        //     .Sum(ds => ds.Zapremina);
                    }


                    //Kumulisemo deb stepene kolicine\
                    //Ako diam = 27.5 onda 11-30
                    //Ako diam = 47.5 onda 31-50
                    //Ako diam = 97.5 onda 51+
                    int idx = 0;
                    switch (diameter)
                    {
                        case 27.5f:
                            idx = 1;
                            break;
                        case 47.5f:
                            idx = 2;
                            break;
                        case 97.5f:
                            idx = 3;
                            break;
                        default:
                            break;
                    }

                    if (idx > 0)
                    {
                        row += 2;
                        int debStepen = CumSumDebStepen(idx, tablica);
                        ws.Cell(row, colBase + 1).Value = debStepen == 0 ? "" : debStepen;
                        ws.Cell(row, colBase + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                }

            }
        }

        private static void AddToCumSum(int vrsta, float zapremina, float diameter)
        {
            // 1. Safe "Get or Add" check for the 'vrsta' key
            if (!cumSumZapremine.TryGetValue(vrsta, out Dictionary<string, float>? current))
            {
                current = new Dictionary<string, float>();
                cumSumZapremine[vrsta] = current;
            }

            // 2. Determine the correct dictionary key based on diameter
            string? key = diameter switch
            {
                <= 30 => "<30",
                <= 50 => "31-50",  // covers 30 < d <= 50
                _ => ">50"     // covers everything above 50
            };

            current.TryGetValue(key, out float existingVolume);
            current[key] = existingVolume + zapremina;
        }




        private static int CumSumDebStepen(int idx, Tablica tablica)
        {
            switch (idx)
            {
                case 1:
                    return tablica.DebStepeni.Where(x => x.DebStepen < 30).Sum(x => x.Kolicina);
                case 2:
                    return tablica.DebStepeni.Where(x => x.DebStepen > 30 && x.DebStepen <= 50).Sum(x => x.Kolicina);
                case 3:
                    return tablica.DebStepeni.Where(x => x.DebStepen > 51).Sum(x => x.Kolicina);
                default:
                    return 0;
            }

        }

        private static void CreateRekapitulacijaSheet(XLWorkbook wb, DokumentDoznaka dokument)
        {
            var ws = wb.Worksheets.Add("Rekapitulacija");

            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 1);
            ws.PageSetup.CenterHorizontally = true;
            ws.PageSetup.CenterVertically = true;
            ws.PageSetup.Margins.Top = 0.12;
            ws.PageSetup.Margins.Bottom = 0.12;
            ws.PageSetup.Margins.Left = 0.12;
            ws.PageSetup.Margins.Right = 0.12;

            // =====================================================
            // COLUMN WIDTHS
            // =====================================================

            ws.Column(1).Width = 8;
            ws.Column(2).Width = 10.0;
            ws.Column(3).Width = 14;

            for (int c = 4; c <= 11; c++)
                ws.Column(c).Width = 5.5;

            ws.Column(12).Width = 7.0;
            ws.Column(13).Width = 9.0;

            // =====================================================
            // ROW HEIGHTS
            // =====================================================

            for (int r = 1; r <= 40; r++)
                ws.Row(r).Height = 20;

            ws.Row(1).Height = 28;
            ws.Row(2).Height = 26;
            ws.Row(3).Height = 20;
            ws.Row(4).Height = 38;
            ws.Row(5).Height = 20;

            // =====================================================
            // TITLE
            // =====================================================

            ws.Range("A1:M1").Merge().Value =
                "REKAPITULACIJA DOZNAČENE DRVNE ZAPREMINE";

            ws.Range("A1:M1").Style.Font.Bold = true;
            ws.Range("A1:M1").Style.Font.FontSize = 14;
            ws.Range("A1:M1").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // =====================================================
            // HEADER BLOCKS
            // =====================================================

            ws.Range("A2:A4").Merge().Value = "Odeljenje";

            ws.Range("B2:B3").Merge().Value =
                "Odsek/Sast.\nUzgojna grupa";

            ws.Range("B4:B4").Merge().Value =
                "Gazdinska klasa / Nam. celina";

            ws.Range("C2:C4").Merge().Value = "Vrsta drveta";

            ws.Range("D2:K2").Merge().Value = "Vrsta prinosa (m3)";

            ws.Range("D3:G3").Merge().Value = "GLAVNI";
            ws.Range("H3:K3").Merge().Value = "PRETHODNI";

            ws.Range("D4:K4").Merge().Value = "Po grupama debljinskih razreda (u cm)";

            ws.Cell("D5").Value = "do 30";
            ws.Cell("E5").Value = "31-50";
            ws.Cell("F5").Value = ">50";
            ws.Cell("G5").Value = "Svega";

            ws.Cell("H5").Value = "do 30";
            ws.Cell("I5").Value = "31-50";
            ws.Cell("J5").Value = ">50";
            ws.Cell("K5").Value = "Svega";

            ws.Range("L2:L5").Merge().Value = "UKUPNO\n(7+11)";
            ws.Range("M2:M5").Merge().Value = "Način seče";

            // =====================================================
            // ALIGNMENT
            // =====================================================

            ws.Range("A2:M5").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Range("A2:M5").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            ws.Range("A2:M5").Style.Alignment.WrapText = true;

            ws.Range("A2:M5").Style.Font.Bold = true;

            ws.Range("B2:B3").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
            ws.Range("B4:B5").Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Top;

            // =====================================================
            // COLUMN NUMBERS ROW
            // =====================================================

            for (int i = 1; i <= 13; i++)
            {
                ws.Cell(6, i).Value = i;

                ws.Cell(6, i).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(6, i).Style.Font.Bold = true;
            }

            // =====================================================
            // DATA GRID
            // =====================================================

            int startData = 7;
            int rows = 28;

            var dataRange = ws.Range(startData, 1, startData + rows, 13);

            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // =====================================================
            // OUTER HEADER BORDER
            // =====================================================

            var header = ws.Range("A2:M6");

            header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // =====================================================
            // FONT
            // =====================================================

            ws.Style.Font.FontName = "Times New Roman";
            ws.Style.Font.FontSize = 10;

            FillRekapitulacijaSheet(ws, dokument);
        }


        public static void FillRekapitulacijaSheet(IXLWorksheet ws, DokumentDoznaka dokument)
        {
            int startRow = 7;

            int baseCol = 1;

            ws.Cell(startRow, baseCol).Value = dokument.BrOdeljenja;
            ws.Cell(startRow, baseCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


            ws.Cell(startRow, baseCol + 1).Value = $"{dokument.Odsek}/";

           

            for (int i = 0; i < dokument.Tablice.Count; i++)
            {
                string vrstaName;

                Tablica tablica = dokument.Tablice[i];

                if (Enum.IsDefined(typeof(VrstaDrvo), tablica.Vrsta))
                {
                    vrstaName = ((VrstaDrvo)tablica.Vrsta).GetDescription();
                }
                else
                {
                    vrstaName = $"Vrsta {tablica.Vrsta}";
                }
                ws.Cell(startRow + i, baseCol + 2).Value = vrstaName;
                ws.Cell(startRow + i, baseCol + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(startRow + i, baseCol + 2).Style.Font.Italic = true;


                //gleadmo dal je prethodni ili glavni
                int startZapreminaColumn = dokument.VrstaPrinosa == 1 ? 2 : 6;

                //Uzimamo po vrsti recnik <string,float>
                // key su 1. <30 , 2. 31-50, 3. >50 i to stringovno reprezentovani
                //value su ukupne zapremine za te raspone

                if (!cumSumZapremine.TryGetValue(tablica.Vrsta, out Dictionary<string, float>? cumSum))
                    continue;

                //null safe checkujemo cisto radi reda da ne bi bilo errora
                float zapManje30 = cumSum?.GetValueOrDefault("<30") ?? 0f;
                float zap31to50 = cumSum?.GetValueOrDefault("31-50") ?? 0f;
                float zapVise50 = cumSum?.GetValueOrDefault(">50") ?? 0f;

                float totalVolume = cumSum?.Sum(x => x.Value) ?? 0f;


                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 1).Value = Math.Round(zapManje30, 2);
                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 2).Value = Math.Round(zap31to50, 2);
                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 3).Value = Math.Round(zapVise50,2);
                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 4).Value = Math.Round(totalVolume,2);
                ws.Cell(startRow + i, baseCol + startZapreminaColumn + 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                ws.Cell(startRow + i, baseCol + 11).Value = Math.Round(totalVolume,2);
                ws.Cell(startRow + i, baseCol + 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(startRow + i, baseCol + 11).Style.Font.Bold = true;


                ws.Cell(startRow + i, baseCol + 12).Value = dokument.VrstaSece;
                ws.Cell(startRow + i, baseCol + 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(startRow + i, baseCol + 12).Style.Font.Bold = true;



            }
        }
    }
}