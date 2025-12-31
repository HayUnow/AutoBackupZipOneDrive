using AutoBackupZipOneDrive.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static AutoBackupZipOneDrive.Core.OneDriveSyncHelper;
using AutoBackupZipOneDrive.Notify;

namespace AutoBackupZipOneDrive.Core
{
    public class BackupWorker
    {
        private readonly AppConfig _cfg;
        private bool _run = true;

        private readonly string _cpFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "checkpoint.txt");
        private DateTime _checkpoint;

        private DateTime _windowStart;
        private bool _hasWindow = false;

        private readonly Dictionary<string, FileCandidate> _files =
            new Dictionary<string, FileCandidate>();


        // ================= UI 缓存 =================
        private string _resultText = "";
        private readonly List<string> _eventLog = new List<string>();
        private readonly List<string> _lastPackedFiles = new List<string>();

        private readonly Action<string> _detailLeft;
        private readonly Action<string> _detailRight;
        // ================= OneDrive 启动环境检测 =================
        private bool _envChecked = false;
        private string _envHeaderText = "";
        private string _oneDriveStatusText = "未知";
        // ================= 当前处理开始时间 =================
        private DateTime _processStartTime;
        // 用于安全中断扫描等待
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);
        // ================= 确认阶段状态机 =================
        private enum ConfirmStage
        {
            Writing,
            StableRound1,
            StableRound2,
            PackConfirmRound1,
            PackConfirmRound2,
            Ready
        }

        private readonly Dictionary<string, ConfirmStage> _stages =
            new Dictionary<string, ConfirmStage>();

        private readonly Dictionary<string, DateTime> _stageSince =
            new Dictionary<string, DateTime>();
        private readonly INotifyChannel _notifier;// Webhook通知通道
        public BackupWorker(
            AppConfig cfg,
           INotifyChannel notifier,// Webhook通知通道
            Action<string> detailLeft,
            Action<string> detailRight)
        {
            _cfg = cfg;
            _notifier = notifier;// Webhook通知通道
            _detailLeft = detailLeft;
            _detailRight = detailRight;
            _checkpoint = Checkpoint.Load(_cpFile, cfg.StartDate);
        }
        // 停止运行
        public void Stop()
        {
            _run = false;
            _waitHandle.Set(); // 唤醒等待中的线程
        }

        public void Run()
        {
            string sevenZipPath;
            if (!SevenZip.TryGet(out sevenZipPath))
            {
                AddEvent("未检测到 7-Zip");
                return;
            }

            Directory.CreateDirectory("ZipTemp");
            AddEvent("新增文件监测中");
            // ===== 主运行循环逻辑 =====
            while (_run)
            {
                try
                {
                    DateTime now = DateTime.Now;// 当前时间点-运行周期起点
                    // ================= 启动时 OneDrive 环境检测（只执行一次，不阻断流程） =================
                    if (!_envChecked) // 仅执行一次 ! 标识取反
                    {
                        List<string> envLines = new List<string>();
                        envLines.Add("【环境检测】");

                        if (string.IsNullOrWhiteSpace(_cfg.OneDrivePath))
                        {
                            _oneDriveStatusText = "未配置（仅打包，不同步）。";
                            envLines.Add("OneDrive：未配置（仅打包，不同步）。");
                        }
                        else if (!OneDriveHelper.IsRunning())
                        {
                            _oneDriveStatusText = "未运行（仅打包，不同步）。";
                            envLines.Add("OneDrive：未运行（仅打包，不同步）。");
                        }
                        else
                        {
                            _oneDriveStatusText = "客户端运行正常。";
                            envLines.Add("OneDrive：运行正常。");
                        }

                        envLines.Add("----------------------------------------------------------------------");
                        envLines.Add("");// 空行分隔

                        _envHeaderText = string.Join(Environment.NewLine, envLines);

                        _resultText = _envHeaderText ;
                        RefreshUI();

                        _envChecked = true;
                    }
                    // ===== 时间范围控制：只控制扫描与后续业务 =====
                    //正常一轮结束后_hasWindow 会被重置为 false, 这里判断 if true 进入下一轮前先判断时间范围
                    // 尚未进入处理窗口(_hasWindow=false)：严格按照配置的时间范围扫描-一旦false=true即进入处理窗口，此处不再判断时间范围
                    if (!_hasWindow)
                    {
                        // ===== 第一层：完整 DateTime 判断（不截取）=====
                        if (now < _cfg.StartDate || now > _cfg.EndDate)
                        {
                            _waitHandle.WaitOne(_cfg.ScanIntervalSeconds * 1000);
                            continue;
                        }

                        // ===== 第二层：每天的时间段判断（只比较 HH:mm:ss）=====
                        TimeSpan nowTime = now.TimeOfDay;
                        TimeSpan startTime = _cfg.StartDate.TimeOfDay;
                        TimeSpan endTime = _cfg.EndDate.TimeOfDay;

                        if (nowTime < startTime || nowTime > endTime)
                        {
                            _waitHandle.WaitOne(_cfg.ScanIntervalSeconds * 1000);
                            continue;
                        }
                    }
                    //最后的打包时间和起始时间取最大值，防止重复打包已经处理的文件
                    DateTime effectiveStart =
                        _checkpoint > _cfg.StartDate ? _checkpoint : _cfg.StartDate;

                    // 计算窗口截止时间（只在窗口已开始时才有意义）
                    DateTime windowDeadline = _hasWindow
                        ? _windowStart.AddMinutes(_cfg.WindowMinutes)
                        : DateTime.MaxValue;
                    // ===== 是否允许“继续收集新文件”的硬判断 =====
                    List<FileInfo> infos = Directory.GetFiles(_cfg.MonitorPath)
                        .Select(f => new FileInfo(f))
                        .Where(f =>
                            f.LastWriteTime > effectiveStart &&
                            f.LastWriteTime <= now && // 禁止未来时间文件
                            f.LastWriteTime <= windowDeadline // 文件时间不能晚于窗口截止时间
                        )
                        .ToList();

                    if (!_hasWindow && infos.Count > 0)
                    {
                        //  记录“程序发现第一个新文件”的时间（真正的起点）
                        _processStartTime = DateTime.Now;
                        _windowStart = now;   // ← 发现文件开始计算周期时间
                        _hasWindow = true;
                        // 企业微信通知兼容 Kuma 状态变更通知
                        _notifier.Notify(
                            DateTime.Now.ToString("HH:mm:ss") + "\n" +
                            "【开始】" + "\n" +
                            "OneDrive：" + _oneDriveStatusText + "\n" +
                            "检测到有新文件，已进入自动化处理流程…"
                        );
                    }

                    foreach (FileInfo fi in infos)
                    {
                        FileCandidate fc;
                        if (!_files.TryGetValue(fi.FullName, out fc))
                        {
                            fc = new FileCandidate
                            {
                                Path = fi.FullName,
                                Size = fi.Length,
                                WriteTime = fi.LastWriteTime,
                                IsStable = false,
                                StateText = "文件写入中"
                            };

                            _files[fi.FullName] = fc;
                            _stages[fi.FullName] = ConfirmStage.Writing;
                            _stageSince[fi.FullName] = now;

                            AddEvent("发现新文件：" + Path.GetFileName(fi.FullName));
                            AddEvent("文件写入中：" + Path.GetFileName(fi.FullName));
                            continue;
                        }

                        // ===== 不稳定：写入中（每轮刷事件）=====
                        if (fi.Length != fc.Size ||
                            fi.LastWriteTime != fc.WriteTime ||
                            IsFileLocked(fc.Path))
                        {
                            fc.Size = fi.Length;
                            fc.WriteTime = fi.LastWriteTime;
                            fc.IsStable = false;

                            _stages[fc.Path] = ConfirmStage.Writing;
                            _stageSince[fc.Path] = now;

                            AddEvent("文件写入中：" + Path.GetFileName(fc.Path));
                            fc.StateText = "文件写入中";
                            continue;
                        }
                        // ===== 仍处于写入阶段：每轮扫描都刷“写入中”事件 =====
                        ConfirmStage currentStage;
                        if (_stages.TryGetValue(fc.Path, out currentStage) &&
                            currentStage == ConfirmStage.Writing)
                        {
                            AddEvent("文件写入中：" + Path.GetFileName(fc.Path));
                        }


                        // ===== 稳定阶段推进 =====
                        ConfirmStage stage = _stages[fc.Path];
                        DateTime since = _stageSince[fc.Path];
                        double elapsed = (now - since).TotalSeconds;

                        if (elapsed >= _cfg.StableSeconds)
                        {
                            switch (stage)
                            {
                                case ConfirmStage.Writing:
                                    stage = ConfirmStage.StableRound1;
                                    AddEvent("稳定确认（1/2）：" + Path.GetFileName(fc.Path));
                                    fc.StateText = "文件稳定，等待确认";
                                    break;

                                case ConfirmStage.StableRound1:
                                    stage = ConfirmStage.StableRound2;
                                    AddEvent("稳定确认（2/2）：" + Path.GetFileName(fc.Path));
                                    fc.StateText = "文件稳定，等待确认";
                                    break;

                                case ConfirmStage.StableRound2:
                                    stage = ConfirmStage.PackConfirmRound1;
                                    AddEvent("打包前确认（1/2）：" + Path.GetFileName(fc.Path));
                                    fc.StateText = "文件稳定，等待确认";
                                    break;

                                case ConfirmStage.PackConfirmRound1:
                                    stage = ConfirmStage.PackConfirmRound2;
                                    AddEvent("打包前确认（2/2）：" + Path.GetFileName(fc.Path));
                                    fc.StateText = "文件状态已经稳定-等待打包";
                                    break;

                                case ConfirmStage.PackConfirmRound2:
                                    stage = ConfirmStage.Ready;
                                    fc.IsStable = true;
                                    fc.StateText = "文件状态已经稳定-等待打包";
                                    break;
                            }

                            _stages[fc.Path] = stage;
                            _stageSince[fc.Path] = now;
                        }
                    }

                    // ===== 左侧结果区刷新 =====
                    if (_hasWindow)
                    {
                        double remainMin =
                            (_windowStart.AddMinutes(_cfg.WindowMinutes) - now).TotalMinutes;
                        if (remainMin < 0) remainMin = 0;

                        List<string> lines = new List<string>();
                        lines.Add("新增文件剩余监测时间约 " + Math.Ceiling(remainMin) + " 分钟");
                        lines.Add("新增文件：" + _files.Count + " 个");
                        int i = 1;
                        foreach (var f in _files.Values)
                        {
                            lines.Add(i++ + ". " +
                                Path.GetFileName(f.Path) +
                                "（" + f.StateText + "）");
                        }

                        _resultText =
                              (_envHeaderText ?? "") +
                              string.Join(Environment.NewLine, lines);
                              RefreshUI();
                    }

                    bool windowExpired =
                        _hasWindow &&
                        now >= _windowStart.AddMinutes(_cfg.WindowMinutes);

                    bool allStable =
                        _files.Any() &&
                        _files.Values.All(f => f.IsStable);
                    // ===== 等待打包阶段：用扫描周期刷新右侧 UI =====
                    if (_hasWindow && !windowExpired && allStable)
                    {
                        TimeSpan remain =
                            _windowStart.AddMinutes(_cfg.WindowMinutes) - now;

                        if (remain < TimeSpan.Zero)
                            remain = TimeSpan.Zero;

                        AddEvent($"⏳ 等待打包中，剩余 {remain.Minutes}分{remain.Seconds}秒");
                    }
                    // ===== 打包ZIP并同步Onedrive =====
                    if (windowExpired && allStable)
                    {
                        string out7z = Path.Combine(
                            "ZipTemp",
                            "Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".7z");

                        _lastPackedFiles.Clear();
                        string[] packFiles = _files.Values.Select(f =>
                        {
                            _lastPackedFiles.Add(Path.GetFileName(f.Path));
                            return f.Path;
                        }).ToArray();

                        if (Zip7Service.Create(sevenZipPath, out7z, _cfg.Password, packFiles))
                        {
                            _checkpoint = windowDeadline;
                            Checkpoint.Save(_cpFile, _checkpoint);// 本轮窗口结束时间作为 checkpoint

                            AddEvent("本次新增文件打包完成：" + Path.GetFileName(out7z));
                            //本地打包日志
                            LogZipSuccess(out7z, _cfg.Password);
                            // 企业微信通知
                            // 构建按行显示的文件列表
                            string files = string.Join(
                                Environment.NewLine,
                                _lastPackedFiles.Select((f, i) => $"{i + 1}. {f}")
                            );

                            // 发送企业微信【打包】通知兼容 Kuma 状态变更通知
                            _notifier.Notify(
                                $"{DateTime.Now:HH:mm:ss}\n" +
                                "【打包】\n" +
                                $"本次共 {_lastPackedFiles.Count} 个文件打包成功。\n" +
                                "文件列表：\n" +
                                files + "\n" +
                                $"压缩包：{Path.GetFileName(out7z)}"
                            );
                            // ===== 左侧结果区：本次打包成功结果（必须保留）=====
                            _resultText +=
                                Environment.NewLine +
                                "----------------------------------------------------------------------" +
                                Environment.NewLine +
                                BuildSuccess(out7z);

                            // ===== OneDrive 同步流程（完整保留）=====
                            string oneDriveResult= "⚠ OneDrive本轮上传超时（2小时），未能完成上传！";
                            if (string.IsNullOrWhiteSpace(_cfg.OneDrivePath))
                            {
                                oneDriveResult = "未安装 OneDrive，同步跳过。";
                                AddEvent("⚠ 未安装 OneDrive，同步跳过。");
                            }
                            else if (!OneDriveHelper.IsRunning())
                            {
                                oneDriveResult = "OneDrive 未运行，同步跳过。";
                                AddEvent("⚠ OneDrive 未运行，同步跳过。");
                            }
                            else
                            {
                                try
                                {
                                    string target =
                                        Path.Combine(_cfg.OneDrivePath, Path.GetFileName(out7z));

                                    File.Move(out7z, target);
                                    AddEvent("☁ 压缩文件已移动到OneDrive指定目录，开始上传（每分钟刷新一次结果）…");

                                    // ===== 上传阶段：完全由同步状态决定，超时2小时后判定失败 =====
                                    int retryCount = 0;
                                    const int MAX_RETRIES = 12; // 12 × 10 分钟 = 2 小时
                                    int uploadFailed = 2; //0=文件不存在，1=超时
                                    while (true)
                                    {
                                        var result  = OneDriveSyncHelper.WaitUploadFinished(
                                            target,
                                            600, // 每次最多等10分钟，600秒
                                            msg => AddEvent(msg)
                                        );

                                        if (result== OneDriveUploadResult.Success)
                                        {
                                            break; // ✅ 上传成功，正常往下走
                                        }
                                        if (result == OneDriveUploadResult.FileNotExists)
                                        {
                                            uploadFailed = 0;// ❌ 文件不存在
                                            break; // ❌ 致命错误，不能重试
                                        }
                                        retryCount++;

                                        if (retryCount >= MAX_RETRIES)
                                        {
                                            uploadFailed = 1;// ❌ 上传超时
                                            break; // ⚠️ 只跳出上传等待循环
                                        }

                                        AddEvent($"⏳ OneDrive 仍在上传中，第 {retryCount}/{MAX_RETRIES} 次等待…");
                                    }

                                    // ===== 上传完成后，进入释放阶段 =====
                                    if (uploadFailed==1) // 上传超时
                                    {
                                        oneDriveResult = "❌ OneDrive 上传超时（已等待 2 小时），本轮处理已安全终止。";
                                        AddEvent(oneDriveResult);
                                    } else if (uploadFailed==0) // 文件不存在
                                    {
                                        oneDriveResult = "❌ 文件不存在，OneDrive 上传被中断。";
                                        AddEvent(oneDriveResult);
                                    }
                                    else
                                    {
                                        bool releaseOk =
                                        OneDriveSyncHelper.ReleaseLocal(target, msg => AddEvent(msg));

                                        if (!releaseOk)
                                        {
                                            oneDriveResult = "OneDrive 释放本地失败（已重试 3 次）。";
                                            AddEvent("❌ OneDrive 释放本地失败，终止释放流程。");
                                        }
                                        else
                                        {
                                            AddEvent("🧹请求成功，开始释放本地文件...");

                                            bool cloudOnly =
                                            OneDriveSyncHelper.WaitForCloudOnly(
                                                target,
                                                300,
                                                5,
                                                msg => AddEvent(msg)
                                            );

                                            oneDriveResult = cloudOnly
                                                ? "✔ OneDrive 本次上传文件和释放空间成功完成（仅云端）。"
                                                : "? OneDrive 已上传，但未成功释放本地。";

                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    oneDriveResult = "OneDrive 同步异常。";
                                    AddEvent("❌ OneDrive 同步异常。");
                                }
                            }

                            _resultText +=
                                Environment.NewLine +
                                "----------------------------------------------------------------------" +
                                Environment.NewLine +
                                "【OneDrive 同步结果】" +
                                Environment.NewLine +
                                oneDriveResult;

                            RefreshUI();
                            // ===== 企业微信通知：本轮处理最终完成 =====
                            Thread.Sleep(1000);//稍作等待，确保UI刷新完成，同时防止通知乱序
                            TimeSpan costTime = TimeSpan.Zero;
                            // 只要本轮真正开始过，就计算耗时
                            if (_processStartTime != DateTime.MinValue)
                            {
                                costTime = DateTime.Now - _processStartTime;
                            }

                            string costText =
                                costTime.Hours + "小时" +
                                costTime.Minutes + "分" +
                                costTime.Seconds + "秒";

                            _notifier.Notify(
                                DateTime.Now.ToString("HH:mm:ss") + "\n" +
                                "【结束】\n" +
                                "OneDrive：" + oneDriveResult + "\n" +
                                "本次自动化处理已结束。\n" +
                                "共计耗时：" + costText
                            );

                            // 清除状态，准备下一轮
                            _files.Clear();
                            _stages.Clear();
                            _stageSince.Clear();
                            _hasWindow = false;
                            _processStartTime = DateTime.MinValue;// 重置处理开始时间
                        }
                        else
                        {
                            _resultText = "❌ 打包失败。";
                            AddEvent("打包失败。");
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddEvent("运行异常:"+ex.ToString());
                }

                CleanupZipTemp();
                // 扫描周期间隔
                _waitHandle.WaitOne(_cfg.ScanIntervalSeconds * 1000);
            }
        }

        private static bool IsFileLocked(string file)
        {
            try
            {
                using (new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                return false;
            }
            catch { return true; }
        }

        private void CleanupZipTemp()
        {
            if (_cfg.ZipTempKeepDays <= 0) return;
            if (!Directory.Exists("ZipTemp")) return;

            foreach (string f in Directory.GetFiles("ZipTemp"))
            {
                try
                {
                    if (File.GetLastWriteTime(f) <
                        DateTime.Now.AddDays(-_cfg.ZipTempKeepDays))
                        File.Delete(f);
                }
                catch { }
            }
        }

        private void AddEvent(string text)
        {
            string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text;
            _eventLog.Add(line);

            while (_eventLog.Count > 12)//日志区只保留最近12行
                _eventLog.RemoveAt(0);

            RefreshUI();
        }

        private void RefreshUI()
        {
            _detailLeft(_resultText);
            _detailRight(string.Join(Environment.NewLine, _eventLog));
        }

        // ===== 构建本次打包成功结果文本 =====
        private string BuildSuccess(string zip)
        {
            List<string> lines = new List<string>();
            lines.Add("本次成功打包的文件：");

            int i = 1;
            foreach (string name in _lastPackedFiles)
                lines.Add(i++ + ". " + name);

            lines.Add("");
            lines.Add("本次成功生成的压缩包：");
            lines.Add(Path.GetFileName(zip));

            return string.Join(Environment.NewLine, lines);
        }
        private void LogZipSuccess(string zipFile, string password)
        {
            string logPath =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup_zip.log");

            string line =
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                " | ZIP=" + Path.GetFileName(zipFile) +
                " | PASSWORD=" + password;

            File.AppendAllText(logPath, line + Environment.NewLine);
        }//打包日志

    }
}
