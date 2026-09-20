using System;
using System.IO;

namespace AfterIdler.Services
{
    public static class LogService
    {
        private static readonly string LogFile =
            Path.Combine(AppContext.BaseDirectory, "AfterIdler.log");

        public static void Write(string message)
        {
            try
            {
                File.AppendAllText(
                    LogFile,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
            catch
            {
                // ログ失敗でアプリを止めない
            }
        }

        public static void Clear()
        {
            try
            {
                File.WriteAllText(LogFile, "");
            }
            catch
            {
            }
        }
    }
}