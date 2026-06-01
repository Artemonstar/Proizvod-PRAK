using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace QuoteKeeperGUI
{
    public static class AutoExportHelper
    {
        private static string autoExportFile = "quotes_auto_export.json";

        public static void AutoExport(List<Quote> quotes)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(quotes, options);
                File.WriteAllText(autoExportFile, json);
                Logger.Log($"Автоэкспорт выполнен: {quotes.Count} цитат", "AUTOEXPORT");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка автоэкспорта", ex);
            }
        }
    }
}