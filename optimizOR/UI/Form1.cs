using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using optimizOR.Core;
using optimizOR.Models;

namespace optimizOR.UI
{
    public partial class Form1 : Form
    {
        private Logger _logger;
        private TweakEngine _engine;
        private Dictionary<string, TweakDefinition> _tweaksById;
        private ThemePalette _theme;
        private PrivateFontCollection _privateFonts;
        private string _language;
        private bool _isLocalizing;
        private bool _isBusy;

        public Form1()
        {
            _language = "en";
            _theme = ThemePalette.Light();
            _privateFonts = new PrivateFontCollection();
            LoadBundledFonts();
            this.Opacity = 0;
            InitializeComponent();
            RegisterThemeEvents();
            cmbLanguage.SelectedIndex = 0;
            ApplyTheme();
            ApplyLanguage();
            BeginEntranceAnimation();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _logger = new Logger(rtbLog);
            _engine = new TweakEngine(new RegistryEditor(_logger), new IniEditor(_logger), new DebloatEditor(_logger), _logger);
            _tweaksById = TweakDefinitions.GetAll().ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);

            bool isAdmin = optimizOR.Program.IsAdministrator();
            UpdateSubtitle(isAdmin);

            if (!isAdmin)
            {
                MessageBox.Show(
                    "This application must be run as Administrator.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            _logger.Log(L("logStarted"), LogLevel.Info);
            _logger.Log(L("logReview"), LogLevel.Warning);
        }

        private async void btnApplyAll_Click(object sender, EventArgs e)
        {
            if (_isBusy)
            {
                return;
            }

            List<string> selectedIds = GetSelectedTweakIds().ToList();
            if (selectedIds.Count == 0)
            {
                MessageBox.Show(L("noTweaks"), L("noTweaksTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                BuildApplyConfirmation(selectedIds),
                L("confirmApplyTitle"),
                MessageBoxButtons.YesNo,
                selectedIds.Any(IsDebloatId) ? MessageBoxIcon.Warning : MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);
            Progress<string> progress = new Progress<string>(message => _logger.Log(message, LogLevel.Info));
            await Task.Run(() => _engine.ApplyAll(selectedIds, progress));
            _logger.Log(L("applyFinished"), LogLevel.Info);
            SetBusy(false);
        }

        private async void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            if (_isBusy)
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                L("confirmRestore"),
                L("confirmRestoreTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);
            Progress<string> progress = new Progress<string>(message => _logger.Log(message, LogLevel.Info));
            await Task.Run(() => _engine.RestoreAll(progress));
            _logger.Log(L("restoreFinished"), LogLevel.Info);
            SetBusy(false);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            bool checkAll = GetTweakCheckBoxes().Any(chk => !chk.Checked);
            foreach (CheckBox checkBox in GetTweakCheckBoxes())
            {
                checkBox.Checked = checkAll;
            }

            btnSelectAll.Text = checkAll ? L("allOff") : L("allOn");
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            if (_logger != null)
            {
                _logger.Clear();
            }
        }

        private void chkTweak_CheckedChanged(object sender, EventArgs e)
        {
            if (btnSelectAll == null)
            {
                return;
            }

            btnSelectAll.Text = GetTweakCheckBoxes().Any(chk => !chk.Checked) ? L("allOn") : L("allOff");
        }

        private void RoundedPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(panel.Parent == null ? _theme.Background : panel.Parent.BackColor);

            Rectangle bounds = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using (GraphicsPath path = RoundedRect(bounds, 16))
            using (Brush brush = new SolidBrush(_theme.Surface))
            using (Pen pen = new Pen(_theme.Border))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private void RegisterThemeEvents()
        {
            grpPriority.Paint += RoundedPanel_Paint;
            grpLatency.Paint += RoundedPanel_Paint;
            grpMemory.Paint += RoundedPanel_Paint;
            grpIniTweaks.Paint += RoundedPanel_Paint;
            grpDebloat.Paint += RoundedPanel_Paint;

            RegisterButtonHover(btnApplyAll, "#2563EB", "#3B82F6");
            RegisterButtonHover(btnRestoreDefaults, "#242A27", "#303834");
            RegisterButtonHover(btnSelectAll, "#242A27", "#303834");
            RegisterButtonHover(btnClearLog, "#242A27", "#303834");
        }

        private void RegisterButtonHover(Button button, string normalHex, string hoverHex)
        {
            button.MouseEnter += delegate
            {
                button.BackColor = ControlPaint.Light(button.BackColor, 0.12f);
                button.Invalidate();
            };
            button.MouseLeave += delegate
            {
                ApplyTheme();
            };
        }

        private void RegisterToolTips()
        {
            foreach (CheckBox checkBox in GetTweakCheckBoxes())
            {
                if (checkBox.Tag != null)
                {
                    tipMain.SetToolTip(checkBox, TweakDescription(checkBox.Tag.ToString()));
                }
            }
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLocalizing)
            {
                return;
            }

            _language = cmbLanguage.SelectedIndex == 1 ? "th" : "en";
            ApplyLanguage();
        }

        private void chkTheme_CheckedChanged(object sender, EventArgs e)
        {
            _theme = chkTheme.Checked ? ThemePalette.Dark() : ThemePalette.Light();
            ApplyTheme();
            ApplyLanguage();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null)
            {
                return;
            }

            if (_tweaksById == null)
            {
                _tweaksById = TweakDefinitions.GetAll().ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
            }

            TweakDefinition tweak;
            if (!_tweaksById.TryGetValue(button.Tag.ToString(), out tweak))
            {
                return;
            }

            MessageBox.Show(
                BuildInfoText(tweak),
                TweakName(tweak.Id),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private string BuildInfoText(TweakDefinition tweak)
        {
            string type = tweak.Type == TweakType.Registry ? L("registry") : (tweak.Type == TweakType.Debloat ? "PowerShell" : L("iniFile"));
            string target = tweak.Type == TweakType.Registry
                ? tweak.Target + "\\" + tweak.ValueName
                : (tweak.Type == TweakType.Debloat ? "Windows 10/11 Appx or policy settings" : tweak.Target + " [" + tweak.IniSection + "] " + tweak.ValueName);

            return TweakDescription(tweak.Id) + Environment.NewLine + Environment.NewLine +
                L("type") + ": " + type + Environment.NewLine +
                L("target") + ": " + target + Environment.NewLine +
                L("applyValue") + ": " + Convert.ToString(tweak.TweakValue);
        }

        private IEnumerable<CheckBox> GetTweakCheckBoxes()
        {
            return new[]
            {
                chkWinPriority,
                chkMMCSS_GamePriority,
                chkMMCSS_HighCategory,
                chkMMCSS_GPUPriority,
                chkMMCSS_Responsiveness,
                chkNetworkThrottle,
                chkZeroTimeSlice,
                chkDynamicTick,
                chkDisablePaging,
                chkLargeSystemCache,
                chkMitigations,
                chkIniIRQ,
                chkIniMinSPs,
                chkIniWinLoad,
                chkDebloatConsumerApps,
                chkDebloatXbox,
                chkDebloatTeamsChat,
                chkDebloatWidgets,
                chkDebloatSuggestions,
                chkDebloatOneDriveStartup
            };
        }

        private IEnumerable<string> GetSelectedTweakIds()
        {
            return GetTweakCheckBoxes()
                .Where(checkBox => checkBox.Checked && checkBox.Tag != null)
                .Select(checkBox => checkBox.Tag.ToString());
        }

        private bool IsDebloatId(string id)
        {
            TweakDefinition tweak;
            if (_tweaksById != null && _tweaksById.TryGetValue(id, out tweak))
            {
                return tweak.Type == TweakType.Debloat;
            }

            return id != null && id.StartsWith("Debloat", StringComparison.OrdinalIgnoreCase);
        }

        private string BuildApplyConfirmation(IEnumerable<string> selectedIds)
        {
            bool includesDebloat = selectedIds.Any(IsDebloatId);
            if (!includesDebloat)
            {
                return L("confirmApply");
            }

            return L("confirmApply") + Environment.NewLine + Environment.NewLine +
                L("debloatDisclaimer") + Environment.NewLine + Environment.NewLine +
                L("debloatResponsibility");
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;
            prgApply.Visible = busy;
            btnApplyAll.Enabled = !busy;
            btnRestoreDefaults.Enabled = !busy;
            btnSelectAll.Enabled = !busy;

            foreach (CheckBox checkBox in GetTweakCheckBoxes())
            {
                checkBox.Enabled = !busy;
            }
        }

        private void ApplyLanguage()
        {
            _isLocalizing = true;
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("English");
            cmbLanguage.Items.Add("ไทย");
            cmbLanguage.SelectedIndex = _language == "th" ? 1 : 0;
            _isLocalizing = false;

            lblLanguage.Text = L("language");
            lblTheme.Text = L("theme");
            chkTheme.Text = chkTheme.Checked ? L("dark") : L("light");
            lblLogTitle.Text = L("operationLog");
            btnApplyAll.Text = L("applySelected");
            btnRestoreDefaults.Text = L("restoreDefaults");
            btnSelectAll.Text = GetTweakCheckBoxes().Any(chk => !chk.Checked) ? L("allOn") : L("allOff");
            btnClearLog.Text = L("clear");
            SetGroupTitle(grpPriority, L("grpPriority"));
            SetGroupTitle(grpLatency, L("grpLatency"));
            SetGroupTitle(grpMemory, L("grpMemory"));
            SetGroupTitle(grpIniTweaks, L("grpIni"));
            SetGroupTitle(grpDebloat, L("grpDebloat"));

            foreach (CheckBox checkBox in GetTweakCheckBoxes())
            {
                if (checkBox.Tag != null)
                {
                    checkBox.Text = TweakName(checkBox.Tag.ToString());
                }
            }

            RegisterToolTips();
            ApplyLanguageFont();
            UpdateSubtitle(optimizOR.Program.IsAdministrator());
        }

        private void ApplyTheme()
        {
            this.BackColor = _theme.Background;
            this.ForeColor = _theme.Text;
            pnlHeader.BackColor = _theme.Background;
            pnlMain.BackColor = _theme.Background;
            pnlActions.BackColor = _theme.Background;
            pnlLog.BackColor = _theme.Surface;
            pnlBrandMark.BackColor = _theme.Background;
            pnlBrandMark.AccentColor = _theme.Accent;
            pnlBrandMark.SurfaceColor = _theme.Surface;
            lblAppTitle.ForeColor = _theme.Title;
            lblSubtitle.ForeColor = _theme.Muted;
            lblLanguage.ForeColor = _theme.Muted;
            lblTheme.ForeColor = _theme.Muted;
            lblLogTitle.ForeColor = _theme.Text;

            cmbLanguage.BackColor = _theme.Control;
            cmbLanguage.ForeColor = _theme.Text;
            rtbLog.BackColor = _theme.LogBackground;
            rtbLog.ForeColor = _theme.LogText;

            ConfigureThemedButton(btnApplyAll, _theme.Accent, _theme.AccentText, Color.Empty, 0);
            ConfigureThemedButton(btnRestoreDefaults, _theme.Control, _theme.Warning, _theme.WarningBorder, 1);
            ConfigureThemedButton(btnSelectAll, _theme.Control, _theme.SecondaryText, _theme.Border, 1);
            ConfigureThemedButton(btnClearLog, _theme.Control, _theme.Muted, _theme.Border, 1);
            foreach (Panel panel in new[] { grpPriority, grpLatency, grpMemory, grpIniTweaks, grpDebloat })
            {
                panel.BackColor = _theme.Surface;
                foreach (Control child in panel.Controls)
                {
                    child.BackColor = _theme.Surface;
                    if (child is Label)
                    {
                        child.ForeColor = _theme.Text;
                    }
                    else if (child is Button)
                    {
                        ConfigureThemedButton((Button)child, _theme.Control, _theme.SecondaryText, _theme.Border, 1);
                    }
                    else if (child is ToggleSwitch)
                    {
                        ApplyToggleTheme((ToggleSwitch)child);
                    }
                }
                panel.Invalidate();
            }

            ApplyToggleTheme((ToggleSwitch)chkTheme);
            chkTheme.BackColor = _theme.Background;
            tipMain.BackColor = _theme.Control;
            tipMain.ForeColor = _theme.Text;
            pnlLog.Invalidate();
            pnlBrandMark.Invalidate();
            this.Invalidate();
        }

        private void ApplyLanguageFont()
        {
            FontFamily family = ResolveLanguageFontFamily();
            ApplyFontRecursive(this, family);

            lblAppTitle.Font = CreateFont(family, 21F, FontStyle.Bold);
            lblSubtitle.Font = CreateFont(family, 8.8F, FontStyle.Regular);
            lblLanguage.Font = CreateFont(family, 8.5F, FontStyle.Bold);
            lblTheme.Font = CreateFont(family, 8.5F, FontStyle.Bold);
            lblLogTitle.Font = CreateFont(family, 10F, FontStyle.Bold);
            rtbLog.Font = CreateFont(family, 8.7F, FontStyle.Regular);

            foreach (Panel panel in new[] { grpPriority, grpLatency, grpMemory, grpIniTweaks, grpDebloat })
            {
                foreach (Control child in panel.Controls)
                {
                    if (child is Label)
                    {
                        child.Font = CreateFont(family, 10F, FontStyle.Bold);
                    }
                    else if (child is Button)
                    {
                        child.Font = CreateFont(family, 8F, FontStyle.Bold);
                    }
                    else if (child is CheckBox)
                    {
                        child.Font = CreateFont(family, 9.2F, FontStyle.Regular);
                    }
                }
            }

            foreach (Button button in new[] { btnApplyAll, btnRestoreDefaults, btnSelectAll })
            {
                button.Font = CreateFont(family, 9.8F, FontStyle.Bold);
            }

            btnClearLog.Font = CreateFont(family, 8.2F, FontStyle.Bold);
            chkTheme.Font = CreateFont(family, 8.5F, FontStyle.Bold);
        }

        private void ApplyFontRecursive(Control control, FontFamily family)
        {
            control.Font = CreateFont(family, control.Font.Size, control.Font.Style);
            foreach (Control child in control.Controls)
            {
                ApplyFontRecursive(child, family);
            }
        }

        private FontFamily ResolveLanguageFontFamily()
        {
            string requested = _language == "th" ? "Anuphan" : "Roboto";
            string fallback = _language == "th" ? "Leelawadee UI" : "Segoe UI";

            FontFamily bundled = FindPrivateFontFamily(requested);
            if (bundled != null)
            {
                return bundled;
            }

            if (IsInstalledFont(requested))
            {
                return new FontFamily(requested);
            }

            if (IsInstalledFont(fallback))
            {
                return new FontFamily(fallback);
            }

            return FontFamily.GenericSansSerif;
        }

        private Font CreateFont(FontFamily family, float size, FontStyle style)
        {
            FontStyle resolvedStyle = family.IsStyleAvailable(style) ? style : FontStyle.Regular;
            return new Font(family, size, resolvedStyle, GraphicsUnit.Point, ((byte)(0)));
        }

        private void LoadBundledFonts()
        {
            if (_privateFonts != null)
            {
                _privateFonts.Dispose();
            }

            _privateFonts = new PrivateFontCollection();
            string fontDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts");
            if (!Directory.Exists(fontDirectory))
            {
                return;
            }

            foreach (string filePath in Directory.GetFiles(fontDirectory, "*.ttf"))
            {
                try
                {
                    _privateFonts.AddFontFile(filePath);
                }
                catch
                {
                    // Ignore invalid font files; installed font fallback remains available.
                }
            }
        }

        private FontFamily FindPrivateFontFamily(string familyName)
        {
            foreach (FontFamily family in _privateFonts.Families)
            {
                if (family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase))
                {
                    return family;
                }
            }

            return null;
        }

        private bool IsInstalledFont(string familyName)
        {
            using (InstalledFontCollection installedFonts = new InstalledFontCollection())
            {
                foreach (FontFamily family in installedFonts.Families)
                {
                    if (family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ConfigureThemedButton(Button button, Color back, Color fore, Color border, int borderSize)
        {
            button.BackColor = back;
            button.ForeColor = fore;
            button.FlatAppearance.BorderColor = border.IsEmpty ? back : border;
            button.FlatAppearance.BorderSize = borderSize;
            button.Invalidate();
        }

        private void ApplyToggleTheme(ToggleSwitch toggle)
        {
            toggle.BackColor = toggle == chkTheme ? _theme.Background : _theme.Surface;
            toggle.ForeColor = _theme.Text;
            toggle.AccentColor = _theme.Accent;
            toggle.TrackOffColor = _theme.ToggleOff;
            toggle.KnobOnColor = _theme.AccentText;
            toggle.KnobOffColor = _theme.ToggleKnobOff;
            toggle.DisabledTextColor = _theme.DisabledText;
            toggle.Invalidate();
        }

        private void BeginEntranceAnimation()
        {
            Timer timer = new Timer();
            timer.Interval = 16;
            timer.Tick += delegate
            {
                this.Opacity = Math.Min(1.0, this.Opacity + 0.08);
                if (this.Opacity >= 1.0)
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        private void UpdateSubtitle(bool isAdmin)
        {
            lblSubtitle.Text = L("subtitle") + " | v1.0 | " + (isAdmin ? L("admin") : L("adminRequired"));
        }

        private void SetGroupTitle(Panel panel, string title)
        {
            foreach (Control child in panel.Controls)
            {
                Label label = child as Label;
                if (label != null)
                {
                    label.Text = title;
                    return;
                }
            }
        }

        private string TweakName(string id)
        {
            if (IsDebloatId(id))
            {
                return DebloatName(id);
            }

            if (_language == "th")
            {
                switch (id)
                {
                    case "WinPriority": return "ปรับลำดับความสำคัญของเกม";
                    case "MMCSS_GamePriority": return "เพิ่ม priority งานเกมของ MMCSS";
                    case "MMCSS_HighCategory": return "ตั้ง scheduling เป็นระดับสูง";
                    case "MMCSS_GPUPriority": return "เพิ่ม priority ให้ GPU";
                    case "MMCSS_Responsiveness": return "ลด system responsiveness reserve";
                    case "NetworkThrottle": return "ปิด network throttling";
                    case "ZeroTimeSlice": return "เพิ่ม priority ให้ IRQ";
                    case "DynamicTick": return "ปรับ timer compatibility";
                    case "DisablePaging": return "ลดการ page kernel ออกจาก RAM";
                    case "LargeSystemCache": return "ตั้งค่า large system cache";
                    case "Mitigations": return "ปิด OS mitigations";
                    case "IniIRQ": return "เพิ่มค่า IRQ ใน SYSTEM.INI";
                    case "IniMinSPs": return "ตั้ง stack pages ใน SYSTEM.INI";
                    case "IniWinLoad": return "ล้างค่า load ใน WIN.INI";
                }
            }

            switch (id)
            {
                case "WinPriority": return "Game foreground priority";
                case "MMCSS_GamePriority": return "MMCSS game task priority";
                case "MMCSS_HighCategory": return "High scheduling category";
                case "MMCSS_GPUPriority": return "GPU priority boost";
                case "MMCSS_Responsiveness": return "System responsiveness reserve";
                case "NetworkThrottle": return "Network throttling";
                case "ZeroTimeSlice": return "IRQ priority boost";
                case "DynamicTick": return "Timer compatibility fix";
                case "DisablePaging": return "Keep kernel code in RAM";
                case "LargeSystemCache": return "Large system cache";
                case "Mitigations": return "OS mitigations tradeoff";
                case "IniIRQ": return "SYSTEM.INI IRQ entry";
                case "IniMinSPs": return "SYSTEM.INI stack pages";
                case "IniWinLoad": return "WIN.INI load entry";
            }

            return id;
        }

        private string TweakDescription(string id)
        {
            if (IsDebloatId(id))
            {
                return DebloatDescription(id);
            }

            if (_language == "th")
            {
                switch (id)
                {
                    case "WinPriority": return "ปรับ Win32PrioritySeparation เพื่อให้เกมที่อยู่หน้า foreground ได้ scheduling ที่ตอบสนองไวขึ้น";
                    case "MMCSS_GamePriority": return "เพิ่ม priority ของ Multimedia Class Scheduler สำหรับ task ประเภท Games";
                    case "MMCSS_HighCategory": return "ยกระดับ scheduling category ของเกมจาก Medium เป็น High";
                    case "MMCSS_GPUPriority": return "เพิ่มค่า GPU Priority สำหรับ profile Games";
                    case "MMCSS_Responsiveness": return "ลดค่า SystemResponsiveness เพื่อลด CPU reserve ที่อาจกระทบ latency";
                    case "NetworkThrottle": return "ปิด multimedia network throttling เพื่อลดข้อจำกัดด้าน network scheduling";
                    case "ZeroTimeSlice": return "เพิ่ม IRQ8Priority เพื่อช่วยงาน timer และ interrupt บางรูปแบบ";
                    case "DynamicTick": return "ตั้ง RealTimeIsUniversal ตามแผนเดิมเพื่อ compatibility ด้าน timer";
                    case "DisablePaging": return "พยายามเก็บ kernel และ driver code ไว้ใน RAM เพื่อลด paging latency";
                    case "LargeSystemCache": return "ตั้ง LargeSystemCache เป็นค่าที่เหมาะกับ gaming workload";
                    case "Mitigations": return "ปิด mitigation บางส่วนเพื่อ performance แต่ลดความปลอดภัย ใช้เฉพาะเครื่องที่เชื่อถือได้";
                    case "IniIRQ": return "เขียน IRQ9=4 ใน SYSTEM.INI สำหรับ compatibility tweak แบบ legacy";
                    case "IniMinSPs": return "เขียน MinSPs=4 ใน SYSTEM.INI เพื่อ compatibility tweak แบบ legacy";
                    case "IniWinLoad": return "ล้างค่า load= ใน WIN.INI เพื่อลด legacy startup entry";
                }
            }

            switch (id)
            {
                case "WinPriority": return "Sets Win32PrioritySeparation to favor foreground game scheduling.";
                case "MMCSS_GamePriority": return "Raises the Multimedia Class Scheduler priority for the Games task.";
                case "MMCSS_HighCategory": return "Moves the Games scheduling category from Medium to High.";
                case "MMCSS_GPUPriority": return "Raises GPU Priority for the Games profile.";
                case "MMCSS_Responsiveness": return "Reduces reserved CPU responsiveness that can affect game latency.";
                case "NetworkThrottle": return "Disables multimedia network throttling to reduce scheduling limits.";
                case "ZeroTimeSlice": return "Adds IRQ8Priority for timer and interrupt related workloads.";
                case "DynamicTick": return "Applies the planned RealTimeIsUniversal timer compatibility value.";
                case "DisablePaging": return "Keeps kernel and driver code resident in RAM to reduce paging latency.";
                case "LargeSystemCache": return "Keeps LargeSystemCache at the gaming-oriented value.";
                case "Mitigations": return "Disables selected mitigations for performance. This reduces security, use only on trusted hardware.";
                case "IniIRQ": return "Writes IRQ9=4 to SYSTEM.INI as a legacy compatibility tweak.";
                case "IniMinSPs": return "Writes MinSPs=4 to SYSTEM.INI as a legacy compatibility tweak.";
                case "IniWinLoad": return "Clears the WIN.INI load entry to reduce legacy startup behavior.";
            }

            return id;
        }

        private string DebloatName(string id)
        {
            bool th = _language == "th";
            switch (id)
            {
                case "DebloatConsumerApps": return th ? "ลบแอปเสริมที่ติดมากับ Windows" : "Remove bundled consumer apps";
                case "DebloatXbox": return th ? "ลบแอปเสริม Xbox" : "Remove Xbox companion apps";
                case "DebloatTeamsChat": return th ? "ลบ Teams Chat สำหรับผู้ใช้ทั่วไป" : "Remove consumer Teams Chat";
                case "DebloatWidgets": return th ? "ปิด Widgets และ News" : "Disable Widgets and News";
                case "DebloatSuggestions": return th ? "ปิดโฆษณาและคำแนะนำ" : "Disable ads and suggestions";
                case "DebloatOneDriveStartup": return th ? "ปิด OneDrive ตอนเปิดเครื่อง" : "Disable OneDrive startup";
            }

            return id;
        }

        private string DebloatDescription(string id)
        {
            bool th = _language == "th";
            switch (id)
            {
                case "DebloatConsumerApps":
                    return th
                        ? "ลบ Appx package เสริมที่มักติดมากับ Windows 10/11 เช่น Clipchamp, News, Weather, Solitaire, Maps, Feedback Hub และแอป consumer อื่น โดยไม่ลบ Store, Security, Edge, Terminal, Photos, Calculator หรือ framework packages"
                        : "Removes common optional bundled Appx packages on Windows 10/11, such as Clipchamp, News, Weather, Solitaire, Maps, Feedback Hub, and other consumer apps. It keeps Store, Security, Edge, Terminal, Photos, Calculator, codecs, and framework packages.";
                case "DebloatXbox":
                    return th
                        ? "ลบแอป Xbox/Game Bar ที่เป็น optional เหมาะสำหรับเครื่องที่ไม่ใช้ Game Bar, capture overlay หรือ PC Game Pass overlay"
                        : "Removes optional Xbox and Game Bar companion packages. Skip this if you use Game Bar capture, Xbox services, PC Game Pass overlays, or Xbox controller overlay features.";
                case "DebloatTeamsChat":
                    return th
                        ? "ลบ Teams/Chat consumer และปิด Chat taskbar integration ไม่กระทบ Microsoft Teams แบบงานที่ติดตั้งแยก"
                        : "Removes consumer Teams/Chat packages and disables Chat taskbar integration. It does not remove business Microsoft Teams installed separately.";
                case "DebloatWidgets":
                    return th
                        ? "ปิด Widgets บน Windows 11 และ News and Interests บน Windows 10 ผ่าน policy/registry"
                        : "Disables Windows 11 Widgets and Windows 10 News and Interests through supported policy/registry values.";
                case "DebloatSuggestions":
                    return th
                        ? "ปิด silent app install, tips, Start suggestions, lock screen suggestions และ web suggestion บางส่วนใน Explorer/Search"
                        : "Disables common Windows tips, silent app installs, Start suggestions, lock screen suggestions, and selected Explorer/Search web suggestions.";
                case "DebloatOneDriveStartup":
                    return th
                        ? "ปิด OneDrive auto-start เท่านั้น ไม่ uninstall OneDrive และไม่ลบไฟล์ผู้ใช้"
                        : "Disables OneDrive auto-start only. It does not uninstall OneDrive and does not delete user files.";
            }

            return id;
        }

        private string L(string key)
        {
            bool th = _language == "th";
            switch (key)
            {
                case "language": return th ? "ภาษา" : "Language";
                case "theme": return th ? "ธีม" : "Theme";
                case "dark": return th ? "มืด" : "Dark";
                case "light": return th ? "สว่าง" : "Light";
                case "subtitle": return th ? "ตัวปรับ latency สำหรับเกม" : "Gaming latency optimizer";
                case "admin": return th ? "กำลังรันแบบผู้ดูแลระบบ" : "Running as Administrator";
                case "adminRequired": return th ? "ต้องใช้สิทธิ์ผู้ดูแลระบบ" : "Administrator required";
                case "operationLog": return th ? "บันทึกการทำงาน" : "Operation log";
                case "applySelected": return th ? "ใช้ค่าที่เปิดไว้" : "Apply selected";
                case "restoreDefaults": return th ? "คืนค่าเดิม" : "Restore defaults";
                case "allOff": return th ? "ปิดทั้งหมด" : "Turn all off";
                case "allOn": return th ? "เปิดทั้งหมด" : "Turn all on";
                case "clear": return th ? "ล้าง" : "Clear";
                case "grpPriority": return th ? "Priority และ Scheduling" : "Priority & Scheduling";
                case "grpLatency": return th ? "Latency และ Timer" : "Latency & Timer";
                case "grpMemory": return th ? "Memory และความปลอดภัย" : "Memory & Security Tradeoffs";
                case "grpIni": return th ? "Legacy INI Tweaks" : "Legacy INI Tweaks";
                case "grpDebloat": return th ? "Debloat Windows 10/11" : "Windows 10/11 Debloat";
                case "noTweaks": return th ? "กรุณาเปิด tweak อย่างน้อยหนึ่งรายการก่อนใช้งาน" : "Turn on at least one tweak before applying.";
                case "noTweaksTitle": return th ? "ยังไม่ได้เปิด Tweak" : "No Tweaks Enabled";
                case "confirmApply": return th ? "ต้องการใช้ tweaks ที่เปิดอยู่หรือไม่ บางค่าอาจต้อง restart เครื่อง" : "Apply enabled tweaks? A restart may be required for some changes.";
                case "confirmApplyTitle": return th ? "ยืนยันการใช้งาน" : "Confirm Apply";
                case "confirmRestore": return th ? "จะคืนค่า Registry และ INI ที่ถูกแก้ใน session นี้ ต้องการดำเนินการต่อหรือไม่" : "This will restore registry and INI values changed during this session. Continue?";
                case "confirmRestoreTitle": return th ? "ยืนยันการคืนค่า" : "Confirm Restore";
                case "debloatDisclaimer": return th ? "คำเตือน: Debloat จะลบหรือปิดบางแอป/ฟีเจอร์ของ Windows 10/11 จริง บางรายการอาจต้องติดตั้งกลับจาก Microsoft Store หรือ PowerShell และ Restore Defaults ไม่สามารถคืน Appx ที่ถูกลบได้อัตโนมัติ" : "Warning: Debloat will actually remove or disable selected Windows 10/11 apps/features. Some items may need to be reinstalled from Microsoft Store or PowerShell. Restore Defaults cannot automatically reinstall removed Appx packages.";
                case "debloatResponsibility": return th ? "การกด Yes หมายความว่าคุณเข้าใจความเสี่ยง ได้สำรองข้อมูลหรือสร้าง Restore Point แล้ว และยอมรับความรับผิดชอบต่อผลลัพธ์บนเครื่องนี้" : "Clicking Yes means you understand the risk, have backed up or created a restore point, and accept responsibility for the result on this PC.";
                case "logStarted": return th ? "optimizOR เริ่มทำงานแล้ว" : "optimizOR started.";
                case "logReview": return th ? "Tweaks ถูกเปิดไว้เป็นค่าเริ่มต้น อ่าน info ก่อนใช้ค่าที่มีความเสี่ยง" : "Tweaks are enabled by default. Open info before applying risky changes.";
                case "applyFinished": return th ? "ดำเนินการใช้ค่าเสร็จแล้ว" : "Apply operation finished.";
                case "restoreFinished": return th ? "ดำเนินการคืนค่าเสร็จแล้ว" : "Restore operation finished.";
                case "registry": return th ? "Registry" : "Registry";
                case "iniFile": return th ? "ไฟล์ INI" : "INI file";
                case "type": return th ? "ประเภท" : "Type";
                case "target": return th ? "ตำแหน่ง" : "Target";
                case "applyValue": return th ? "ค่าที่จะใช้" : "Apply value";
            }

            return key;
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

        private sealed class ThemePalette
        {
            public Color Background;
            public Color Surface;
            public Color Control;
            public Color Border;
            public Color Text;
            public Color Title;
            public Color Muted;
            public Color SecondaryText;
            public Color Accent;
            public Color AccentText;
            public Color Warning;
            public Color WarningBorder;
            public Color LogBackground;
            public Color LogText;
            public Color ToggleOff;
            public Color ToggleKnobOff;
            public Color DisabledText;

            public static ThemePalette Dark()
            {
                return new ThemePalette
                {
                    Background = ColorTranslator.FromHtml("#0F172A"),
                    Surface = ColorTranslator.FromHtml("#151F33"),
                    Control = ColorTranslator.FromHtml("#1D2A44"),
                    Border = ColorTranslator.FromHtml("#2C3B59"),
                    Text = ColorTranslator.FromHtml("#EAF2FF"),
                    Title = ColorTranslator.FromHtml("#F4F8FF"),
                    Muted = ColorTranslator.FromHtml("#94A3B8"),
                    SecondaryText = ColorTranslator.FromHtml("#CBD5E1"),
                    Accent = ColorTranslator.FromHtml("#60A5FA"),
                    AccentText = ColorTranslator.FromHtml("#07111F"),
                    Warning = ColorTranslator.FromHtml("#FDBA74"),
                    WarningBorder = ColorTranslator.FromHtml("#7C3E12"),
                    LogBackground = ColorTranslator.FromHtml("#0B1220"),
                    LogText = ColorTranslator.FromHtml("#C7D2E3"),
                    ToggleOff = ColorTranslator.FromHtml("#334155"),
                    ToggleKnobOff = ColorTranslator.FromHtml("#CBD5E1"),
                    DisabledText = ColorTranslator.FromHtml("#64748B")
                };
            }

            public static ThemePalette Light()
            {
                return new ThemePalette
                {
                    Background = ColorTranslator.FromHtml("#F5F8FF"),
                    Surface = ColorTranslator.FromHtml("#FFFFFF"),
                    Control = ColorTranslator.FromHtml("#EAF1FF"),
                    Border = ColorTranslator.FromHtml("#D7E3F8"),
                    Text = ColorTranslator.FromHtml("#172033"),
                    Title = ColorTranslator.FromHtml("#0F172A"),
                    Muted = ColorTranslator.FromHtml("#64748B"),
                    SecondaryText = ColorTranslator.FromHtml("#334155"),
                    Accent = ColorTranslator.FromHtml("#2563EB"),
                    AccentText = ColorTranslator.FromHtml("#F8FBFF"),
                    Warning = ColorTranslator.FromHtml("#B45309"),
                    WarningBorder = ColorTranslator.FromHtml("#FED7AA"),
                    LogBackground = ColorTranslator.FromHtml("#EFF5FF"),
                    LogText = ColorTranslator.FromHtml("#334155"),
                    ToggleOff = ColorTranslator.FromHtml("#D7E3F8"),
                    ToggleKnobOff = ColorTranslator.FromHtml("#64748B"),
                    DisabledText = ColorTranslator.FromHtml("#94A3B8")
                };
            }
        }
    }
}

