using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AutoBackupZipOneDrive.Core
{
    public static class OneDriveSyncHelper
    {
        // ====================== ① 等待 OneDrive 上传完成 ======================
        public static OneDriveUploadResult WaitUploadFinished(
            string file,
            int timeoutSeconds,
            Action<string> status)
        {
            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalSeconds < timeoutSeconds)
            {
                if (!File.Exists(file))
                {
                    status?.Invoke("❌ 文件不存在，本次上传已终止。");
                    return OneDriveUploadResult.FileNotExists;
                }
                Thread.Sleep(60000);//每60秒检查一次

                string availability = GetOneDriveAvailabilityText(file);

                if (string.IsNullOrEmpty(availability))
                {
                    status?.Invoke("⏳ 等待 OneDrive 同步状态确认…");
                    continue;
                }

                // 正在上传
                if (availability.Contains("同步"))
                {
                    status?.Invoke("⏳ OneDrive 正在上传（同步挂起）…");
                    continue;
                }

                // 已上传完成（不关心是否释放）
                if (availability.Contains("在此设备上可用"))
                {
                    status?.Invoke("☁ OneDrive 上传完成（在此设备上可用）");
                    return OneDriveUploadResult.Success;
                }

                if (availability.Contains("联机"))
                {
                    status?.Invoke("☁ OneDrive 已上传完成并释放本地文件成功（仅联机）。");
                    return OneDriveUploadResult.Success;
                }

                status?.Invoke("⏳ 等待 OneDrive 上传状态确认…");
            }

            status?.Invoke("⏳ OneDrive 上传时间较长，请耐心等待上传完成…");
            return OneDriveUploadResult.Timeout;
        }

        // ====================== ② 等待文件进入“仅云端” ======================
        // ⚠️ 这个阶段是“释放验证”，不是上传判定
        public static bool WaitForCloudOnly(
            string file,
            int timeoutSeconds,
            int retrySeconds,
            Action<string> status)
        {
            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalSeconds < timeoutSeconds)
            {
                if (!File.Exists(file))
                    return false;

                string availability = GetOneDriveAvailabilityText(file);

                if (!string.IsNullOrEmpty(availability) &&
                    availability.Contains("联机"))
                {
                    status?.Invoke(
                        "☁ OneDrive 本次上传文件和释放空间成功完成（仅云端）。"
                        + Environment.NewLine
                        + "===========================================================================");
                    return true;
                }

                status?.Invoke("⏳ 等待 OneDrive 释放本地文件（每5秒刷新一次）…");
                Thread.Sleep(Math.Max(retrySeconds, 5) * 1000);
            }

            status?.Invoke(
                "❌ 本轮-等待 OneDrive 释放本地超时。"
                + Environment.NewLine
                + "===========================================================================");
            return false;
        }

        // ====================== ③ 请求 OneDrive 释放本地文件 ======================
        // ✔ 内部重试 3 次
        // ✔ 不再抛异常
        // ✔ 失败返回 false
        public static bool ReleaseLocal(string file, Action<string> status)
        {
            const int maxRetry = 3;

            for (int i = 1; i <= maxRetry; i++)
            {
                try
                {
                    status?.Invoke($"📤 请求释放本地文件（第 {i} 次）...");

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c attrib +U -P \"" + file + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });

                    // 命令成功发出即可返回 true
                    return true;
                }
                catch (Exception ex)
                {
                    status?.Invoke($"⚠️ 第 {i} 次释放失败：{ex.Message}");

                    if (i < maxRetry)
                        Thread.Sleep(3000);
                }
            }

            status?.Invoke("❌ 已连续 3 次尝试释放本地文件失败，放弃本次处理");
            return false;
        }
        // ====================== 核心：直接读取 Explorer 显示文本 ======================
        private static string GetOneDriveAvailabilityText(string file)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(file);
                string fileName = Path.GetFileName(file);

                Type shellType = Type.GetTypeFromProgID("Shell.Application");
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic folder = shell.NameSpace(folderPath);
                if (folder == null)
                    return null;

                dynamic item = folder.ParseName(fileName);
                if (item == null)
                    return null;

                int availabilityIndex = -1;

                // 查找“可用性状态”列（与测试程序一致）
                for (int i = 0; i < 500; i++)
                {
                    string header = folder.GetDetailsOf(null, i);
                    if (header == "可用性状态")
                    {
                        availabilityIndex = i;
                        break;
                    }
                }

                if (availabilityIndex < 0)
                    return null;

                return folder.GetDetailsOf(item, availabilityIndex);
            }
            catch
            {
                return null;
            }
        }
        // ====================== 上传结果枚举 ======================
        public enum OneDriveUploadResult
        {
            Success,        // 上传完成
            FileNotExists,  // 文件不存在
            Timeout         // 等待超时
        }
    }
}
