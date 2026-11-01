using System.Numerics;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Windows.Sections.Config;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Pages;

internal sealed class SettingsPage
{
    private const float TabGap = 16f;

    private static readonly LocString[] TabNames = new LocString[3]
    {
        L.Settings.CatGeneral,
        L.Settings.CatTyping,
        L.Humanizer.Tab
    };

    private static readonly string[] tabLabels = new string[TabNames.Length];

    private int activeTab;

    private bool resetScroll;

    public void Draw(Plugin plugin)
    {
        SettingsControls.Translate(TabNames, tabLabels);
        var selected = activeTab;
        ImGui.SetNextItemWidth(-float.Epsilon);
        if (Gw.Segmented(ref selected, tabLabels) && selected != activeTab)
        {
            activeTab = selected;
            resetScroll = true;
        }
        Styling.VSpace(TabGap);
        using var content = ImRaii.Child("##akb_settings_content", new Vector2(-1f, -1f), border: false, ImGuiWindowFlags.None);
        if (!content)
        {
            return;
        }
        if (resetScroll)
        {
            ImGui.SetScrollY(0f);
            resetScroll = false;
        }
        using (Motion.PushSwitch("##akb_settings_tab", activeTab))
        {
            switch (activeTab)
            {
                case 0:
                    GeneralSettings.Draw(plugin.Configuration);
                    break;
                case 1:
                    TypingSettings.Draw(plugin.Configuration);
                    break;
                default:
                    HumanizerSettings.Draw(plugin.Configuration);
                    break;
            }
        }
    }
}
