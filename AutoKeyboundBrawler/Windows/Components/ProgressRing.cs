using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace AutoKeyboundBrawler.Windows.Components;

internal static class ProgressRing
{
    private const float Top = -(float)Math.PI / 2f;

    private static Vector2 Dir(float angle)
    {
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    private static void Arc(Vector2 center, float radius, float thickness, float startAngle, float endAngle, uint color)
    {
        var drawList = ImGui.GetWindowDrawList();
        var span = MathF.Abs(endAngle - startAngle);
        var segments = Math.Max(2, (int)MathF.Ceiling(span / ((float)Math.PI / 48f)));
        var previous = center + Dir(startAngle) * radius;
        for (var i = 1; i <= segments; i++)
        {
            var angle = startAngle + (endAngle - startAngle) * ((float)i / (float)segments);
            var point = center + Dir(angle) * radius;
            drawList.AddLine(previous, point, color, thickness);
            previous = point;
        }
        var capRadius = thickness * 0.5f;
        drawList.AddCircleFilled(center + Dir(startAngle) * radius, capRadius, color);
        drawList.AddCircleFilled(center + Dir(endAngle) * radius, capRadius, color);
    }

    public static void Glow(Vector2 center, float radius, Vector4 color, float intensity)
    {
        var drawList = ImGui.GetWindowDrawList();
        for (var layer = 4; layer >= 1; layer--)
        {
            var layerRadius = radius * (0.72f + (float)layer * 0.17f);
            var alpha = Math.Clamp(intensity * 0.05f * (float)(5 - layer), 0f, 0.5f);
            drawList.AddCircleFilled(center, layerRadius, Paint.Col(Styling.WithAlpha(color, alpha)));
        }
    }

    public static void Disc(Vector2 center, float radius, Vector4 color)
    {
        ImGui.GetWindowDrawList().AddCircleFilled(center, radius, Paint.Col(color));
    }

    public static void Track(Vector2 center, float radius, float thickness, Vector4 color)
    {
        Arc(center, radius, thickness, Top, 4.712389f, Paint.Col(color));
    }

    public static void Fill(Vector2 center, float radius, float thickness, float fraction, Vector4 color)
    {
        fraction = Math.Clamp(fraction, 0f, 1f);
        if (fraction <= 0.0001f)
        {
            return;
        }
        Arc(center, radius, thickness, Top, Top + fraction * (float)Math.PI * 2f, Paint.Col(color));
    }

    public static void Sweep(Vector2 center, float radius, float thickness, Vector4 color, double periodMs, float arcLength, float headAlpha)
    {
        var drawList = ImGui.GetWindowDrawList();
        var headAngle = Top + Styling.Phase(periodMs) * (float)Math.PI * 2f;
        var tailAngle = headAngle - arcLength;
        var segments = Math.Max(10, (int)MathF.Ceiling(arcLength / ((float)Math.PI / 36f)));
        var previous = center + Dir(tailAngle) * radius;
        for (var i = 1; i <= segments; i++)
        {
            var t = (float)i / (float)segments;
            var angle = tailAngle + (headAngle - tailAngle) * t;
            var point = center + Dir(angle) * radius;
            drawList.AddLine(previous, point, Paint.Col(Styling.WithAlpha(color, headAlpha * t * t)), thickness);
            previous = point;
        }
        drawList.AddCircleFilled(center + Dir(headAngle) * radius, thickness * 0.62f, Paint.Col(Styling.WithAlpha(color, headAlpha)));
    }

    public static void CenterValue(Vector2 center, string big, string? small, Vector4 bigColor, Vector4 smallColor)
    {
        Vector2 bigSize;
        using (Fonts.PushTitle())
        {
            bigSize = TextDraw.Measure(big);
        }
        var hasSmall = !string.IsNullOrEmpty(small);
        var smallSize = Vector2.Zero;
        if (hasSmall)
        {
            using (Fonts.PushCaption())
            {
                smallSize = TextDraw.Measure(small!);
            }
        }
        var gap = (hasSmall ? (1f * ImGuiHelpers.GlobalScale) : 0f);
        var top = center.Y - (bigSize.Y + gap + smallSize.Y) * 0.5f;
        using (Fonts.PushTitle())
        {
            TextDraw.At(big, new Vector2(center.X - bigSize.X * 0.5f, top), bigColor);
        }
        if (!hasSmall)
        {
            return;
        }
        using (Fonts.PushCaption())
        {
            TextDraw.At(small!, new Vector2(center.X - smallSize.X * 0.5f, top + bigSize.Y + gap), smallColor);
        }
    }

    public static void CenterIcon(Vector2 center, FontAwesomeIcon icon, Vector4 color, float targetHeight)
    {
        var glyph = icon.ToIconString();
        using (Fonts.PushIconFor(targetHeight))
        {
            var glyphSize = ImGui.CalcTextSize(glyph);
            if (glyphSize.Y <= 0f)
            {
                return;
            }
            var scale = targetHeight / glyphSize.Y;
            var scaledSize = glyphSize * scale;
            ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), ImGui.GetFontSize() * scale, center - scaledSize * 0.5f, Paint.Col(color), glyph);
        }
    }
}
