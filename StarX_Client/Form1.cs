namespace StarX_Client;

public partial class Form1 : Form
{
    private string _plan = "free";       // free | pro | owner
    private string _inactiveText = "";
    private string _note = "";
    private string _ownerKey = string.Empty;
    private readonly List<DockKind> _kinds = [DockKind.Home, DockKind.Settings];
    private System.Windows.Forms.Timer? _beat;
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

        LoadAppIcon();
        floatingDock.SelectedChanged += OnDockSelected;
        btnActivate.Click += async (s, e) => await OnActivateAsync();
        exitIcon.Click += async (s, e) => await OnDeactivateAsync();
        txtKey.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await OnActivateAsync(); }
        };
        btnActivate.MouseEnter += (s, e) => { btnActivate.BackColor = Color.FromArgb(225, 225, 225); };
        btnActivate.MouseLeave += (s, e) => { btnActivate.BackColor = Color.White; };
        btnCreate.Click += async (s, e) => await OnCreateKeyAsync();
        btnRefresh.Click += async (s, e) => await RefreshOwnerAsync();
        btnDelete.Click += async (s, e) => await OnDeleteKeyAsync();
        btnCheck.Click += async (s, e) => await OnManualCheckAsync();
        btnUpdateNow.Click += async (s, e) => await OnUpdateNowClickedAsync();
        btnPublish.Click += async (s, e) => await OnPublishAsync();
        cmbLang.Items.AddRange(["العربية", "English"]);
        cmbLang.SelectedIndex = Lang.IsArabic ? 0 : 1;
        btnLangSave.Click += (s, e) =>
        {
            Lang.Current = cmbLang.SelectedIndex == 1 ? Lang.English : Lang.Arabic;
            ApplyLang(true);
        };
        ApplyLang(false);
        floatingDock.SetSelected(0, raise: false);
        ShowIndex(0);
        LayoutPages();
        PositionDock();
        await RefreshLicenseAsync();

        _beat = new System.Windows.Forms.Timer(components) { Interval = 60000 };
        _beat.Tick += async (s, e) => await BeatAsync();
        _beat.Start();

        lblCurVer.Text = $"{Lang.T("upd_current")}: v{AppVersion.Current}";
        FormClosing += (s, e) => _updCts?.Cancel();
        _updCts = new CancellationTokenSource();
        _ = UpdateLoopAsync(_updCts.Token);
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
        if (kind == DockKind.Owner) _ = RefreshOwnerAsync();
        LayoutPages();
    }

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

    private void LayoutPages()
    {
        if (main.Width <= 0) return;
        int cx = main.Width / 2;
        void C(Control c) => c.Left = Math.Max(0, cx - c.Width / 2);
        C(txtKey); C(btnActivate);
        C(lblLang); C(cmbLang); C(btnLangSave);
        C(lblUpdH); C(lblCurVer); C(btnCheck); C(btnUpdateNow);
        lblUpdStatus.Left = Math.Max(10, cx - lblUpdStatus.Width / 2);
        lblLicStatus.Left = Math.Max(10, cx - lblLicStatus.Width / 2);
        C(lblProNote);
        C(lblOnlineH); C(lvOnline);
        C(lblLicH); C(lvLicenses);
        // صف الإنشاء: ثلاث كنترولات جنب بعض في النص
        int rowW = btnCreate.Width + 10 + txtNote.Width + 10 + cmbDays.Width;
        btnCreate.Left = Math.Max(0, cx - rowW / 2);
        txtNote.Left = btnCreate.Right + 10;
        cmbDays.Left = txtNote.Right + 10;
        // النتيجة + التحديث كمجموعة في النص
        int g2 = txtNewKey.Width + 10 + btnRefresh.Width + 10 + btnDelete.Width;
        txtNewKey.Left = Math.Max(0, cx - g2 / 2);
        btnRefresh.Left = txtNewKey.Right + 10;
        btnDelete.Left = btnRefresh.Right + 10;
        lblOwnStatus.Left = Math.Max(10, cx - lblOwnStatus.Width / 2);
        C(lblPubH);
        int prow = txtPubVersion.Width + 10 + txtToken.Width + 10 + txtPubNotes.Width;
        txtPubVersion.Left = Math.Max(0, cx - prow / 2);
        txtToken.Left = txtPubVersion.Right + 10;
        txtPubNotes.Left = txtToken.Right + 10;
        C(btnPublish);
        lblPubStatus.Left = Math.Max(10, cx - lblPubStatus.Width / 2);
    }

    // ---------- اللغة ----------

    private void ApplyLang(bool refresh)
    {
        txtKey.PlaceholderText = Lang.T("key_ph");
        btnActivate.Text = Lang.T("activate");
        lblLang.Text = Lang.T("lang_label");
        btnLangSave.Text = Lang.T("save");
        lblProNote.Text = Lang.T("pro_note");
        lblOnlineH.Text = Lang.T("online_h");
        lblLicH.Text = Lang.T("lic_h");
        btnCreate.Text = Lang.T("create");
        txtNote.PlaceholderText = Lang.T("note_ph");
        btnRefresh.Text = Lang.T("refresh");
        btnDelete.Text = Lang.T("delete_key");
        lblUpdH.Text = Lang.T("upd_title");
        lblCurVer.Text = $"{Lang.T("upd_current")}: v{AppVersion.Current}";
        btnCheck.Text = Lang.T("upd_check");
        btnUpdateNow.Text = Lang.T("upd_update");
        lblPubH.Text = Lang.T("pub_title");
        txtPubVersion.PlaceholderText = Lang.T("pub_version_ph");
        txtToken.PlaceholderText = Lang.T("pub_token_ph");
        txtPubNotes.PlaceholderText = Lang.T("pub_notes_ph");
        btnPublish.Text = Lang.T("pub_publish");
        RefreshColumns();
        FillDays();
        floatingDock.RefreshLanguage();
        exitIcon.RefreshTip();
        if (refresh)
        {
            if (_plan == "pro" || _plan == "owner") UpdateLicenseUi();
            else _ = RefreshLicenseAsync();
            if (pageOwner.Visible) _ = RefreshOwnerAsync();
        }
        LayoutPages();
    }

    private void RefreshColumns()
    {
        string[] on = ["col_device", "col_key", "col_plan", "col_seen"];
        string[] li = ["col_key", "col_plan", "col_active", "col_devices", "col_expires", "col_note"];
        int[] onW = [130, 140, 70, 150];
        int[] liW = [150, 70, 55, 60, 110, 110];
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

    // ---------- الترخيص ----------

    private async Task RefreshLicenseAsync()
    {
        SetBusy(true, "st_checking");
        try
        {
            var st = await LicenseManager.GetStatusAsync().ConfigureAwait(true);
            _plan = st.Active ? st.Plan : "free";
            _ownerKey = st is { Active: true, Plan: "owner" }
                ? LicenseManager.LoadCache()?.Key ?? string.Empty
                : string.Empty;
            _note = st.Active && st.Detail == Lang.T("st_offline") ? " (" + st.Detail + ")" : "";
            if (!st.Active) _inactiveText = st.Detail;
            UpdateLicenseUi();
            RebuildDock();
        }
        finally
        {
            SetBusy(false);
        }
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

        SetBusy(true, "st_activating");
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
            _note = "";
            txtKey.Clear();
            UpdateLicenseUi();
            RebuildDock();
            floatingDock.SetSelected(0);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task OnDeactivateAsync()
    {
        var cache = LicenseManager.LoadCache();
        SetBusy(true, "st_deactivating");
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
            _note = "";
            _inactiveText = Lang.T("st_inactive");
            UpdateLicenseUi();
            RebuildDock();
            SetBusy(false);
        }
    }

    private void UpdateLicenseUi()
    {
        bool active = _plan == "pro" || _plan == "owner";
        lblLicStatus.Text = active
            ? Lang.T(_plan == "owner" ? "st_owner" : "st_pro") + _note
            : _inactiveText;
        txtKey.Visible = !active;
        btnActivate.Visible = !active;
        LayoutPages();
    }

    private void SetBusy(bool busy, string? statusKey = null)
    {
        btnActivate.Enabled = !busy;
        txtKey.Enabled = !busy;
        if (statusKey != null)
        {
            lblLicStatus.Text = Lang.T(statusKey);
            LayoutPages();
        }
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
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

    private async Task CheckBackgroundAsync(CancellationToken ct)
    {
        if (IsDisposed || Disposing) return;
        var r = await UpdateManager.CheckAsync(silent: true, ct).ConfigureAwait(true);
        if (r.Available && r.Release != null) ShowUpdateDialog(r.Release);
    }

    private void ShowUpdateDialog(ReleaseInfo rel)
    {
        if (InvokeRequired) { Invoke(ShowUpdateDialog, rel); return; }
        if (IsDisposed || Disposing) return;
        var bUpdate = new TaskDialogButton(Lang.T("upd_update"));
        var bLater = new TaskDialogButton(Lang.T("upd_later"));
        var page = new TaskDialogPage
        {
            Caption = "StarX",
            Heading = $"{Lang.T("upd_notify_h")}: v{rel.Version}",
            Text = $"{Lang.T("upd_current")}: v{AppVersion.Current}\n{Lang.T("upd_latest")}: v{rel.Version}",
            Icon = TaskDialogIcon.Information,
            Buttons = { bUpdate, bLater },
            DefaultButton = bUpdate,
        };
        var res = TaskDialog.ShowDialog(this, page);
        if (res == bUpdate) _ = UpdateNowAsync(rel);
        else UpdateManager.MarkSuppressed(rel.Version);
    }

    private async Task OnManualCheckAsync()
    {
        btnCheck.Enabled = false;
        btnUpdateNow.Visible = false;
        _pending = null;
        lblUpdStatus.Text = Lang.T("upd_checking");
        LayoutPages();
        try
        {
            var r = await UpdateManager.CheckAsync(silent: false).ConfigureAwait(true);
            if (r.Release == null)
            {
                lblUpdStatus.Text = r.Message ?? Lang.T("upd_failed");
            }
            else if (!r.Available)
            {
                lblUpdStatus.Text = r.Message ??
                    $"{Lang.T("upd_uptodate")} (v{AppVersion.Current})";
            }
            else
            {
                _pending = r.Release;
                UpdateManager.ClearSuppression();
                lblUpdStatus.Text =
                    $"{Lang.T("upd_available")}: v{AppVersion.Current} → v{r.Release.Version}";
                btnUpdateNow.Visible = true;
            }
        }
        finally
        {
            btnCheck.Enabled = true;
            LayoutPages();
        }
    }

    private async Task OnUpdateNowClickedAsync()
    {
        if (_pending == null) return;
        btnUpdateNow.Enabled = false;
        try
        {
            await UpdateNowAsync(_pending);
        }
        finally
        {
            btnUpdateNow.Enabled = true;
        }
    }

    private async Task UpdateNowAsync(ReleaseInfo rel)
    {
        lblUpdStatus.Text = Lang.T("upd_starting");
        LayoutPages();
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
        try
        {
            string token = txtToken.Text;
            var progress = new Progress<string>(m =>
            {
                lblPubStatus.Text = m;
                LayoutPages();
            });
            var (ok, msg) = await PublishManager.PublishAsync(
                txtPubVersion.Text, token, txtPubNotes.Text, progress).ConfigureAwait(true);
            lblPubStatus.Text = msg;
            if (ok) txtToken.Clear();
        }
        finally
        {
            btnPublish.Enabled = true;
            LayoutPages();
        }
    }

    private async Task BeatAsync()
    {
        if (_plan == "free") return;
        var cache = LicenseManager.LoadCache();
        if (cache == null) return;
        await LicenseManager.PingAsync(cache.Key, cache.Hwid).ConfigureAwait(true);
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
            string hw = u.Hwid.Length > 20 ? u.Hwid[..20] + "…" : u.Hwid;
            var it = new ListViewItem(hw);
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
