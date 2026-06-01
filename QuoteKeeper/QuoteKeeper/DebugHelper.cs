using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QuoteKeeperGUI
{
    public static class DebugHelper
    {
        private static bool debugMode = false;

        public static void EnableDebug() { debugMode = true; }
        public static void DisableDebug() { debugMode = false; }
        public static bool IsDebugEnabled() { return debugMode; }

        public static void ShowDebugInfo(string title, string info)
        {
            if (debugMode)
            {
                MessageBox.Show(info, $"[DEBUG] {title}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void ShowQuoteListDebug(List<Quote> quotes)
        {
            if (!debugMode) return;

            string info = $"Всего цитат: {quotes.Count}\n\n";
            foreach (var q in quotes)
            {
                info += $"[{q.Id}] {q.Author} - {q.Book} ({q.Theme})\n";
                info += $"    Текст: {q.Text.Substring(0, Math.Min(30, q.Text.Length))}...\n\n";
            }
            MessageBox.Show(info, "[DEBUG] Список цитат", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}