using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Components;

internal static class IconButton
{
    public static bool Draw(FontAwesomeIcon icon, string id, float size, Vector4? color = null, string? tooltip = null, bool enabled = true)
    {
        var buttonSize = size <= GlassworkSizes.Height(ButtonSize.Small) ? ButtonSize.Small : size <= GlassworkSizes.Height(ButtonSize.Regular) ? ButtonSize.Regular : ButtonSize.Large;
        ImGui.PushID(id);
        ImGui.BeginDisabled(!enabled);
        Color? tint = color is { } colorValue ? Styling.ToGlasswork(colorValue) : null;
        var clicked = Gw.IconButton(icon.ToIconString(), ButtonStyle.Plain, buttonSize, tooltip ?? string.Empty, tint);
        ImGui.EndDisabled();
        ImGui.PopID();
        return clicked;
    }
}
