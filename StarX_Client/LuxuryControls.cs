using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace StarX_Client;

// Simple flat black & white controls.
// No animation, no gradients, no shadows, no 3D effects — solid colors only.

internal sealed class MenuBarPanel : Panel
{
    public MenuBarPanel() { DoubleBuffered = true; BackColor = Color.Black; }
}

// Thin flat divider line.
internal sealed class Separator3D : Control
{
    public Separator3D() { Height = 1; Dock = DockStyle.Top; BackColor = Color.White; }
}

// Plain black background panel.
internal sealed class LuxuryBackground : Panel
{
    public LuxuryBackground() { DoubleBuffered = true; Dock = DockStyle.Fill; BackColor = Color.Black; }
}

// Flat button: Primary = white background + black text,
// otherwise black background + white text with a white border.
internal sealed class GlassButton : Button
{
    private bool _primary;

    [DefaultValue(false)]
    public bool Primary
    {
        get => _primary;
        set { _primary = value; ApplyColors(); }
    }

    public GlassButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        Cursor = Cursors.Hand;
        ApplyColors();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyColors();
    }

    private void ApplyColors()
    {
        if (Primary)
        {
            BackColor = Color.White;
            ForeColor = Color.Black;
            FlatAppearance.MouseOverBackColor = Color.White;
            FlatAppearance.MouseDownBackColor = Color.White;
        }
        else
        {
            BackColor = Color.Black;
            ForeColor = Color.White;
            FlatAppearance.MouseOverBackColor = Color.Black;
            FlatAppearance.MouseDownBackColor = Color.Black;
        }
    }
}

// Plain text box: black background, white text, thin white border.
internal sealed class GlassTextBox : TextBox
{
    public GlassTextBox()
    {
        BackColor = Color.Black;
        ForeColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        Font = new Font("Segoe UI", 10.5F);
    }
}

// Flat menu button (kept for compatibility).
internal sealed class LuxuryMenuButton : Button
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Selected { get; set; }

    public LuxuryMenuButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = Color.White;
        FlatAppearance.MouseOverBackColor = Color.Black;
        FlatAppearance.MouseDownBackColor = Color.Black;
        BackColor = Color.Black;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        AutoSize = false;
        Size = new Size(92, 64);
        Cursor = Cursors.Hand;
    }
}

// Flat settings button (kept for compatibility).
internal sealed class GearButton : Button
{
    public GearButton()
    {
        Size = new Size(46, 46);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = Color.White;
        FlatAppearance.MouseOverBackColor = Color.Black;
        FlatAppearance.MouseDownBackColor = Color.Black;
        BackColor = Color.Black;
        ForeColor = Color.White;
        Cursor = Cursors.Hand;
    }
}

// iPhone-style dock: spring indicator, bounce scale, monochrome depth.
internal enum DockKind { Home, Settings, Pro, Owner }

internal sealed class FloatingDock : Control
{
    private const int PadX = 6;
    private const int CellW = 64;
    private const int BarH = 60;
    private readonly List<DockKind> _kinds = [DockKind.Home, DockKind.Settings];
    private readonly ToolTip _tips = new();
    private readonly System.Windows.Forms.Timer _anim = new() { Interval = 15 };
    private int _selected;
    private int _hover = -1;
    private int _pressedIdx = -1;
    private float _ix; // spring indicator x
    private float _iv;
    private float[] _sc = [1f, 1f]; // per-icon scale
    private float[] _sv = [0f, 0f]; // per-icon scale velocity

    public event Action<int>? SelectedChanged;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex => _selected;

