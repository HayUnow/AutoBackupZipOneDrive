using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace AutoBackupZipOneDrive.Core
{
    public static class OneDriveHelper
    {
        public static string DetectDefaultPath()
        {
            // ① 常见注册表路径
            string path = ReadReg(@"Software\Microsoft\OneDrive", "UserFolder");
            if (Directory.Exists(path)) return path;

            // ② 商业版 / 新版本
            path = ReadReg(@"Software\Microsoft\OneDrive\Accounts\Business1", "UserFolder");
            if (Directory.Exists(path)) return path;

            // ③ 环境变量兜底
            string env = Environment.GetEnvironmentVariable("OneDrive");
            if (Directory.Exists(env)) return env;

            return null;
        }

        private static string ReadReg(string key, string name)
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(key))
                {
                    return k != null ? k.GetValue(name) as string : null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool IsRunning()
        {
            return Process.GetProcessesByName("OneDrive").Length > 0;
        }
    }
}
