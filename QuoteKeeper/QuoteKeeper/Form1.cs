using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace QuoteKeeperGUI
{
    public partial class Form1 : Form
    {
        private List<Quote> quotes = new List<Quote>();
        private string dataFile = "quotes.txt";
        private ListBox lstQuotes;
        private TextBox txtAuthor, txtBook, txtQuoteText, txtTheme, txtSearch;
        private Button btnAdd, btnViewAll, btnSearchAuthor, btnSearchTheme, btnRandom, btnDelete, btnSave, btnLoad, btnExportJson;
        private Label lblStatus;
        private TabControl tabControl;

        public Form1()
        {
            Logger.Log("Программа запущена", "START");
            InitializeComponent();
            LoadData();
            Logger.Log($"Загружено {quotes.Count} цитат", "LOAD");

            // Подписка на событие закрытия формы для автоэкспорта
            this.FormClosing += Form1_FormClosing;
        }

        private void InitializeComponent()
        {
            this.Text = "QuoteKeeper - Каталог цитат";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl { Dock = DockStyle.Fill };

            // ========== Вкладка: Управление цитатами ==========
            TabPage managePage = new TabPage("Управление цитатами");

            Label lblAuthor = new Label { Text = "Автор:", Location = new System.Drawing.Point(20, 20), Width = 80 };
            txtAuthor = new TextBox { Location = new System.Drawing.Point(110, 20), Width = 250 };

            Label lblBook = new Label { Text = "Книга:", Location = new System.Drawing.Point(20, 50), Width = 80 };
            txtBook = new TextBox { Location = new System.Drawing.Point(110, 50), Width = 250 };

            Label lblQuoteText = new Label { Text = "Цитата:", Location = new System.Drawing.Point(20, 80), Width = 80 };
            txtQuoteText = new TextBox { Location = new System.Drawing.Point(110, 80), Width = 250 };

            Label lblTheme = new Label { Text = "Тема:", Location = new System.Drawing.Point(20, 110), Width = 80 };
            txtTheme = new TextBox { Location = new System.Drawing.Point(110, 110), Width = 250 };

            btnAdd = new Button { Text = "➕ Добавить цитату", Location = new System.Drawing.Point(110, 150), Width = 150, BackColor = System.Drawing.Color.LightGreen };
            btnAdd.Click += BtnAdd_Click!;

            lstQuotes = new ListBox { Location = new System.Drawing.Point(400, 20), Width = 460, Height = 400 };

            managePage.Controls.AddRange(new Control[] { lblAuthor, txtAuthor, lblBook, txtBook, lblQuoteText, txtQuoteText, lblTheme, txtTheme, btnAdd, lstQuotes });

            // ========== Вкладка: Поиск ==========
            TabPage searchPage = new TabPage("Поиск");

            Label lblSearch = new Label { Text = "Поиск:", Location = new System.Drawing.Point(20, 20), Width = 80 };
            txtSearch = new TextBox { Location = new System.Drawing.Point(110, 20), Width = 250 };
            btnSearchAuthor = new Button { Text = "🔍 По автору", Location = new System.Drawing.Point(110, 60), Width = 120, BackColor = System.Drawing.Color.LightBlue };
            btnSearchTheme = new Button { Text = "🔍 По теме", Location = new System.Drawing.Point(240, 60), Width = 120, BackColor = System.Drawing.Color.LightBlue };
            btnSearchAuthor.Click += BtnSearchAuthor_Click!;
            btnSearchTheme.Click += BtnSearchTheme_Click!;

            ListBox lstSearchResults = new ListBox { Location = new System.Drawing.Point(20, 100), Width = 840, Height = 350, Name = "lstSearchResults" };

            searchPage.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearchAuthor, btnSearchTheme, lstSearchResults });

            // ========== Вкладка: Действия ==========
            TabPage actionsPage = new TabPage("Действия");

            btnViewAll = new Button { Text = "📋 Все цитаты", Location = new System.Drawing.Point(20, 20), Width = 200, Height = 50 };
            btnRandom = new Button { Text = "🎲 Цитата дня", Location = new System.Drawing.Point(240, 20), Width = 200, Height = 50 };
            btnDelete = new Button { Text = "❌ Удалить цитату", Location = new System.Drawing.Point(460, 20), Width = 200, Height = 50 };
            btnSave = new Button { Text = "💾 Сохранить", Location = new System.Drawing.Point(20, 90), Width = 200, Height = 50 };
            btnLoad = new Button { Text = "📂 Загрузить", Location = new System.Drawing.Point(240, 90), Width = 200, Height = 50 };
            btnExportJson = new Button { Text = "📤 Экспорт в JSON", Location = new System.Drawing.Point(460, 90), Width = 200, Height = 50 };

            btnViewAll.Click += BtnViewAll_Click!;
            btnRandom.Click += BtnRandom_Click!;
            btnDelete.Click += BtnDelete_Click!;
            btnSave.Click += BtnSave_Click!;
            btnLoad.Click += BtnLoad_Click!;
            btnExportJson.Click += BtnExportJson_Click!;

            actionsPage.Controls.AddRange(new Control[] { btnViewAll, btnRandom, btnDelete, btnSave, btnLoad, btnExportJson });

            // ========== Статусная строка ==========
            lblStatus = new Label { Text = "Готово", Dock = DockStyle.Bottom, Height = 30, BackColor = System.Drawing.Color.LightGray, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

            tabControl.TabPages.Add(managePage);
            tabControl.TabPages.Add(searchPage);
            tabControl.TabPages.Add(actionsPage);

            this.Controls.Add(tabControl);
            this.Controls.Add(lblStatus);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Logger.UserAction($"Попытка добавить цитату: Автор={txtAuthor.Text}, Книга={txtBook.Text}");

            if (string.IsNullOrWhiteSpace(txtAuthor.Text) ||
                string.IsNullOrWhiteSpace(txtBook.Text) ||
                string.IsNullOrWhiteSpace(txtQuoteText.Text) ||
                string.IsNullOrWhiteSpace(txtTheme.Text))
            {
                lblStatus.Text = "❌ Ошибка: все поля должны быть заполнены!";
                Logger.Log("Ошибка добавления: пустые поля", "WARNING");
                return;
            }

            Quote newQuote = new Quote
            {
                Id = quotes.Count > 0 ? quotes.Max(q => q.Id) + 1 : 1,
                Author = txtAuthor.Text.Trim(),
                Book = txtBook.Text.Trim(),
                Text = txtQuoteText.Text.Trim(),
                Theme = txtTheme.Text.Trim()
            };

            quotes.Add(newQuote);
            SaveToFile();
            RefreshQuoteList();
            ClearInputFields();
            lblStatus.Text = $"✅ Цитата добавлена! Всего цитат: {quotes.Count}";
            Logger.Log($"Цитата добавлена: ID={newQuote.Id}, Автор={newQuote.Author}", "ADD");

            // Фоновая интеграция (без изменения визуала)
            BackupHelper.CreateBackup(quotes);
            AutoExportHelper.AutoExport(quotes);
        }

        private void BtnViewAll_Click(object sender, EventArgs e)
        {
            Logger.UserAction("Просмотр всех цитат");
            RefreshQuoteList();
            lblStatus.Text = $"📖 Показано {quotes.Count} цитат";
        }

        private void BtnSearchAuthor_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            Logger.UserAction($"Поиск по автору: {searchTerm}");

            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Введите автора для поиска", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatus.Text = "⚠️ Введите автора для поиска";
                return;
            }

            var results = quotes.Where(q => q.Author.ToLower().Contains(searchTerm.ToLower())).ToList();

            string message = results.Count == 0 ? "Ничего не найдено" : $"Найдено {results.Count} цитат:\n\n";
            foreach (var q in results)
            {
                message += $"📌 {q.Author} — {q.Book}\n💬 {q.Text}\n🏷️ {q.Theme}\n\n";
            }

            MessageBox.Show(message, $"Результаты поиска по автору: {searchTerm}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblStatus.Text = $"🔍 Найдено {results.Count} цитат по автору";
            Logger.Log($"Поиск по автору '{searchTerm}': найдено {results.Count}", "SEARCH");
        }

        private void BtnSearchTheme_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            Logger.UserAction($"Поиск по теме: {searchTerm}");

            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Введите тему для поиска", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatus.Text = "⚠️ Введите тему для поиска";
                return;
            }

            var results = quotes.Where(q => q.Theme.ToLower().Contains(searchTerm.ToLower())).ToList();

            string message = results.Count == 0 ? "Ничего не найдено" : $"Найдено {results.Count} цитат:\n\n";
            foreach (var q in results)
            {
                message += $"📌 {q.Author} — {q.Book}\n💬 {q.Text}\n🏷️ {q.Theme}\n\n";
            }

            MessageBox.Show(message, $"Результаты поиска по теме: {searchTerm}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblStatus.Text = $"🔍 Найдено {results.Count} цитат по теме";
            Logger.Log($"Поиск по теме '{searchTerm}': найдено {results.Count}", "SEARCH");
        }

        private void BtnRandom_Click(object sender, EventArgs e)
        {
            Logger.UserAction("Запрос случайной цитаты");

            if (quotes.Count == 0)
            {
                lblStatus.Text = "⚠️ Нет цитат. Добавьте первую цитату!";
                return;
            }

            Random rnd = new Random();
            Quote randomQuote = quotes[rnd.Next(quotes.Count)];

            MessageBox.Show($"\"{randomQuote.Text}\"\n\n— {randomQuote.Author}, {randomQuote.Book}\nТема: {randomQuote.Theme}",
                          "🎲 Цитата дня",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
            lblStatus.Text = "🎲 Показана случайная цитата";
            Logger.Log($"Показана случайная цитата ID={randomQuote.Id}", "RANDOM");
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (quotes.Count == 0)
            {
                lblStatus.Text = "⚠️ Нет цитат для удаления";
                return;
            }

            if (lstQuotes.SelectedItem == null)
            {
                lblStatus.Text = "⚠️ Выберите цитату для удаления из списка";
                return;
            }

            string selected = lstQuotes.SelectedItem.ToString() ?? "";
            int id = ExtractId(selected);

            if (id == -1)
            {
                lblStatus.Text = "⚠️ Не удалось определить ID цитаты";
                return;
            }

            Quote? quoteToDelete = quotes.FirstOrDefault(q => q.Id == id);
            if (quoteToDelete != null)
            {
                Logger.UserAction($"Удаление цитаты ID={id}");

                DialogResult result = MessageBox.Show($"Удалить цитату:\n\n\"{quoteToDelete.Text}\"\n— {quoteToDelete.Author} ?",
                                            "Подтверждение удаления",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    quotes.Remove(quoteToDelete);
                    SaveToFile();
                    RefreshQuoteList();
                    lblStatus.Text = $"🗑️ Цитата удалена. Осталось {quotes.Count} цитат";
                    Logger.Log($"Цитата удалена: ID={id}", "DELETE");

                    // Фоновая интеграция (без изменения визуала)
                    BackupHelper.CreateBackup(quotes);
                    AutoExportHelper.AutoExport(quotes);
                }
                else
                {
                    Logger.Log($"Удаление отменено: ID={id}", "CANCEL");
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Logger.UserAction("Ручное сохранение");
            SaveToFile();
            lblStatus.Text = "💾 Данные сохранены в файл";
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            Logger.UserAction("Ручная загрузка");
            LoadData();
            RefreshQuoteList();
            lblStatus.Text = "📂 Данные загружены из файла";
        }

        private void BtnExportJson_Click(object sender, EventArgs e)
        {
            Logger.UserAction($"Экспорт в JSON ({quotes.Count} цитат)");

            try
            {
                string jsonFile = "quotes_export.json";
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(quotes, options);
                File.WriteAllText(jsonFile, json);
                lblStatus.Text = $"📤 Экспорт выполнен: {jsonFile}";
                MessageBox.Show($"Экспортировано {quotes.Count} цитат в файл:\n{Path.GetFullPath(jsonFile)}",
                              "Экспорт в JSON",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
                Logger.Log($"Экспорт завершён: {jsonFile}", "EXPORT");
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Ошибка экспорта: {ex.Message}";
                Logger.Error("Ошибка экспорта в JSON", ex);
            }
        }

        private void SaveToFile()
        {
            try
            {
                var lines = quotes.Select(q => $"{q.Author}|{q.Book}|{q.Text}|{q.Theme}");
                File.WriteAllLines(dataFile, lines);
                Logger.Log($"Сохранено {quotes.Count} цитат в файл {dataFile}", "SAVE");
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Ошибка сохранения: {ex.Message}";
                Logger.Error("Ошибка сохранения", ex);
            }
        }

        private void LoadData()
        {
            try
            {
                if (!File.Exists(dataFile))
                {
                    quotes = new List<Quote>();
                    SaveToFile();
                    Logger.Log($"Файл не найден, создан новый: {dataFile}", "INIT");
                    return;
                }

                var lines = File.ReadAllLines(dataFile);
                quotes = new List<Quote>();
                int id = 1;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length == 4)
                    {
                        quotes.Add(new Quote
                        {
                            Id = id++,
                            Author = parts[0],
                            Book = parts[1],
                            Text = parts[2],
                            Theme = parts[3]
                        });
                    }
                }
                Logger.Log($"Загружено {quotes.Count} цитат из {dataFile}", "LOAD");
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Ошибка загрузки: {ex.Message}";
                quotes = new List<Quote>();
                Logger.Error("Ошибка загрузки данных", ex);
            }
        }

        private void RefreshQuoteList()
        {
            lstQuotes.Items.Clear();
            foreach (var q in quotes)
            {
                string preview = q.Text.Length > 40 ? q.Text.Substring(0, 40) + "..." : q.Text;
                lstQuotes.Items.Add($"[{q.Id}] {q.Author} — {q.Book} | {q.Theme} | \"{preview}\"");
            }
        }

        private void ClearInputFields()
        {
            txtAuthor.Clear();
            txtBook.Clear();
            txtQuoteText.Clear();
            txtTheme.Clear();
        }

        private int ExtractId(string selectedText)
        {
            if (string.IsNullOrEmpty(selectedText)) return -1;

            if (selectedText.StartsWith("["))
            {
                int closeBracket = selectedText.IndexOf(']');
                if (closeBracket > 1)
                {
                    string idStr = selectedText.Substring(1, closeBracket - 1);
                    if (int.TryParse(idStr, out int id))
                        return id;
                }
            }
            return -1;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.UserAction("Закрытие программы");
            AutoExportHelper.AutoExport(quotes);
            Logger.SyncToPublicFolder();
            Logger.Log("Программа завершена", "SHUTDOWN");
        }
    }

    public class Quote
    {
        public int Id { get; set; }
        public string Author { get; set; } = "";
        public string Book { get; set; } = "";
        public string Text { get; set; } = "";
        public string Theme { get; set; } = "";
    }
}