    public FloatingDock()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
        ForeColor = Color.White;
        Size = new Size(PadX * 2 + _kinds.Count * CellW, BarH);
        Cursor = Cursors.Hand;
        _ix = CellCenterX(_selected);
        _anim.Tick += (s, e) => Step();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _anim.Dispose(); _tips.Dispose(); }
        base.Dispose(disposing);
    }

    private static string KindName(DockKind k) => k switch
    {
        DockKind.Home => Lang.T("nav_home"),
        DockKind.Settings => Lang.T("nav_settings"),
        DockKind.Pro => Lang.T("nav_pro"),
        DockKind.Owner => Lang.T("nav_owner"),
        _ => "",
    };

    public void RefreshLanguage()
    {
        _tips.SetToolTip(this, _hover >= 0 && _hover < _kinds.Count ? KindName(_kinds[_hover]) : string.Empty);
        Invalidate();
    }

    private void EnsureScales()
    {
        if (_sc.Length != _kinds.Count)
        {
            _sc = new float[_kinds.Count];
            _sv = new float[_kinds.Count];
            for (int i = 0; i < _sc.Length; i++) _sc[i] = 1f;
        }
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0) return;
        try
        {
            Region?.Dispose();
            using var p = StarXTheme.GetRoundedPath(new RectangleF(0, 0, Width, Height), 22);
            Region = new Region(p);
        }
        catch { }
    }

    public void SetItems(IEnumerable<DockKind> kinds)
    {
        _kinds.Clear();
        _kinds.AddRange(kinds);
        if (_kinds.Count == 0) _kinds.Add(DockKind.Home);
        if (_selected >= _kinds.Count) _selected = 0;
        if (_hover >= _kinds.Count) _hover = -1;
        _pressedIdx = -1;
        Width = PadX * 2 + _kinds.Count * CellW;
        Height = BarH;
        UpdateRegion();
        EnsureScales();
        _ix = CellCenterX(_selected);
        _iv = 0;
        for (int i = 0; i < _sc.Length; i++) { _sc[i] = i == _selected ? 1.18f : 1f; _sv[i] = 0; }
        _anim.Stop();
        Invalidate();
    }

    public void SetSelected(int index, bool raise = true)
    {
        index = Math.Clamp(index, 0, _kinds.Count - 1);
        _selected = index;
        EnsureScales();
        _anim.Start();
        Invalidate();
        if (raise) SelectedChanged?.Invoke(index);
    }

    private float CellCenterX(int i) => PadX + i * CellW + CellW / 2f;

    private int HitTest(int x)
    {
        for (int i = 0; i < _kinds.Count; i++)
            if (Math.Abs(x - CellCenterX(i)) < CellW / 2f) return i;
        return -1;
    }

    private void Step()
    {
        EnsureScales();
        float target = CellCenterX(_selected);
        _iv += (target - _ix) * 0.045f;
        _iv *= 0.82f;
        _ix += _iv;
        bool settled = Math.Abs(_iv) < 0.05f && Math.Abs(target - _ix) < 0.4f;
        if (settled) { _ix = target; _iv = 0; }
        for (int i = 0; i < _sc.Length; i++)
        {
            float st = i == _pressedIdx ? 0.86f
                : i == _selected ? 1.18f
                : i == _hover ? 1.07f : 1f;
            _sv[i] += (st - _sc[i]) * 0.06f;
            _sv[i] *= 0.78f;
            _sc[i] = Math.Clamp(_sc[i] + _sv[i], 0.7f, 1.35f);
            if (Math.Abs(_sv[i]) > 0.001f || Math.Abs(st - _sc[i]) > 0.002f) settled = false;
        }
        if (settled) _anim.Stop();
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int h = HitTest(e.X);
        if (h != _hover)
        {
            _hover = h;
            Cursor = h >= 0 ? Cursors.Hand : Cursors.Default;
            _tips.SetToolTip(this, h >= 0 ? KindName(_kinds[h]) : string.Empty);
            _anim.Start();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = -1;
        _tips.SetToolTip(this, string.Empty);
        _anim.Start();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        int h = HitTest(e.X);
        if (h >= 0) { _pressedIdx = h; _anim.Start(); }
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_pressedIdx >= 0)
        {
            int h = HitTest(e.X);
            int idx = _pressedIdx;
            _pressedIdx = -1;
            if (h == idx) SetSelected(idx);
            else { _anim.Start(); Invalidate(); }
        }
        base.OnMouseUp(e);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        EnsureScales();
        _ix = CellCenterX(_selected);
        _iv = 0;
        UpdateRegion();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = BarH;
        UpdateRegion();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (Width <= 0 || Height <= 0) return;
        EnsureScales();
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Black);
        // pill-shaped bar
        using (var barPath = StarXTheme.GetRoundedPath(new RectangleF(1, 1, Width - 2, Height - 2), 21))
        {
            using var bg = new SolidBrush(Color.Black);
            g.FillPath(bg, barPath);
            using var frame = new Pen(Color.White, 1);
            g.DrawPath(frame, barPath);
        }
        float cy = BarH / 2f;
        for (int i = 0; i < _kinds.Count; i++)
        {
            float cx = i == _selected ? _ix : CellCenterX(i);
            bool sel = i == _selected;
            float s = Math.Clamp(_sc[i], 0.7f, 1.35f);
            if (sel)
            {
                // soft offset squircle shadow for depth (monochrome)
                using (var shp = StarXTheme.GetRoundedPath(new RectangleF(cx - 16 + 2.5f, cy - 16 + 3.5f, 32, 32), 11))
                using (var sh = new SolidBrush(Color.FromArgb(70, 70, 70)))
                    g.FillPath(sh, shp);
            }
            var state = g.Save();
            g.TranslateTransform(cx, cy);
            g.ScaleTransform(s, s);
            if (sel)
            {
                // iOS-style rounded-square tile
                using (var tile = StarXTheme.GetRoundedPath(new RectangleF(-16, -16, 32, 32), 10))
                using (var bg = new SolidBrush(Color.White))
                    g.FillPath(bg, tile);
            }
            DrawIcon(g, _kinds[i], 0, 0, sel ? Color.Black : Color.White);
            g.Restore(state);
            if (sel)
            {
                using var ringPath = StarXTheme.GetRoundedPath(new RectangleF(cx - 19, cy - 19, 38, 38), 12);
                using var ring = new Pen(Color.White, 1);
                g.DrawPath(ring, ringPath);
            }
        }
    }

    private static void DrawIcon(Graphics g, DockKind kind, float cx, float cy, Color c)
    {
        using var pen = new Pen(c, 2.2f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
        };
        using var brush = new SolidBrush(c);
        switch (kind)
        {
            case DockKind.Home:
            {
                g.DrawLines(pen, [new PointF(cx - 11, cy - 1), new PointF(cx, cy - 10.5f), new PointF(cx + 11, cy - 1)]);
                g.DrawRectangle(pen, cx - 7.5f, cy - 1, 15, 10.5f);
                g.FillRectangle(brush, cx - 2.4f, cy + 3.4f, 4.8f, 6.1f);
                break;
            }
            case DockKind.Settings:
            {
                float[] rows = [-8f, 0f, 8f];
                float[] knobs = [-4.5f, 5f, -0.5f];
                for (int k = 0; k < 3; k++)
                {
                    float yy = cy + rows[k];
                    g.DrawLine(pen, cx - 10, yy, cx + 10, yy);
                    g.FillEllipse(brush, cx + knobs[k] - 4, yy - 4, 8, 8);
                }
                break;
            }
            case DockKind.Pro:
            {
                var pts = new PointF[10];
                for (int s = 0; s < 10; s++)
                {
                    float ang = (-90 + s * 36) * (float)Math.PI / 180f;
                    float rr = s % 2 == 0 ? 10.5f : 4.4f;
                    pts[s] = new PointF(cx + rr * (float)Math.Cos(ang), cy + rr * (float)Math.Sin(ang));
                }
                g.FillPolygon(brush, pts);
                break;
            }
            case DockKind.Owner:
            {
                var crown = new[]
                {
                    new PointF(cx - 11, cy + 7), new PointF(cx - 11, cy - 2),
                    new PointF(cx - 6, cy + 2.5f), new PointF(cx - 3.5f, cy - 7),
                    new PointF(cx, cy + 1.5f), new PointF(cx + 3.5f, cy - 7),
                    new PointF(cx + 6, cy + 2.5f), new PointF(cx + 11, cy - 2),
                    new PointF(cx + 11, cy + 7),
                };
                g.FillPolygon(brush, crown);
                g.FillEllipse(brush, cx - 5.1f, cy - 8.6f, 3.2f, 3.2f);
                g.FillEllipse(brush, cx + 1.9f, cy - 8.6f, 3.2f, 3.2f);
                break;
            }
        }
    }
}

