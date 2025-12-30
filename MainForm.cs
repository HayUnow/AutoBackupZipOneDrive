using AutoBackupZipOneDrive.Core;
using AutoBackupZipOneDrive.Models;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AutoBackupZipOneDrive
{
    public partial class MainForm : Form
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;

        private Thread _thread;
        private BackupWorker _worker;
        private readonly string _webhookFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wx_webhook.txt");//微信webhook
        public MainForm()
        {
            InitializeComponent();
            FormClosing += MainForm_FormClosing; // 可以正常拦截X关闭按钮-然后最小化到托盘的关键语句
            string od = OneDriveHelper.DetectDefaultPath();
            if (!string.IsNullOrEmpty(od))
            {
                txtOneDrive.Text = od;
            }
            else
            {
                MessageBox.Show(
                     this,
                    "未检测到 OneDrive 默认同步目录，请手动选择。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        // ================= 浏览 OneDrive =================
        private void btnBrowseOD_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog f = new FolderBrowserDialog())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    txtOneDrive.Text = f.SelectedPath;
                }
            }
        }

        // ================= UI 锁定 =================
        private void LockUI(bool run)
        {
            txtOneDrive.Enabled = !run;
            btnBrowseOD.Enabled = !run;
            numZipKeep.Enabled = !run;
            txtPath.Enabled = !run;
            dtStart.Enabled = !run;
            numScan.Enabled = !run;
            numStable.Enabled = !run;
            numWindow.Enabled = !run;
            txtPwd.Enabled = !run;
            btnBrowse.Enabled = !run;
            txtWeComWebhook.Enabled = !run;
            btnStart.Enabled = !run;
            btnStop.Enabled = run;
            dtEnd.Enabled = !run;
        }

        // ================= 浏览监控目录 =================
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog f = new FolderBrowserDialog())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = f.SelectedPath;
                }
            }
        }

        // ================= 启动 =================
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_thread != null && _thread.IsAlive)
            {
                MessageBox.Show("程序已在运行中。");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPwd.Text))
            {
                MessageBox.Show(
                                 "请输入压缩密码！",
                                 "提示",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Information
                                );
                return;
            }
            if (dtStart.Value > dtEnd.Value) 
            {
                MessageBox.Show(
                                 "监控开始日期不能晚于结束日期！",
                                 "提示",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Information
                                );
                return;
            }
            AppConfig cfg = new AppConfig
            {
                MonitorPath = txtPath.Text,
                OneDrivePath = txtOneDrive.Text,
                StartDate = dtStart.Value,//监控开始日期
                EndDate = dtEnd.Value,//监控结束日期
                ScanIntervalSeconds = (int)numScan.Value,
                StableSeconds = (int)numStable.Value,
                WindowMinutes = (int)numWindow.Value,
                ZipTempKeepDays = (int)numZipKeep.Value,
                Password = txtPwd.Text
            };
            // 初始化企业微信通知器（即使不使用也初始化以防报错）
            var notifier = new WeComWebhookNotifier(txtWeComWebhook.Text);
            // 检查监控目录是否存在
            if (!Directory.Exists(cfg.MonitorPath))
            {
                MessageBox.Show(
                        "监控目录不存在！",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                       );
                return;

            }
            SaveWebhook();//保存微信webhook
            // ★ 关键：左右两个 UI 输出通道
            _worker = new BackupWorker(
        cfg,
        notifier,
        left =>
        {
            if (!IsDisposed)
                BeginInvoke((MethodInvoker)(() =>
                    txtDetail.Text = left));   // 左：结果
        },
        right =>
        {
            if (!IsDisposed)
                BeginInvoke((MethodInvoker)(() =>
                    txtEvent.Text = right));   // 右：事件
        }
    );

            _thread = new Thread(_worker.Run)
            {
                IsBackground = true
            };
            _thread.Start();

            LockUI(true);
            lblStatus.Text = "● 运行中";
        }

        // ================= 停止 =================
        private void btnStop_Click(object sender, EventArgs e)
        {
           StopProgram();
        }
        private void StopProgram() // ★ 核心：一键立刻停止后台线程-统一封装
        {
            try
            {
                // 1️ 通知 Worker 停止
                if (_worker != null)
                {
                    _worker.Stop();
                }

            }
            catch
            {
                // 忽略停止过程中的异常
            }

            // 2 释放线程 / worker 引用
            _thread = null;
            _worker = null;
            // ===== UI 复位 =====
            LockUI(false);

            lblStatus.Text = "■ 已停止";

            // 清空 UI
            txtDetail.Clear(); // 左侧
            txtEvent.Clear();  // 右侧
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitTray();
            LoadWebhook();//加载微信webhook
            dtStart.Value = DateTime.Now;//默认当前时间
        }
        private void InitTray()
        {
            _trayMenu = new ContextMenuStrip
            {
                ShowItemToolTips = true // ★ 必须开启，ToolTipText 才生效
            };

            // ===== 关于（不可点击）=====
            // ===== 关于（信息型，不点击）=====
            var aboutItem = new ToolStripMenuItem("关于程序")
            {
                Enabled = true, // 控制是否可点击
                Image = Properties.Resources.about // ★ 仅新增
            };
            aboutItem.Click += (s, e) =>
            {
                MessageBox.Show(
                    "程序名称：AutoBackupZipOneDrive\n\n" +
                    "版本：v1.0.0\n" +
                    "作者：Fly Cat & ChatGpt & Gemini\n" +
                    "编译：2025-12-25\n\n" +
                    "说明：\n" +
                    "用于自动监控目录变化，\n" +
                    "打包压缩后同步至 OneDrive。",
                    "关于程序",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
                /*aboutItem.DropDownItems.Add(new ToolStripMenuItem("🧩 版本：v1.0.0") { Enabled = false });
                aboutItem.DropDownItems.Add(new ToolStripMenuItem("👤 作者：Good Lucky") { Enabled = false });
                aboutItem.DropDownItems.Add(new ToolStripMenuItem("⏱ 编译：2025-12-25") { Enabled = false });*/

                _trayMenu.Items.Add(aboutItem);


            // ===== 停止运行（动态可用）=====
            var stopItem = new ToolStripMenuItem("停止运行")
            {
                Image = Properties.Resources.stop// ★ 仅新增
            };
            stopItem.Click += (s, e) =>
            {
                StopProgram();
            };
            _trayMenu.Items.Add(stopItem);

            // ===== 退出程序 =====
            var exitItem = new ToolStripMenuItem("一键退出")
            {
                Image = Properties.Resources.exit // ★ 加图标
            };
            exitItem.Click += (s, e) =>
            {
                ExitApp();
            };
            _trayMenu.Items.Add(exitItem);

            // ★ 菜单弹出前，动态刷新“停止运行”状态
            _trayMenu.Opening += (s, e) =>
            {
                stopItem.Enabled = (_thread != null && _thread.IsAlive);
            };

            // ===== 托盘图标 =====
            _trayIcon = new NotifyIcon
            {
                Icon = this.Icon,
                Text = "后台运行中",
                Visible = true,
                ContextMenuStrip = _trayMenu
            };

            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ShowMainWindow();
                }
            };
        }


        private void ExitApp() // ★ 核心：托盘一键退出程序-统一封装
        {
            try
            {
                // 先停止程序
                StopProgram();
            }
            catch
            {
            }

            // 关闭托盘图标（防止残留）
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            Application.Exit();
        }
        private void ShowMainWindow() // ★ 核心：托盘一键显示主窗口-统一封装
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }
        /*        protected override void OnResize(EventArgs e) // ★ 核心：窗口最小化时隐藏窗口-统一封装
                {
                    base.OnResize(e);

                    // 当窗口被最小化时，隐藏窗口（进入托盘）
                    if (this.WindowState == FormWindowState.Minimized)
                    {
                        this.Hide();
                    }
                }*/
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ✅ 只有【正在运行】时，才拦截关闭 → 最小化到托盘
            if (_thread != null && _thread.IsAlive)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                Hide();
                return;
            }
            // ❌ 未运行：不拦截，正常关闭程序并清理托盘图标
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
            
        }
        // ================= 加载微信 Webhook =================
        private void LoadWebhook()
        {
            try
            {
                if (File.Exists(_webhookFile))
                {
                    txtWeComWebhook.Text =
                        File.ReadAllText(_webhookFile).Trim();
                }
            }
            catch
            {
                // 读取失败直接忽略
            }
        }
        // ================= 保存微信 Webhook =================
        private void SaveWebhook()
        {
            try
            {
                var text = txtWeComWebhook.Text?.Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    File.WriteAllText(_webhookFile, text);
                }
                else
                {
                    // 如果用户清空了，可以选择删除文件
                    if (File.Exists(_webhookFile))
                        File.Delete(_webhookFile);
                }
            }
            catch
            {
                // 写入失败不影响启动
            }
        }
    }
}
