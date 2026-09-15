using System.Drawing.Drawing2D;

namespace StarX_Client;

// شريط علوي فخم: زجاج داكن + إبراز علوي + توهج خلف اللوجو + ظل داخلي سفلي
internal sealed class MenuBarPanel : Panel
{
    public MenuBarPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(17, 17, 17);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (Width <= 0 || Height <= 0) return;

        // تدرج أساسي أعمق
        using (var bg = new LinearGradientBrush(
            ClientRectangle,
            Color.FromArgb(48, 48, 48),
            Color.FromArgb(8, 8, 8),
            LinearGradientMode.Vertical))
            g.FillRectangle(bg, ClientRectangle);

        // لمعة زجاجية على النصف العلوي
        using (var gloss = new LinearGradientBrush(
            new Rectangle(0, 0, Width, Math.Max(1, Height / 2)),
            Color.FromArgb(60, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.Vertical))
            g.FillRectangle(gloss, 0, 0, Width, Height / 2);

        // توهج ناعم خلف اللوجو (مكان الأيقونة أعلى اليسار)
        using (var path = new GraphicsPath())
        {
            path.AddEllipse(2, -6, 72, 72);
            using var halo = new PathGradientBrush(path);
            halo.CenterColor = Color.FromArgb(55, 255, 255, 255);
            halo.SurroundColors = [Color.FromArgb(0, 255, 255, 255)];
            g.FillEllipse(halo, 2, -6, 72, 72);
        }

        // خط علوي مضيء + خط ثانٍ خافت (إحساس معدني)
        using (var p = new Pen(Color.FromArgb(230, 255, 255, 255)))
            g.DrawLine(p, 0, 0, Width, 0);
        using (var p = new Pen(Color.FromArgb(95, 95, 95)))
            g.DrawLine(p, 0, 1, Width, 1);

        // ظل داخلي سفلي يعطي عمق فوق الفاصل الـ 3D
        using (var sh = new LinearGradientBrush(
            new Rectangle(0, Height - 5, Width, 5),
            Color.FromArgb(0, 0, 0, 0),
            Color.FromArgb(170, 0, 0, 0),
            LinearGradientMode.Vertical))
            g.FillRectangle(sh, 0, Height - 5, Width, 5);
    }
}

// فاصل 3D تحت المنيو بار: إبراز أبيض + ظل يعطي عمق
internal sealed class Separator3D : Control
{
    public Separator3D()
    {
        Height = 6;
        Dock = DockStyle.Top;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        // 1px أبيض (إبراز علوي)
        using (var p = new Pen(Color.FromArgb(255, 255, 255)))
            g.DrawLine(p, 0, 0, Width, 0);
        // 1px رمادي متوسط
        using (var p = new Pen(Color.FromArgb(140, 140, 140)))
            g.DrawLine(p, 0, 1, Width, 1);
        // 2px رمادي غامق
        using (var b = new SolidBrush(Color.FromArgb(42, 42, 42)))
            g.FillRectangle(b, 0, 2, Width, 2);
        // 2px ظل أسود
        using (var b = new SolidBrush(Color.FromArgb(0, 0, 0)))
            g.FillRectangle(b, 0, 4, Width, 2);
    }
}

// خلفية سيمبل: أسود سادة (بدون تأثيرات)
internal sealed class LuxuryBackground : Panel
{
    public LuxuryBackground()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = Color.Black;
    }
}

// زر قائمة فخم: نص أبيض + خط سفلي عند التحويم/التحديد
internal sealed class LuxuryMenuButton : Button
{
    private bool _hover;
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Selected { get; set; }

    public LuxuryMenuButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        BackColor = Color.Transparent;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        AutoSize = false;
        Size = new Size(92, 64);
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Color.FromArgb(17, 17, 17));

        if (_hover || Selected)
        {
            using var bg = new SolidBrush(Color.FromArgb(38, 38, 38));
            g.FillRectangle(bg, 4, 8, Width - 8, Height - 16);
        }

        var fore = (_hover || Selected) ? Color.White : Color.FromArgb(210, 210, 210);
        TextRenderer.DrawText(g, Text, Font, new Rectangle(0, 0, Width, Height - 4),
            fore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        if (_hover || Selected)
        {
            using var underline = new SolidBrush(Color.White);
            g.FillRectangle(underline, Width / 2 - 22, Height - 12, 44, 2);
        }
    }
}

// زر إعدادات: ترس مرسوم بالكود فقط (بدون أي ملف خارجي)
internal sealed class GearButton : Button
{
    private bool _hover;
    private float _angle;

    public GearButton()
    {
        Size = new Size(46, 46);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        BackColor = Color.Transparent;
        ForeColor = Color.White;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        _angle = 12f;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        _angle = 0f;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.FromArgb(17, 17, 17));

        if (_hover)
        {
            using var bg = new SolidBrush(Color.FromArgb(45, 45, 45));
            g.FillEllipse(bg, 2, 2, Width - 4, Height - 4);
            using var ring = new Pen(Color.FromArgb(255, 255, 255), 1);
            g.DrawEllipse(ring, 2, 2, Width - 4, Height - 4);
        }

