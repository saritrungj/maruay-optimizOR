using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace optimizOR.UI
{
    partial class Form1
    {
        private IContainer components = null;
        private Panel pnlHeader;
        private LightningMark pnlBrandMark;
        private Panel pnlMain;
        private Panel pnlActions;
        private Panel pnlLog;
        private Panel grpPriority;
        private Panel grpLatency;
        private Panel grpMemory;
        private Panel grpIniTweaks;
        private Panel grpDebloat;
        private TabControl tabDebloat;
        private TabPage tabWin10Debloat;
        private TabPage tabWin11Debloat;
        private FlowLayoutPanel flpWin10Debloat;
        private FlowLayoutPanel flpWin11Debloat;
        private CheckBox chkWinPriority;
        private CheckBox chkMMCSS_GamePriority;
        private CheckBox chkMMCSS_HighCategory;
        private CheckBox chkMMCSS_GPUPriority;
        private CheckBox chkMMCSS_Responsiveness;
        private CheckBox chkNetworkThrottle;
        private CheckBox chkZeroTimeSlice;
        private CheckBox chkDynamicTick;
        private CheckBox chkDisablePaging;
        private CheckBox chkLargeSystemCache;
        private CheckBox chkMitigations;
        private CheckBox chkIniIRQ;
        private CheckBox chkIniMinSPs;
        private CheckBox chkIniWinLoad;
        private CheckBox chkDebloatConsumerApps;
        private CheckBox chkDebloatXbox;
        private CheckBox chkDebloatTeamsChat;
        private CheckBox chkDebloatWidgets;
        private CheckBox chkDebloatSuggestions;
        private CheckBox chkDebloatOneDriveStartup;
        private Button btnInfoWinPriority;
        private Button btnInfoMMCSS_GamePriority;
        private Button btnInfoMMCSS_HighCategory;
        private Button btnInfoMMCSS_GPUPriority;
        private Button btnInfoMMCSS_Responsiveness;
        private Button btnInfoNetworkThrottle;
        private Button btnInfoZeroTimeSlice;
        private Button btnInfoDynamicTick;
        private Button btnInfoDisablePaging;
        private Button btnInfoLargeSystemCache;
        private Button btnInfoMitigations;
        private Button btnInfoIniIRQ;
        private Button btnInfoIniMinSPs;
        private Button btnInfoIniWinLoad;
        private Button btnInfoDebloatConsumerApps;
        private Button btnInfoDebloatXbox;
        private Button btnInfoDebloatTeamsChat;
        private Button btnInfoDebloatWidgets;
        private Button btnInfoDebloatSuggestions;
        private Button btnInfoDebloatOneDriveStartup;
        private Button btnApplyAll;
        private Button btnRestoreDefaults;
        private Button btnSelectAll;
        private Button btnClearLog;
        private RichTextBox rtbLog;
        private Label lblAppTitle;
        private Label lblSubtitle;
        private Label lblLanguage;
        private ComboBox cmbLanguage;
        private Label lblTheme;
        private CheckBox chkTheme;
        private Label lblLogTitle;
        private ProgressBar prgApply;
        private ToolTip tipMain;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.pnlHeader = new Panel();
            this.pnlBrandMark = new LightningMark();
            this.lblAppTitle = new Label();
            this.lblSubtitle = new Label();
            this.lblLanguage = new Label();
            this.cmbLanguage = new ComboBox();
            this.lblTheme = new Label();
            this.chkTheme = new ToggleSwitch();
            this.pnlMain = new Panel();
            this.grpPriority = new Panel();
            this.chkWinPriority = new ToggleSwitch();
            this.chkMMCSS_GamePriority = new ToggleSwitch();
            this.chkMMCSS_HighCategory = new ToggleSwitch();
            this.chkMMCSS_GPUPriority = new ToggleSwitch();
            this.chkMMCSS_Responsiveness = new ToggleSwitch();
            this.btnInfoWinPriority = new RoundedButton();
            this.btnInfoMMCSS_GamePriority = new RoundedButton();
            this.btnInfoMMCSS_HighCategory = new RoundedButton();
            this.btnInfoMMCSS_GPUPriority = new RoundedButton();
            this.btnInfoMMCSS_Responsiveness = new RoundedButton();
            this.grpLatency = new Panel();
            this.chkNetworkThrottle = new ToggleSwitch();
            this.chkZeroTimeSlice = new ToggleSwitch();
            this.chkDynamicTick = new ToggleSwitch();
            this.btnInfoNetworkThrottle = new RoundedButton();
            this.btnInfoZeroTimeSlice = new RoundedButton();
            this.btnInfoDynamicTick = new RoundedButton();
            this.grpMemory = new Panel();
            this.chkDisablePaging = new ToggleSwitch();
            this.chkLargeSystemCache = new ToggleSwitch();
            this.chkMitigations = new ToggleSwitch();
            this.btnInfoDisablePaging = new RoundedButton();
            this.btnInfoLargeSystemCache = new RoundedButton();
            this.btnInfoMitigations = new RoundedButton();
            this.grpIniTweaks = new Panel();
            this.chkIniIRQ = new ToggleSwitch();
            this.chkIniMinSPs = new ToggleSwitch();
            this.chkIniWinLoad = new ToggleSwitch();
            this.btnInfoIniIRQ = new RoundedButton();
            this.btnInfoIniMinSPs = new RoundedButton();
            this.btnInfoIniWinLoad = new RoundedButton();
            this.grpDebloat = new Panel();
            this.tabDebloat = new TabControl();
            this.tabWin10Debloat = new TabPage();
            this.tabWin11Debloat = new TabPage();
            this.flpWin10Debloat = new FlowLayoutPanel();
            this.flpWin11Debloat = new FlowLayoutPanel();
            this.chkDebloatConsumerApps = new ToggleSwitch();
            this.chkDebloatXbox = new ToggleSwitch();
            this.chkDebloatTeamsChat = new ToggleSwitch();
            this.chkDebloatWidgets = new ToggleSwitch();
            this.chkDebloatSuggestions = new ToggleSwitch();
            this.chkDebloatOneDriveStartup = new ToggleSwitch();
            this.btnInfoDebloatConsumerApps = new RoundedButton();
            this.btnInfoDebloatXbox = new RoundedButton();
            this.btnInfoDebloatTeamsChat = new RoundedButton();
            this.btnInfoDebloatWidgets = new RoundedButton();
            this.btnInfoDebloatSuggestions = new RoundedButton();
            this.btnInfoDebloatOneDriveStartup = new RoundedButton();
            this.pnlActions = new Panel();
            this.btnApplyAll = new RoundedButton();
            this.btnRestoreDefaults = new RoundedButton();
            this.btnSelectAll = new RoundedButton();
            this.prgApply = new ProgressBar();
            this.pnlLog = new Panel();
            this.rtbLog = new RichTextBox();
            this.lblLogTitle = new Label();
            this.btnClearLog = new RoundedButton();
            this.tipMain = new ToolTip(this.components);
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.AcceptButton = this.btnApplyAll;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = ColorTranslator.FromHtml("#F5F8FF");
            this.ClientSize = new Size(1100, 740);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlActions);
            this.Controls.Add(this.pnlLog);
            this.Controls.Add(this.pnlHeader);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = ColorTranslator.FromHtml("#172033");
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new Size(1116, 778);
            this.Name = "Form1";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "JaiDee-Optimize v1.0 - Windows Gaming Optimizer";
            this.Load += new System.EventHandler(this.Form1_Load);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = ColorTranslator.FromHtml("#F5F8FF");
            this.pnlHeader.Controls.Add(this.pnlBrandMark);
            this.pnlHeader.Controls.Add(this.lblAppTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblLanguage);
            this.pnlHeader.Controls.Add(this.cmbLanguage);
            this.pnlHeader.Controls.Add(this.lblTheme);
            this.pnlHeader.Controls.Add(this.chkTheme);
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Location = new Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new Padding(22, 16, 22, 10);
            this.pnlHeader.Size = new Size(1100, 56);
            this.pnlHeader.TabIndex = 0;
            // 
            // 
            // pnlBrandMark
            // 
            this.pnlBrandMark.BackColor = ColorTranslator.FromHtml("#F5F8FF");
            this.pnlBrandMark.Location = new Point(22, 17);
            this.pnlBrandMark.Name = "pnlBrandMark";
            this.pnlBrandMark.Size = new Size(38, 38);
            this.pnlBrandMark.TabIndex = 0;
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new Font("Segoe UI", 20F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.lblAppTitle.ForeColor = ColorTranslator.FromHtml("#0F172A");
            this.lblAppTitle.Location = new Point(68, 13);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new Size(251, 37);
            this.lblAppTitle.TabIndex = 1;
            this.lblAppTitle.Text = "JaiDee-Optimize";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtitle.ForeColor = ColorTranslator.FromHtml("#64748B");
            this.lblSubtitle.Location = new Point(71, 51);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new Size(317, 15);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "Gaming latency optimizer | v1.0 | Checking privileges";
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this.lblLanguage.ForeColor = ColorTranslator.FromHtml("#64748B");
            this.lblLanguage.Location = new Point(566, 18);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new Size(61, 15);
            this.lblLanguage.TabIndex = 3;
            this.lblLanguage.Text = "Language";
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.BackColor = ColorTranslator.FromHtml("#EAF1FF");
            this.cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbLanguage.FlatStyle = FlatStyle.Flat;
            this.cmbLanguage.Font = new Font("Segoe UI", 8.5F);
            this.cmbLanguage.ForeColor = ColorTranslator.FromHtml("#172033");
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English",
            "Thai"});
            this.cmbLanguage.Location = new Point(635, 14);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new Size(104, 23);
            this.cmbLanguage.TabIndex = 34;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblTheme
            // 
            this.lblTheme.AutoSize = true;
            this.lblTheme.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this.lblTheme.ForeColor = ColorTranslator.FromHtml("#64748B");
            this.lblTheme.Location = new Point(566, 47);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new Size(43, 15);
            this.lblTheme.TabIndex = 4;
            this.lblTheme.Text = "Theme";
            // 
            // chkTheme
            // 
            this.chkTheme.BackColor = ColorTranslator.FromHtml("#F5F8FF");
            this.chkTheme.Checked = false;
            this.chkTheme.CheckState = CheckState.Unchecked;
            this.chkTheme.Cursor = Cursors.Hand;
            this.chkTheme.FlatStyle = FlatStyle.Flat;
            this.chkTheme.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this.chkTheme.ForeColor = ColorTranslator.FromHtml("#172033");
            this.chkTheme.Location = new Point(635, 43);
            this.chkTheme.Name = "chkTheme";
            this.chkTheme.Size = new Size(104, 24);
            this.chkTheme.TabIndex = 35;
            this.chkTheme.Tag = "Theme";
            this.chkTheme.Text = "Light";
            this.chkTheme.UseVisualStyleBackColor = false;
            this.chkTheme.CheckedChanged += new System.EventHandler(this.chkTheme_CheckedChanged);
            // 
            // pnlMain
            // 
            this.pnlMain.AutoScroll = true;
            this.pnlMain.BackColor = ColorTranslator.FromHtml("#F5F8FF");
            this.pnlMain.Controls.Add(this.grpPriority);
            this.pnlMain.Controls.Add(this.grpLatency);
            this.pnlMain.Controls.Add(this.grpMemory);
            this.pnlMain.Controls.Add(this.grpIniTweaks);
            this.pnlMain.Controls.Add(this.grpDebloat);
            this.pnlMain.Location = new Point(0, 78);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new Padding(18, 0, 18, 12);
            this.pnlMain.Size = new Size(558, 516);
            this.pnlMain.TabIndex = 1;
            // 
            // tweak groups
            // 
            ConfigureGroup(this.grpPriority, "Priority & Scheduling", 18, 0, 516, 222);
            ConfigureToggle(this.chkWinPriority, "chkWinPriority", "WinPriority", "Win32 priority separation", 18, 62, 0);
            ConfigureToggle(this.chkMMCSS_GamePriority, "chkMMCSS_GamePriority", "MMCSS_GamePriority", "MMCSS game task priority", 18, 91, 1);
            ConfigureToggle(this.chkMMCSS_HighCategory, "chkMMCSS_HighCategory", "MMCSS_HighCategory", "MMCSS scheduling category", 18, 120, 2);
            ConfigureToggle(this.chkMMCSS_GPUPriority, "chkMMCSS_GPUPriority", "MMCSS_GPUPriority", "MMCSS GPU priority", 18, 149, 3);
            ConfigureToggle(this.chkMMCSS_Responsiveness, "chkMMCSS_Responsiveness", "MMCSS_Responsiveness", "System responsiveness", 18, 178, 4);
            ConfigureInfoButton(this.btnInfoWinPriority, "btnInfoWinPriority", "WinPriority", 470, 62, 20);
            ConfigureInfoButton(this.btnInfoMMCSS_GamePriority, "btnInfoMMCSS_GamePriority", "MMCSS_GamePriority", 470, 91, 21);
            ConfigureInfoButton(this.btnInfoMMCSS_HighCategory, "btnInfoMMCSS_HighCategory", "MMCSS_HighCategory", 470, 120, 22);
            ConfigureInfoButton(this.btnInfoMMCSS_GPUPriority, "btnInfoMMCSS_GPUPriority", "MMCSS_GPUPriority", 470, 149, 23);
            ConfigureInfoButton(this.btnInfoMMCSS_Responsiveness, "btnInfoMMCSS_Responsiveness", "MMCSS_Responsiveness", 470, 178, 24);
            this.grpPriority.Controls.AddRange(new Control[] { this.chkWinPriority, this.chkMMCSS_GamePriority, this.chkMMCSS_HighCategory, this.chkMMCSS_GPUPriority, this.chkMMCSS_Responsiveness, this.btnInfoWinPriority, this.btnInfoMMCSS_GamePriority, this.btnInfoMMCSS_HighCategory, this.btnInfoMMCSS_GPUPriority, this.btnInfoMMCSS_Responsiveness });
            // 
            ConfigureGroup(this.grpLatency, "Latency & Timer", 18, 236, 516, 160);
            ConfigureToggle(this.chkNetworkThrottle, "chkNetworkThrottle", "NetworkThrottle", "Network throttling", 18, 62, 5);
            ConfigureToggle(this.chkZeroTimeSlice, "chkZeroTimeSlice", "ZeroTimeSlice", "IRQ priority boost", 18, 91, 6);
            ConfigureToggle(this.chkDynamicTick, "chkDynamicTick", "DynamicTick", "Timer compatibility fix", 18, 120, 7);
            ConfigureInfoButton(this.btnInfoNetworkThrottle, "btnInfoNetworkThrottle", "NetworkThrottle", 470, 62, 25);
            ConfigureInfoButton(this.btnInfoZeroTimeSlice, "btnInfoZeroTimeSlice", "ZeroTimeSlice", 470, 91, 26);
            ConfigureInfoButton(this.btnInfoDynamicTick, "btnInfoDynamicTick", "DynamicTick", 470, 120, 27);
            this.grpLatency.Controls.AddRange(new Control[] { this.chkNetworkThrottle, this.chkZeroTimeSlice, this.chkDynamicTick, this.btnInfoNetworkThrottle, this.btnInfoZeroTimeSlice, this.btnInfoDynamicTick });
            // 
            ConfigureGroup(this.grpMemory, "Memory & Security Tradeoffs", 18, 410, 516, 160);
            ConfigureToggle(this.chkDisablePaging, "chkDisablePaging", "DisablePaging", "Disable paging executive", 18, 62, 8);
            ConfigureToggle(this.chkLargeSystemCache, "chkLargeSystemCache", "LargeSystemCache", "Large system cache", 18, 91, 9);
            ConfigureToggle(this.chkMitigations, "chkMitigations", "Mitigations", "Disable OS mitigations", 18, 120, 10);
            ConfigureInfoButton(this.btnInfoDisablePaging, "btnInfoDisablePaging", "DisablePaging", 470, 62, 28);
            ConfigureInfoButton(this.btnInfoLargeSystemCache, "btnInfoLargeSystemCache", "LargeSystemCache", 470, 91, 29);
            ConfigureInfoButton(this.btnInfoMitigations, "btnInfoMitigations", "Mitigations", 470, 120, 30);
            this.grpMemory.Controls.AddRange(new Control[] { this.chkDisablePaging, this.chkLargeSystemCache, this.chkMitigations, this.btnInfoDisablePaging, this.btnInfoLargeSystemCache, this.btnInfoMitigations });
            // 
            ConfigureGroup(this.grpIniTweaks, "Legacy INI Tweaks", 18, 584, 516, 160);
            ConfigureToggle(this.chkIniIRQ, "chkIniIRQ", "IniIRQ", "SYSTEM.INI IRQ entry", 18, 62, 11);
            ConfigureToggle(this.chkIniMinSPs, "chkIniMinSPs", "IniMinSPs", "SYSTEM.INI stack pages", 18, 91, 12);
            ConfigureToggle(this.chkIniWinLoad, "chkIniWinLoad", "IniWinLoad", "WIN.INI load entry", 18, 120, 13);
            ConfigureInfoButton(this.btnInfoIniIRQ, "btnInfoIniIRQ", "IniIRQ", 470, 62, 31);
            ConfigureInfoButton(this.btnInfoIniMinSPs, "btnInfoIniMinSPs", "IniMinSPs", 470, 91, 32);
            ConfigureInfoButton(this.btnInfoIniWinLoad, "btnInfoIniWinLoad", "IniWinLoad", 470, 120, 33);
            this.grpIniTweaks.Controls.AddRange(new Control[] { this.chkIniIRQ, this.chkIniMinSPs, this.chkIniWinLoad, this.btnInfoIniIRQ, this.btnInfoIniMinSPs, this.btnInfoIniWinLoad });
            // 
            ConfigureGroup(this.grpDebloat, "Windows Debloat", 18, 758, 516, 360);
            ConfigureDebloatTabs();
            ConfigureToggle(this.chkDebloatConsumerApps, "chkDebloatConsumerApps", "DebloatConsumerApps", "Remove bundled consumer apps", 18, 62, 34);
            ConfigureToggle(this.chkDebloatXbox, "chkDebloatXbox", "DebloatXbox", "Remove Xbox companion apps", 18, 91, 35);
            ConfigureToggle(this.chkDebloatTeamsChat, "chkDebloatTeamsChat", "DebloatTeamsChat", "Remove Teams Chat", 18, 120, 36);
            ConfigureToggle(this.chkDebloatWidgets, "chkDebloatWidgets", "DebloatWidgets", "Disable Widgets and News", 18, 149, 37);
            ConfigureToggle(this.chkDebloatSuggestions, "chkDebloatSuggestions", "DebloatSuggestions", "Disable ads and suggestions", 18, 178, 38);
            ConfigureToggle(this.chkDebloatOneDriveStartup, "chkDebloatOneDriveStartup", "DebloatOneDriveStartup", "Disable OneDrive startup", 18, 207, 39);
            ConfigureInfoButton(this.btnInfoDebloatConsumerApps, "btnInfoDebloatConsumerApps", "DebloatConsumerApps", 470, 62, 40);
            ConfigureInfoButton(this.btnInfoDebloatXbox, "btnInfoDebloatXbox", "DebloatXbox", 470, 91, 41);
            ConfigureInfoButton(this.btnInfoDebloatTeamsChat, "btnInfoDebloatTeamsChat", "DebloatTeamsChat", 470, 120, 42);
            ConfigureInfoButton(this.btnInfoDebloatWidgets, "btnInfoDebloatWidgets", "DebloatWidgets", 470, 149, 43);
            ConfigureInfoButton(this.btnInfoDebloatSuggestions, "btnInfoDebloatSuggestions", "DebloatSuggestions", 470, 178, 44);
            ConfigureInfoButton(this.btnInfoDebloatOneDriveStartup, "btnInfoDebloatOneDriveStartup", "DebloatOneDriveStartup", 470, 207, 45);
            this.chkDebloatConsumerApps.Checked = false;
            this.chkDebloatConsumerApps.CheckState = CheckState.Unchecked;
            this.chkDebloatXbox.Checked = false;
            this.chkDebloatXbox.CheckState = CheckState.Unchecked;
            this.chkDebloatTeamsChat.Checked = false;
            this.chkDebloatTeamsChat.CheckState = CheckState.Unchecked;
            this.chkDebloatWidgets.Checked = false;
            this.chkDebloatWidgets.CheckState = CheckState.Unchecked;
            this.chkDebloatSuggestions.Checked = false;
            this.chkDebloatSuggestions.CheckState = CheckState.Unchecked;
            this.chkDebloatOneDriveStartup.Checked = false;
            this.chkDebloatOneDriveStartup.CheckState = CheckState.Unchecked;
            this.grpDebloat.Controls.Add(this.tabDebloat);

            // 
            // pnlActions
            // 
            this.pnlActions.BackColor = ColorTranslator.FromHtml("#F5F8FF");
            this.pnlActions.Controls.Add(this.btnApplyAll);
            this.pnlActions.Controls.Add(this.btnRestoreDefaults);
            this.pnlActions.Controls.Add(this.btnSelectAll);
            this.pnlActions.Controls.Add(this.prgApply);
            this.pnlActions.Location = new Point(0, 594);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new Size(558, 67);
            this.pnlActions.TabIndex = 2;
            // 
            // btnApplyAll
            // 
            ConfigureActionButton(this.btnApplyAll, "btnApplyAll", "Apply selected", "#2563EB", "#F8FBFF", 18, 10, 178, 42, 14);
            this.btnApplyAll.Click += new System.EventHandler(this.btnApplyAll_Click);
            // 
            // btnRestoreDefaults
            // 
            ConfigureActionButton(this.btnRestoreDefaults, "btnRestoreDefaults", "Restore defaults", "#FFFFFF", "#B45309", 207, 10, 166, 42, 15);
            this.btnRestoreDefaults.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FED7AA");
            this.btnRestoreDefaults.FlatAppearance.BorderSize = 1;
            this.btnRestoreDefaults.Click += new System.EventHandler(this.btnRestoreDefaults_Click);
            // 
            // btnSelectAll
            // 
            ConfigureActionButton(this.btnSelectAll, "btnSelectAll", "Turn all off", "#FFFFFF", "#334155", 384, 10, 150, 42, 16);
            this.btnSelectAll.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D7E3F8");
            this.btnSelectAll.FlatAppearance.BorderSize = 1;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // prgApply
            // 
            this.prgApply.Location = new Point(18, 58);
            this.prgApply.Name = "prgApply";
            this.prgApply.Size = new Size(516, 6);
            this.prgApply.Style = ProgressBarStyle.Marquee;
            this.prgApply.TabIndex = 17;
            this.prgApply.Visible = false;
            // 
            // pnlLog
            // 
            this.pnlLog.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.pnlLog.Controls.Add(this.rtbLog);
            this.pnlLog.Controls.Add(this.lblLogTitle);
            this.pnlLog.Controls.Add(this.btnClearLog);
            this.pnlLog.Dock = DockStyle.Right;
            this.pnlLog.Location = new Point(558, 78);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Padding = new Padding(14);
            this.pnlLog.Size = new Size(326, 583);
            this.pnlLog.TabIndex = 3;
            this.pnlLog.Paint += new PaintEventHandler(this.RoundedPanel_Paint);
            // 
            // lblLogTitle
            // 
            this.lblLogTitle.AutoSize = true;
            this.lblLogTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            this.lblLogTitle.ForeColor = ColorTranslator.FromHtml("#172033");
            this.lblLogTitle.Location = new Point(16, 17);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Size = new Size(98, 19);
            this.lblLogTitle.TabIndex = 0;
            this.lblLogTitle.Text = "Operation log";
            // 
            // rtbLog
            // 
            this.rtbLog.BackColor = ColorTranslator.FromHtml("#EFF5FF");
            this.rtbLog.BorderStyle = BorderStyle.None;
            this.rtbLog.Font = new Font("Consolas", 8.5F);
            this.rtbLog.ForeColor = ColorTranslator.FromHtml("#334155");
            this.rtbLog.Location = new Point(14, 52);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            this.rtbLog.Size = new Size(298, 516);
            this.rtbLog.TabIndex = 18;
            this.rtbLog.Text = "";
            this.rtbLog.WordWrap = true;
            // 
            // btnClearLog
            // 
            ConfigureActionButton(this.btnClearLog, "btnClearLog", "Clear", "#EAF1FF", "#64748B", 246, 12, 66, 28, 19);
            this.btnClearLog.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // tipMain
            // 
            this.tipMain.AutoPopDelay = 5000;
            this.tipMain.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.tipMain.ForeColor = ColorTranslator.FromHtml("#172033");
            this.tipMain.InitialDelay = 350;
            this.tipMain.ReshowDelay = 100;
            // 
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlActions.ResumeLayout(false);
            this.pnlLog.ResumeLayout(false);
            this.pnlLog.PerformLayout();
            this.ResumeLayout(false);
        }

        private void ConfigureGroup(Panel panel, string title, int x, int y, int width, int height)
        {
            panel.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            panel.Location = new Point(x, y);
            panel.Name = "grp" + title.Replace(" ", string.Empty).Replace("&", string.Empty);
            panel.Size = new Size(width, height);
            panel.TabStop = false;
            panel.Tag = title;
            panel.Paint += new PaintEventHandler(this.RoundedPanel_Paint);

            // Category accent label — coloured pill for visual grouping
            Label categoryBadge = new Label();
            categoryBadge.AutoSize = true;
            categoryBadge.BackColor = GetGroupAccentColor(title);
            categoryBadge.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            categoryBadge.ForeColor = ColorTranslator.FromHtml("#FFFFFF");
            categoryBadge.Location = new Point(18, 12);
            categoryBadge.Padding = new Padding(6, 2, 6, 2);
            categoryBadge.Text = GetGroupBadgeText(title);
            panel.Controls.Add(categoryBadge);

            Label heading = new Label();
            heading.AutoSize = true;
            heading.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            heading.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            heading.ForeColor = ColorTranslator.FromHtml("#172033");
            heading.Location = new Point(18, 34);
            heading.Text = title;
            panel.Controls.Add(heading);
        }

        private Color GetGroupAccentColor(string title)
        {
            if (title.Contains("Priority")) return ColorTranslator.FromHtml("#185FA5");
            if (title.Contains("Latency")) return ColorTranslator.FromHtml("#0E8A5E");
            if (title.Contains("Memory")) return ColorTranslator.FromHtml("#7C3AED");
            if (title.Contains("INI")) return ColorTranslator.FromHtml("#B45309");
            if (title.Contains("Debloat")) return ColorTranslator.FromHtml("#B91C1C");
            return ColorTranslator.FromHtml("#374151");
        }

        private string GetGroupBadgeText(string title)
        {
            if (title.Contains("Priority")) return "SCHEDULER";
            if (title.Contains("Latency")) return "TIMER";
            if (title.Contains("Memory")) return "MEMORY";
            if (title.Contains("INI")) return "LEGACY";
            if (title.Contains("Debloat")) return "DEBLOAT";
            return "TWEAK";
        }

        private void ConfigureToggle(CheckBox checkBox, string name, string tag, string text, int x, int y, int tabIndex)
        {
            checkBox.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            checkBox.Checked = true;
            checkBox.CheckState = CheckState.Checked;
            checkBox.Cursor = Cursors.Hand;
            checkBox.FlatStyle = FlatStyle.Flat;
            checkBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            checkBox.ForeColor = ColorTranslator.FromHtml("#172033");
            checkBox.Location = new Point(x, y);
            checkBox.Name = name;
            checkBox.Size = new Size(430, 24);
            checkBox.TabIndex = tabIndex;
            checkBox.Tag = tag;
            checkBox.Text = text;
            checkBox.UseVisualStyleBackColor = false;
            checkBox.CheckedChanged += new System.EventHandler(this.chkTweak_CheckedChanged);
        }

        private void ConfigureInfoButton(Button button, string name, string tag, int x, int y, int tabIndex)
        {
            button.BackColor = ColorTranslator.FromHtml("#EAF1FF");
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D7E3F8");
            button.FlatAppearance.BorderSize = 1;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
            button.ForeColor = ColorTranslator.FromHtml("#2563EB");
            button.Location = new Point(x, y + 1);
            button.Name = name;
            button.Size = new Size(24, 22);
            button.TabIndex = tabIndex;
            button.Tag = tag;
            button.Text = "i";
            button.UseVisualStyleBackColor = false;
            button.Click += new System.EventHandler(this.btnInfo_Click);
        }

        private void ConfigureActionButton(Button button, string name, string text, string backColor, string foreColor, int x, int y, int width, int height, int tabIndex)
        {
            button.BackColor = ColorTranslator.FromHtml(backColor);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            button.ForeColor = ColorTranslator.FromHtml(foreColor);
            button.Location = new Point(x, y);
            button.Name = name;
            button.Size = new Size(width, height);
            button.TabIndex = tabIndex;
            button.Text = text;
            button.UseVisualStyleBackColor = false;
        }

        private void ConfigureDebloatTabs()
        {
            this.tabDebloat.Location = new Point(16, 42);
            this.tabDebloat.Name = "tabDebloat";
            this.tabDebloat.SelectedIndex = 0;
            this.tabDebloat.Size = new Size(484, 276);
            this.tabDebloat.TabIndex = 34;

            this.tabWin10Debloat.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.tabWin10Debloat.Location = new Point(4, 24);
            this.tabWin10Debloat.Name = "tabWin10Debloat";
            this.tabWin10Debloat.Padding = new Padding(6);
            this.tabWin10Debloat.Size = new Size(476, 248);
            this.tabWin10Debloat.TabIndex = 0;
            this.tabWin10Debloat.Text = "Win10";

            this.tabWin11Debloat.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.tabWin11Debloat.Location = new Point(4, 24);
            this.tabWin11Debloat.Name = "tabWin11Debloat";
            this.tabWin11Debloat.Padding = new Padding(6);
            this.tabWin11Debloat.Size = new Size(476, 248);
            this.tabWin11Debloat.TabIndex = 1;
            this.tabWin11Debloat.Text = "Win11";

            ConfigureDebloatFlow(this.flpWin10Debloat, "flpWin10Debloat");
            ConfigureDebloatFlow(this.flpWin11Debloat, "flpWin11Debloat");
            this.tabWin10Debloat.Controls.Add(this.flpWin10Debloat);
            this.tabWin11Debloat.Controls.Add(this.flpWin11Debloat);
            this.tabDebloat.Controls.Add(this.tabWin10Debloat);
            this.tabDebloat.Controls.Add(this.tabWin11Debloat);
        }

        private void ConfigureDebloatFlow(FlowLayoutPanel panel, string name)
        {
            panel.AutoScroll = true;
            panel.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.TopDown;
            panel.Name = name;
            panel.Padding = new Padding(4, 6, 4, 6);
            panel.WrapContents = false;
        }
    }

    internal sealed class ToggleSwitch : CheckBox
    {
        private readonly Timer _animationTimer;
        private float _position;

        public ToggleSwitch()
        {
            _position = this.Checked ? 1f : 0f;
            _animationTimer = new Timer();
            _animationTimer.Interval = 15;
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        public Color AccentColor { get; set; }
        public Color TrackOffColor { get; set; }
        public Color KnobOnColor { get; set; }
        public Color KnobOffColor { get; set; }
        public Color DisabledTextColor { get; set; }

        protected override void OnCheckedChanged(System.EventArgs e)
        {
            base.OnCheckedChanged(e);
            _animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, System.EventArgs e)
        {
            float target = this.Checked ? 1f : 0f;
            _position += (target - _position) * 0.35f;
            if (System.Math.Abs(target - _position) < 0.02f)
            {
                _position = target;
                _animationTimer.Stop();
            }
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.BackColor);

            Color disabled = DisabledTextColor.IsEmpty ? ColorTranslator.FromHtml("#66706A") : DisabledTextColor;
            Rectangle track = new Rectangle(this.Width - 46, 2, 36, 20);
            Rectangle textBounds = new Rectangle(0, 1, track.Left - 8, this.Height - 2);
            using (Brush textBrush = new SolidBrush(this.Enabled ? this.ForeColor : disabled))
            using (StringFormat textFormat = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            {
                e.Graphics.DrawString(this.Text, this.Font, textBrush, textBounds, textFormat);
            }

            Color accent = AccentColor.IsEmpty ? ColorTranslator.FromHtml("#2563EB") : AccentColor;
            Color off = TrackOffColor.IsEmpty ? ColorTranslator.FromHtml("#D7E3F8") : TrackOffColor;
            Color knobOn = KnobOnColor.IsEmpty ? ColorTranslator.FromHtml("#102016") : KnobOnColor;
            Color knobOff = KnobOffColor.IsEmpty ? ColorTranslator.FromHtml("#9AA59E") : KnobOffColor;
            Color trackColor = this.Checked ? accent : off;
            Color knobColor = this.Checked ? knobOn : knobOff;

            using (GraphicsPath trackPath = RoundedRect(track, 10))
            using (Brush trackBrush = new SolidBrush(trackColor))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
            }

            int knobX = track.Left + 3 + (int)(16 * _position);
            Rectangle knob = new Rectangle(knobX, track.Top + 3, 14, 14);
            using (Brush knobBrush = new SolidBrush(knobColor))
            {
                e.Graphics.FillEllipse(knobBrush, knob);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class RoundedButton : Button
    {
        private readonly Timer _pulseTimer;
        private int _pulseFrames;
        private float _pulse;

        public RoundedButton()
        {
            _pulseTimer = new Timer();
            _pulseTimer.Interval = 15;
            _pulseTimer.Tick += PulseTimer_Tick;
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _pulseFrames = 0;
            _pulse = 1f;
            _pulseTimer.Start();
        }

        private void PulseTimer_Tick(object sender, System.EventArgs e)
        {
            _pulseFrames++;
            _pulse = System.Math.Max(0f, 1f - (_pulseFrames / 12f));
            if (_pulseFrames >= 12)
            {
                _pulse = 0f;
                _pulseTimer.Stop();
            }
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.Clear(this.Parent == null ? ColorTranslator.FromHtml("#F5F8FF") : this.Parent.BackColor);

            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = RoundedRect(bounds, 10))
            using (Brush brush = new SolidBrush(this.BackColor))
            using (Pen pen = new Pen(this.FlatAppearance.BorderColor, this.FlatAppearance.BorderSize))
            using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (Brush textBrush = new SolidBrush(this.ForeColor))
            {
                pevent.Graphics.FillPath(brush, path);
                if (this.FlatAppearance.BorderSize > 0)
                {
                    pevent.Graphics.DrawPath(pen, path);
                }

                if (_pulse > 0f)
                {
                    int inset = (int)(8 * (1f - _pulse));
                    Rectangle pulseBounds = Rectangle.Inflate(bounds, -inset, -inset);
                    using (GraphicsPath pulsePath = RoundedRect(pulseBounds, 10))
                    using (Pen pulsePen = new Pen(Color.FromArgb((int)(70 * _pulse), ColorTranslator.FromHtml("#F8FBFF")), 1))
                    {
                        pevent.Graphics.DrawPath(pulsePen, pulsePath);
                    }
                }

                pevent.Graphics.DrawString(this.Text, this.Font, textBrush, bounds, format);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class LightningMark : Control
    {
        public Color AccentColor { get; set; }
        public Color SurfaceColor { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.BackColor);

            Color surface = SurfaceColor.IsEmpty ? ColorTranslator.FromHtml("#FFFFFF") : SurfaceColor;
            Color accent = AccentColor.IsEmpty ? ColorTranslator.FromHtml("#2563EB") : AccentColor;
            Rectangle bounds = new Rectangle(1, 1, this.Width - 3, this.Height - 3);

            using (GraphicsPath circle = new GraphicsPath())
            using (Brush surfaceBrush = new SolidBrush(surface))
            using (Pen border = new Pen(Color.FromArgb(70, accent), 1))
            {
                circle.AddEllipse(bounds);
                e.Graphics.FillPath(surfaceBrush, circle);
                e.Graphics.DrawPath(border, circle);
            }

            Point[] bolt =
            {
                new Point(21, 7),
                new Point(12, 22),
                new Point(20, 22),
                new Point(16, 32),
                new Point(28, 16),
                new Point(20, 16)
            };

            using (Brush boltBrush = new SolidBrush(accent))
            {
                e.Graphics.FillPolygon(boltBrush, bolt);
            }
        }
    }
}