// Flat card: black background, thin white border, white header text.
internal sealed class CardPanel : Panel
{
    private string? _headerText;

    public CardPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
        ForeColor = Color.White;
        Margin = new Padding(4);
    }

    [DefaultValue(typeof(Color), "White")]
    public Color Accent { get; set; } = Color.White;

    [DefaultValue(null)]
    public string? HeaderText
    {
        get => _headerText;
        set
        {
            if (_headerText != value)
            {
                _headerText = value;
                if (IsHandleCreated) Invalidate();
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        try { PaintLuxe(e); } catch { }
    }

    private void PaintLuxe(PaintEventArgs e)
    {
        var g = e.Graphics;
        if (g == null || Width <= 0 || Height <= 0) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new RectangleF(1, 1, Width - 2, Height - 2);
        const int rad = 14;

        // 1) soft lift shadow (monochrome)
        using (var shp = StarXTheme.GetRoundedPath(new RectangleF(rect.X + 1, rect.Y + 3, rect.Width, rect.Height), rad))
        using (var sh = new SolidBrush(Color.FromArgb(60, 60, 60)))
            g.FillPath(sh, shp);

        // 2) body: subtle vertical sheen, dark gray -> black
        using (var body = StarXTheme.GetRoundedPath(rect, rad))
        using (var bg = new LinearGradientBrush(
            new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, Math.Max(1, (int)rect.Height)),
            Color.FromArgb(26, 26, 28), Color.Black, LinearGradientMode.Vertical))
            g.FillPath(bg, body);

        // 3) static glass light falling from the top
        using (var clip = StarXTheme.GetRoundedPath(rect, rad))
        {
            g.SetClip(clip);
            using var sheen = new LinearGradientBrush(
                new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, Math.Max(1, (int)(rect.Height / 2))),
                Color.FromArgb(34, 255, 255, 255), Color.FromArgb(0, 255, 255, 255),
                LinearGradientMode.Vertical);
            g.FillRectangle(sheen, rect.X, rect.Y, rect.Width, rect.Height / 2f + 1);
            g.ResetClip();
        }

        // 4) soft frame + bright top edge (less glare)
        using (var bp = StarXTheme.GetRoundedPath(rect, rad))
        using (var frame = new Pen(Color.FromArgb(215, 215, 220), 1))
            g.DrawPath(frame, bp);
        using (var hl = new Pen(Color.FromArgb(120, 255, 255, 255), 1))
            g.DrawLine(hl, rect.X + rad, rect.Y + 0.5f, rect.Right - rad, rect.Y + 0.5f);

        // 5) header: white bar + bold title + divider (same metrics as before)
        if (!string.IsNullOrEmpty(HeaderText))
        {
            using (var bar = StarXTheme.GetRoundedPath(new RectangleF(16, 12, 4, 20), 2))
            using (var wb = new SolidBrush(Color.White))
                g.FillPath(wb, bar);
            using var hf = new Font("Segoe UI", 12F, FontStyle.Bold);
            TextRenderer.DrawText(g, HeaderText, hf,
                new Rectangle(28, 8, Math.Max(10, (int)rect.Width - 46), 28),
                Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            using var div = new Pen(Color.FromArgb(80, 255, 255, 255), 1);
            g.DrawLine(div, 16, 42, rect.Right - 2, 42);
        }
    }
}

