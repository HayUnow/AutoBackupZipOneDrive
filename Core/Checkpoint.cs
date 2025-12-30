
using System;
using System.IO;

namespace AutoBackupZipOneDrive.Core
{
    public static class Checkpoint
    {
        public static DateTime Load(string file, DateTime fallback)
        {
            if (!File.Exists(file))
                return fallback;

            try
            {
                string text = File.ReadAllText(file).Trim();
                if (string.IsNullOrEmpty(text))
                    return fallback;

                return DateTime.Parse(text);
            }
            catch
            {
                // ½âÎöÊ§°Ü£¬ºöÂÔ checkpoint
                return fallback;
            }
        }

        public static void Save(string file, DateTime dt)
        {
            File.WriteAllText(file, dt.ToString("O"));
        }
    }
}
