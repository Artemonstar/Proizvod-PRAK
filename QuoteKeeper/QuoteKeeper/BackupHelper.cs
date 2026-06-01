using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QuoteKeeperGUI
{
    public static class BackupHelper
    {
        private static string backupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QuoteKeeperBackup");
        private static string backupFile = "quotes_backup.txt";

        static BackupHelper()
        {
            // Создаём папку для бэкапов, если её нет
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
        }

        public static void CreateBackup(List<Quote> quotes)
        {
            try
            {
                string fullPath = Path.Combine(backupFolder, backupFile);
                var lines = quotes.Select(q => $"{q.Author}|{q.Book}|{q.Text}|{q.Theme}");
                File.WriteAllLines(fullPath, lines);

                // Дополнительно: сохраняем с датой (для истории)
                string datedBackup = Path.Combine(backupFolder, $"quotes_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
                File.WriteAllLines(datedBackup, lines);

                // Удаляем старые бэкапы (старше 7 дней)
                CleanOldBackups();
            }
            catch (Exception ex)
            {
                // Тихо падаем, чтобы не мешать пользователю
                Logger.Error("Ошибка резервного копирования", ex);
            }
        }

        private static void CleanOldBackups()
        {
            try
            {
                var oldFiles = Directory.GetFiles(backupFolder, "quotes_*.txt")
                    .Where(f => File.GetCreationTime(f) < DateTime.Now.AddDays(-7));
                foreach (var file in oldFiles)
                {
                    File.Delete(file);
                }
            }
            catch { }
        }

        public static bool TryRestoreLatestBackup(out List<Quote> quotes)
        {
            quotes = new List<Quote>();
            try
            {
                string fullPath = Path.Combine(backupFolder, backupFile);
                if (!File.Exists(fullPath)) return false;

                var lines = File.ReadAllLines(fullPath);
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
                return quotes.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}