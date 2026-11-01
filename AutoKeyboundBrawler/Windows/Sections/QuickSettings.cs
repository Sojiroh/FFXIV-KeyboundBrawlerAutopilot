using System;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using AutoKeyboundBrawler.Windows.Sections.Config;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Sections;

internal static class QuickSettings
{
    private const float FullWidth = -float.Epsilon;

    private const float CardPadding = 16f;

    private const float TitleGap = 12f;

    private const float LabelGap = 4f;

    private static readonly LocString[] SpeedNames = new LocString[4]
    {
        L.Settings.SpeedRelaxed,
        L.Settings.SpeedSteady,
        L.Settings.SpeedFast,
        L.Settings.SpeedBlazing
    };

    private static readonly LocString[] OrderNames = new LocString[3]
    {
        L.Settings.OrderSmart,
        L.Settings.OrderClosest,
        L.Settings.OrderShortest
    };

    private static readonly string[] speedLabels = new string[SpeedNames.Length];

    private static readonly string[] orderLabels = new string[OrderNames.Length];

    private static readonly string settingsGlyph = FontAwesomeIcon.SlidersH.ToIconString();

    public static void Draw(Plugin plugin)
    {
        Configuration cfg = plugin.Configuration;
        if (Gw.BeginCard(CardPadding))
        {
            Styling.SectionLabel(Loc.T(L.Brawl.Quick));
            Styling.VSpace(TitleGap);
            DrawSpeed(cfg);
            DrawOrder(cfg);
            SettingsControls.ToggleRow(Loc.T(L.Humanizer.Enabled), Loc.T(L.Humanizer.EnabledHelp), "##akb_quick_human", cfg, cfg.HumanizerEnabled, (bool value) =>
            {
                cfg.HumanizerEnabled = value;
            });
            SettingsControls.ToggleRow(Loc.T(L.Settings.AutoFocus), Loc.T(L.Settings.AutoFocusHelp), "##akb_quick_focus", cfg, cfg.AutoFocusInput, (bool value) =>
            {
                cfg.AutoFocusInput = value;
            });
            SettingsControls.ToggleRow(Loc.T(L.Settings.Letterlock), Loc.T(L.Settings.LetterlockHelp), "##akb_quick_letterlock", cfg, cfg.LetterlockGuard, (bool value) =>
            {
                cfg.LetterlockGuard = value;
            });
            SettingsControls.ToggleRow(Loc.T(L.Settings.AutoQueue), Loc.T(L.Settings.AutoQueueHelp), "##akb_quick_queue", cfg, cfg.AutoQueue, (bool value) =>
            {
                cfg.AutoQueue = value;
            });
        }
        Gw.EndCard();
        if (Gw.Button(Loc.T(L.Brawl.AllSettings), ButtonStyle.Plain, ButtonSize.Regular, settingsGlyph))
        {
            plugin.ToggleConfigUi();
        }
    }

    private static void DrawSpeed(Configuration cfg)
    {
        Gw.Text(Loc.T(L.Settings.Speed), TextRole.Body, Frame.Look.SecondaryLabel);
        Gw.Tooltip(Loc.T(L.Settings.SpeedHelp));
        Styling.VSpace(LabelGap);
        SettingsControls.Translate(SpeedNames, speedLabels);
        int selected = Math.Max(0, Array.IndexOf(TypingPaces.All, cfg.Speed));
        ImGui.SetNextItemWidth(FullWidth);
        if (Gw.Segmented(ref selected, speedLabels))
        {
            cfg.Speed = TypingPaces.All[selected];
            cfg.SaveDebounced();
        }
    }

    private static void DrawOrder(Configuration cfg)
    {
        Styling.VSpace(12f);
        Gw.Text(Loc.T(L.Settings.Order), TextRole.Body, Frame.Look.SecondaryLabel);
        Gw.Tooltip(Loc.T(L.Settings.OrderHelp));
        Styling.VSpace(LabelGap);
        SettingsControls.Translate(OrderNames, orderLabels);
        int selected = Math.Max(0, Array.IndexOf(TargetPicker.Orders, cfg.Order));
        ImGui.SetNextItemWidth(FullWidth);
        if (Gw.Segmented(ref selected, orderLabels))
        {
            cfg.Order = TargetPicker.Orders[selected];
            cfg.SaveDebounced();
        }
    }
}
