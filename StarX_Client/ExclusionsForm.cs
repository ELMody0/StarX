namespace StarX_Client;

/// <summary>نافذة إدارة المجلدات المستثناة من الفحص والمسح (تُحفظ فوراً).</summary>
internal sealed class ExclusionsForm : Form
{
    private readonly LuxuryListView _list = new();
    private readonly Button _btnAdd = new();
    private readonly Button _btnRemove = new();
    private readonly Button _btnClose = new();

    public ExclusionsForm()
    {
        Text = Lang.T("excl_title");
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.Black;
        ForeColor = Color.White;
        ClientSize = new Size(560, 360);
        Font = new Font("Segoe UI", 10F);

        _list.BackColor = Color.Black;
        _list.ForeColor = Color.White;
        _list.BorderStyle = BorderStyle.FixedSingle;
        _list.FullRowSelect = true;
        _list.MultiSelect = false;
        _list.View = View.Details;
        _list.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _list.Columns.Add(Lang.T("col_file"), 160);
        _list.Columns.Add(Lang.T("col_path"), 360);
        _list.Location = new Point(16, 16);
        _list.Size = new Size(528, 258);
        _list.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        Controls.Add(_list);

        void Flat(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Color.White;
            b.FlatAppearance.MouseOverBackColor = b.BackColor;
            b.FlatAppearance.MouseDownBackColor = b.BackColor;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        _btnAdd.BackColor = Color.White;
        _btnAdd.ForeColor = Color.Black;
        _btnAdd.Text = Lang.T("excl_add");
        _btnAdd.Location = new Point(16, 288);
        _btnAdd.Size = new Size(160, 34);
        _btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        Flat(_btnAdd);
        _btnAdd.Click += (s, e) => OnAdd();
        Controls.Add(_btnAdd);

        _btnRemove.BackColor = Color.Black;
        _btnRemove.ForeColor = Color.White;
        _btnRemove.Text = Lang.T("excl_remove");
        _btnRemove.Location = new Point(186, 288);
        _btnRemove.Size = new Size(160, 34);
        _btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        Flat(_btnRemove);
        _btnRemove.Click += (s, e) => OnRemove();
        Controls.Add(_btnRemove);

        _btnClose.BackColor = Color.Black;
        _btnClose.ForeColor = Color.White;
        _btnClose.Text = Lang.T("excl_close");
        _btnClose.Location = new Point(394, 288);
        _btnClose.Size = new Size(150, 34);
        _btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Flat(_btnClose);
        _btnClose.Click += (s, e) => Close();
        Controls.Add(_btnClose);

        RefreshList();
    }

    private void RefreshList()
    {
        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var p in CleanerEngine.GetExcludes())
        {
            string name;
            try { name = Path.GetFileName(p); if (string.IsNullOrEmpty(name)) name = p; }
            catch { name = p; }
            var it = new ListViewItem(name) { Tag = p };
            it.SubItems.Add(p);
            _list.Items.Add(it);
        }
        _list.EndUpdate();
    }

    private void OnAdd()
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var all = CleanerEngine.GetExcludes();
        string p = dlg.SelectedPath.Trim();
        if (!all.Contains(p, StringComparer.OrdinalIgnoreCase)) all.Add(p);
        CleanerEngine.SetExcludes(all);
        RefreshList();
    }

    private void OnRemove()
    {
        if (_list.SelectedItems.Count == 0 || _list.SelectedItems[0].Tag is not string sel) return;
        var all = CleanerEngine.GetExcludes();
        all.RemoveAll(p => p.Equals(sel, StringComparison.OrdinalIgnoreCase));
        CleanerEngine.SetExcludes(all);
        RefreshList();
    }
}
