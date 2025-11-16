using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeddyBearExport.Services;
using System.Data.OleDb;
using System.Text.RegularExpressions;


namespace TeddyBearExport
{
    public partial class MainWindow : Window
    {
        private readonly WoodometerPdfService _pdfService = new WoodometerPdfService();
        private readonly DoznakaPdfService _doznakaPdfService = new DoznakaPdfService();
        private string selectedFilePath = string.Empty;
        private string _templatePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnJsonToPdf_Click(object sender, RoutedEventArgs e)
        {
            btnJsonToPdf.ContextMenu.IsOpen = true;
        }

        private async void Button_Woodometer_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select a JSON file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                var dokument = _pdfService.ReadJson(selectedFilePath);

                if (dokument != null)
                {
                    MessageBox.Show("Dokument uspešno učitan!\nPokreće se konverzija u PDF...");

                    await _pdfService.ConvertToPdfAsync(selectedFilePath);

                    MessageBox.Show($"Uspešno prebacivanje dokumenta u PDF!\n Dokument se nalazi u {_templatePath}\\KrugoviPDF folderu!");
                }
                else
                {
                    MessageBox.Show("Neuspešno učitavanje dokumenta!");
                }
            }
        }


        private async void Button_Doznaka_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select a JSON file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                var dokument = _doznakaPdfService.ReadJson(selectedFilePath);

                if (dokument != null)
                {
                    MessageBox.Show("Dokument uspešno učitan!\nPokreće se konverzija u PDF...");

                    await _doznakaPdfService.ConvertToPdfAsync(selectedFilePath);

                    MessageBox.Show($"Uspešno prebacivanje dokumenta u PDF!\n Dokument se nalazi u {_templatePath}\\DoznakaPDF folderu!");
                }
                else
                {
                    MessageBox.Show("Neuspešno učitavanje dokumenta!");
                }
            }
        }



        private void RunTransfer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1️⃣ Choose MDB file
                var dbDialog = new OpenFileDialog
                {
                    Filter = "Access Databases (*.mdb)|*.mdb",
                    Title = "Izaberite bazu Osnova (mdb)"
                };
                if (dbDialog.ShowDialog() != true)
                {
                    MessageBox.Show("Niste izabrali bazu.");
                    return;
                }

                string mdbPath = dbDialog.FileName;

                // 2️⃣ Choose one or more text files
                var txtDialog = new OpenFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    Title = "Izaberite jedan ili više tekstualnih fajlova iz OSS aplikacije",
                    Multiselect = true
                };
                if (txtDialog.ShowDialog() != true)
                {
                    MessageBox.Show("Niste izabrali tekstualne fajlove.");
                    return;
                }

                // 3️⃣ Connect to Access .mdb
                string connString = $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={mdbPath};";
                using var conn = new OleDbConnection(connString);
                conn.Open();

                // 4️⃣ Loop through each selected text file
                foreach (var txtPath in txtDialog.FileNames)
                {
                    string sqlContent = File.ReadAllText(txtPath, Encoding.UTF8);

                    // Clean & split SQL commands
                    var statements = sqlContent
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                        .Split(';')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();

                    // 5️⃣ Execute SQL statements for this file
                    foreach (var sqlRaw in statements)
                    {
                        string sql = FixReservedWords(sqlRaw);   // <-- 🟩 AUTOMATSKA ISPRAVKA

                        try
                        {
                            using var cmd = new OleDbCommand(sql, conn);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Greška u upitu:\n{sql}\n\n{ex.Message}",
                                "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    MessageBox.Show($"✅ Fajl '{System.IO.Path.GetFileName(txtPath)}' uspešno prenesen!");
                }

                conn.Close();
                MessageBox.Show("Svi fajlovi su uspešno preneseni u bazu!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Greška: {ex.Message}");
            }
    }
    
    private string FixReservedWords(string sql)
        {
            // --- UPDATE SET No = X ---
            sql = Regex.Replace(sql, @"SET\s+No\s*=", "SET [No] =", RegexOptions.IgnoreCase);

            // --- , No = X ---
            sql = Regex.Replace(sql, @",\s*No\s*=", ", [No] =", RegexOptions.IgnoreCase);

            // --- INSERT INTO (No, ---
            sql = Regex.Replace(sql, @"\(\s*No\s*,", "([No],", RegexOptions.IgnoreCase);

            // --- , No, ---
            sql = Regex.Replace(sql, @",\s*No\s*,", ", [No],", RegexOptions.IgnoreCase);

            // --- , No) ---
            sql = Regex.Replace(sql, @",\s*No\s*\)", ", [No])", RegexOptions.IgnoreCase);

            return sql;
        }
    }
}