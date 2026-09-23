namespace StarX_Client;

public partial class Form1 : Form
{
    private string _plan = "free";       // free | pro | owner
    private string _ownerKey = string.Empty;
    private readonly List<DockKind> _kinds = [DockKind.Home, DockKind.Settings];
    private System.Windows.Forms.Timer? _beat;
    private CancellationTokenSource? _cleanCts;
    private CancellationTokenSource? _updCts;
    private ReleaseInfo? _pending;

    public Form1()
    {
        InitializeComponent();
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        RefreshColumns();
        FillDays();
        cmbDays.SelectedIndex = 3;
        RefreshJunkColumns();

        LoadAppIcon();
        btnUpd = titleBar.UpdButton;
        titleBar.Title = "StarX v" + AppVersion.Current;
        titleBar.MinButton.Click += (s, e) => WindowState = FormWindowState.Minimized;
        titleBar.MaxButton.Click += (s, e) => ToggleMaximize();
        titleBar.CloseButton.Click += (s, e) => Close();
        titleBar.DoubleClick += (s, e) => ToggleMaximize();
        try { MaximizedBounds = Screen.FromHandle(Handle).WorkingArea; } catch { /* ignore */ }
        floatingDock.SelectedChanged += OnDockSelected;
        btnAgent.Click += (s, e) => ShowCleaner();
        btnActivate.Click += async (s, e) => await OnActivateAsync();
        exitIcon.Click += async (s, e) => await OnDeactivateAsync();
        lblExit.Click += async (s, e) => await OnDeactivateAsync();
        txtKey.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await OnActivateAsync(); }
        };
        btnCleanerBack.Click += (s, e) => { _cleanCts?.Cancel(); ShowIndex(0); floatingDock.SetSelected(0); };
        btnScan.Click += async (s, e) => await OnScanAsync();
        btnCancel.Click += (s, e) => _cleanCts?.Cancel();
        btnSelectAll.Click += (s, e) => { foreach (ListViewItem it in lvJunk.Items) it.Checked = true; UpdateTotal(); };
        btnSelectNone.Click += (s, e) => { foreach (ListViewItem it in lvJunk.Items) it.Checked = false; UpdateTotal(); };
        btnSelectSafe.Click += (s, e) => { foreach (ListViewItem it in lvJunk.Items) it.Checked = it.Tag is JunkResult r && r.Recommended && r.Bytes > 0; UpdateTotal(); };
        lvJunk.ItemChecked += (s, e) => UpdateTotal();
        btnClean.Click += async (s, e) => await OnCleanAsync();
        btnExcludes.Click += (s, e) => { using var f = new ExclusionsForm(); f.ShowDialog(this); };
        btnCreate.Click += async (s, e) => await OnCreateKeyAsync();
        btnRefresh.Click += async (s, e) => await RefreshOwnerAsync();
        btnDelete.Click += async (s, e) => await OnDeleteKeyAsync();
        btnUpd.Click += async (s, e) => await OnUpdIconClickedAsync();
        btnPublish.Click += async (s, e) => await OnPublishAsync();
        cmbLang.Items.AddRange([Lang.T("lang_ar"), Lang.T("lang_en")]);
        cmbLang.SelectedIndex = Lang.IsArabic ? 0 : 1;
        cmbLang.SelectedIndexChanged += (s, e) =>
        {
            Lang.Current = cmbLang.SelectedIndex == 1 ? Lang.English : Lang.Arabic;
            ApplyLang(true);
        };
        btnLangSave.Click += (s, e) =>
        {
            Lang.Current = cmbLang.SelectedIndex == 1 ? Lang.English : Lang.Arabic;
            ApplyLang(true);
        };
        chkStartup.Checked = StartupGet();
        chkStartup.CheckedChanged += (s, e) => StartupSet(chkStartup.Checked);
        chkTray.Checked = CloseToTrayGet();
        chkTray.CheckedChanged += (s, e) => CloseToTraySet(chkTray.Checked);
        chkCloud.Checked = CleanerCloud.Enabled;
        chkCloud.CheckedChanged += (s, e) => CleanerCloud.Enabled = chkCloud.Checked;
        SetupTray();
        ApplyLang(false);
        floatingDock.SetSelected(0, raise: false);
        ShowIndex(0);
        LayoutPages();
        PositionDock();
        await RefreshLicenseAsync();

        _beat = new System.Windows.Forms.Timer(components) { Interval = 60000 };
        _beat.Tick += async (s, e) => await BeatAsync();
        _beat.Start();

        FormClosing += OnFormClosing;
        Resize += (s, e) =>
        {
            if (WindowState == FormWindowState.Minimized) HideToTray();
            try { titleBar.MaxButton.Text = WindowState == FormWindowState.Maximized ? "⧉" : "□"; } catch { /* ignore */ }
        };
        _updCts = new CancellationTokenSource();
        _ = UpdateLoopAsync(_updCts.Token);
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
    }

    // حواف سحب لتكبير النافذة بدون حدود (النافذة بلا إطار)
    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x0084;
        if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
        {
            base.WndProc(ref m);
            var p = PointToClient(new Point(m.LParam.ToInt32()));
            const int g = 6;
            bool l = p.X <= g, r = p.X >= ClientSize.Width - g;
            bool t = p.Y <= g, b = p.Y >= ClientSize.Height - g;
            if (t && l) m.Result = (IntPtr)13;
            else if (t && r) m.Result = (IntPtr)14;
            else if (b && l) m.Result = (IntPtr)16;
            else if (b && r) m.Result = (IntPtr)17;
            else if (l) m.Result = (IntPtr)10;
            else if (r) m.Result = (IntPtr)11;
            else if (t) m.Result = (IntPtr)12;
            else if (b) m.Result = (IntPtr)15;
            return;
        }
        base.WndProc(ref m);
    }

    private void Form1_Resize(object? sender, EventArgs e)
    {
        LayoutPages();
        PositionDock();
    }

    // ---------- التنقل ----------

    private void OnDockSelected(int index) => ShowIndex(index);

    private void ShowIndex(int index)
    {
        if (index < 0 || index >= _kinds.Count) index = 0;
        ShowPage(_kinds[index]);
    }

    private void ShowPage(DockKind kind)
    {
        pageSettings.Visible = kind == DockKind.Settings;
        pagePro.Visible = kind == DockKind.Pro;
        pageOwner.Visible = kind == DockKind.Owner;
        pageCleaner.Visible = false;
        bool home = kind == DockKind.Home;
        btnAgent.Visible = home;
        lblAgentSub.Visible = home;
        if (kind == DockKind.Owner) _ = RefreshOwnerAsync();
        LayoutPages();
    }

    private void ShowCleaner()
    {
        pageSettings.Visible = pagePro.Visible = pageOwner.Visible = false;
        pageCleaner.Visible = true;
        btnAgent.Visible = false;
        lblAgentSub.Visible = false;
        LayoutPages();
    }

    private bool IsActivePlan() => _plan == "pro" || _plan == "owner";

    private void RebuildDock()
    {
        _kinds.Clear();
        _kinds.Add(DockKind.Home);
        if (_plan == "pro" || _plan == "owner") _kinds.Add(DockKind.Pro);
        if (_plan == "owner") _kinds.Add(DockKind.Owner);
        _kinds.Add(DockKind.Settings); // الإعدادات دائماً آخر زر
        floatingDock.SetItems(_kinds);
        ShowIndex(floatingDock.SelectedIndex);
        PositionDock();
    }

    private void LayoutSettings()
    {
        // ترتيب عمودي من فوق لتحت: خيارات / لغة — يتمدد ويتوسطن مع النافذة
        if (pageSettings.ClientSize.Width <= 0 || pageSettings.ClientSize.Height <= 0) return;
        int viewW = Math.Max(0, pageSettings.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        int viewH = pageSettings.ClientSize.Height;
        int extraW = Math.Max(0, viewW - 1000);
        int cardW = Math.Min(940, Math.Max(300, 620 + extraW / 2));
        if (cardW > viewW - 24) cardW = Math.Max(300, viewW - 24);
        int x = Math.Max(12, (viewW - cardW) / 2);
        const int gap = 12;
        int totalH = 170 + gap + 156 + gap + 108;
        int y = Math.Max(12, (viewH - totalH) / 2);

        cardLicense.Location = new Point(x, y);
        cardLicense.Size = new Size(cardW, 170);
        y += cardLicense.Height + gap;

        cardOptions.Location = new Point(x, y);
        cardOptions.Size = new Size(cardW, 156);
        y += cardOptions.Height + gap;

        cardLang.Location = new Point(x, y);
        cardLang.Size = new Size(cardW, 108);

        // --- داخل cardLicense: الحالة في النص + صف (مفتاح + تفعيل) ---
        lblLicStatus.Left = Math.Max(20, (cardW - lblLicStatus.Width) / 2);
        lblLicStatus.Top = 52;
        int keyW = Math.Min(340, cardW - 200);
        int btnW = 150;
        int rowW = keyW + 10 + btnW;
        int rowX = Math.Max(20, (cardW - rowW) / 2);
        txtKey.Location = new Point(rowX, 86);
        txtKey.Size = new Size(keyW, 34);
        btnActivate.Location = new Point(rowX + keyW + 10, 86);
        btnActivate.Size = new Size(btnW, 34);

        // --- داخل cardOptions: ثلاثة خيارات تحت بعض بملء العرض ---
        chkStartup.Location = new Point(20, 48);
        chkStartup.Size = new Size(cardW - 40, 32);
        chkTray.Location = new Point(20, 82);
        chkTray.Size = new Size(cardW - 40, 32);
        chkCloud.Location = new Point(20, 116);
        chkCloud.Size = new Size(cardW - 40, 32);

        // --- داخل cardLang: صف (لغة + حفظ) ---
        int langW = Math.Min(230, cardW / 2);
        int saveW = 120;
        int langRow = langW + 10 + saveW;
        int langX = Math.Max(20, (cardW - langRow) / 2);
        cmbLang.Location = new Point(langX, 56);
        cmbLang.Size = new Size(langW, 30);
        btnLangSave.Location = new Point(langX + langW + 10, 52);
        btnLangSave.Size = new Size(saveW, 36);
    }

    private void LayoutPages()
    {
        if (main.Width <= 0) return;
        int cx = main.Width / 2;
        void C(Control c) => c.Left = Math.Max(0, cx - c.Width / 2);
        // الإعدادات: توزيع عمودي نظيف + سكرول
        LayoutSettings();
        // الهوم: يتمدد ويتوسطن عمودياً مع النافذة
        int homeExtraW = Math.Max(0, main.Width - 1000);
        btnAgent.Size = new Size(Math.Min(340, 300 + homeExtraW / 8), 64);
        btnAgent.Left = Math.Max(0, cx - btnAgent.Width / 2);
        lblAgentSub.Left = Math.Max(10, cx - lblAgentSub.Width / 2);
        int homeBlockH = btnAgent.Height + 8 + lblAgentSub.Height;
        int hy = Math.Max(80, (main.Height - 100 - homeBlockH) / 2);
        btnAgent.Top = hy;
        lblAgentSub.Top = btnAgent.Bottom + 8;
        // صفحة الفحص الذكي: القائمة تتمدد مع النافذة وباقي الصفوف تتبعها
        int junkW = Math.Clamp(main.Width - 224, 640, 1100);
        int junkH = Math.Clamp(main.Height - 430, 220, 700);
        C(btnScan); C(btnCancel);
        btnScan.Top = 20; btnCancel.Top = 20;
        int pw = Math.Min(junkW, 560 + Math.Max(0, main.Width - 1000) / 4);
        progressScan.Size = new Size(pw, 14);
        progressScan.Left = Math.Max(0, cx - progressScan.Width / 2);
        progressScan.Top = 68;
        lblScanStatus.Left = Math.Max(10, cx - lblScanStatus.Width / 2);
        lblScanStatus.Top = 88;
        lvJunk.Location = new Point(Math.Max(0, cx - junkW / 2), 110);
        lvJunk.Size = new Size(junkW, junkH);
        RefreshJunkColumns();
        int jw = btnSelectAll.Width + 10 + btnSelectNone.Width + 10 + btnSelectSafe.Width + 10 + btnExcludes.Width;
        btnSelectAll.Left = Math.Max(0, cx - jw / 2);
        btnSelectAll.Top = lvJunk.Bottom + 8;
        btnSelectNone.Left = btnSelectAll.Right + 10;
        btnSelectNone.Top = btnSelectAll.Top;
        btnSelectSafe.Left = btnSelectNone.Right + 10;
        btnSelectSafe.Top = btnSelectAll.Top;
        btnExcludes.Left = btnSelectSafe.Right + 10;
        btnExcludes.Top = btnSelectAll.Top;
        lblTotal.Left = Math.Max(10, cx - lblTotal.Width / 2);
        lblTotal.Top = btnSelectAll.Bottom + 8;
        btnClean.Left = Math.Max(0, cx - btnClean.Width / 2);
        btnClean.Top = lblTotal.Bottom + 6;
        lblAiSummary.Left = Math.Max(10, cx - lblAiSummary.Width / 2);
        lblAiSummary.Top = btnClean.Bottom + 10;
        btnCleanerBack.Left = Math.Max(0, main.Width - btnCleanerBack.Width - 16);
        lblProNote.Top = Math.Max(120, main.Height / 2 - 120);
        C(lblProNote);
        lblExit.Left = exitIcon.Right + 8;
        lblExit.Top = exitIcon.Top + Math.Max(0, (exitIcon.Height - lblExit.Height) / 2);
        // لوحة المالك: القوائم تتمدد مع النافذة وباقي الصفوف تتبعها
        int ow = Math.Clamp(main.Width - 424, 480, 920);
        int oh = Math.Clamp(main.Height - 528, 84, 320);
        int oy = 54;
        lblOnlineH.Left = Math.Max(10, cx - lblOnlineH.Width / 2);
        lblOnlineH.Top = oy;
        oy += lblOnlineH.Height + 4;
        lvOnline.Location = new Point(Math.Max(0, cx - ow / 2), oy);
        lvOnline.Size = new Size(ow, oh);
        oy += oh + 10;
        lblLicH.Left = Math.Max(10, cx - lblLicH.Width / 2);
        lblLicH.Top = oy;
        oy += lblLicH.Height + 4;
        lvLicenses.Location = new Point(Math.Max(0, cx - ow / 2), oy);
        lvLicenses.Size = new Size(ow, oh);
        oy += oh + 10;
        // صف الإنشاء: ثلاث كنترولات جنب بعض في النص
        int rowW = btnCreate.Width + 10 + txtNote.Width + 10 + cmbDays.Width;
        btnCreate.Left = Math.Max(0, cx - rowW / 2);
        btnCreate.Top = oy;
        txtNote.Left = btnCreate.Right + 10;
        txtNote.Top = oy;
        cmbDays.Left = txtNote.Right + 10;
        cmbDays.Top = oy;
        oy += btnCreate.Height + 8;
        // النتيجة + التحديث كمجموعة في النص
        int g2 = txtNewKey.Width + 10 + btnRefresh.Width + 10 + btnDelete.Width;
        txtNewKey.Left = Math.Max(0, cx - g2 / 2);
        txtNewKey.Top = oy;
        btnRefresh.Left = txtNewKey.Right + 10;
        btnRefresh.Top = oy;
        btnDelete.Left = btnRefresh.Right + 10;
        btnDelete.Top = oy;
        oy += txtNewKey.Height + 8;
        lblOwnStatus.Left = Math.Max(10, cx - lblOwnStatus.Width / 2);
        lblOwnStatus.Top = oy;
        oy += lblOwnStatus.Height + 8;
        lblPubH.Left = Math.Max(10, cx - lblPubH.Width / 2);
        lblPubH.Top = oy;
        oy += lblPubH.Height + 6;
        int prow = txtPubVersion.Width + 10 + txtToken.Width + 10 + txtPubNotes.Width;
        txtPubVersion.Left = Math.Max(0, cx - prow / 2);
        txtPubVersion.Top = oy;
        txtToken.Left = txtPubVersion.Right + 10;
        txtToken.Top = oy;
        txtPubNotes.Left = txtToken.Right + 10;
        txtPubNotes.Top = oy;
        oy += txtPubVersion.Height + 8;
        btnPublish.Left = Math.Max(0, cx - btnPublish.Width / 2);
        btnPublish.Top = oy;
        oy += btnPublish.Height + 8;
        lblPubStatus.Left = Math.Max(10, cx - lblPubStatus.Width / 2);
        lblPubStatus.Top = oy;
    }

    // ---------- اللغة ----------

    private void ApplyLang(bool refresh)
    {
        txtKey.PlaceholderText = Lang.T("key_ph");
        btnActivate.Text = Lang.T("activate");
        cardLicense.HeaderText = Lang.T("sec_lic");
        cardOptions.HeaderText = Lang.T("sec_options");
        cardLang.HeaderText = Lang.T("sec_lang");
        btnLangSave.Text = Lang.T("save");
        chkStartup.Text = Lang.T("startup");
        chkTray.Text = Lang.T("opt_tray");
        chkCloud.Text = Lang.T("opt_cloud");
        lblProNote.Text = Lang.T("pro_note");
        lblOnlineH.Text = Lang.T("online_h");
        lblLicH.Text = Lang.T("lic_h");
        btnCreate.Text = Lang.T("create");
        txtNote.PlaceholderText = Lang.T("note_ph");
        btnRefresh.Text = Lang.T("refresh");
        btnDelete.Text = Lang.T("delete_key");
        btnAgent.Text = Lang.T("agent_btn");
        lblAgentSub.Text = Lang.T("agent_sub");
        btnScan.Text = Lang.T("clean_scan");
        btnCancel.Text = Lang.T("clean_cancel");
        btnCleanerBack.Text = Lang.T("clean_back");
        btnSelectAll.Text = Lang.T("clean_select_all");
        btnSelectNone.Text = Lang.T("clean_select_none");
        btnSelectSafe.Text = Lang.T("clean_select_safe");
        btnExcludes.Text = Lang.T("excl_btn");
        btnClean.Text = Lang.T("clean_delete");
        RefreshJunkColumns();
        for (int i = 0; i < lvJunk.Items.Count; i++)
        {
            ListViewItem? it = lvJunk.Items[i];
            if (it != null && it.Tag is JunkResult r && it.SubItems.Count > 5)
            {
                it.SubItems[5].Text = CleanerEngine.VerdictOf(r.Recommended);
                if (it.SubItems.Count > 6)
                    it.SubItems[6].Text = SmartAdvisor.Describe(r);
            }
        }
        if (lvJunk.Items.Count > 0)
        {
            var rs = new List<JunkResult>();
            for (int i = 0; i < lvJunk.Items.Count; i++)
                if (lvJunk.Items[i]?.Tag is JunkResult r) rs.Add(r);
            lblAiSummary.Text = SmartAdvisor.Summarize(rs);
        }
        UpdateTotal();
        btnUpd.RefreshTip();
        lblPubH.Text = Lang.T("pub_title");
        txtPubVersion.PlaceholderText = Lang.T("pub_version_ph");
        txtToken.PlaceholderText = Lang.T("pub_token_ph");
        txtPubNotes.PlaceholderText = Lang.T("pub_notes_ph");
        btnPublish.Text = Lang.T("pub_publish");
        RefreshColumns();
        FillDays();
        floatingDock.RefreshLanguage();
        exitIcon.RefreshTip();
        lblExit.Text = Lang.T("exit_tip");
        if (_miOpen != null) _miOpen.Text = Lang.T("tray_open");
        if (_miExit != null) _miExit.Text = Lang.T("tray_exit");
        if (refresh)
        {
            if (IsActivePlan()) UpdateLicenseUi();
            else _ = RefreshLicenseAsync();
            if (pageOwner.Visible) _ = RefreshOwnerAsync();
        }
        LayoutPages();
    }

    private void RefreshColumns()
    {
        string[] on = ["col_device", "col_key", "col_plan", "col_seen"];
        string[] li = ["col_key", "col_plan", "col_active", "col_devices", "col_expires", "col_note"];
        int[] onW = [150, 120, 70, Math.Max(160, lvOnline.Width - 340)];
        int[] liW = [150, 70, 55, 60, 110, Math.Max(110, lvLicenses.Width - 445)];
        while (lvOnline.Columns.Count < on.Length) lvOnline.Columns.Add("", 100);
        while (lvLicenses.Columns.Count < li.Length) lvLicenses.Columns.Add("", 100);
        for (int i = 0; i < on.Length; i++) { lvOnline.Columns[i].Text = Lang.T(on[i]); lvOnline.Columns[i].Width = onW[i]; }
        for (int i = 0; i < li.Length; i++) { lvLicenses.Columns[i].Text = Lang.T(li[i]); lvLicenses.Columns[i].Width = liW[i]; }
    }

    private void FillDays()
    {
        int sel = Math.Max(0, cmbDays.SelectedIndex);
        cmbDays.Items.Clear();
        cmbDays.Items.AddRange([Lang.T("dur_day"), Lang.T("dur_2days"), Lang.T("dur_3days"), Lang.T("dur_week"), Lang.T("dur_month"), Lang.T("dur_year"), Lang.T("dur_lifetime")]);
        cmbDays.SelectedIndex = Math.Min(sel, cmbDays.Items.Count - 1);
    }

    private void PositionDock()
    {
        floatingDock.Left = Math.Max(0, (ClientSize.Width - floatingDock.Width) / 2);
        floatingDock.Top = ClientSize.Height - floatingDock.Height - 22;
        floatingDock.BringToFront();
    }

    private NotifyIcon? _tray;
    private ToolStripMenuItem? _miOpen;
    private ToolStripMenuItem? _miExit;
    private bool _allowClose;
    private bool _trayHintShown;

    // ---------- التريّ والتشغيل التلقائي ----------

    private void SetupTray()
    {
        var menu = new ContextMenuStrip();
        _miOpen = new ToolStripMenuItem(Lang.T("tray_open"));
        _miExit = new ToolStripMenuItem(Lang.T("tray_exit"));
        _miOpen.Click += (s, e) => RestoreFromTray();
        _miExit.Click += (s, e) => { _allowClose = true; Application.Exit(); };
        menu.Items.Add(_miOpen);
        menu.Items.Add(_miExit);
        _tray = new NotifyIcon(components)
        {
            Text = "StarX",
            ContextMenuStrip = menu,
            Visible = false,
        };
        if (Icon != null)
        {
            try { _tray.Icon = (Icon)Icon.Clone(); } catch { /* ignore */ }
        }
        _tray.DoubleClick += (s, e) => RestoreFromTray();
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        ShowInTaskbar = true;
        if (_tray != null) _tray.Visible = false;
        Activate();
    }

    internal static bool CloseToTrayGet() =>
        AppSettings.GetString("closeToTray", "1") != "0";

    internal static void CloseToTraySet(bool enable) =>
        AppSettings.Set("closeToTray", enable ? "1" : "0");

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _updCts?.Cancel();
        _cleanCts?.Cancel();
        if (!_allowClose && CloseToTrayGet() && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = HideToTray();
        }
    }

    // ترجع true إذا تم الإخفاء (ليُلغى الإغلاق)
    private bool HideToTray()
    {
        if (!CloseToTrayGet()) return false;
        if (_tray == null || _tray.Icon == null) return false;
        Hide();
        ShowInTaskbar = false;
        _tray.Visible = true;
        if (!_trayHintShown)
        {
            _trayHintShown = true;
            _tray.ShowBalloonTip(2500, "StarX", Lang.T("tray_min"), ToolTipIcon.Info);
        }
        return true;
    }

    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "StarX";

    internal static bool StartupGet()
    {
        try
        {
            using var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            var v = k?.GetValue(RunValueName)?.ToString();
            return !string.IsNullOrWhiteSpace(v) &&
                   v.Contains("StarX", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    internal static void StartupSet(bool enable)
    {
        try
        {
            using var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (k == null) return;
            if (enable)
                k.SetValue(RunValueName, "\"" + Application.ExecutablePath + "\"");
            else if (k.GetValue(RunValueName) != null)
                k.DeleteValue(RunValueName);
        }
        catch { /* ignore */ }
    }

    private void LoadAppIcon()
    {
        foreach (var path in new[] { @"..\app_icon.ico", "app_icon.ico", @"D:\StarX\app_icon.ico" })
        {
            try
            {
                if (File.Exists(path))
                {
                    Icon = new Icon(path);
                    return;
                }
            }
            catch { /* try next */ }
        }
        try
        {
            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon != null) Icon = (Icon)icon.Clone();
        }
        catch { /* default icon */ }
    }

    // ---------- الترخيص (كارت الإعدادات) ----------

    private string _inactiveText = "";

    private async Task RefreshLicenseAsync()
    {
        try
        {
            var st = await LicenseManager.GetStatusAsync().ConfigureAwait(true);
            _plan = st.Active ? st.Plan : "free";
            _ownerKey = st is { Active: true, Plan: "owner" }
                ? LicenseManager.LoadCache()?.Key ?? string.Empty
                : string.Empty;
            _inactiveText = st.Active ? string.Empty : st.Detail;
        }
        catch { _plan = "free"; }
        UpdateLicenseUi();
        RebuildDock();
    }

    private async Task OnActivateAsync()
    {
        string key = txtKey.Text.Trim();
        if (string.IsNullOrEmpty(key))
        {
            _inactiveText = Lang.T("st_enter_key");
            UpdateLicenseUi();
            txtKey.Focus();
            return;
        }

        Cursor = Cursors.WaitCursor;
        try
        {
            var (ok, reason, plan, _) = await LicenseManager
                .ValidateOnlineAsync(key, LicenseManager.GetHwid()).ConfigureAwait(true);
            if (!ok)
            {
                string msg = LicenseManager.ReasonText(reason);
                _inactiveText = msg;
                UpdateLicenseUi();
                MessageBox.Show(msg, Lang.T("mb_activate"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LicenseManager.SaveCache(key, LicenseManager.GetHwid(), plan);
            _plan = plan;
            _ownerKey = plan == "owner" ? key : string.Empty;
            txtKey.Clear();
            _inactiveText = string.Empty;
            UpdateLicenseUi();
            RebuildDock();
            floatingDock.SetSelected(0);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async Task OnDeactivateAsync()
    {
        var cache = LicenseManager.LoadCache();
        try
        {
            if (cache != null)
                await LicenseManager.ReleaseOnlineAsync(cache.Key, cache.Hwid).ConfigureAwait(true);
        }
        finally
        {
            LicenseManager.ClearCache();
            _plan = "free";
            _ownerKey = string.Empty;
            _inactiveText = Lang.T("st_inactive");
            UpdateLicenseUi();
            RebuildDock();
        }
    }

    private void UpdateLicenseUi()
    {
        bool active = IsActivePlan();
        txtKey.Visible = btnActivate.Visible = !active;
        lblLicStatus.Text = active
            ? (_plan == "owner" ? Lang.T("st_owner") : Lang.T("st_pro"))
            : (string.IsNullOrEmpty(_inactiveText) ? Lang.T("st_inactive") : _inactiveText);
        lblLicStatus.Visible = true;
        LayoutPages();
    }

    // ---------- التحديثات (GitHub Releases) ----------

    private async Task UpdateLoopAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(UpdateConfig.InitialDelaySeconds * 1000, ct).ConfigureAwait(true);
            while (!ct.IsCancellationRequested)
            {
                await CheckBackgroundAsync(ct).ConfigureAwait(true);
                await Task.Delay(UpdateConfig.CheckIntervalMinutes * 60 * 1000, ct).ConfigureAwait(true);
            }
        }
        catch (TaskCanceledException) { /* إغلاق التطبيق */ }
    }

    private readonly HashSet<string> _autoFailed = new(StringComparer.OrdinalIgnoreCase);
    private bool _cleanBusy;
    private bool _publishBusy;

    private bool IsUserBusy() => _cleanBusy || _publishBusy;

    private async Task CheckBackgroundAsync(CancellationToken ct)
    {
        if (IsDisposed || Disposing) return;
        var r = await UpdateManager.CheckAsync(silent: true, ct).ConfigureAwait(true);
        bool has = r.Available && r.Release != null;
        _pending = has ? r.Release : null;
        if (!IsDisposed && !Disposing && IsHandleCreated)
        {
            try { Invoke(() => btnUpd.SetHasUpdate(has, has ? r.Release!.Version : "")); }
            catch { /* ignore */ }
        }
        // تحديث تلقائي كامل عند الخمول — لو المستخدم مشغول نكتفي بالشارة
        if (has && r.Release != null &&
            !_autoFailed.Contains(r.Release.Version) && !IsUserBusy())
            await AutoUpdateAsync(r.Release);
    }

    private async Task AutoUpdateAsync(ReleaseInfo rel)
    {
        string? err = await UpdateManager.LaunchUpdaterAsync(rel).ConfigureAwait(true);
        if (err != null)
        {
            // فشل التثبيت التلقائي: لا تحاول تاني لوحدك، الشارة تفضل للضغط اليدوي
            _autoFailed.Add(rel.Version);
            return;
        }
        Application.Exit();
    }

    // ضغطة واحدة على الأيقونة = فحص ثم تحديث فوري بدون أي سؤال
    private async Task OnUpdIconClickedAsync()
    {
        if (_pending != null)
        {
            await UpdateNowAsync(_pending);
            return;
        }
        btnUpd.Enabled = false;
        try
        {
            var r = await UpdateManager.CheckAsync(silent: false).ConfigureAwait(true);
            if (r.Available && r.Release != null)
            {
                _pending = r.Release;
                btnUpd.SetHasUpdate(true, r.Release.Version);
                await UpdateNowAsync(r.Release);
            }
            else
            {
                _pending = null;
                btnUpd.SetHasUpdate(false, "");
                btnUpd.ShowHint(Lang.T("upd_uptodate"));
            }
        }
        finally
        {
            btnUpd.Enabled = true;
        }
    }

    private async Task UpdateNowAsync(ReleaseInfo rel)
    {
        string? err = await UpdateManager.LaunchUpdaterAsync(rel).ConfigureAwait(true);
        if (err != null)
        {
            MessageBox.Show(err, "StarX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        Application.Exit();
    }

    // ---------- النشر من لوحة المالك ----------

    private async Task OnPublishAsync()
    {
        if (_plan != "owner") return;
        btnPublish.Enabled = false;
        _publishBusy = true;
        try
        {
            string token = txtToken.Text;
            var progress = new Progress<string>(m =>
            {
                if (IsDisposed || Disposing) return;
                lblPubStatus.Text = m;
                LayoutPages();
            });
            var (ok, msg) = await PublishManager.PublishAsync(
                txtPubVersion.Text, token, txtPubNotes.Text, progress).ConfigureAwait(true);
            lblPubStatus.Text = msg;
        }
        finally
        {
            txtToken.Clear(); // لا نترك الرمز في الحقل مهما كانت النتيجة
            _publishBusy = false;
            btnPublish.Enabled = true;
            LayoutPages();
        }
    }

    // ---------- الوكيل الذكي لتنظيف الجهاز ----------

    private void RefreshJunkColumns()
    {
        string[] keys = ["", "col_cat", "col_path", "col_size", "clean_files", "col_verdict", "ai_advice"];
        int[] ws = [36, 110, 170, 80, 60, 95, Math.Max(150, lvJunk.Width - 551)];
        while (lvJunk.Columns.Count < keys.Length) lvJunk.Columns.Add("", 80);
        for (int i = 0; i < keys.Length; i++)
        {
            lvJunk.Columns[i].Text = keys[i] == "" ? "" : Lang.T(keys[i]);
            lvJunk.Columns[i].Width = ws[i];
        }
    }

    private void SetCleanerBusy(bool busy)
    {
        _cleanBusy = busy;
        btnScan.Visible = !busy;
        btnCancel.Visible = busy;
        btnClean.Enabled = !busy;
        btnSelectAll.Enabled = !busy;
        btnSelectNone.Enabled = !busy;
        btnSelectSafe.Enabled = !busy;
        btnExcludes.Enabled = !busy;
        progressScan.Style = ProgressBarStyle.Blocks;
        if (!busy) progressScan.Value = 0;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private async Task OnScanAsync()
    {
        _cleanCts?.Cancel();
        _cleanCts = new CancellationTokenSource();
        var ct = _cleanCts.Token;
        SetCleanerBusy(true);
        lvJunk.Items.Clear();
        lblScanStatus.Text = Lang.T("clean_scanning");
        lblAiSummary.Text = "";
        LayoutPages();
        var prog = new Progress<string>(m => { if (!IsDisposed && !Disposing) lblScanStatus.Text = m; });
        try
        {
            CloudRules? cloudRules = null;
            if (CleanerCloud.Enabled)
            {
                lblScanStatus.Text = Lang.T("clean_scanning");
                cloudRules = await CleanerCloud.FetchRulesAsync(ct).ConfigureAwait(true);
            }
            var rows = await CleanerEngine.ScanAsync(prog, ct).ConfigureAwait(true);
            if (IsDisposed || Disposing) return;
            rows = SmartAdvisor.Analyze(rows, cloudRules);
            lvJunk.BeginUpdate();
            lvJunk.Items.Clear();
            foreach (var r in rows)
            {
                var it = new ListViewItem("");
                it.SubItems.Add(r.Title);
                it.SubItems.Add(r.Path);
                it.SubItems.Add(CleanerEngine.FormatSize(r.Bytes));
                it.SubItems.Add(r.Files.ToString());
                it.SubItems.Add(CleanerEngine.VerdictOf(r.Recommended));
                it.SubItems.Add(SmartAdvisor.Describe(r));
                it.Checked = r.Recommended && r.Bytes > 0;
                it.Tag = r;
                lvJunk.Items.Add(it);
            }
            lvJunk.EndUpdate();
            lblScanStatus.Text = Lang.T("clean_ready");
            lblAiSummary.Text = SmartAdvisor.Summarize(rows) +
                (cloudRules != null ? " • " + Lang.T("clean_cloud_rules") : "");
            UpdateTotal();
        }
        catch (OperationCanceledException)
        {
            lblScanStatus.Text = Lang.T("clean_cancel");
        }
        finally
        {
            SetCleanerBusy(false);
            LayoutPages();
        }
    }

    private void UpdateTotal()
    {
        long bytes = 0;
        try
        {
            for (int i = 0; i < lvJunk.Items.Count; i++)
            {
                ListViewItem? it = lvJunk.Items[i];
                if (it == null || !it.Checked) continue;
                if (it.Tag is JunkResult r) bytes += r.Bytes;
            }
        }
        catch { /* حالة UI عابرة أثناء التحديث — التجميع مستمر */ }

        try
        {
            lblTotal.Text = $"{Lang.T("clean_total")}: {CleanerEngine.FormatSize(bytes)}";
            if (main.Width > 0)
                lblTotal.Left = Math.Max(10, main.Width / 2 - lblTotal.Width / 2);
        }
        catch { /* تجاهل عابر */ }
    }

    private async Task OnCleanAsync()
    {
        var selected = new List<JunkResult>();
        for (int i = 0; i < lvJunk.Items.Count; i++)
        {
            ListViewItem? it = lvJunk.Items[i];
            if (it != null && it.Checked && it.Tag is JunkResult r && r.Bytes > 0)
                selected.Add(r);
        }
        if (selected.Count == 0)
        {
            lblScanStatus.Text = Lang.T("clean_nothing");
            LayoutPages();
            return;
        }
        long total = 0;
        foreach (var r in selected) total += r.Bytes;
        if (MessageBox.Show($"{Lang.T("clean_confirm")}\n{CleanerEngine.FormatSize(total)}",
                "StarX", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        _cleanCts?.Cancel();
        _cleanCts = new CancellationTokenSource();
        var ct = _cleanCts.Token;
        SetCleanerBusy(true);
        lblScanStatus.Text = Lang.T("clean_deleting");
        var prog = new Progress<double>(p =>
        {
            if (IsDisposed || Disposing) return;
            progressScan.Style = ProgressBarStyle.Blocks;
            progressScan.Value = Math.Clamp((int)(p * 100), 0, 100);
        });
        try
        {
            var deletedFiles = new List<CleanedFile>();
            var (deleted, freed, failed) = await CleanerEngine.DeleteAsync(selected, prog, ct, deletedFiles).ConfigureAwait(true);
            if (IsDisposed || Disposing) return;
            CleanHistory.Add(deleted, freed, failed, deletedFiles);
            lblScanStatus.Text =
                $"{Lang.T("clean_done")} — {deleted} {Lang.T("clean_deleted")} • " +
                $"{Lang.T("clean_freed")}: {CleanerEngine.FormatSize(freed)}" +
                (failed > 0 ? $" • {Lang.T("clean_failed")}: {failed} ({Lang.T("clean_inuse")})" : "");
            if (CleanerCloud.Enabled)
            {
                bool synced = await CleanerCloud.ReportRunAsync(deleted, freed, failed, ct).ConfigureAwait(true);
                if (!IsDisposed && !Disposing && synced)
                    lblScanStatus.Text += " • " + Lang.T("clean_cloud_synced");
            }
            await OnScanAsync();
        }
        catch (OperationCanceledException)
        {
            lblScanStatus.Text = Lang.T("clean_cancel");
        }
        finally
        {
            SetCleanerBusy(false);
            LayoutPages();
        }
    }

    private async Task BeatAsync()
    {
        string hwid = LicenseManager.GetHwid();
        string name = LicenseManager.GetDeviceName();
        var cache = LicenseManager.LoadCache();
        if (_plan == "free" || cache == null || string.IsNullOrWhiteSpace(cache.Key))
        {
            await LicenseManager.DevicePingAsync(hwid, name, "free").ConfigureAwait(true);
            return;
        }
        await LicenseManager.PingAsync(cache.Key, cache.Hwid).ConfigureAwait(true);
        await LicenseManager.DevicePingAsync(cache.Hwid, name, _plan, cache.Key).ConfigureAwait(true);
    }

    // ---------- لوحة المالك ----------

    private static int DaysFor(int i) => i switch
    {
        0 => 1, 1 => 2, 2 => 3, 3 => 7, 4 => 30, 5 => 365, _ => 0,
    };

    private async Task OnCreateKeyAsync()
    {
        if (_plan != "owner" || string.IsNullOrEmpty(_ownerKey)) return;
        btnCreate.Enabled = false;
        lblOwnStatus.Text = Lang.T("st_creating");
        LayoutPages();
        try
        {
            var (ok, key, exp, err) = await LicenseManager.CreateKeyAsync(
                _ownerKey, DaysFor(cmbDays.SelectedIndex), txtNote.Text.Trim(), 1).ConfigureAwait(true);
            if (!ok)
            {
                lblOwnStatus.Text = err;
                return;
            }
            txtNewKey.Text = key;
            txtNewKey.Focus();
            txtNewKey.SelectAll();
            lblOwnStatus.Text = Lang.T("st_created") +
                (exp != null ? $" — {Lang.T("st_expires")} {(exp.Length > 16 ? exp[..16] : exp)}" : $" — {Lang.T("st_lifetime")}");
            await RefreshOwnerAsync();
        }
        finally
        {
            btnCreate.Enabled = true;
            LayoutPages();
        }
    }

    private async Task OnDeleteKeyAsync()
    {
        if (_plan != "owner" || string.IsNullOrEmpty(_ownerKey)) return;
        if (lvLicenses.SelectedItems.Count == 0)
        {
            lblOwnStatus.Text = Lang.T("st_select_first");
            LayoutPages();
            return;
        }
        string key = lvLicenses.SelectedItems[0].Text;
        if (MessageBox.Show($"{Lang.T("confirm_delete")}\n{key}", Lang.T("mb_delete"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        btnDelete.Enabled = false;
        try
        {
            var (ok, err) = await LicenseManager.DeleteKeyAsync(_ownerKey, key).ConfigureAwait(true);
            if (!ok)
            {
                lblOwnStatus.Text = err;
                return;
            }
            if (txtNewKey.Text == key) txtNewKey.Clear();
            await RefreshOwnerAsync();
            lblOwnStatus.Text = Lang.T("st_deleted");
        }
        finally
        {
            btnDelete.Enabled = true;
            LayoutPages();
        }
    }

    private async Task RefreshOwnerAsync()
    {
        if (_plan != "owner" || string.IsNullOrEmpty(_ownerKey)) return;
        lblOwnStatus.Text = Lang.T("st_updating");
        var (ok, err, online, licenses) =
            await LicenseManager.OverviewAsync(_ownerKey).ConfigureAwait(true);
        if (!ok)
        {
            lblOwnStatus.Text = err;
            LayoutPages();
            return;
        }

        lvOnline.BeginUpdate();
        lvOnline.Items.Clear();
        foreach (var u in online)
        {
            string name = string.IsNullOrWhiteSpace(u.DeviceName) ? u.Hwid : u.DeviceName;
            var it = new ListViewItem(name);
            it.SubItems.Add(u.KeyMasked);
            it.SubItems.Add(PlanName(u.Plan));
            it.SubItems.Add((u.Online ? Lang.T("online_now") : "") + u.LastSeen);
            lvOnline.Items.Add(it);
        }
        lvOnline.EndUpdate();

        lvLicenses.BeginUpdate();
        lvLicenses.Items.Clear();
        foreach (var l in licenses)
        {
            var it = new ListViewItem(l.Key);
            it.SubItems.Add(PlanName(l.Plan));
            it.SubItems.Add(l.Active ? Lang.T("yes") : Lang.T("no"));
            it.SubItems.Add(l.Devices.ToString());
            it.SubItems.Add(l.ExpiresAt != null && l.ExpiresAt.Length > 16 ? l.ExpiresAt[..16] : (l.ExpiresAt ?? Lang.T("st_lifetime")));
            it.SubItems.Add(l.Note ?? "");
            lvLicenses.Items.Add(it);
        }
        lvLicenses.EndUpdate();

        lblOwnStatus.Text = $"{Lang.T("st_online_n")}: {online.Count(o => o.Online)} • {Lang.T("st_keys_n")}: {licenses.Count}";
        LayoutPages();
    }

    private static string PlanName(string plan) => plan switch
    {
        "owner" => Lang.T("plan_owner"),
        "pro" => Lang.T("plan_pro"),
        _ => Lang.T("plan_free"),
    };
}
