using System;
using System.IO;

namespace ThumbnailPreviewer.Infrastructure
{
    internal static class ThumbnailLogger
    {
        private static readonly string LogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ThumbnailPreviewer", "Logs");

        private static readonly object Lock = new object();
        private static bool _enabled;
        private static bool _initialized;

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            // Enable logging if the log directory exists (user opted in)
            // or if a debug flag file exists
            var flagFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ThumbnailPreviewer", "debug.flag");
            _enabled = File.Exists(flagFile);

            if (_enabled && !Directory.Exists(LogDir))
            {
                try { Directory.CreateDirectory(LogDir); }
                catch { _enabled = false; }
            }
        }

        public static void Info(string handler, string message)
        {
            Log("INFO", handler, message);
        }

        public static void Debug(string handler, string message)
        {
            Log("DEBUG", handler, message);
        }

        public static void Error(string handler, string message, Exception ex = null)
        {
            var msg = ex != null ? $"{message}: {ex.GetType().Name}: {ex.Message}" : message;
            Log("ERROR", handler, msg);
        }

        private static void Log(string level, string handler, string message)
        {
            EnsureInitialized();
            if (!_enabled) return;

            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [{handler}] {message}";
                var logFile = Path.Combine(LogDir, $"thumbnail_{DateTime.Now:yyyyMMdd}.log");

                lock (Lock)
                {
                    File.AppendAllText(logFile, line + Environment.NewLine);
                }
            }
            catch
            {
                // Never throw from logging
            }
        }
    }
}