        // رسم الترس
        var cx = Width / 2f;
        var cy = Height / 2f;
        var outer = 13f;
        var inner = 9f;
        const int teeth = 8;

        g.TranslateTransform(cx, cy);
        g.RotateTransform(_angle);

        using var gearBrush = new SolidBrush(Color.White);
        for (int i = 0; i < teeth; i++)
        {
            float a = i * 360f / teeth;
            var state = g.Save();
            g.RotateTransform(a);
            g.FillRectangle(gearBrush, -2.2f, -outer - 1.5f, 4.4f, 5f);
            g.Restore(state);
        }

        g.FillEllipse(gearBrush, -inner, -inner, inner * 2, inner * 2);

        // الثقب الداخلي (بلون الخلفية لمحاكاة التفريغ)
        var holeColor = _hover ? Color.FromArgb(45, 45, 45) : (Parent?.BackColor ?? Color.FromArgb(17, 17, 17));
        using var hole = new SolidBrush(holeColor);
        g.FillEllipse(hole, -5.5f, -5.5f, 11f, 11f);
        using var holeRing = new Pen(Color.White, 2f);
        g.DrawEllipse(holeRing, -5.5f, -5.5f, 11f, 11f);

        g.ResetTransform();
    }
}

// منصة تحكم زجاجية فاخرة: أزرار بارزة 3D + زنبرك منزلق
// أنواع أزرار الفقاعة: هوم • إعدادات • برو • مالك
internal enum DockKind { Home, Settings, Pro, Owner }