// Flat button: Primary = white background + black text,
// otherwise black background + white text with a white border.
internal sealed class LuxuryButton : Button
{
    private bool _primary = true;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Primary
    {
        get => _primary;
        set { _primary = value; ApplyColors(); }
    }

    public LuxuryButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        Cursor = Cursors.Hand;
        ApplyColors();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyColors();
    }

    private void ApplyColors()
    {
        if (Primary)
        {
            BackColor = Color.White;
            ForeColor = Color.Black;
            FlatAppearance.MouseOverBackColor = Color.White;
            FlatAppearance.MouseDownBackColor = Color.White;
        }
        else
        {
            BackColor = Color.Black;
            ForeColor = Color.White;
            FlatAppearance.MouseOverBackColor = Color.Black;
            FlatAppearance.MouseDownBackColor = Color.Black;
        }
    }
}

// Soft monochrome list: dark alternating rows, white selection. No animation.
internal sealed class LuxuryListView : ListView
{
    private static readonly Color RowEven = Color.FromArgb(16, 16, 20);
    private static readonly Color RowOdd = Color.FromArgb(22, 22, 29);
    private static readonly Color RowHover = Color.FromArgb(32, 32, 42);
    private static readonly Color HeadBg = Color.FromArgb(27, 27, 33);
    private static readonly Color LineColor = Color.FromArgb(38, 38, 46);
    private static readonly Color TextSoft = Color.FromArgb(237, 237, 239);
    private static readonly Color TextDim = Color.FromArgb(160, 160, 170);
    private static readonly Color SelectBg = Color.FromArgb(237, 237, 239);

