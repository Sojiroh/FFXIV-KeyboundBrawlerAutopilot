using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Dalamud;

namespace AutoKeyboundBrawler.Windows.Shell;

internal static class Dock
{
    public static void Background(ImDrawListPtr dl, Vector2 origin, Vector2 end, float windowRounding)
    {
        Bar(dl, origin, end, new Corners(0f, 0f, windowRounding, windowRounding));
        Paint.Hairline(dl, origin, new Vector2(end.X, origin.Y));
    }

    public static void Bar(ImDrawListPtr dl, Vector2 min, Vector2 max, Corners corners)
    {
        GlassworkHost glassworkHost = Plugin.Instance.GlassworkHost;
        Look look = glassworkHost.Look;
        Glasswork.Backend.Paint.Material(dl, new Rect(min, max), corners, look.CornerStyle, in look.BarMaterial, in look, glassworkHost.Glass, ImGuiHelpers.GlobalScale);
    }
}
