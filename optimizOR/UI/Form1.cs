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
        private List<CheckBox> _dynamicDebloatCheckBoxes;
        private Panel _pnlSidebar;
        private Panel _pnlContent;
        private Panel _pageHome;
        private Panel _pageTweaks;
        private Panel _pageLog;
        private Panel _pageSettings;
        private Panel _pageAbout;
        private Label _lblPageTitle;
        private Label _lblPageSubtitle;
        private RoundedButton _btnOsBadge;
        private RoundedButton _navHome;
        private RoundedButton _navTweaks;
        private RoundedButton _navLog;
        private RoundedButton _navSettings;
        private RoundedButton _navAbout;
        private RoundedButton _btnExportLog;
        private RoundedButton _btnWin11DebloatSegment;
        private RoundedButton _btnWin10DebloatSegment;
        private Panel _pnlDebloatHost;
        private Label _lblWin10Warning;
        private string _activePage;
        private string _osProfile;
        private string _language;
        private bool _isLocalizing;
        private bool _isBusy;

        public Form1()
        {
            _language = "en";
            _theme = ThemePalette.Light();
            _privateFonts = new PrivateFontCollection();
            _dynamicDebloatCheckBoxes = new List<CheckBox>();
            _activePage = "Home";
            _osProfile = "Windows 11";
            LoadBundledFonts();
            this.Opacity = 0;
            InitializeComponent();
            BuildLiquidShell();
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
            BuildDebloatPages();
            ApplyOsProfileRules();
            ApplyTheme();
            ApplyLanguage();

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
                ShowLargeInfoDialog(L("noTweaksTitle"), L("noTweaks"), L("noTweaksHelp"), false);
                return;
            }

            bool includesDebloat = selectedIds.Any(IsDebloatId);
            DialogResult result = ShowLargeDecisionDialog(
                L("confirmApplyTitle"),
                includesDebloat ? L("confirmApplyDebloatSubtitle") : L("confirmApplySubtitle"),
                BuildApplyConfirmation(selectedIds),
                L("applySelected"),
                L("cancel"),
                includesDebloat);

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

            DialogResult result = ShowLargeDecisionDialog(
                L("confirmRestoreTitle"),
                L("confirmRestoreSubtitle"),
                L("confirmRestore"),
                L("restoreDefaults"),
                L("cancel"),
                true);

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

        private void btnExportLog_Click(object sender, EventArgs e)
        {
            if (_logger == null)
            {
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FileName = "JaiDee-Optimize-log.txt";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _logger.Export(dialog.FileName);
                }
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
            using (GraphicsPath path = RoundedRect(bounds, 14))
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

        private void BuildLiquidShell()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1100, 740);
            this.MinimumSize = new Size(1116, 778);
            this.MaximizeBox = false;

            this.Controls.Remove(pnlHeader);
            this.Controls.Remove(pnlMain);
            this.Controls.Remove(pnlActions);
            this.Controls.Remove(pnlLog);

            _pnlSidebar = new Panel();
            _pnlSidebar.Name = "pnlSidebar";
            _pnlSidebar.Dock = DockStyle.Left;
            _pnlSidebar.Width = 220;
            _pnlSidebar.Padding = new Padding(12, 14, 12, 14);
            _pnlSidebar.Paint += Sidebar_Paint;

            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 56;
            pnlHeader.Padding = new Padding(24, 10, 22, 8);
            pnlHeader.Controls.Clear();
            pnlHeader.Paint += Topbar_Paint;

            _lblPageTitle = new Label();
            _lblPageTitle.AutoSize = true;
            _lblPageTitle.Location = new Point(24, 9);
            _lblPageTitle.Text = "Home";

            _lblPageSubtitle = new Label();
            _lblPageSubtitle.AutoSize = true;
            _lblPageSubtitle.Location = new Point(24, 30);
            _lblPageSubtitle.Text = "System profile loaded";

            _btnOsBadge = new RoundedButton();
            _btnOsBadge.Location = new Point(850, 14);
            _btnOsBadge.Size = new Size(130, 30);
            _btnOsBadge.Text = _osProfile;
            _btnOsBadge.Click += delegate { ShowPage("Settings"); };

            pnlHeader.Controls.Add(_lblPageTitle);
            pnlHeader.Controls.Add(_lblPageSubtitle);
            pnlHeader.Controls.Add(_btnOsBadge);

            _pnlContent = new Panel();
            _pnlContent.Dock = DockStyle.Fill;
            _pnlContent.Padding = new Padding(24);

            _pageHome = CreatePage("pageHome");
            _pageTweaks = CreatePage("pageTweaks");
            _pageLog = CreatePage("pageLog");
            _pageSettings = CreatePage("pageSettings");
            _pageAbout = CreatePage("pageAbout");

            BuildSidebar();
            BuildHomePage();
            BuildTweaksPage();
            BuildLogPage();
            BuildSettingsPage();
            BuildAboutPage();

            _pnlContent.Controls.AddRange(new Control[] { _pageHome, _pageTweaks, _pageLog, _pageSettings, _pageAbout });
            this.Controls.Add(_pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(_pnlSidebar);
            ShowPage("Home");
            this.ResumeLayout(false);
        }

        private Panel CreatePage(string name)
        {
            Panel page = new Panel();
            page.Name = name;
            page.Dock = DockStyle.Fill;
            page.AutoScroll = true;
            page.Visible = false;
            return page;
        }

        private void BuildSidebar()
        {
            // Brand block
            Panel brandBlock = new Panel();
            brandBlock.Name = "pnlBrand";
            brandBlock.Location = new Point(0, 0);
            brandBlock.Size = new Size(220, 68);
            brandBlock.BackColor = Color.Transparent;

            LightningMark logo = new LightningMark();
            logo.Name = "logoMark";
            logo.Location = new Point(18, 15);
            logo.Size = new Size(36, 36);
            logo.Click += delegate { ShowPage("Home"); };

            Label brandName = new Label();
            brandName.AutoSize = true;
            brandName.Location = new Point(62, 16);
            brandName.Text = "JaiDee";
            brandName.Font = new Font("Segoe UI", 14F, FontStyle.Regular);

            Label brandSub = new Label();
            brandSub.AutoSize = true;
            brandSub.Location = new Point(63, 38);
            brandSub.Text = "Optimize";
            brandSub.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);

            brandBlock.Controls.Add(logo);
            brandBlock.Controls.Add(brandName);
            brandBlock.Controls.Add(brandSub);
            _pnlSidebar.Controls.Add(brandBlock);

            // Separator after brand
            Panel brandSep = new Panel();
            brandSep.Name = "brandSep";
            brandSep.Location = new Point(18, 70);
            brandSep.Size = new Size(184, 1);
            brandSep.Paint += delegate(object s, PaintEventArgs ev)
            {
                ev.Graphics.Clear(((Control)s).BackColor);
                using (Pen p = new Pen(Color.FromArgb(30, 128, 128, 128), 1))
                {
                    ev.Graphics.DrawLine(p, 0, 0, ((Control)s).Width, 0);
                }
            };
            _pnlSidebar.Controls.Add(brandSep);

            // Nav section label
            Label navLabel = CreateSidebarSectionLabel("NAVIGATION", 18, 82);
            _pnlSidebar.Controls.Add(navLabel);

            _navHome = CreateNavButton("Home", "\uE80F", "Home", 100);
            _navTweaks = CreateNavButton("Tweaks", "\uE713", "Tweaks", 148);
            _navLog = CreateNavButton("Log", "\uE8A5", "Activity log", 196);

            // Tools section label
            Label toolsLabel = CreateSidebarSectionLabel("TOOLS", 18, 256);
            _pnlSidebar.Controls.Add(toolsLabel);

            _navSettings = CreateNavButton("Settings", "\uE713", "Settings", 274);
            _navAbout = CreateNavButton("About", "\uE946", "About", 322);

            _pnlSidebar.Controls.AddRange(new Control[] { _navHome, _navTweaks, _navLog, _navSettings, _navAbout });
        }

        private Label CreateSidebarSectionLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.Location = new Point(x, y);
            label.Size = new Size(184, 18);
            label.Text = text;
            label.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private RoundedButton CreateNavButton(string page, string icon, string label, int y)
        {
            RoundedButton button = new RoundedButton();
            button.Name = "nav" + page;
            button.Location = new Point(10, y);
            button.Size = new Size(200, 42);
            button.Text = icon + "  " + label;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 0, 0);
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            button.Tag = page;
            button.Click += delegate { ShowPage(page); };
            tipMain.SetToolTip(button, page);
            return button;
        }

        private void BuildHomePage()
        {
            // Hero panel — spans most of the width
            Panel hero = new Panel();
            hero.Name = "pnlHero";
            hero.Location = new Point(0, 0);
            hero.Size = new Size(860, 200);
            hero.Paint += HeroPanel_Paint;

            LightningMark heroIcon = new LightningMark();
            heroIcon.Location = new Point(28, 28);
            heroIcon.Size = new Size(80, 80);

            Label title = CreateTextLabel("Optimize your system", 128, 38, 440, 34, 22f, FontStyle.Regular);
            Label subtitle = CreateMutedLabel("Tweaks ready — " + _osProfile + " profile loaded", 128, 80, 460, 22);
            Label callout = CreateMutedLabel("Fine-tuned registry, timer, and debloat tweaks", 128, 108, 460, 20);

            RoundedButton optimize = new RoundedButton();
            ConfigureActionButton(optimize, "btnOptimizeNow", "Optimize now", "#185FA5", "#FFFFFF", 128, 148, 160, 38, 0);
            optimize.Click += btnApplyAll_Click;

            RoundedButton gotoTweaks = new RoundedButton();
            ConfigureActionButton(gotoTweaks, "btnGoTweaks", "View tweaks", "#F1F4F9", "#283545", 300, 148, 140, 38, 0);
            gotoTweaks.FlatAppearance.BorderSize = 1;
            gotoTweaks.Click += delegate { ShowPage("Tweaks"); };

            hero.Controls.AddRange(new Control[] { heroIcon, title, subtitle, callout, optimize, gotoTweaks });
            _pageHome.Controls.Add(hero);

            // Stats row
            Panel stats = new Panel();
            stats.Name = "pnlStats";
            stats.Location = new Point(0, 220);
            stats.Size = new Size(860, 100);
            stats.Paint += GlassPanel_Paint;
            AddStatCell(stats, "Active tweaks", "14+", 30);
            AddStatCell(stats, "OS profile", _osProfile, 318);
            AddStatCell(stats, "Last run", "Not yet", 606);
            _pageHome.Controls.Add(stats);

            // Quick status panel
            Panel status = new Panel();
            status.Name = "pnlQuickStatus";
            status.Location = new Point(0, 338);
            status.Size = new Size(414, 220);
            status.Paint += GlassPanel_Paint;
            status.Controls.Add(CreateTextLabel("Category overview", 20, 16, 280, 24, 12.5f, FontStyle.Regular));
            status.Controls.Add(CreateStatusRow("Priority & Scheduling", "5 tweaks", "#185FA5", 22, 54));
            status.Controls.Add(CreateStatusRow("Latency & Timer", "3 tweaks", "#0E8A5E", 22, 96));
            status.Controls.Add(CreateStatusRow("Memory & Security", "3 tweaks", "#7C3AED", 22, 138));
            status.Controls.Add(CreateStatusRow("Legacy INI", "3 tweaks", "#B45309", 22, 180));
            _pageHome.Controls.Add(status);

            // Tips panel
            Panel tips = new Panel();
            tips.Name = "pnlTips";
            tips.Location = new Point(434, 338);
            tips.Size = new Size(426, 220);
            tips.Paint += GlassPanel_Paint;
            tips.Controls.Add(CreateTextLabel("Quick guide", 20, 16, 280, 24, 12.5f, FontStyle.Regular));
            tips.Controls.Add(CreateMutedLabel("1. Go to Tweaks — review each group", 22, 54, 380, 20));
            tips.Controls.Add(CreateMutedLabel("2. Enable or disable individual tweaks", 22, 82, 380, 20));
            tips.Controls.Add(CreateMutedLabel("3. Click Apply selected to run", 22, 110, 380, 20));
            tips.Controls.Add(CreateMutedLabel("4. Restart if prompted to complete", 22, 138, 380, 20));
            tips.Controls.Add(CreateMutedLabel("5. Use Restore defaults to revert", 22, 166, 380, 20));
            _pageHome.Controls.Add(tips);
        }

        private Panel CreateStatusRow(string labelText, string valueText, string accentHex, int x, int y)
        {
            Panel row = new Panel();
            row.Location = new Point(x, y);
            row.Size = new Size(370, 34);
            row.BackColor = Color.Transparent;

            Panel accent = new Panel();
            accent.Location = new Point(0, 6);
            accent.Size = new Size(3, 20);
            accent.BackColor = ColorTranslator.FromHtml(accentHex);

            Label lbl = new Label();
            lbl.AutoSize = false;
            lbl.Location = new Point(12, 8);
            lbl.Size = new Size(220, 18);
            lbl.Text = labelText;
            lbl.Font = new Font("Segoe UI", 9.2F, FontStyle.Regular);

            Label val = new Label();
            val.AutoSize = false;
            val.Location = new Point(240, 8);
            val.Size = new Size(120, 18);
            val.Text = valueText;
            val.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            val.TextAlign = ContentAlignment.MiddleRight;
            val.ForeColor = ColorTranslator.FromHtml(accentHex);

            row.Controls.Add(accent);
            row.Controls.Add(lbl);
            row.Controls.Add(val);
            return row;
        }

        private Label CreateStatLabel(string caption, string value, int x)
        {
            // Delegate to AddStatCell pattern — but since we return Label here,
            // and caller adds to stats.Controls, we return a hidden placeholder.
            // This method should not be called directly; use AddStatCell instead.
            Label lbl = new Label();
            lbl.AutoSize = false;
            lbl.Location = new Point(x, 12);
            lbl.Size = new Size(248, 76);
            lbl.Text = value + "\n" + caption;
            lbl.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            return lbl;
        }

        private void AddStatCell(Panel statsPanel, string caption, string value, int x)
        {
            Panel cell = new Panel();
            cell.BackColor = Color.Transparent;
            cell.Location = new Point(x, 12);
            cell.Size = new Size(256, 76);

            Label valLabel = new Label();
            valLabel.AutoSize = false;
            valLabel.Location = new Point(0, 4);
            valLabel.Size = new Size(256, 36);
            valLabel.Text = value;
            valLabel.Font = new Font("Segoe UI", 20F, FontStyle.Regular);

            Label captionLabel = new Label();
            captionLabel.AutoSize = false;
            captionLabel.Location = new Point(0, 42);
            captionLabel.Size = new Size(256, 18);
            captionLabel.Text = caption;
            captionLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);

            cell.Controls.Add(valLabel);
            cell.Controls.Add(captionLabel);
            statsPanel.Controls.Add(cell);
        }

        private void BuildTweaksPage()
        {
            pnlMain.Parent = _pageTweaks;
            pnlActions.Parent = _pageTweaks;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Size = new Size(860, 590);
            pnlMain.Padding = new Padding(0, 0, 10, 12);
            pnlActions.Location = new Point(0, 608);
            pnlActions.Size = new Size(860, 58);
            btnApplyAll.Location = new Point(0, 8);
            btnApplyAll.Size = new Size(180, 40);
            btnRestoreDefaults.Location = new Point(192, 8);
            btnRestoreDefaults.Size = new Size(166, 40);
            btnSelectAll.Location = new Point(370, 8);
            btnSelectAll.Size = new Size(148, 40);
            prgApply.Location = new Point(0, 52);
            prgApply.Size = new Size(860, 5);
            NormalizeTweakCards();
        }

        private void NormalizeTweakCards()
        {
            ConfigureTweakCard(grpPriority, 0, 0, 840, 222);
            ConfigureTweakCard(grpLatency, 0, 238, 840, 160);
            ConfigureTweakCard(grpMemory, 0, 414, 840, 160);
            ConfigureTweakCard(grpIniTweaks, 0, 590, 840, 160);
            ConfigureTweakCard(grpDebloat, 0, 766, 840, 384);

            BuildDebloatSegmentedSelector();
        }

        private void BuildDebloatSegmentedSelector()
        {
            tabDebloat.Visible = false;
            grpDebloat.Controls.Remove(tabDebloat);

            if (_btnWin11DebloatSegment == null)
            {
                _btnWin11DebloatSegment = new RoundedButton();
                _btnWin11DebloatSegment.Name = "btnWin11DebloatSegment";
                _btnWin11DebloatSegment.Text = "Windows 11";
                _btnWin11DebloatSegment.Location = new Point(18, 44);
                _btnWin11DebloatSegment.Size = new Size(120, 30);
                _btnWin11DebloatSegment.Click += delegate
                {
                    _osProfile = "Windows 11";
                    _btnOsBadge.Text = _osProfile;
                    ApplyOsProfileRules();
                    ApplyTheme();
                    UpdatePageHeader();
                };
                grpDebloat.Controls.Add(_btnWin11DebloatSegment);
            }

            if (_btnWin10DebloatSegment == null)
            {
                _btnWin10DebloatSegment = new RoundedButton();
                _btnWin10DebloatSegment.Name = "btnWin10DebloatSegment";
                _btnWin10DebloatSegment.Text = "Windows 10";
                _btnWin10DebloatSegment.Location = new Point(146, 44);
                _btnWin10DebloatSegment.Size = new Size(120, 30);
                _btnWin10DebloatSegment.Click += delegate
                {
                    _osProfile = "Windows 10";
                    _btnOsBadge.Text = _osProfile;
                    ApplyOsProfileRules();
                    ApplyTheme();
                    UpdatePageHeader();
                };
                grpDebloat.Controls.Add(_btnWin10DebloatSegment);
            }

            if (_pnlDebloatHost == null)
            {
                _pnlDebloatHost = new Panel();
                _pnlDebloatHost.Name = "pnlDebloatHost";
                _pnlDebloatHost.Location = new Point(18, 84);
                _pnlDebloatHost.Size = new Size(636, 258);
                _pnlDebloatHost.BackColor = _theme.Surface;
                grpDebloat.Controls.Add(_pnlDebloatHost);
            }

            flpWin10Debloat.Parent = _pnlDebloatHost;
            flpWin11Debloat.Parent = _pnlDebloatHost;
            flpWin10Debloat.Dock = DockStyle.Fill;
            flpWin11Debloat.Dock = DockStyle.Fill;
            ApplyOsProfileRules();
        }

        private void ConfigureTweakCard(Panel panel, int x, int y, int width, int height)
        {
            panel.Location = new Point(x, y);
            panel.Size = new Size(width, height);
            foreach (Control child in panel.Controls)
            {
                ToggleSwitch toggle = child as ToggleSwitch;
                if (toggle != null)
                {
                    toggle.Location = new Point(20, toggle.Location.Y);
                    toggle.Size = new Size(width - 86, 24);
                }

                Button info = child as Button;
                if (info != null)
                {
                    info.Location = new Point(width - 48, info.Location.Y);
                }
            }
        }

        private void BuildLogPage()
        {
            pnlLog.Parent = _pageLog;
            pnlLog.Dock = DockStyle.None;
            pnlLog.Location = new Point(0, 0);
            pnlLog.Size = new Size(860, 640);
            lblLogTitle.Location = new Point(18, 18);
            btnClearLog.Location = new Point(754, 14);
            btnClearLog.Size = new Size(86, 30);
            _btnExportLog = new RoundedButton();
            ConfigureActionButton(_btnExportLog, "btnExportLog", "Export .txt", "#F1F4F9", "#6B7280", 648, 14, 98, 30, 20);
            _btnExportLog.Click += btnExportLog_Click;
            rtbLog.Location = new Point(18, 58);
            rtbLog.Size = new Size(824, 552);
            pnlLog.Controls.Add(_btnExportLog);
        }

        private void BuildSettingsPage()
        {
            Panel profile = new Panel();
            profile.Name = "pnlProfile";
            profile.Location = new Point(0, 0);
            profile.Size = new Size(860, 180);
            profile.Paint += GlassPanel_Paint;
            profile.Controls.Add(CreateTextLabel("OS profile", 20, 16, 220, 22, 14f, FontStyle.Regular));
            profile.Controls.Add(CreateMutedLabel("Choose your Windows version to apply the correct tweak set", 20, 42, 640, 20));
            profile.Controls.Add(CreateProfileButton("Windows 11", 20, 68));
            profile.Controls.Add(CreateProfileButton("Windows 10", 248, 68));
            _lblWin10Warning = CreateMutedLabel("Windows 10 profile adjusts registry paths and skips Win11-only tweaks. Review the Tweaks page before applying.", 20, 146, 820, 28);
            _pageSettings.Controls.Add(profile);
            profile.Controls.Add(_lblWin10Warning);

            Panel prefs = new Panel();
            prefs.Name = "pnlPreferences";
            prefs.Location = new Point(0, 200);
            prefs.Size = new Size(860, 160);
            prefs.Paint += GlassPanel_Paint;
            prefs.Controls.Add(CreateTextLabel("Preferences", 20, 16, 220, 22, 14f, FontStyle.Regular));
            lblLanguage.Location = new Point(24, 60);
            cmbLanguage.Location = new Point(140, 56);
            lblTheme.Location = new Point(24, 102);
            chkTheme.Location = new Point(140, 98);
            prefs.Controls.Add(lblLanguage);
            prefs.Controls.Add(cmbLanguage);
            prefs.Controls.Add(lblTheme);
            prefs.Controls.Add(chkTheme);
            _pageSettings.Controls.Add(prefs);
        }

        private RoundedButton CreateProfileButton(string profile, int x, int y)
        {
            RoundedButton button = new RoundedButton();
            button.Location = new Point(x, y);
            button.Size = new Size(192, 72);
            button.Text = profile;
            button.Tag = profile;
            button.Click += delegate
            {
                _osProfile = profile;
                _btnOsBadge.Text = _osProfile;
                ApplyOsProfileRules();
                ApplyTheme();
                UpdatePageHeader();
            };
            return button;
        }

        private void BuildAboutPage()
        {
            Panel about = new Panel();
            about.Name = "pnlAbout";
            about.Location = new Point(0, 0);
            about.Size = new Size(860, 360);
            about.Paint += GlassPanel_Paint;
            about.Controls.Add(CreateTextLabel("JaiDee-Optimize", 24, 24, 380, 32, 22f, FontStyle.Regular));
            about.Controls.Add(CreateMutedLabel("Minimal Windows gaming optimizer", 24, 64, 420, 22));
            about.Controls.Add(CreateMutedLabel("Version: 1.0", 24, 112, 420, 22));
            about.Controls.Add(CreateMutedLabel("Platform: .NET Framework 4.8 WinForms", 24, 142, 480, 22));
            about.Controls.Add(CreateMutedLabel("Requirements: Administrator privileges", 24, 172, 480, 22));
            about.Controls.Add(CreateMutedLabel("Registry backup: Session based restore", 24, 202, 480, 22));
            about.Controls.Add(CreateMutedLabel("Profile: " + _osProfile, 24, 232, 480, 22));
            about.Controls.Add(CreateMutedLabel("Review selected tweaks before applying. Some debloat changes require Store or PowerShell reinstall to revert.", 24, 290, 800, 44));
            _pageAbout.Controls.Add(about);
        }

        private Label CreateTextLabel(string text, int x, int y, int w, int h, float size, FontStyle style)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.Location = new Point(x, y);
            label.Size = new Size(w, h);
            label.Text = text;
            label.Font = new Font("Segoe UI", size, style);
            return label;
        }

        private Label CreateMutedLabel(string text, int x, int y, int w, int h)
        {
            Label label = CreateTextLabel(text, x, y, w, h, 9f, FontStyle.Regular);
            return label;
        }

        private void ShowPage(string page)
        {
            _activePage = page;
            if (_pageHome != null) _pageHome.Visible = page == "Home";
            if (_pageTweaks != null) _pageTweaks.Visible = page == "Tweaks";
            if (_pageLog != null) _pageLog.Visible = page == "Log";
            if (_pageSettings != null) _pageSettings.Visible = page == "Settings";
            if (_pageAbout != null) _pageAbout.Visible = page == "About";
            UpdatePageHeader();
            ApplyTheme();
        }

        private void ApplyOsProfileRules()
        {
            bool win10 = _osProfile == "Windows 10";
            if (tabDebloat != null)
            {
                tabDebloat.SelectedTab = win10 ? tabWin10Debloat : tabWin11Debloat;
            }

            if (flpWin10Debloat != null)
            {
                flpWin10Debloat.Visible = win10;
            }

            if (flpWin11Debloat != null)
            {
                flpWin11Debloat.Visible = !win10;
            }

            if (chkDynamicTick != null)
            {
                chkDynamicTick.Enabled = !win10 && !_isBusy;
            }

            SetDebloatFlowEnabled(flpWin11Debloat, !win10);
            SetDebloatFlowEnabled(flpWin10Debloat, win10);
        }

        private void SetDebloatFlowEnabled(Control root, bool enabled)
        {
            if (root == null)
            {
                return;
            }

            foreach (Control child in root.Controls)
            {
                if (child is CheckBox || child is Button)
                {
                    child.Enabled = enabled && !_isBusy;
                }

                if (child.HasChildren)
                {
                    SetDebloatFlowEnabled(child, enabled);
                }
            }
        }

        private void UpdatePageHeader()
        {
            if (_lblPageTitle == null)
            {
                return;
            }

            _lblPageTitle.Text = _activePage;
            _lblPageSubtitle.Text = _activePage == "Home"
                ? "Optimize your system"
                : (_activePage == "Tweaks" ? "Review selected registry, INI, and debloat actions" :
                (_activePage == "Log" ? "Operation log and export tools" :
                (_activePage == "Settings" ? "Theme and OS profile" : "App details and safety notes")));
        }

        private void GlassPanel_Paint(object sender, PaintEventArgs e)
        {
            Control control = sender as Control;
            if (control == null || _theme == null)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(control.Parent == null ? _theme.Background : control.Parent.BackColor);
            Rectangle bounds = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
            using (GraphicsPath path = RoundedRect(bounds, 14))
            using (Brush brush = new SolidBrush(_theme.Glass))
            using (Pen pen = new Pen(_theme.Border))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private void HeroPanel_Paint(object sender, PaintEventArgs e)
        {
            Control control = sender as Control;
            if (control == null || _theme == null)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(control.Parent == null ? _theme.Background : control.Parent.BackColor);
            Rectangle bounds = new Rectangle(0, 0, control.Width - 1, control.Height - 1);

            // Subtle gradient tint from accent color for the hero
            Color topColor = _theme.AccentTint;
            Color midColor = _theme.Glass;
            using (GraphicsPath path = RoundedRect(bounds, 14))
            {
                using (System.Drawing.Drawing2D.LinearGradientBrush grad =
                    new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Point(0, 0), new Point(control.Width, control.Height),
                        topColor, midColor))
                {
                    e.Graphics.FillPath(grad, path);
                }
                using (Pen pen = new Pen(_theme.AccentBorder))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void Sidebar_Paint(object sender, PaintEventArgs e)
        {
            Control control = sender as Control;
            if (control == null || _theme == null)
            {
                return;
            }

            e.Graphics.Clear(_theme.Surface);
            // Right edge separator line
            using (Pen pen = new Pen(_theme.Border, 1))
            {
                e.Graphics.DrawLine(pen, control.Width - 1, 0, control.Width - 1, control.Height);
            }
        }

        private void Topbar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(_theme.Surface);
            using (Pen pen = new Pen(_theme.Border))
            {
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            }
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

            ShowLargeInfoDialog(TweakName(tweak.Id), TweakDescription(tweak.Id), BuildInfoText(tweak), tweak.Type == TweakType.Debloat);
        }

        private DialogResult ShowLargeDecisionDialog(string title, string subtitle, string body, string primaryText, string secondaryText, bool warning)
        {
            return ShowLargeDialog(title, subtitle, body, primaryText, secondaryText, warning, true);
        }

        private void ShowLargeInfoDialog(string title, string subtitle, string body, bool warning)
        {
            ShowLargeDialog(title, subtitle, body, L("done"), null, warning, false);
        }

        private DialogResult ShowLargeDialog(string title, string subtitle, string body, string primaryText, string secondaryText, bool warning, bool decision)
        {
            int width = Math.Max(560, Math.Min(720, this.ClientSize.Width - 80));
            int height = Math.Max(360, Math.Min(500, this.ClientSize.Height - 80));

            using (Form dialog = new Form())
            {
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.None;
                dialog.ShowInTaskbar = false;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.Size = new Size(width, height);
                dialog.BackColor = _theme.Surface;
                dialog.ForeColor = _theme.Text;
                dialog.Font = CreateFont(ResolveLanguageFontFamily(), 9F, FontStyle.Regular);
                dialog.Padding = new Padding(1);

                dialog.Paint += delegate(object s, PaintEventArgs e)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle bounds = new Rectangle(0, 0, dialog.Width - 1, dialog.Height - 1);
                    using (GraphicsPath path = RoundedRect(bounds, 16))
                    using (Brush brush = new SolidBrush(_theme.Surface))
                    using (Pen pen = new Pen(warning ? _theme.WarningBorder : _theme.AccentBorder, 1))
                    {
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }
                };

                dialog.SizeChanged += delegate
                {
                    using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, dialog.Width, dialog.Height), 16))
                    {
                        dialog.Region = new Region(path);
                    }
                };

                Panel header = new Panel();
                header.Location = new Point(1, 1);
                header.Size = new Size(width - 2, 72);
                header.BackColor = _theme.Surface;

                Label titleLabel = CreateTextLabel(title, 24, 16, width - 112, 24, 13.5F, FontStyle.Regular);
                titleLabel.ForeColor = _theme.Text;
                Label subtitleLabel = CreateMutedLabel(subtitle, 24, 42, width - 112, 18);
                subtitleLabel.ForeColor = warning ? _theme.Warning : _theme.Muted;

                RoundedButton close = new RoundedButton();
                close.Text = "\uE711";
                close.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular);
                close.Location = new Point(width - 52, 18);
                close.Size = new Size(30, 30);
                close.Click += delegate { dialog.DialogResult = decision ? DialogResult.No : DialogResult.OK; dialog.Close(); };
                ConfigureThemedButton(close, _theme.Control, _theme.Muted, _theme.Border, 1);
                header.Controls.AddRange(new Control[] { titleLabel, subtitleLabel, close });

                Panel bodyPanel = new Panel();
                bodyPanel.Location = new Point(24, 88);
                bodyPanel.Size = new Size(width - 48, height - 168);
                bodyPanel.BackColor = _theme.Control;
                bodyPanel.Paint += delegate(object s, PaintEventArgs e)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.Clear(_theme.Surface);
                    Rectangle bounds = new Rectangle(0, 0, bodyPanel.Width - 1, bodyPanel.Height - 1);
                    using (GraphicsPath path = RoundedRect(bounds, 12))
                    using (Brush brush = new SolidBrush(_theme.Glass))
                    using (Pen pen = new Pen(_theme.Border, 1))
                    {
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }
                };

                RichTextBox bodyText = new RichTextBox();
                bodyText.BorderStyle = BorderStyle.None;
                bodyText.ReadOnly = true;
                bodyText.ScrollBars = RichTextBoxScrollBars.Vertical;
                bodyText.WordWrap = true;
                bodyText.BackColor = _theme.Glass;
                bodyText.ForeColor = _theme.Text;
                bodyText.Font = CreateFont(ResolveLanguageFontFamily(), 9.2F, FontStyle.Regular);
                bodyText.Location = new Point(18, 16);
                bodyText.Size = new Size(bodyPanel.Width - 36, bodyPanel.Height - 32);
                bodyText.Text = body;
                bodyPanel.Controls.Add(bodyText);

                Panel footer = new Panel();
                footer.Location = new Point(1, height - 72);
                footer.Size = new Size(width - 2, 70);
                footer.BackColor = _theme.Surface;

                RoundedButton primary = new RoundedButton();
                primary.Text = primaryText;
                primary.Size = new Size(decision ? 138 : 110, 38);
                primary.Location = new Point(width - primary.Width - 24, 16);
                primary.Click += delegate { dialog.DialogResult = DialogResult.Yes; dialog.Close(); };
                ConfigureThemedButton(primary, warning ? _theme.Warning : _theme.Accent, ColorTranslator.FromHtml("#FFFFFF"), Color.Empty, 0);

                footer.Controls.Add(primary);

                if (!string.IsNullOrEmpty(secondaryText))
                {
                    RoundedButton secondary = new RoundedButton();
                    secondary.Text = secondaryText;
                    secondary.Size = new Size(108, 38);
                    secondary.Location = new Point(primary.Left - 116, 16);
                    secondary.Click += delegate { dialog.DialogResult = DialogResult.No; dialog.Close(); };
                    ConfigureThemedButton(secondary, _theme.Control, _theme.SecondaryText, _theme.Border, 1);
                    footer.Controls.Add(secondary);
                }

                dialog.Controls.AddRange(new Control[] { header, bodyPanel, footer });
                dialog.AcceptButton = primary;
                dialog.CancelButton = close;
                return dialog.ShowDialog(this);
            }
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
                chkIniWinLoad
            }.Concat(_dynamicDebloatCheckBoxes ?? Enumerable.Empty<CheckBox>());
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

            ApplyOsProfileRules();
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
            tabWin10Debloat.Text = L("win10Debloat");
            tabWin11Debloat.Text = L("win11Debloat");
            if (_btnWin10DebloatSegment != null) _btnWin10DebloatSegment.Text = L("win10Debloat");
            if (_btnWin11DebloatSegment != null) _btnWin11DebloatSegment.Text = L("win11Debloat");

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
            pnlHeader.BackColor = _theme.Surface;
            pnlMain.BackColor = _theme.Background;
            pnlActions.BackColor = _theme.Background;
            pnlLog.BackColor = _theme.Surface;
            if (pnlHeader != null && _btnOsBadge != null)
            {
                _btnOsBadge.Location = new Point(Math.Max(700, pnlHeader.Width - 158), 14);
            }
            if (_pnlSidebar != null) _pnlSidebar.BackColor = _theme.Surface;
            if (_pnlContent != null) _pnlContent.BackColor = _theme.Background;
            if (_pageHome != null) _pageHome.BackColor = _theme.Background;
            if (_pageTweaks != null) _pageTweaks.BackColor = _theme.Background;
            if (_pageLog != null) _pageLog.BackColor = _theme.Background;
            if (_pageSettings != null) _pageSettings.BackColor = _theme.Background;
            if (_pageAbout != null) _pageAbout.BackColor = _theme.Background;
            pnlBrandMark.BackColor = _theme.Background;
            pnlBrandMark.AccentColor = _theme.Accent;
            pnlBrandMark.SurfaceColor = _theme.Surface;
            lblAppTitle.ForeColor = _theme.Title;
            lblSubtitle.ForeColor = _theme.Muted;
            if (_lblPageTitle != null) _lblPageTitle.ForeColor = _theme.Text;
            if (_lblPageSubtitle != null) _lblPageSubtitle.ForeColor = _theme.Muted;
            lblLanguage.ForeColor = _theme.Muted;
            lblTheme.ForeColor = _theme.Muted;
            lblLogTitle.ForeColor = _theme.Text;
            tabDebloat.BackColor = _theme.Surface;
            tabDebloat.ForeColor = _theme.Text;
            tabWin10Debloat.BackColor = _theme.Surface;
            tabWin10Debloat.ForeColor = _theme.Text;
            tabWin11Debloat.BackColor = _theme.Surface;
            tabWin11Debloat.ForeColor = _theme.Text;
            flpWin10Debloat.BackColor = _theme.Surface;
            flpWin11Debloat.BackColor = _theme.Surface;

            cmbLanguage.BackColor = _theme.Control;
            cmbLanguage.ForeColor = _theme.Text;
            rtbLog.BackColor = _theme.LogBackground;
            rtbLog.ForeColor = _theme.LogText;

            ConfigureThemedButton(btnApplyAll, _theme.Accent, _theme.AccentText, Color.Empty, 0);
            ConfigureThemedButton(btnRestoreDefaults, _theme.Control, _theme.Warning, _theme.WarningBorder, 1);
            ConfigureThemedButton(btnSelectAll, _theme.Control, _theme.SecondaryText, _theme.Border, 1);
            ConfigureThemedButton(btnClearLog, _theme.Control, _theme.Muted, _theme.Border, 1);
            if (_btnExportLog != null) ConfigureThemedButton(_btnExportLog, _theme.Control, _theme.Muted, _theme.Border, 1);
            if (_btnOsBadge != null)
            {
                Color badgeBack = _osProfile == "Windows 10" ? Color.FromArgb(28, _theme.Warning) : _theme.AccentTint;
                Color badgeBorder = _osProfile == "Windows 10" ? _theme.WarningBorder : _theme.AccentBorder;
                Color badgeText = _osProfile == "Windows 10" ? _theme.Warning : _theme.Accent;
                ConfigureThemedButton(_btnOsBadge, badgeBack, badgeText, badgeBorder, 1);
            }
            if (_lblWin10Warning != null)
            {
                _lblWin10Warning.Visible = _osProfile == "Windows 10";
                _lblWin10Warning.ForeColor = _theme.Warning;
            }

            ApplyNavButtonTheme(_navHome, "Home");
            ApplyNavButtonTheme(_navTweaks, "Tweaks");
            ApplyNavButtonTheme(_navLog, "Log");
            ApplyNavButtonTheme(_navSettings, "Settings");
            ApplyNavButtonTheme(_navAbout, "About");
            ApplyNavFonts();

            // Apply sidebar brand & section label colors
            if (_pnlSidebar != null)
            {
                foreach (Control child in _pnlSidebar.Controls)
                {
                    Label sectionLbl = child as Label;
                    if (sectionLbl != null)
                    {
                        sectionLbl.BackColor = _theme.Surface;
                        sectionLbl.ForeColor = _theme.Muted;
                    }
                    Panel brandBlock = child as Panel;
                    if (brandBlock != null && brandBlock.Name == "pnlBrand")
                    {
                        brandBlock.BackColor = _theme.Surface;
                        foreach (Control c in brandBlock.Controls)
                        {
                            c.BackColor = _theme.Surface;
                            c.ForeColor = c.Font != null && c.Font.Size >= 12f ? _theme.Title : _theme.Muted;
                        }
                    }
                }
                _pnlSidebar.BackColor = _theme.Surface;
                _pnlSidebar.Invalidate();
            }

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
            ApplyDebloatTheme(flpWin10Debloat);
            ApplyDebloatTheme(flpWin11Debloat);
            ApplyDebloatSegmentTheme();
            chkTheme.BackColor = _theme.Background;
            tipMain.BackColor = _theme.Control;
            tipMain.ForeColor = _theme.Text;
            ApplyLiquidThemeRecursive(_pageHome);
            ApplyLiquidThemeRecursive(_pageSettings);
            ApplyLiquidThemeRecursive(_pageAbout);
            if (_pnlSidebar != null) _pnlSidebar.Invalidate();
            pnlLog.Invalidate();
            pnlBrandMark.Invalidate();
            this.Invalidate();
        }

        private void ApplyDebloatSegmentTheme()
        {
            if (_btnWin11DebloatSegment == null || _btnWin10DebloatSegment == null)
            {
                return;
            }

            bool win10 = _osProfile == "Windows 10";
            ConfigureThemedButton(_btnWin11DebloatSegment, !win10 ? _theme.AccentTint : _theme.Control, !win10 ? _theme.Accent : _theme.SecondaryText, !win10 ? _theme.AccentBorder : _theme.Border, 1);
            ConfigureThemedButton(_btnWin10DebloatSegment, win10 ? Color.FromArgb(28, _theme.Warning) : _theme.Control, win10 ? _theme.Warning : _theme.SecondaryText, win10 ? _theme.WarningBorder : _theme.Border, 1);

            if (_pnlDebloatHost != null)
            {
                _pnlDebloatHost.BackColor = _theme.Surface;
                _pnlDebloatHost.Invalidate();
            }
        }

        private void ApplyNavFonts()
        {
            Font navFont = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            foreach (RoundedButton button in new[] { _navHome, _navTweaks, _navLog, _navSettings, _navAbout })
            {
                if (button != null)
                {
                    button.Font = navFont;
                }
            }
        }

        private void ApplyNavButtonTheme(RoundedButton button, string page)
        {
            if (button == null)
            {
                return;
            }

            bool active = string.Equals(_activePage, page, StringComparison.OrdinalIgnoreCase);
            Color backColor = active ? _theme.AccentTint : Color.Transparent;
            Color foreColor = active ? _theme.Accent : _theme.Muted;
            Color borderColor = active ? _theme.AccentBorder : Color.Transparent;
            int borderSize = active ? 1 : 0;
            ConfigureThemedButton(button, backColor, foreColor, borderColor, borderSize);
        }

        private void ApplyLiquidThemeRecursive(Control root)
        {
            if (root == null)
            {
                return;
            }

            foreach (Control child in root.Controls)
            {
                Label label = child as Label;
                if (label != null)
                {
                    label.BackColor = Color.Transparent;
                    if (label.ForeColor != _theme.Warning)
                    {
                        label.ForeColor = label.Font.Size <= 9.1f ? _theme.Muted : _theme.Text;
                    }
                }

                RoundedButton button = child as RoundedButton;
                if (button != null && button.Tag is string)
                {
                    bool selectedProfile = string.Equals((string)button.Tag, _osProfile, StringComparison.OrdinalIgnoreCase);
                    ConfigureThemedButton(button, selectedProfile ? _theme.AccentTint : _theme.Control, selectedProfile ? _theme.Accent : _theme.SecondaryText, selectedProfile ? _theme.AccentBorder : _theme.Border, 1);
                }

                LightningMark mark = child as LightningMark;
                if (mark != null)
                {
                    mark.BackColor = child.Parent == null ? _theme.Background : child.Parent.BackColor;
                    mark.AccentColor = _theme.Accent;
                    mark.SurfaceColor = _theme.AccentTint;
                    mark.Invalidate();
                }

                if (child is Panel)
                {
                    child.BackColor = _theme.Background;
                    child.Invalidate();
                }

                if (child.HasChildren)
                {
                    ApplyLiquidThemeRecursive(child);
                }
            }
        }

        private void ApplyLanguageFont()
        {
            FontFamily family = ResolveLanguageFontFamily();
            ApplyFontRecursive(this, family);

            lblAppTitle.Font = CreateFont(family, 20F, FontStyle.Regular);
            lblSubtitle.Font = CreateFont(family, 8.8F, FontStyle.Regular);
            lblLanguage.Font = CreateFont(family, 8.5F, FontStyle.Regular);
            lblTheme.Font = CreateFont(family, 8.5F, FontStyle.Regular);
            lblLogTitle.Font = CreateFont(family, 10F, FontStyle.Regular);
            if (_lblPageTitle != null) _lblPageTitle.Font = CreateFont(family, 10F, FontStyle.Regular);
            if (_lblPageSubtitle != null) _lblPageSubtitle.Font = CreateFont(family, 8.5F, FontStyle.Regular);
            rtbLog.Font = CreateFont(family, 8.7F, FontStyle.Regular);

            foreach (Panel panel in new[] { grpPriority, grpLatency, grpMemory, grpIniTweaks, grpDebloat })
            {
                foreach (Control child in panel.Controls)
                {
                    if (child is Label)
                    {
                        child.Font = CreateFont(family, 10F, FontStyle.Regular);
                    }
                    else if (child is Button)
                    {
                        child.Font = CreateFont(family, 8F, FontStyle.Regular);
                    }
                    else if (child is CheckBox)
                    {
                        child.Font = CreateFont(family, 9.2F, FontStyle.Regular);
                    }
                }
            }

            foreach (Button button in new[] { btnApplyAll, btnRestoreDefaults, btnSelectAll })
            {
                button.Font = CreateFont(family, 9.8F, FontStyle.Regular);
            }

            btnClearLog.Font = CreateFont(family, 8.2F, FontStyle.Regular);
            chkTheme.Font = CreateFont(family, 8.5F, FontStyle.Regular);
        }

        private void BuildDebloatPages()
        {
            if (_tweaksById == null)
            {
                return;
            }

            _dynamicDebloatCheckBoxes.Clear();
            flpWin10Debloat.Controls.Clear();
            flpWin11Debloat.Controls.Clear();

            AddDebloatRows(flpWin10Debloat, _tweaksById.Values.Where(t => t.Type == TweakType.Debloat && string.Equals(t.Category, "Win10Debloat", StringComparison.OrdinalIgnoreCase)));
            AddDebloatRows(flpWin11Debloat, _tweaksById.Values.Where(t => t.Type == TweakType.Debloat && string.Equals(t.Category, "Win11Debloat", StringComparison.OrdinalIgnoreCase)));
        }

        private void AddDebloatRows(FlowLayoutPanel target, IEnumerable<TweakDefinition> tweaks)
        {
            string currentGroup = null;
            foreach (TweakDefinition tweak in tweaks.OrderBy(t => t.ValueName).ThenBy(t => t.DisplayName))
            {
                string group = string.IsNullOrWhiteSpace(tweak.ValueName) ? L("misc") : tweak.ValueName;
                if (!string.Equals(group, currentGroup, StringComparison.OrdinalIgnoreCase))
                {
                    target.Controls.Add(CreateDebloatSectionLabel(group));
                    currentGroup = group;
                }

                Panel row = new Panel();
                row.BackColor = _theme.Surface;
                row.Margin = new Padding(0, 0, 0, 6);
                row.Size = new Size(580, 30);

                ToggleSwitch toggle = new ToggleSwitch();
                toggle.BackColor = _theme.Surface;
                toggle.Checked = false;
                toggle.CheckState = CheckState.Unchecked;
                toggle.Cursor = Cursors.Hand;
                toggle.FlatStyle = FlatStyle.Flat;
                toggle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                toggle.ForeColor = _theme.Text;
                toggle.Location = new Point(0, 2);
                toggle.Name = "chk" + tweak.Id;
                toggle.Size = new Size(532, 24);
                toggle.TabIndex = 50 + _dynamicDebloatCheckBoxes.Count;
                toggle.Tag = tweak.Id;
                toggle.Text = tweak.DisplayName;
                toggle.UseVisualStyleBackColor = false;
                toggle.CheckedChanged += new EventHandler(this.chkTweak_CheckedChanged);
                ApplyToggleTheme(toggle);

                Button info = new RoundedButton();
                info.BackColor = _theme.Control;
                info.Cursor = Cursors.Hand;
                info.FlatAppearance.BorderColor = _theme.Border;
                info.FlatAppearance.BorderSize = 1;
                info.FlatStyle = FlatStyle.Flat;
                info.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
                info.ForeColor = _theme.SecondaryText;
                info.Location = new Point(548, 3);
                info.Name = "btnInfo" + tweak.Id;
                info.Size = new Size(24, 22);
                info.TabIndex = 200 + _dynamicDebloatCheckBoxes.Count;
                info.Tag = tweak.Id;
                info.Text = "i";
                info.UseVisualStyleBackColor = false;
                info.Click += new EventHandler(this.btnInfo_Click);

                row.Controls.Add(toggle);
                row.Controls.Add(info);
                target.Controls.Add(row);
                _dynamicDebloatCheckBoxes.Add(toggle);
            }
        }

        private Label CreateDebloatSectionLabel(string text)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.BackColor = _theme.Surface;
            label.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            label.ForeColor = _theme.Muted;
            label.Margin = new Padding(0, 8, 0, 3);
            label.Size = new Size(580, 20);
            label.Text = text;
            return label;
        }

        private void ApplyDebloatTheme(Control root)
        {
            foreach (Control child in root.Controls)
            {
                child.BackColor = _theme.Surface;
                child.ForeColor = child is Label ? _theme.Muted : _theme.Text;

                ToggleSwitch toggle = child as ToggleSwitch;
                if (toggle != null)
                {
                    ApplyToggleTheme(toggle);
                }

                Button button = child as Button;
                if (button != null)
                {
                    ConfigureThemedButton(button, _theme.Control, _theme.SecondaryText, _theme.Border, 1);
                }

                if (child.HasChildren)
                {
                    ApplyDebloatTheme(child);
                }
            }
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
            toggle.AccentColor = _theme.AccentDim;
            toggle.TrackOffColor = _theme.ToggleOff;
            toggle.KnobOnColor = ColorTranslator.FromHtml("#FFFFFF");
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
            // Skip badge label (index 0) and update heading label (index 1)
            int labelCount = 0;
            foreach (Control child in panel.Controls)
            {
                Label label = child as Label;
                if (label != null)
                {
                    labelCount++;
                    if (labelCount == 2) // second label is the heading
                    {
                        label.Text = title;
                        return;
                    }
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
            TweakDefinition tweak;
            if (_tweaksById != null && _tweaksById.TryGetValue(id, out tweak))
            {
                return tweak.DisplayName;
            }

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
            TweakDefinition tweak;
            if (_tweaksById != null && _tweaksById.TryGetValue(id, out tweak))
            {
                return tweak.Description;
            }

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
                case "win10Debloat": return th ? "Windows 10" : "Windows 10";
                case "win11Debloat": return th ? "Windows 11" : "Windows 11";
                case "misc": return th ? "อื่น ๆ" : "Other";
                case "noTweaks": return th ? "กรุณาเปิด tweak อย่างน้อยหนึ่งรายการก่อนใช้งาน" : "Turn on at least one tweak before applying.";
                case "noTweaksHelp": return th ? "ไปที่หน้า Tweaks แล้วเปิดรายการที่ต้องการ จากนั้นกดใช้ค่าที่เปิดไว้ ระบบจะทำเฉพาะรายการที่เปิดอยู่เท่านั้น" : "Open the Tweaks page, turn on the rows you want, then apply selected. JaiDee-Optimize only runs enabled rows.";
                case "noTweaksTitle": return th ? "ยังไม่ได้เปิด Tweak" : "No Tweaks Enabled";
                case "confirmApply": return th ? "ต้องการใช้ tweaks ที่เปิดอยู่หรือไม่ บางค่าอาจต้อง restart เครื่อง" : "Apply enabled tweaks? A restart may be required for some changes.";
                case "confirmApplySubtitle": return th ? "ตรวจรายการก่อนเริ่ม ระบบจะทำงานเฉพาะ tweak ที่เปิดอยู่" : "Review the enabled rows before starting. Only selected tweaks will run.";
                case "confirmApplyDebloatSubtitle": return th ? "มีรายการ Debloat รวมอยู่ อ่านคำเตือนก่อนดำเนินการ" : "Debloat actions are included. Read the warning before continuing.";
                case "confirmApplyTitle": return th ? "ยืนยันการใช้งาน" : "Confirm Apply";
                case "confirmRestore": return th ? "จะคืนค่า Registry และ INI ที่ถูกแก้ใน session นี้ ต้องการดำเนินการต่อหรือไม่" : "This will restore registry and INI values changed during this session. Continue?";
                case "confirmRestoreSubtitle": return th ? "คืนค่าเฉพาะรายการที่ระบบสำรองไว้ใน session นี้" : "Restores only values backed up during this app session.";
                case "confirmRestoreTitle": return th ? "ยืนยันการคืนค่า" : "Confirm Restore";
                case "cancel": return th ? "ยกเลิก" : "Cancel";
                case "done": return th ? "ตกลง" : "Done";
                case "debloatDisclaimer": return th ? "คำเตือน: Debloat จะลบหรือปิดบางแอป/ฟีเจอร์ของ Windows 10/11 จริง บางรายการอาจต้องติดตั้งกลับจาก Microsoft Store หรือ PowerShell และ Restore Defaults ไม่สามารถคืน Appx ที่ถูกลบได้อัตโนมัติ" : "Warning: Debloat will actually remove or disable selected Windows 10/11 apps/features. Some items may need to be reinstalled from Microsoft Store or PowerShell. Restore Defaults cannot automatically reinstall removed Appx packages.";
                case "debloatResponsibility": return th ? "การกด Yes หมายความว่าคุณเข้าใจความเสี่ยง ได้สำรองข้อมูลหรือสร้าง Restore Point แล้ว และยอมรับความรับผิดชอบต่อผลลัพธ์บนเครื่องนี้" : "Clicking Yes means you understand the risk, have backed up or created a restore point, and accept responsibility for the result on this PC.";
                case "logStarted": return th ? "JaiDee-Optimize เริ่มทำงานแล้ว" : "JaiDee-Optimize started.";
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
            public Color Glass;
            public Color GlassHover;
            public Color Text;
            public Color Title;
            public Color Muted;
            public Color SecondaryText;
            public Color Accent;
            public Color AccentDim;
            public Color AccentTint;
            public Color AccentBorder;
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
                    Background = ColorTranslator.FromHtml("#0F1117"),
                    Surface = ColorTranslator.FromHtml("#161922"),
                    Control = ColorTranslator.FromHtml("#1E2230"),
                    Border = Color.FromArgb(31, 255, 255, 255),
                    Glass = Color.FromArgb(15, 255, 255, 255),
                    GlassHover = Color.FromArgb(26, 255, 255, 255),
                    Text = ColorTranslator.FromHtml("#F0F0F0"),
                    Title = ColorTranslator.FromHtml("#F0F0F0"),
                    Muted = ColorTranslator.FromHtml("#8A8FA8"),
                    SecondaryText = ColorTranslator.FromHtml("#DCE7F7"),
                    Accent = ColorTranslator.FromHtml("#2D8EFF"),
                    AccentDim = ColorTranslator.FromHtml("#1A5FA8"),
                    AccentTint = Color.FromArgb(31, 45, 142, 255),
                    AccentBorder = Color.FromArgb(76, 45, 142, 255),
                    AccentText = ColorTranslator.FromHtml("#FFFFFF"),
                    Warning = ColorTranslator.FromHtml("#FDBA74"),
                    WarningBorder = ColorTranslator.FromHtml("#7C3E12"),
                    LogBackground = ColorTranslator.FromHtml("#08090D"),
                    LogText = ColorTranslator.FromHtml("#6A9FD8"),
                    ToggleOff = ColorTranslator.FromHtml("#1E2230"),
                    ToggleKnobOff = ColorTranslator.FromHtml("#FFFFFF"),
                    DisabledText = ColorTranslator.FromHtml("#5F657A")
                };
            }

            public static ThemePalette Light()
            {
                return new ThemePalette
                {
                    Background = ColorTranslator.FromHtml("#F8F9FC"),
                    Surface = ColorTranslator.FromHtml("#FFFFFF"),
                    Control = ColorTranslator.FromHtml("#F1F4F9"),
                    Border = Color.FromArgb(20, 0, 0, 0),
                    Glass = Color.FromArgb(10, 0, 0, 0),
                    GlassHover = Color.FromArgb(15, 0, 0, 0),
                    Text = ColorTranslator.FromHtml("#111318"),
                    Title = ColorTranslator.FromHtml("#111318"),
                    Muted = ColorTranslator.FromHtml("#6B7280"),
                    SecondaryText = ColorTranslator.FromHtml("#283545"),
                    Accent = ColorTranslator.FromHtml("#185FA5"),
                    AccentDim = ColorTranslator.FromHtml("#0C3D73"),
                    AccentTint = Color.FromArgb(20, 24, 95, 165),
                    AccentBorder = Color.FromArgb(64, 24, 95, 165),
                    AccentText = ColorTranslator.FromHtml("#FFFFFF"),
                    Warning = ColorTranslator.FromHtml("#A35E16"),
                    WarningBorder = ColorTranslator.FromHtml("#F0D6B7"),
                    LogBackground = ColorTranslator.FromHtml("#F4F5F7"),
                    LogText = ColorTranslator.FromHtml("#6A9FD8"),
                    ToggleOff = ColorTranslator.FromHtml("#F1F4F9"),
                    ToggleKnobOff = ColorTranslator.FromHtml("#FFFFFF"),
                    DisabledText = ColorTranslator.FromHtml("#9AA1AE")
                };
            }
        }
    }
}

