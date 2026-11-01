using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows;

internal static class GlassworkSizes
{
    public static float Height(ButtonSize size)
    {
        Look look = Frame.Look;
        return size switch
        {
            ButtonSize.Small => look.ControlHeightSmall,
            ButtonSize.Large => look.ControlHeightLarge,
            _ => look.ControlHeight,
        } * ImGuiHelpers.GlobalScale;
    }
}
