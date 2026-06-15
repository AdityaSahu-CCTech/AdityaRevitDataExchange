using System;
using System.IO;

namespace AdityaRevitDataExchange.Services
{
    public static class Logger
    {
        private static readonly object LockObj = new object();
        private static readonly string LogDir = Path.Combine("C:", "Log");
        private static readonly string LogFilePath = Path.Combine(LogDir, "AdityaRevitDataExchange.log");

        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public static void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            var fullMessage = ex == null ? message : $"{message} - Exception: {ex}\n{ex?.StackTrace}";
            WriteLog("ERROR", fullMessage);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (LockObj)
                {
                    if (!Directory.Exists(LogDir))
                    {
                        Directory.CreateDirectory(LogDir);
                    }

                    var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
                    File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
                }
            }
            catch
            {
                // Swallow any logging exceptions to avoid impacting host application
            }
        }
    }
}
