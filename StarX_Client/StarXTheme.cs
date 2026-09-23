using System.Drawing;
using System.Drawing.Drawing2D;

namespace StarX_Client;

/// <summary>
/// Centralized design tokens for the StarX futuristic dark-theme system.
/// All colors, radii, spacing, and effects are defined here for global consistency.
/// </summary>
internal static class StarXTheme
{
    // ── Color Palette (ARGB format) ──
    public static readonly Color DeepBlack = Color.FromArgb(9, 9, 11);
    public static readonly Color PureBlack = Color.FromArgb(0, 0, 0);
    public static readonly Color SurfaceDark = Color.FromArgb(18, 18, 22);
    public static readonly Color SurfaceElevated = Color.FromArgb(26, 26, 34);
    public static readonly Color SurfaceHover = Color.FromArgb(34, 34, 44);
    public static readonly Color BorderSubtle = Color.FromArgb(20, 20, 32);
    public static readonly Color BorderGlass = Color.FromArgb(20, 255, 255, 255);
    public static readonly Color BorderGlassHover = Color.FromArgb(40, 255, 255, 255);
    public static readonly Color BorderAccent = Color.FromArgb(28, 102, 255);
    public static readonly Color TextPrimary = Color.FromArgb(250, 250, 250);
    public static readonly Color TextSecondary = Color.FromArgb(156, 163, 175);
    public static readonly Color TextDisabled = Color.FromArgb(107, 114, 128);
    public static readonly Color TextInverse = Color.FromArgb(9, 9, 11);

    // ── Accent / Glow ──
    public static readonly Color AccentCyan = Color.FromArgb(0, 180, 255);
    public static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
    public static readonly Color AccentGlow = Color.FromArgb(26, 68, 255);
    public static readonly Color AccentGlowWeak = Color.FromArgb(13, 34, 68);
    public static readonly Color GlowWhite = Color.FromArgb(68, 255, 255, 255);

    // ── Glassmorphism Surfaces ──
    public static readonly Color GlassBg1 = Color.FromArgb(188, 18, 18, 22);
    public static readonly Color GlassBg2 = Color.FromArgb(204, 26, 26, 34);
    public static readonly Color GlassBg3 = Color.FromArgb(153, 15, 15, 20);

    // ── Shadows & Glow ──
    public static readonly Color ShadowOuter = Color.FromArgb(51, 0, 0, 0);
    public static readonly Color ShadowInnerTop = Color.FromArgb(24, 255, 255, 255);
    public static readonly Color ShadowInnerBottom = Color.FromArgb(0, 0, 0, 0);
    public static readonly Color ShadowGlowAccent = Color.FromArgb(26, 59, 130, 255);

    // ── Radius ──
    public const int RadiusSm = 6;
    public const int RadiusMd = 10;
    public const int RadiusLg = 16;
    public const int RadiusXl = 22;
    public const int RadiusPill = 9999;

    // ── Spacing ──
    public const int SpacingXs = 4;
    public const int SpacingSm = 8;
    public const int SpacingMd = 12;
    public const int SpacingLg = 16;
    public const int SpacingXl = 24;

    // ── Animation ──
    public const int AnimFast = 12;
    public const int AnimNormal = 15;
    public const int AnimSlow = 25;

    // ── Helper Conversions ──
    public static Color DeepBlackColor => DeepBlack;
    public static Color PureBlackColor => PureBlack;
    public static Color SurfaceDarkColor => SurfaceDark;
    public static Color SurfaceElevatedColor => SurfaceElevated;
    public static Color SurfaceHoverColor => SurfaceHover;
    public static Color BorderGlassColor => BorderGlass;
    public static Color BorderGlassHoverColor => BorderGlassHover;
    public static Color BorderAccentColor => BorderAccent;
    public static Color TextPrimaryColor => TextPrimary;
    public static Color TextSecondaryColor => TextSecondary;
    public static Color TextDisabledColor => TextDisabled;
    public static Color TextInverseColor => TextInverse;
    public static Color AccentCyanColor => AccentCyan;
    public static Color AccentBlueColor => AccentBlue;
    public static Color AccentGlowColor => AccentGlow;
    public static Color GlowWhiteColor => GlowWhite;

