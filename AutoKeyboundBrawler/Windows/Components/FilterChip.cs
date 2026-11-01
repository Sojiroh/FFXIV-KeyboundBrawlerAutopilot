using System.Numerics;
using Dalamud.Interface;

namespace AutoKeyboundBrawler.Windows.Components;

internal static class FilterChip
{
    private const string CountSeparator = "  ";

    public static float Width(string label, string? count, FontAwesomeIcon? trailingIcon = null)
    {
        return PillButton.Width(Text(label, count), trailingIcon);
    }

    public static bool Draw(string id, string label, string? count, Vector4 accent, bool active, float height, FontAwesomeIcon? trailingIcon = null, string? tooltip = null)
    {
        return PillButton.Draw(id, Text(label, count), accent, active ? PillButton.Emphasis.Tinted : PillButton.Emphasis.Ghost, trailingIcon, enabled: true, height, tooltip);
    }

    private static string Text(string label, string? count)
    {
        if (count != null)
        {
            return label + CountSeparator + count;
        }
        return label;
    }
}
