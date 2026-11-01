using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Components;

internal static class SearchField
{
    public const int DefaultMaxLength = 96;

    private static readonly string searchGlyph = FontAwesomeIcon.Search.ToIconString();

    public static bool Draw(string id, string hint, ref string text, ref bool focused, float width, float height, bool requestFocus = false, int maxLength = DefaultMaxLength)
    {
        ImGui.PushID(id);
        if (requestFocus)
        {
            ImGui.SetKeyboardFocusHere();
        }
        ImGui.SetNextItemWidth(width);
        var changed = Gw.TextField(ref text, hint, default, maxLength, password: false, searchGlyph);
        focused = ImGui.IsItemActive();
        ImGui.PopID();
        return changed;
    }
}
