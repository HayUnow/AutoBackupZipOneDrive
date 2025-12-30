
using System.IO;

namespace AutoBackupZipOneDrive.Core
{
    public static class SevenZip
    {
        public static bool TryGet(out string path)
        {
            path = @"C:\Program Files\7-Zip\7z.exe";
            return File.Exists(path);
        }
    }
}
