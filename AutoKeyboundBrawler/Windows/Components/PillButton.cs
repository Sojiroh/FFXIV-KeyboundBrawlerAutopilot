using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Components;

internal static class PillButton
{
    public enum Emphasis
    {
        Ghost,
        Tinted,
        Filled
    }

    private const float DefaultHeight = 28f;

    private const float Slack = 4f;

    public static float Width(string label, FontAwesomeIcon? icon = null)
    {
        var look = Frame.Look;
        var globalScale = ImGuiHelpers.GlobalScale;
        var iconWidth = icon is { } glyph ? TextDraw.IconSize(glyph).X + look.SpaceSmall * globalScale : 0f;
        return (look.SpaceMedium * 2f + Slack) * globalScale + iconWidth + TextDraw.Measure(label).X;
    }

    public static bool Draw(string id, string label, Vector4 accent, Emphasis emphasis = Emphasis.Tinted, FontAwesomeIcon? icon = null, bool enabled = true, float height = DefaultHeight, string? tooltip = null)
    {
        var style = emphasis switch
        {
            Emphasis.Filled => ButtonStyle.Prominent,
            Emphasis.Tinted => ButtonStyle.Tinted,
            _ => ButtonStyle.Plain,
        };
        ImGui.PushID(id);
        ImGui.BeginDisabled(!enabled);
        var iconText = icon is { } glyph ? glyph.ToIconString() : string.Empty;
        Color? tint = ((emphasis == Emphasis.Ghost) ? ((Color?)null) : new Color?(Styling.ToGlasswork(accent)));
        var clicked = Gw.Button(label, style, ButtonSize.Small, iconText, Width(label, icon) / ImGuiHelpers.GlobalScale, tint);
        if (tooltip != null)
        {
            Gw.Tooltip(tooltip);
        }
        ImGui.EndDisabled();
        ImGui.PopID();
        return clicked;
    }
}
