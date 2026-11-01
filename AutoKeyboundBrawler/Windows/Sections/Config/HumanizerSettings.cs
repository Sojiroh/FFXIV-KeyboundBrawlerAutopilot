using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Sections.Config;

internal static class HumanizerSettings
{
    private readonly record struct DelayRow(HumanAction Action, LocString Label, LocString Help, string RowId, string MinId, string MaxId);

    private const float RangeSliderWidth = 130f;

    private static readonly DelayRow[] rows = new DelayRow[2]
    {
        new DelayRow(HumanAction.React, L.Humanizer.React, L.Humanizer.ReactHelp, "##akb_human_react", "##akb_human_react_min", "##akb_human_react_max"),
        new DelayRow(HumanAction.Submit, L.Humanizer.Submit, L.Humanizer.SubmitHelp, "##akb_human_submit", "##akb_human_submit_min", "##akb_human_submit_max")
    };

    public static void Draw(Configuration cfg)
    {
        DrawDelayGroup(cfg);
        DrawSlipGroup(cfg);
    }

    private static void DrawDelayGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Humanizer.Group)))
        {
            SettingsControls.ToggleRow(Loc.T(L.Humanizer.Enabled), Loc.T(L.Humanizer.EnabledHelp), "##akb_human_enabled", cfg, cfg.HumanizerEnabled, (bool value) =>
            {
                cfg.HumanizerEnabled = value;
            });
            if (cfg.HumanizerEnabled)
            {
                for (int index = 0; index < rows.Length; index++)
                {
                    DrawRange(cfg, in rows[index]);
                }
            }
        }
        Gw.EndSection();
    }

    private static void DrawSlipGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Humanizer.GroupSlips)))
        {
            ImGui.BeginDisabled(!cfg.HumanizerEnabled);
            SettingsControls.ToggleRow(Loc.T(L.Humanizer.Typos), Loc.T(L.Humanizer.TyposHelp), "##akb_human_typos", cfg, cfg.HumanizerTypos, (bool value) =>
            {
                cfg.HumanizerTypos = value;
            });
            ImGui.EndDisabled();
        }
        Gw.EndSection();
    }

    private static void DrawRange(Configuration cfg, in DelayRow row)
    {
        float nextItemWidth = RangeSliderWidth * ImGuiHelpers.GlobalScale;
        string format = SettingsControls.SecondsFormat();
        DelayRange range = cfg.DelayFor(row.Action);
        float minSeconds = range.MinSeconds;
        float maxSeconds = range.MaxSeconds;
        Gw.BeginRow(Loc.T(row.Label), default, row.RowId);
        ImGui.SetNextItemWidth(nextItemWidth);
        bool minChanged = Gw.Slider(ref minSeconds, 0f, 3f, row.MinId, 0.05f, format);
        Gw.Tooltip(Loc.T(row.Help));
        ImGui.SameLine();
        Gw.Text(Loc.T(L.Humanizer.RangeTo), TextRole.Body, Frame.Look.SecondaryLabel);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(nextItemWidth);
        bool changed = minChanged | Gw.Slider(ref maxSeconds, 0f, 3f, row.MaxId, 0.05f, format);
        Gw.Tooltip(Loc.T(row.Help));
        Gw.EndRow();
        if (!changed)
        {
            return;
        }
        cfg.SetDelay(row.Action, new DelayRange(minSeconds, maxSeconds));
        cfg.SaveDebounced();
    }
}