    public static Color GetGlassBrush(int alpha, int r = 18, int g = 18, int b = 22)
    {
        return Color.FromArgb(alpha, r, g, b);
    }

    public static Color GetElevatedGlassBrush(int alpha)
    {
        return Color.FromArgb(alpha, 26, 26, 34);
    }

    // ── int (0xRRGGBB or 0xAARRGGBB) -> Color ──
    // NOTE: Color.FromArgb(int) treats 0x0E0E14 as alpha=0 (transparent) and
    // TextBox/ComboBox throw "Control does not support transparent background colors".
    // So 6-digit RGB must become opaque explicitly; 8-digit keeps its alpha.
    public static Color Opaque(int rgb) =>
        Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);

    public static Color FromInt(int c) =>
        c > 0xFFFFFF ? Color.FromArgb(c) : Opaque(c);

    // ── Gradient Helpers ──
    public static LinearGradientBrush GetVerticalGradient(Rectangle bounds, int topColor, int bottomColor)
    {
        return new LinearGradientBrush(bounds, FromInt(topColor), FromInt(bottomColor), LinearGradientMode.Vertical);
    }

    public static LinearGradientBrush GetHorizontalGradient(Rectangle bounds, int leftColor, int rightColor)
    {
        return new LinearGradientBrush(bounds, FromInt(leftColor), FromInt(rightColor), LinearGradientMode.Horizontal);
    }

    public static LinearGradientBrush GetVerticalGradient(Rectangle bounds, Color topColor, Color bottomColor)
    {
        return new LinearGradientBrush(bounds, topColor, bottomColor, LinearGradientMode.Vertical);
    }

    public static LinearGradientBrush GetHorizontalGradient(Rectangle bounds, Color leftColor, Color rightColor)
    {
        return new LinearGradientBrush(bounds, leftColor, rightColor, LinearGradientMode.Horizontal);
    }

    public static LinearGradientBrush GetAccentGradient(Rectangle bounds)
    {
        return new LinearGradientBrush(bounds, AccentGlow, AccentCyan, LinearGradientMode.Vertical);
    }

    // ── Radial Gradient (solid fallback) ──
    public static Brush GetRadialBrush(Rectangle bounds, Color centerColor, Color edgeColor)
    {
        return new SolidBrush(Color.FromArgb(
            (int)(centerColor.A * 0.5f),
            (centerColor.R + edgeColor.R) / 2,
            (centerColor.G + edgeColor.G) / 2,
            (centerColor.B + edgeColor.B) / 2));
    }

    public static Brush GetRadialBrush(PointF center, float radius, Color centerColor, Color edgeColor)
    {
        return new SolidBrush(Color.FromArgb(
            (int)(centerColor.A * 0.5f),
            (centerColor.R + edgeColor.R) / 2,
            (centerColor.G + edgeColor.G) / 2,
            (centerColor.B + edgeColor.B) / 2));
    }

    // ── Draw helpers ──
    public static GraphicsPath GetRoundedPath(RectangleF rect, int radius)
    {
        float r = Math.Max(1, radius);
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
        path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
        path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void DrawGlassBorder(Graphics g, RectangleF rect, int radius, Color borderColor, float width = 1f)
    {
        using var path = GetRoundedPath(rect, radius);
        using var pen = new Pen(borderColor, width) { Alignment = PenAlignment.Inset };
        g.DrawPath(pen, path);
    }

    public static void DrawGlow(Graphics g, RectangleF rect, Color glowColor, float intensity = 0.15f)
    {
        using var glowBrush = new SolidBrush(Color.FromArgb(
            (int)(255 * intensity), glowColor));
        using var path = GetRoundedPath(rect, (int)(rect.Width / 2f));
        g.FillPath(glowBrush, path);
    }
}
