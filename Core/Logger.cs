
using System;
using System.IO;

namespace AutoBackupZipOneDrive.Core
{
    public static class Logger
    {
        static readonly string Dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        static readonly string FilePath = Path.Combine(Dir, "backup.log");

        public static void Write(string msg)
        {
            Directory.CreateDirectory(Dir);
            File.AppendAllText(FilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
        }
    }
}