    private int _hover = -1;
    private readonly Font _headFont = new("Segoe UI", 9F, FontStyle.Bold);

    public LuxuryListView()
    {
        OwnerDraw = true;
        DoubleBuffered = true;
        BackColor = Color.Black;
        ForeColor = TextSoft;
        BorderStyle = BorderStyle.FixedSingle;
        FullRowSelect = true;
        HideSelection = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _headFont.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int h = HitTest(e.X, e.Y).Item?.Index ?? -1;
        if (h != _hover) { _hover = h; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = -1;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
    {
        var g = e.Graphics;
        using (var bg = new SolidBrush(HeadBg))
            g.FillRectangle(bg, e.Bounds);
        TextRenderer.DrawText(g, e.Header?.Text ?? "", _headFont,
            new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height),
            TextSoft, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        using (var div = new Pen(LineColor))
        {
            g.DrawLine(div, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            g.DrawLine(div, e.Bounds.Right - 1, e.Bounds.Y + 4, e.Bounds.Right - 1, e.Bounds.Bottom - 4);
        }
    }

    protected override void OnDrawItem(DrawListViewItemEventArgs e)
    {
        e.DrawDefault = false;
    }

    protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
    {
        var g = e.Graphics;
        bool sel = e.Item?.Selected ?? false;
        bool hov = e.Item?.Index == _hover && !sel;
        Color back = sel ? SelectBg
            : hov ? RowHover
            : (e.Item?.Index ?? 0) % 2 == 0 ? RowEven : RowOdd;
        using (var b = new SolidBrush(back))
            g.FillRectangle(b, e.Bounds);
        using (var ln = new Pen(LineColor))
        {
            g.DrawLine(ln, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            g.DrawLine(ln, e.Bounds.Right - 1, e.Bounds.Y, e.Bounds.Right - 1, e.Bounds.Bottom);
        }

        Color fore = sel ? Color.Black : TextSoft;
        if (e.ColumnIndex == 0 && CheckBoxes)
        {
            // soft round checkbox
            float cx = e.Bounds.X + 8, cyy = e.Bounds.Y + (e.Bounds.Height - 14) / 2f;
            bool on = e.Item?.Checked ?? false;
            Color ring = sel ? Color.Black : on ? TextSoft : TextDim;
            using (var p = new Pen(ring, 1.5f))
                g.DrawEllipse(p, cx, cyy, 14, 14);
            if (on)
            {
                using var f = new SolidBrush(sel ? Color.Black : Color.White);
                g.FillEllipse(f, cx + 3.5f, cyy + 3.5f, 7, 7);
            }
            return;
        }

        TextRenderer.DrawText(g, e.SubItem?.Text ?? "", Font,
            new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height),
            fore, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
    }
}

// Logout button: vivid red door + exiting arrow on black.
internal sealed class ExitIconButton : Button
{
    private static readonly Color LogoutRed = Color.FromArgb(229, 57, 53);
    private readonly ToolTip _tips = new();

    public ExitIconButton()
    {
        Size = new Size(36, 36);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Color.Black;
        FlatAppearance.MouseDownBackColor = Color.Black;
        BackColor = Color.Black;
        ForeColor = LogoutRed;
        Text = "";
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        _tips.SetToolTip(this, Lang.T("exit_tip"));
    }

    public void RefreshTip() => _tips.SetToolTip(this, Lang.T("exit_tip"));

    protected override void Dispose(bool disposing)
    {
        if (disposing) _tips.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        if (Width <= 0 || Height <= 0) return;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Black);
        using (var frame = new Pen(LogoutRed, 1))
            g.DrawRectangle(frame, 0, 0, Width - 1, Height - 1);
        using var pen = new Pen(LogoutRed, 2.6f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
        };
        // door frame open on the right: top / left / bottom
        g.DrawLine(pen, 9, 8, 17, 8);
        g.DrawLine(pen, 9, 8, 9, 28);
        g.DrawLine(pen, 9, 28, 17, 28);
        // bold arrow leaving the door to the right
        g.DrawLine(pen, 13, 18, 27, 18);
        g.DrawLine(pen, 22, 13, 27, 18);
        g.DrawLine(pen, 22, 23, 27, 18);
    }
}

// Standard checkbox: black background, white text.
internal sealed class LuxuryCheckBox : CheckBox
{
    public LuxuryCheckBox()
    {
        AutoSize = true;
        BackColor = Color.Black;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        Padding = new Padding(6, 0, 0, 0);
    }
}

// Flat dropdown list: black field, dark popup menu, white text.
internal sealed class LuxuryComboBox : ComboBox
{
    public LuxuryComboBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        FlatStyle = FlatStyle.Flat;
        BackColor = Color.Black;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        ItemHeight = 24;
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        try
        {
            var g = e.Graphics;
            if (g == null) return;
            if (e.Index < 0 || e.Index >= Items.Count) { e.DrawBackground(); return; }
            bool edit = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            string txt = Items[e.Index]?.ToString() ?? "";
            if (edit)
            {
                e.DrawBackground();
                TextRenderer.DrawText(g, txt, Font, e.Bounds, ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                return;
            }
            using (var b = new SolidBrush(sel ? Color.White : Color.FromArgb(16, 16, 20)))
                g.FillRectangle(b, e.Bounds);
            TextRenderer.DrawText(g, txt, Font,
                new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height),
                sel ? Color.Black : Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }
        catch { /* ignore */ }
    }
}

// Small title-bar update icon: dim circular arrow when idle,
// bright arrow + white badge dot when an update is pending.
internal sealed class UpdateIconButton : Control
{
    private bool _hasUpdate;
    private string _version = "";
    private readonly ToolTip _tips = new();

    public UpdateIconButton()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
        Size = new Size(34, 34);
        Cursor = Cursors.Hand;
        RefreshTip();
    }

    public void SetHasUpdate(bool has, string version)
    {
        _hasUpdate = has;
        _version = version ?? "";
        RefreshTip();
        Invalidate();
    }

    public void RefreshTip()
    {
        _tips.SetToolTip(this, _hasUpdate
            ? $"{Lang.T("upd_available")}: v{_version}"
            : $"{Lang.T("upd_title")} • v{AppVersion.Current}");
    }

    public void ShowHint(string text)
    {
        try { _tips.Show(text, this, 2500); } catch { /* ignore */ }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _tips.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (Width <= 0 || Height <= 0) return;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        float cx = Width / 2f;
        // download glyph: bold down-arrow into a tray (always bright white)
        using (var pen = new Pen(Color.White, 2.8f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
        })
        {
            g.DrawLine(pen, cx, 6, cx, 19);
            g.DrawLine(pen, cx - 5.5f, 14, cx, 19.5f);
            g.DrawLine(pen, cx + 5.5f, 14, cx, 19.5f);
            g.DrawLine(pen, cx - 9, 25, cx + 9, 25);
            g.DrawLine(pen, cx - 9, 25, cx - 9, 22);
            g.DrawLine(pen, cx + 9, 25, cx + 9, 22);
        }
        if (_hasUpdate)
        {
            // badge dot, top-right
            float bx = Width - 8, by = 7;
            using (var ring = new Pen(Color.Black, 2))
                g.DrawEllipse(ring, bx - 6.5f, by - 6.5f, 13, 13);
            using var dot = new SolidBrush(Color.White);
            g.FillEllipse(dot, bx - 5, by - 5, 10, 10);
        }
    }
}

// Custom frameless title bar: app title + update icon + min/max/close.
// Dragging the bar moves the window (with Aero snap); double-click toggles maximize.
internal sealed class TitleBar : Control
{
    public const int BarH = 38;
    private const int BtnW = 46;

    private readonly Label _title = new();

    public UpdateIconButton UpdButton { get; } = new();
    public Button MinButton { get; }
    public Button MaxButton { get; }
    public Button CloseButton { get; }

    public string Title
    {
        get => _title.Text;
        set => _title.Text = value;
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int HTCAPTION = 2;

    public TitleBar()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Top;
        Height = BarH;
        BackColor = Color.Black;
        SetStyle(ControlStyles.StandardDoubleClick, true);

        _title.AutoSize = false;
        _title.BackColor = Color.Black;
        _title.ForeColor = Color.White;
        _title.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        _title.TextAlign = ContentAlignment.MiddleLeft;
        _title.MouseDown += (s, e) => BeginDrag(e);
        _title.DoubleClick += (s, e) => OnDoubleClick(e);
        Controls.Add(_title);

        MinButton = MakeCaptionButton("–");
        MaxButton = MakeCaptionButton("□");
        CloseButton = MakeCaptionButton("✕");
        CloseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
        CloseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 28, 18);

        UpdButton.Cursor = Cursors.Hand;
        Controls.Add(UpdButton);
    }

    private Button MakeCaptionButton(string text)
    {
        var b = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.White,
            BackColor = Color.Black,
            Size = new Size(BtnW, BarH),
            TabStop = false,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand,
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 42, 48);
        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(42, 42, 48);
        Controls.Add(b);
        return b;
    }

    private void BeginDrag(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        try
        {
            var f = FindForm();
            if (f == null) return;
            ReleaseCapture();
            SendMessage(f.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }
        catch { /* ignore */ }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        BeginDrag(e);
        base.OnMouseDown(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = BarH;
        // قد يُستدعى أثناء الـconstructor قبل إنشاء الأزرار
        if (CloseButton == null || MaxButton == null || MinButton == null) return;
        CloseButton.Location = new Point(Width - BtnW, 0);
        MaxButton.Location = new Point(Width - BtnW * 2, 0);
        MinButton.Location = new Point(Width - BtnW * 3, 0);
        UpdButton.Location = new Point(Width - BtnW * 3 - 40, 2);
        _title.Location = new Point(12, 0);
        _title.Size = new Size(Math.Max(50, Width - BtnW * 3 - 40 - 24), BarH);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (Width <= 0 || Height <= 0) return;
        g.Clear(Color.Black);
        using var sep = new Pen(Color.FromArgb(46, 46, 54), 1);
        g.DrawLine(sep, 0, Height - 1, Width, Height - 1);
    }
}