internal sealed class FloatingDock : Control
{
    private const int PadX = 3;
    private const int CellMin = 71;
    private readonly List<DockKind> _kinds = [DockKind.Home, DockKind.Settings];

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
        if (_hover >= 0 && _hover < _kinds.Count)
            _tips.SetToolTip(this, KindName(_kinds[_hover]));
        Invalidate();
    }

    private int Items => _kinds.Count;

    private int _selected;
    private int _hover = -1;
    private float _x;            // موضع المؤشر (زنبرك فيزيائي)
    private float _v;            // سرعة الزنبرك
    private float _target;
    private bool _snapped;
    private float _press;        // انضغاط اللمس 0..1
    private float _pressTarget;
    private float _hoverGlow;    // توهج التحويم 0..1
    private readonly System.Windows.Forms.Timer _anim;
    private readonly ToolTip _tips;

    public event Action<int>? SelectedChanged;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex => _selected;

    public FloatingDock()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        _tips = new ToolTip();
        _anim = new System.Windows.Forms.Timer { Interval = 15 };
        Size = new Size(148, 60);
        Cursor = Cursors.Hand;
        _anim.Tick += (s, e) =>
        {
            // زنبرك فيزيائي مخمد: انزلاق + ارتداد خفيف ستايل iOS
            float f = (_target - _x) * 0.16f;
            _v = (_v + f) * 0.70f;
            _x += _v;
            if (Math.Abs(_v) < 0.05f && Math.Abs(_target - _x) < 0.4f) { _x = _target; _v = 0; }
            // انضغاط اللمس + توهج التحويم
            _press += (_pressTarget - _press) * 0.35f;
            float hg = (_hover >= 0 && _hover != _selected) ? 1f : 0f;
            _hoverGlow += (hg - _hoverGlow) * 0.25f;
            bool calm = _x == _target && _v == 0 &&
                        Math.Abs(_pressTarget - _press) < 0.01f &&
                        Math.Abs(hg - _hoverGlow) < 0.01f;
            if (calm) { _press = _pressTarget; _hoverGlow = hg; _anim.Stop(); }
            Invalidate();
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _anim.Dispose(); _tips.Dispose(); }
        base.Dispose(disposing);
    }

    public void SetItems(IEnumerable<DockKind> kinds)
    {
        _kinds.Clear();
        _kinds.AddRange(kinds);
        if (_kinds.Count == 0) _kinds.Add(DockKind.Home);
        if (_selected >= _kinds.Count) _selected = 0;
        Width = PadX * 2 + _kinds.Count * CellMin;
        _target = CellCenterX(_selected);
        _x = _target;
        _v = 0;
        _anim.Start();
        Invalidate();
    }

    public void SetSelected(int index, bool raise = true)
    {
        index = Math.Clamp(index, 0, Items - 1);
        _selected = index;
        _target = CellCenterX(index);
        if (!_snapped) { _x = _target; _v = 0; _snapped = true; }
        _anim.Start();
        Invalidate();
        if (raise) SelectedChanged?.Invoke(index);
    }

    private float CellW => (Width - PadX * 2) / (float)Items;
    private float CellCenterX(int i) => PadX + i * CellW + CellW / 2f;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _target = CellCenterX(_selected);
        if (!_anim.Enabled) { _x = _target; _v = 0; }
        Invalidate();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        _target = CellCenterX(_selected);
        _x = _target;
        _v = 0;
        _snapped = true;
    }

    private int HitTest(int x)
    {
        for (int i = 0; i < Items; i++)
            if (Math.Abs(x - CellCenterX(i)) < CellW / 2f) return i;
        return -1;
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
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = -1;
        _pressTarget = 0;
        _anim.Start();
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        int h = HitTest(e.X);
        _pressTarget = 1;
        _anim.Start();
        if (h >= 0) SetSelected(h);
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _pressTarget = 0;
        _anim.Start();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        if (Width <= 0 || Height <= 0) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // فقاعة سيمبل: أسود سادة + إطار خفيف + إبراز علوي
        var pill = new RectangleF(3, 4, Width - 6, Height - 8);
        using (var body = new SolidBrush(Color.FromArgb(20, 20, 20)))
        using (var pp = RoundedRect(pill, 26))
            g.FillPath(body, pp);
        using (var border = new Pen(Color.FromArgb(38, 255, 255, 255), 1))
        using (var bp = RoundedRect(pill, 26))
            g.DrawPath(border, bp);
        using (var hl = new Pen(Color.FromArgb(55, 255, 255, 255), 1))
            g.DrawLine(hl, pill.X + 24, pill.Y + 1, pill.Right - 24, pill.Y + 1);

        // دائرة التحديد البيضاء + ظل ناعم
        float sc = 1f - 0.10f * _press;
        float r = 19 * sc;
        float cyk = Height / 2f + 1;
        using (var sh = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            g.FillEllipse(sh, _x - r, cyk - r + 2.5f, r * 2, r * 2);
        using (var w = new SolidBrush(Color.White))
            g.FillEllipse(w, _x - r, cyk - r, r * 2, r * 2);

        // تحويم خفيف + أيقونات خطية رفيعة
        if (_hoverGlow > 0.02f && _hover >= 0 && _hover != _selected)
        {
            float hx = CellCenterX(_hover);
            using var hbg = new SolidBrush(Color.FromArgb((int)(22 * _hoverGlow), 255, 255, 255));
            g.FillEllipse(hbg, hx - 18, cyk - 18, 36, 36);
        }
        for (int i = 0; i < Items; i++)
        {
            float cx = CellCenterX(i);
            bool sel = i == _selected;
            Color c = sel ? Color.Black : (_hover == i ? Color.White : Color.FromArgb(175, 175, 175));
            DrawIcon(g, (int)_kinds[i], cx, cyk, c, c);
        }
    }

    private static void DrawIcon(Graphics g, int kind, float cx, float cy, Color c, Color bg)
    {
        var prev = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(c, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        using var brush = new SolidBrush(c);
        switch (kind)
        {
            case 0: // هوم
                {
                    var roof = new[] { new PointF(cx - 11, cy - 1), new PointF(cx, cy - 10.5f), new PointF(cx + 11, cy - 1) };
                    g.DrawLines(pen, roof);
                    g.DrawRectangle(pen, cx - 7.5f, cy - 1, 15, 10.5f);
                    g.FillRectangle(brush, cx - 2.4f, cy + 3.4f, 4.8f, 6.1f);
                    break;
                }
            case 1: // الإعدادات: سلايدرز سيمبل
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
            case 2: // البرو: نجمة
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
            case 3: // المالك: تاج
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
                    g.DrawPolygon(pen, crown);
                    g.FillEllipse(brush, cx - 5.1f, cy - 8.6f, 3.2f, 3.2f);
                    g.FillEllipse(brush, cx + 1.9f, cy - 8.6f, 3.2f, 3.2f);
                    break;
                }
            default:
                goto case 1;
        }
        g.SmoothingMode = prev;
    }

    private static GraphicsPath RoundedRect(RectangleF r, float radius)
    {
        float d = Math.Max(1, radius * 2);
        var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }
}

// أيقونة خروج حمراء صغيرة (باب + سهم)
internal sealed class ExitIconButton : Control
{
    private bool _hover;
    private readonly ToolTip _tips;

    public ExitIconButton()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Size = new Size(34, 34);
        Cursor = Cursors.Hand;
        _tips = new ToolTip();
        _tips.SetToolTip(this, Lang.T("exit_tip"));
    }

    public void RefreshTip() => _tips.SetToolTip(this, Lang.T("exit_tip"));

    protected override void Dispose(bool disposing)
    {
        if (disposing) _tips.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        if (Width <= 0 || Height <= 0) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        Color c = _hover ? Color.FromArgb(255, 120, 120) : Color.FromArgb(205, 92, 92);
        if (_hover)
        {
            using var glow = new SolidBrush(Color.FromArgb(45, 255, 90, 90));
            g.FillEllipse(glow, 2, 2, Width - 4, Height - 4);
        }
        using var pen = new Pen(c, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        // الباب (مفتوح يساراً)
        g.DrawLine(pen, 26, 8, 26, 26);
        g.DrawLine(pen, 26, 8, 17, 8);
        g.DrawLine(pen, 26, 26, 17, 26);
        // السهم للخارج (يساراً)
        g.DrawLine(pen, 22, 17, 10, 17);
        g.DrawLine(pen, 14, 13, 10, 17);
        g.DrawLine(pen, 14, 21, 10, 17);
    }
}
