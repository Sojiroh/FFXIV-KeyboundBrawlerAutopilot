using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Glasswork;

namespace AutoKeyboundBrawler.Windows;

internal static class Styling
{
    private sealed class ChromeScope(IDisposable style, IDisposable color) : IDisposable
    {
        public void Dispose()
        {
            color.Dispose();
            style.Dispose();
        }
    }

    private static Palette palette = Palette.Grindless();

    public static readonly Vector4 AccentViolet = new(0.62f, 0.42f, 0.96f, 1f);

    public static readonly Vector4 AccentVioletSoft = new(0.78f, 0.6f, 1f, 1f);

    public static readonly Vector4 AccentPink = new(0.95f, 0.45f, 0.78f, 1f);

    public static readonly Vector4 InkOnAccent = new(0.078f, 0.059f, 0.133f, 1f);

    public const float WindowRounding = 14f;

    public const float PanelRounding = 12f;

    public const float CardRounding = 10f;

    public const float FrameRounding = 7f;

    public const double PulseFast = 600.0;

    public const double PulseMedium = 800.0;

    public const double PulseBreath = 2600.0;

    public const double PulseCalm = 1900.0;

    public const double PulseOrbit = 3400.0;

    public static Vector4 AccentTeal => palette.AccentTeal;

    public static Vector4 AccentTealSoft => palette.AccentTealSoft;

    public static Vector4 AccentTealDeep => palette.AccentTealDeep;

    public static Vector4 AccentMint => palette.AccentMint;

    public static Vector4 AccentMintSoft => palette.AccentMintSoft;

    public static Vector4 AccentAmber => palette.AccentAmber;

    public static Vector4 AccentAmberSoft => palette.AccentAmberSoft;

    public static Vector4 AccentRose => palette.AccentRose;

    public static Vector4 AccentRoseSoft => palette.AccentRoseSoft;

    public static Vector4 AccentBlue => palette.AccentBlue;

    public static Vector4 AccentBlueSoft => palette.AccentBlueSoft;

    public static Vector4 WindowBg => palette.WindowBg;

    public static Vector4 Surface0 => palette.Surface0;

    public static Vector4 Surface1 => palette.Surface1;

    public static Vector4 Surface2 => palette.Surface2;

    public static Vector4 Surface3 => palette.Surface3;

    public static Vector4 CardBg => palette.CardBg;

    public static Vector4 CardBgSoft => palette.CardBgSoft;

    public static Vector4 CardBgHover => palette.CardBgHover;

    public static Vector4 SliderBg => palette.SliderBg;

    public static Vector4 BorderDim => palette.BorderDim;

    public static Vector4 TextStrong => palette.TextStrong;

    public static Vector4 TextSecondary => palette.TextSecondary;

    public static Vector4 TextDim => palette.TextDim;

    public static Vector4 TextMuted => palette.TextMuted;

    public static Vector4 Hairline
    {
        get
        {
            var ink = palette.Ink;
            ink.W = 0.055f;
            return ink;
        }
    }

    public static Vector4 Highlight
    {
        get
        {
            var ink = palette.Ink;
            ink.W = 0.075f;
            return ink;
        }
    }

    public static void Use(Palette next)
    {
        palette = next;
    }

    public static Vector4 Ink(float alpha)
    {
        var ink = palette.Ink;
        ink.W = alpha;
        return ink;
    }

    public static float Pulse(double periodMs = PulseMedium)
    {
        return (float)((Math.Sin((double)Environment.TickCount % periodMs / periodMs * Math.PI * 2.0) + 1.0) * 0.5);
    }

    public static Vector4 PulseColor(Vector4 a, Vector4 b, double periodMs = PulseMedium)
    {
        return Vector4.Lerp(a, b, Pulse(periodMs));
    }

    public static float Phase(double periodMs)
    {
        return (float)((double)Environment.TickCount % periodMs / periodMs);
    }

    public static Vector4 WithAlpha(Vector4 c, float a)
    {
        var withAlpha = c;
        withAlpha.W = a;
        return withAlpha;
    }

    public static Color ToGlasswork(Vector4 color)
    {
        return new Color(color.X, color.Y, color.Z, color.W);
    }

    public static Vector4 ForegroundOn(Vector4 fill)
    {
        if (!(Luminance(fill) > 0.65f))
        {
            return TextStrong;
        }
        return InkOnAccent;
    }

    private static float Luminance(Vector4 color)
    {
        return 0.2126f * Linear(color.X) + 0.7152f * Linear(color.Y) + 0.0722f * Linear(color.Z);
    }

    private static float Linear(float channel)
    {
        if (!(channel <= 0.04045f))
        {
            return MathF.Pow((channel + 0.055f) / 1.055f, 2.4f);
        }
        return channel / 12.92f;
    }

    public static Vector4 Lighten(Vector4 c, float t)
    {
        var lightened = Vector4.Lerp(c, Vector4.One, t);
        lightened.W = c.W;
        return lightened;
    }

    public static Vector4 Darken(Vector4 c, float t)
    {
        var darkened = Vector4.Lerp(c, Vector4.Zero, t);
        darkened.W = c.W;
        return darkened;
    }

