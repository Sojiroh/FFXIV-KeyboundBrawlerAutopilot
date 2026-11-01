using AutoKeyboundBrawler.Core.Localization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Sections.Config;

internal static class GeneralSettings
{
    private const float AppearanceWidth = 200f;

    private static readonly LocString[] AppearanceNames = new LocString[2]
    {
        L.Settings.AppearanceDark,
        L.Settings.AppearanceLight
    };

    private static readonly string[] appearanceLabels = new string[AppearanceNames.Length];

    private static readonly string linkGlyph = FontAwesomeIcon.ExternalLinkAlt.ToIconString();

    public static void Draw(Configuration cfg)
    {
        DrawLanguageGroup(cfg);
        DrawThemeGroup(cfg);
        DrawWindowGroup(cfg);
        DrawDiagnosticsGroup(cfg);
    }

    private static void DrawLanguageGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.Language)))
        {
            Gw.BeginRow(Loc.T(L.Settings.Language));
            SettingsControls.DrawLanguageCombo(cfg);
            Gw.Tooltip(Loc.T(L.Settings.LanguageHelp));
            Gw.EndRow();
        }
        Gw.EndSection();
    }

    private static void DrawThemeGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GroupTheme)))
        {
            Gw.BeginRow(Loc.T(L.Settings.Theme));
            DrawThemeCombo(cfg);
            Gw.Tooltip(Loc.T(L.Settings.ThemeHelp));
            Gw.EndRow();
            Gw.BeginRow(Loc.T(L.Settings.Appearance));
            DrawAppearance(cfg);
            Gw.Tooltip(Loc.T(L.Settings.AppearanceHelp));
            Gw.EndRow();
        }
        Gw.EndSection();
        if (Gw.Button(Loc.T(L.Settings.ThemesBy), ButtonStyle.Plain, ButtonSize.Regular, linkGlyph))
        {
            UrlActions.Open(Themes.GlassworkUrl);
        }
        Gw.Tooltip(Themes.GlassworkUrl);
    }

    private static void DrawThemeCombo(Configuration cfg)
    {
        int selected = Themes.IndexOf(cfg.Theme);
        ImGui.SetNextItemWidth(SettingsControls.ComboWidth * ImGuiHelpers.GlobalScale);
        ImGui.PushID("akb_theme");
        bool changed = Gw.ComboBox(ref selected, Themes.Names);
        ImGui.PopID();
        if (!changed)
        {
            return;
        }
        cfg.Theme = Themes.IdAt(selected);
        cfg.Save();
        Plugin.Instance.ApplyTheme();
    }

    private static void DrawAppearance(Configuration cfg)
    {
        SettingsControls.Translate(AppearanceNames, appearanceLabels);
        int selected = (cfg.LightTheme ? 1 : 0);
        ImGui.SetNextItemWidth(AppearanceWidth * ImGuiHelpers.GlobalScale);
        if (!Gw.Segmented(ref selected, appearanceLabels))
        {
            return;
        }
        cfg.LightTheme = selected == 1;
        cfg.Save();
        Plugin.Instance.ApplyTheme();
    }

    private static void DrawWindowGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GroupWindow)))
        {
            SettingsControls.ToggleRow(Loc.T(L.Settings.DragAnywhere), Loc.T(L.Settings.DragAnywhereHelp), "##akb_draganywhere", cfg, cfg.DragWindowFromAnywhere, (bool value) =>
            {
                cfg.DragWindowFromAnywhere = value;
            });
        }
        Gw.EndSection();
    }

    private static void DrawDiagnosticsGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GroupDiagnostics)))
        {
            SettingsControls.ToggleRow(Loc.T(L.Settings.Snapshots), Loc.T(L.Settings.SnapshotsHelp), "##akb_snapshots", cfg, cfg.RecordSnapshots, (bool value) =>
            {
                cfg.RecordSnapshots = value;
            });
        }
        Gw.EndSection();
    }
}
