using System;
using System.IO;
using System.Windows.Forms;

namespace QuoteKeeperGUI
{
    public static class Logger
    {
        private static string logFile = "app.log";
        private static bool enabled = true;

        public static void Enable() { enabled = true; }
        public static void Disable() { enabled = false; }

        public static void Log(string message, string type = "INFO")
        {
            if (!enabled) return;

            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}] {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch { /* тихо падаем, чтобы логирование не ломало программу */ }
        }

        public static void Error(string message, Exception ex = null)
        {
            string errorMsg = message;
            if (ex != null)
                errorMsg += $" | {ex.Message}";
            Log(errorMsg, "ERROR");
        }

        public static void UserAction(string action)
        {
            Log($"Пользователь: {action}", "ACTION");
        }
        public static void SyncToPublicFolder()
        {
            try
            {
                string sourceLog = "app.log";
                string publicFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "QuoteKeeperLogs");

                if (!Directory.Exists(publicFolder))
                    Directory.CreateDirectory(publicFolder);

                string destLog = Path.Combine(publicFolder, $"app_{DateTime.Now:yyyy-MM-dd}.log");

                if (File.Exists(sourceLog))
                    File.Copy(sourceLog, destLog, true);

                Logger.Log("Синхронизация лога в общую папку выполнена", "SYNC");
            }
            catch { }
        }
    }

}
