
using System.Windows.Forms;

namespace AutoBackupZipOneDrive
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnBrowse;

        private System.Windows.Forms.Label lblOneDrive;
        private System.Windows.Forms.TextBox txtOneDrive;
        private System.Windows.Forms.Button btnBrowseOD;

        private System.Windows.Forms.Label lblZipKeep;
        private System.Windows.Forms.NumericUpDown numZipKeep;

        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.DateTimePicker dtStart;

        private System.Windows.Forms.Label lblScan;
        private System.Windows.Forms.NumericUpDown numScan;
        private System.Windows.Forms.Label lblStable;
        private System.Windows.Forms.NumericUpDown numStable;

        private System.Windows.Forms.Label lblWindow;
        private System.Windows.Forms.NumericUpDown numWindow;
        private System.Windows.Forms.Label lblPwd;
        private System.Windows.Forms.TextBox txtPwd;

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtDetail;
        private System.Windows.Forms.TextBox txtEvent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lblPath = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblOneDrive = new System.Windows.Forms.Label();
            this.txtOneDrive = new System.Windows.Forms.TextBox();
            this.btnBrowseOD = new System.Windows.Forms.Button();
            this.lblZipKeep = new System.Windows.Forms.Label();
            this.numZipKeep = new System.Windows.Forms.NumericUpDown();
            this.lblDate = new System.Windows.Forms.Label();
            this.dtStart = new System.Windows.Forms.DateTimePicker();
            this.lblScan = new System.Windows.Forms.Label();
            this.numScan = new System.Windows.Forms.NumericUpDown();
            this.lblStable = new System.Windows.Forms.Label();
            this.numStable = new System.Windows.Forms.NumericUpDown();
            this.lblWindow = new System.Windows.Forms.Label();
            this.numWindow = new System.Windows.Forms.NumericUpDown();
            this.lblPwd = new System.Windows.Forms.Label();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtDetail = new System.Windows.Forms.TextBox();
            this.txtEvent = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWeComWebhook = new System.Windows.Forms.TextBox();
            this.dtEnd = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.notype = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numZipKeep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWindow)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPath
            // 
            this.lblPath.Location = new System.Drawing.Point(28, 9);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(350, 16);
            this.lblPath.TabIndex = 0;
            this.lblPath.Text = "监测目录：用于监测新增文件的文件夹";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(28, 29);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(360, 21);
            this.txtPath.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(409, 26);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(139, 28);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblOneDrive
            // 
            this.lblOneDrive.Location = new System.Drawing.Point(28, 55);
            this.lblOneDrive.Name = "lblOneDrive";
            this.lblOneDrive.Size = new System.Drawing.Size(350, 16);
            this.lblOneDrive.TabIndex = 3;
            this.lblOneDrive.Text = "OneDrive 同步目录";
            // 
            // txtOneDrive
            // 
            this.txtOneDrive.Location = new System.Drawing.Point(28, 74);
            this.txtOneDrive.Name = "txtOneDrive";
            this.txtOneDrive.Size = new System.Drawing.Size(360, 21);
            this.txtOneDrive.TabIndex = 4;
            // 
            // btnBrowseOD
            // 
            this.btnBrowseOD.Location = new System.Drawing.Point(409, 69);
            this.btnBrowseOD.Name = "btnBrowseOD";
            this.btnBrowseOD.Size = new System.Drawing.Size(139, 28);
            this.btnBrowseOD.TabIndex = 5;
            this.btnBrowseOD.Text = "浏览";
            this.btnBrowseOD.Click += new System.EventHandler(this.btnBrowseOD_Click);
            // 
            // lblZipKeep
            // 
            this.lblZipKeep.Location = new System.Drawing.Point(28, 106);
            this.lblZipKeep.Name = "lblZipKeep";
            this.lblZipKeep.Size = new System.Drawing.Size(200, 16);
            this.lblZipKeep.TabIndex = 6;
            this.lblZipKeep.Text = "ZipTemp临时压缩文件保留天数";
            // 
            // numZipKeep
            // 
            this.numZipKeep.Location = new System.Drawing.Point(28, 126);
            this.numZipKeep.Name = "numZipKeep";
            this.numZipKeep.Size = new System.Drawing.Size(80, 21);
            this.numZipKeep.TabIndex = 7;
            this.numZipKeep.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lblDate
            // 
            this.lblDate.Location = new System.Drawing.Point(28, 156);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(184, 16);
            this.lblDate.TabIndex = 8;
            this.lblDate.Text = "监测开始日期*时间表示每日时段";
            // 
            // dtStart
            // 
            this.dtStart.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dtStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStart.Location = new System.Drawing.Point(28, 176);
            this.dtStart.Name = "dtStart";
            this.dtStart.ShowUpDown = true;
            this.dtStart.Size = new System.Drawing.Size(142, 21);
            this.dtStart.TabIndex = 9;
            this.dtStart.Value = new System.DateTime(2025, 12, 29, 18, 7, 27, 0);
            // 
            // lblScan
            // 
            this.lblScan.Location = new System.Drawing.Point(28, 206);
            this.lblScan.Name = "lblScan";
            this.lblScan.Size = new System.Drawing.Size(160, 16);
            this.lblScan.TabIndex = 10;
            this.lblScan.Text = "监测目录扫描间隔（秒）";
            // 
            // numScan
            // 
            this.numScan.Location = new System.Drawing.Point(28, 226);
            this.numScan.Name = "numScan";
            this.numScan.Size = new System.Drawing.Size(168, 21);
            this.numScan.TabIndex = 11;
            this.numScan.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // lblStable
            // 
            this.lblStable.Location = new System.Drawing.Point(218, 206);
            this.lblStable.Name = "lblStable";
            this.lblStable.Size = new System.Drawing.Size(210, 16);
            this.lblStable.TabIndex = 12;
            this.lblStable.Text = "新增文件稳定时间（秒）";
            // 
            // numStable
            // 
            this.numStable.Location = new System.Drawing.Point(220, 226);
            this.numStable.Name = "numStable";
            this.numStable.Size = new System.Drawing.Size(168, 21);
            this.numStable.TabIndex = 13;
            this.numStable.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lblWindow
            // 
            this.lblWindow.Location = new System.Drawing.Point(28, 256);
            this.lblWindow.Name = "lblWindow";
            this.lblWindow.Size = new System.Drawing.Size(160, 16);
            this.lblWindow.TabIndex = 14;
            this.lblWindow.Text = "新增文件监测周期（分钟）";
            // 
            // numWindow
            // 
            this.numWindow.Location = new System.Drawing.Point(28, 276);
            this.numWindow.Name = "numWindow";
            this.numWindow.Size = new System.Drawing.Size(168, 21);
            this.numWindow.TabIndex = 15;
            this.numWindow.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // lblPwd
            // 
            this.lblPwd.Location = new System.Drawing.Point(218, 256);
            this.lblPwd.Name = "lblPwd";
            this.lblPwd.Size = new System.Drawing.Size(160, 16);
            this.lblPwd.TabIndex = 16;
            this.lblPwd.Text = "压缩包密码（必填）";
            // 
            // txtPwd
            // 
            this.txtPwd.Location = new System.Drawing.Point(220, 276);
            this.txtPwd.Name = "txtPwd";
            this.txtPwd.PasswordChar = '●';
            this.txtPwd.Size = new System.Drawing.Size(168, 21);
            this.txtPwd.TabIndex = 17;
            this.txtPwd.Text = "233";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(28, 304);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 32);
            this.btnStart.TabIndex = 18;
            this.btnStart.Text = "开始运行";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(220, 304);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 32);
            this.btnStop.TabIndex = 19;
            this.btnStop.Text = "停止";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(28, 339);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(400, 20);
            this.lblStatus.TabIndex = 20;
            this.lblStatus.Text = "● 就绪";
            // 
            // txtDetail
            // 
            this.txtDetail.Location = new System.Drawing.Point(28, 359);
            this.txtDetail.MaxLength = 33000;
            this.txtDetail.Multiline = true;
            this.txtDetail.Name = "txtDetail";
            this.txtDetail.ReadOnly = true;
            this.txtDetail.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetail.Size = new System.Drawing.Size(520, 222);
            this.txtDetail.TabIndex = 21;
            // 
            // txtEvent
            // 
            this.txtEvent.Location = new System.Drawing.Point(28, 587);
            this.txtEvent.Multiline = true;
            this.txtEvent.Name = "txtEvent";
            this.txtEvent.ReadOnly = true;
            this.txtEvent.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtEvent.Size = new System.Drawing.Size(520, 161);
            this.txtEvent.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(218, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 16);
            this.label1.TabIndex = 23;
            this.label1.Text = "Webhook(微信、钉钉、TG)";
            // 
            // txtWeComWebhook
            // 
            this.txtWeComWebhook.Location = new System.Drawing.Point(220, 125);
            this.txtWeComWebhook.Name = "txtWeComWebhook";
            this.txtWeComWebhook.PasswordChar = '●';
            this.txtWeComWebhook.Size = new System.Drawing.Size(168, 21);
            this.txtWeComWebhook.TabIndex = 24;
            this.txtWeComWebhook.Text = "233";
            // 
            // dtEnd
            // 
            this.dtEnd.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dtEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtEnd.Location = new System.Drawing.Point(220, 176);
            this.dtEnd.Name = "dtEnd";
            this.dtEnd.ShowUpDown = true;
            this.dtEnd.Size = new System.Drawing.Size(142, 21);
            this.dtEnd.TabIndex = 25;
            this.dtEnd.Value = new System.DateTime(2099, 12, 31, 23, 59, 59, 0);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(218, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(190, 16);
            this.label2.TabIndex = 26;
            this.label2.Text = "监测结束日期*时间表示每日时段";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(407, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 16);
            this.label3.TabIndex = 28;
            this.label3.Text = "通知类型（自动识别）";
            // 
            // notype
            // 
            this.notype.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.notype.ForeColor = System.Drawing.SystemColors.WindowText;
            this.notype.Location = new System.Drawing.Point(409, 126);
            this.notype.Name = "notype";
            this.notype.ReadOnly = true;
            this.notype.Size = new System.Drawing.Size(139, 23);
            this.notype.TabIndex = 29;
            this.notype.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(574, 761);
            this.Controls.Add(this.notype);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtEnd);
            this.Controls.Add(this.dtStart);
            this.Controls.Add(this.txtWeComWebhook);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblPwd);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblOneDrive);
            this.Controls.Add(this.txtOneDrive);
            this.Controls.Add(this.btnBrowseOD);
            this.Controls.Add(this.lblZipKeep);
            this.Controls.Add(this.numZipKeep);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.lblScan);
            this.Controls.Add(this.numScan);
            this.Controls.Add(this.lblStable);
            this.Controls.Add(this.numStable);
            this.Controls.Add(this.lblWindow);
            this.Controls.Add(this.numWindow);
            this.Controls.Add(this.txtPwd);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtDetail);
            this.Controls.Add(this.txtEvent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自动打包文件并上传至OneDrive";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numZipKeep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWindow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Label label1;
        private TextBox txtWeComWebhook;
        private DateTimePicker dtEnd;
        private Label label2;
        private Label label3;
        private TextBox notype;
    }
}
