using System;
using AutoKeyboundBrawler.Core.Localization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Sections.Config;

internal static class SettingsControls
{
    public const float ComboWidth = 200f;

    public const float FullWidth = -float.Epsilon;

    private const string PrintfTwoDecimals = "%.2f";

    private const string PrintfWhole = "%d";

    private const string DotNetTwoDecimals = "0.00";

    private static readonly string[] languageLabels = BuildLanguageLabels();

    private static string secondsSource = string.Empty;

    private static string secondsFormat = string.Empty;

    private static string countSource = string.Empty;

    private static int countValue = -1;

    private static string countText = string.Empty;

    public static void ToggleRow(string title, string help, string id, Configuration cfg, bool value, Action<bool> setter)
    {
        Gw.BeginRow(title, default, id);
        if (Gw.Toggle(ref value, id))
        {
            setter(value);
            cfg.SaveDebounced();
        }
        Gw.Tooltip(help);
        Gw.EndRow();
    }

    public static void Translate(LocString[] names, string[] labels)
    {
        for (int i = 0; i < names.Length; i++)
        {
            labels[i] = Loc.T(names[i]);
        }
    }

    public static void DrawLanguageCombo(Configuration cfg)
    {
        LanguageInfo[] all = Languages.All;
        int selected = 0;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == Loc.Current)
            {
                selected = i;
            }
        }
        ImGui.SetNextItemWidth(ComboWidth * ImGuiHelpers.GlobalScale);
        ImGui.PushID("akb_language");
        bool changed = Gw.ComboBox(ref selected, languageLabels);
        ImGui.PopID();
        if (!changed)
        {
            return;
        }
        cfg.Language = all[selected].Code;
        cfg.Save();
        Loc.SetLanguage(cfg.Language);
        Fonts.OnLanguageChanged();
        Plugin.Instance.OnLanguageChanged();
    }

    public static string SecondsFormat()
    {
        string localized = Loc.T(L.Humanizer.SecondsFormat);
        if ((object)localized != secondsSource)
        {
            secondsSource = localized;
            secondsFormat = localized.Replace(PrintfTwoDecimals, DotNetTwoDecimals, StringComparison.Ordinal);
        }
        return secondsFormat;
    }

    public static string Count(LocString format, int value)
    {
        string localized = Loc.T(format);
        if ((object)localized != countSource || value != countValue)
        {
            countSource = localized;
            countValue = value;
            countText = localized.Replace(PrintfWhole, Formatting.Number(value), StringComparison.Ordinal);
        }
        return countText;
    }

    private static string[] BuildLanguageLabels()
    {
        LanguageInfo[] all = Languages.All;
        string[] labels = new string[all.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i] = all[i].NativeName;
        }
        return labels;
    }
}