    public static Vector4 Tint(Vector4 baseColor, Vector4 accent, float amount)
    {
        var tinted = Vector4.Lerp(baseColor, accent, amount);
        tinted.W = baseColor.W;
        return tinted;
    }

    public static void TextCentered(string text, Vector4 color)
    {
        var textWidth = ImGui.CalcTextSize(text).X;
        var availableWidth = ImGui.GetContentRegionAvail().X;
        if (availableWidth > textWidth)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availableWidth - textWidth) * 0.5f);
        }
        using (ImRaii.PushColor(ImGuiCol.Text, color))
        {
            ImGui.TextUnformatted(text);
        }
    }

    public static void VSpace(float pixels)
    {
        ImGui.Dummy(new Vector2(0f, pixels * ImGuiHelpers.GlobalScale));
    }

    public static void CenterNextItem(float width)
    {
        var availableWidth = ImGui.GetContentRegionAvail().X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (availableWidth - width) * 0.5f));
    }

    public static void SectionLabel(string label)
    {
        using (Fonts.PushHeadline())
        {
            using (ImRaii.PushColor(ImGuiCol.Text, TextStrong))
            {
                ImGui.TextUnformatted(label);
            }
        }
    }

    public static IDisposable PushChrome(Vector2 windowPadding)
    {
        var globalScale = ImGuiHelpers.GlobalScale;
        var style = ImRaii.PushStyle(ImGuiStyleVar.WindowRounding, WindowRounding * globalScale).Push(ImGuiStyleVar.WindowBorderSize, 1f).Push(ImGuiStyleVar.WindowPadding, windowPadding * globalScale)
            .Push(ImGuiStyleVar.ChildRounding, CardRounding * globalScale)
            .Push(ImGuiStyleVar.ChildBorderSize, 0f)
            .Push(ImGuiStyleVar.PopupRounding, CardRounding * globalScale)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f)
            .Push(ImGuiStyleVar.FrameRounding, FrameRounding * globalScale)
            .Push(ImGuiStyleVar.FramePadding, new Vector2(10f, 6f) * globalScale)
            .Push(ImGuiStyleVar.FrameBorderSize, 0f)
            .Push(ImGuiStyleVar.ItemSpacing, new Vector2(10f, 8f) * globalScale)
            .Push(ImGuiStyleVar.ItemInnerSpacing, new Vector2(6f, 4f) * globalScale)
            .Push(ImGuiStyleVar.ScrollbarSize, 9f * globalScale)
            .Push(ImGuiStyleVar.ScrollbarRounding, 9f * globalScale)
            .Push(ImGuiStyleVar.GrabRounding, 6f * globalScale)
            .Push(ImGuiStyleVar.GrabMinSize, 12f * globalScale);
        var baseColors = ImRaii.PushColor(ImGuiCol.WindowBg, WindowBg).Push(ImGuiCol.ChildBg, Vector4.Zero);
        var color = baseColors.Push(ImGuiCol.PopupBg, Surface1 with { W = 0.985f }).Push(ImGuiCol.Border, Ink(0.09f)).Push(ImGuiCol.BorderShadow, Vector4.Zero)
            .Push(ImGuiCol.FrameBg, SliderBg)
            .Push(ImGuiCol.FrameBgHovered, Surface2)
            .Push(ImGuiCol.FrameBgActive, Surface3)
            .Push(ImGuiCol.ScrollbarBg, Vector4.Zero)
            .Push(ImGuiCol.ScrollbarGrab, Ink(0.12f))
            .Push(ImGuiCol.ScrollbarGrabHovered, Ink(0.2f))
            .Push(ImGuiCol.ScrollbarGrabActive, Ink(0.28f))
            .Push(ImGuiCol.Button, Surface1)
            .Push(ImGuiCol.ButtonHovered, Surface2)
            .Push(ImGuiCol.ButtonActive, Tint(Surface2, AccentTeal, 0.35f))
            .Push(ImGuiCol.Header, Tint(Surface1, AccentTeal, 0.3f))
            .Push(ImGuiCol.HeaderHovered, Surface2)
            .Push(ImGuiCol.HeaderActive, Tint(Surface2, AccentTeal, 0.4f))
            .Push(ImGuiCol.CheckMark, AccentTealSoft)
            .Push(ImGuiCol.SliderGrab, AccentTeal)
            .Push(ImGuiCol.SliderGrabActive, AccentTealSoft)
            .Push(ImGuiCol.Text, TextStrong)
            .Push(ImGuiCol.TextDisabled, TextMuted)
            .Push(ImGuiCol.Separator, Hairline)
            .Push(ImGuiCol.ResizeGrip, Vector4.Zero)
            .Push(ImGuiCol.ResizeGripHovered, Vector4.Zero)
            .Push(ImGuiCol.ResizeGripActive, Vector4.Zero)
            .Push(ImGuiCol.TextSelectedBg, WithAlpha(AccentTeal, 0.35f));
        return new ChromeScope(style, color);
    }
}
