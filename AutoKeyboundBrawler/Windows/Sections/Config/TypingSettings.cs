using System;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Sections.Config;

internal static class TypingSettings
{
    private static readonly LocString[] SpeedNames = new LocString[4]
    {
        L.Settings.SpeedRelaxed,
        L.Settings.SpeedSteady,
        L.Settings.SpeedFast,
        L.Settings.SpeedBlazing
    };

    private static readonly LocString[] SpeedDetails = new LocString[4]
    {
        L.Settings.SpeedRelaxedDetail,
        L.Settings.SpeedSteadyDetail,
        L.Settings.SpeedFastDetail,
        L.Settings.SpeedBlazingDetail
    };

    private static readonly LocString[] OrderNames = new LocString[3]
    {
        L.Settings.OrderSmart,
        L.Settings.OrderClosest,
        L.Settings.OrderShortest
    };

    private static readonly LocString[] OrderDetails = new LocString[3]
    {
        L.Settings.OrderSmartDetail,
        L.Settings.OrderClosestDetail,
        L.Settings.OrderShortestDetail
    };

    private static readonly LocString[] MethodNames = new LocString[2]
    {
        L.Settings.MethodTyping,
        L.Settings.MethodDirect
    };

    private static readonly LocString[] MethodDetails = new LocString[2]
    {
        L.Settings.MethodTypingDetail,
        L.Settings.MethodDirectDetail
    };

    private static readonly string[] speedLabels = new string[SpeedNames.Length];

    private static readonly string[] orderLabels = new string[OrderNames.Length];

    private static readonly string[] methodLabels = new string[MethodNames.Length];

    private const int CrowdedMin = 2;

    private const int CrowdedMax = 12;

    private const float MethodWidth = 280f;

    private const int MatchLimitMin = 0;

    private const int MatchLimitMax = 999;

    private static int stopAfterValue = -1;

    private static LanguageInfo? stopAfterLanguage;

    private static string stopAfterText = string.Empty;

    private static int lifetimeMatches = -1;

    private static long lifetimeMgc = -1L;

    private static long lifetimeMgp = -1L;

    private static LanguageInfo? lifetimeLanguage;

    private static string lifetimeText = string.Empty;

    public static string SpeedName(TypingSpeed speed)
    {
        return Loc.T(SpeedNames[SpeedIndex(speed)]);
    }

    public static string OrderName(TargetOrder order)
    {
        return Loc.T(OrderNames[OrderIndex(order)]);
    }

    public static void Draw(Configuration cfg)
    {
        DrawSpeedGroup(cfg);
        DrawTargetGroup(cfg);
        DrawInputGroup(cfg);
        DrawCastGroup(cfg);
        DrawQueueGroup(cfg);
    }

    private static void DrawSpeedGroup(Configuration cfg)
    {
        int selected = SpeedIndex(cfg.Speed);
        if (Gw.BeginSection(Loc.T(L.Settings.GroupSpeed), Loc.T(SpeedDetails[selected])))
        {
            SettingsControls.Translate(SpeedNames, speedLabels);
            ImGui.SetNextItemWidth(-float.Epsilon);
            if (Gw.Segmented(ref selected, speedLabels))
            {
                cfg.Speed = TypingPaces.All[selected];
                cfg.SaveDebounced();
            }
            Gw.Tooltip(Loc.T(L.Settings.SpeedHelp));
        }
        Gw.EndSection();
    }

    private static void DrawTargetGroup(Configuration cfg)
    {
        int selected = OrderIndex(cfg.Order);
        if (Gw.BeginSection(Loc.T(L.Settings.GroupTargets), Loc.T(OrderDetails[selected])))
        {
            SettingsControls.Translate(OrderNames, orderLabels);
            ImGui.SetNextItemWidth(-float.Epsilon);
            if (Gw.Segmented(ref selected, orderLabels))
            {
                cfg.Order = TargetPicker.Orders[selected];
                cfg.SaveDebounced();
            }
            Gw.Tooltip(Loc.T(L.Settings.OrderHelp));
            if (cfg.Order == TargetOrder.Smart)
            {
                DrawCrowdedAt(cfg);
            }
        }
        Gw.EndSection();
    }

    private static void DrawCrowdedAt(Configuration cfg)
    {
        int value = cfg.CrowdedAt;
        Gw.BeginRow(Loc.T(L.Settings.CrowdedAt), SettingsControls.Count(L.Settings.CrowdedAtFormat, value), "##akb_crowded_row");
        if (Gw.Stepper(ref value, CrowdedMin, CrowdedMax, 1, "##akb_crowded_at"))
        {
            cfg.CrowdedAt = value;
            cfg.SaveDebounced();
        }
        Gw.Tooltip(Loc.T(L.Settings.CrowdedAtHelp));
        Gw.EndRow();
    }

    private static void DrawInputGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GroupInput), Loc.T(L.Settings.SafetyFootnote)))
        {
            DrawInputMethod(cfg);
            SettingsControls.ToggleRow(Loc.T(L.Settings.AutoFocus), Loc.T(L.Settings.AutoFocusHelp), "##akb_autofocus", cfg, cfg.AutoFocusInput, (bool value) =>
            {
                cfg.AutoFocusInput = value;
            });
        }
        Gw.EndSection();
    }

    private static void DrawInputMethod(Configuration cfg)
    {
        int selected = ((cfg.InputMethod == InputMethod.Direct) ? 1 : 0);
        Gw.BeginRow(Loc.T(L.Settings.InputMethod), Loc.T(MethodDetails[selected]), "##akb_method_row");
        SettingsControls.Translate(MethodNames, methodLabels);
        ImGui.SetNextItemWidth(MethodWidth * ImGuiHelpers.GlobalScale);
        if (Gw.Segmented(ref selected, methodLabels))
        {
            cfg.InputMethod = ((selected == 1) ? InputMethod.Direct : InputMethod.Typing);
            cfg.SaveDebounced();
        }
        Gw.Tooltip(Loc.T(L.Settings.InputMethodHelp));
        Gw.EndRow();
    }

    private static void DrawCastGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GroupCasts), Loc.T(L.Settings.CastsFootnote)))
        {
            SettingsControls.ToggleRow(Loc.T(L.Settings.Letterlock), Loc.T(L.Settings.LetterlockHelp), "##akb_letterlock", cfg, cfg.LetterlockGuard, (bool value) =>
            {
                cfg.LetterlockGuard = value;
            });
        }
        Gw.EndSection();
    }

    private static void DrawQueueGroup(Configuration cfg)
    {
        if (Gw.BeginSection(Loc.T(L.Settings.GroupQueue), Loc.T(L.Settings.QueueFootnote)))
        {
            SettingsControls.ToggleRow(Loc.T(L.Settings.AutoQueue), Loc.T(L.Settings.AutoQueueHelp), "##akb_auto_queue", cfg, cfg.AutoQueue, (bool value) =>
            {
                cfg.AutoQueue = value;
            });
            SettingsControls.ToggleRow(Loc.T(L.Settings.WalkToRegistrar), Loc.T(L.Settings.WalkToRegistrarHelp), "##akb_walk_registrar", cfg, cfg.WalkToRegistrar, (bool value) =>
            {
                cfg.WalkToRegistrar = value;
            });
            DrawStopAfter(cfg);
            SettingsControls.ToggleRow(Loc.T(L.Settings.StopAtMgcCap), Loc.T(L.Settings.StopAtMgcCapHelp), "##akb_stop_mgc_cap", cfg, cfg.StopAtMgcCap, (bool value) =>
            {
                cfg.StopAtMgcCap = value;
            });
            DrawLifetime(cfg);
        }
        Gw.EndSection();
    }

    private static void DrawStopAfter(Configuration cfg)
    {
        int value = cfg.StopAfterMatches;
        Gw.BeginRow(Loc.T(L.Settings.StopAfterMatches), StopAfterLabel(value), "##akb_stop_after_row");
        if (Gw.Stepper(ref value, MatchLimitMin, MatchLimitMax, 1, "##akb_stop_after"))
        {
            cfg.StopAfterMatches = value;
            cfg.SaveDebounced();
        }
        Gw.Tooltip(Loc.T(L.Settings.StopAfterMatchesHelp));
        Gw.EndRow();
    }

    private static void DrawLifetime(Configuration cfg)
    {
        Gw.BeginRow(Loc.T(L.Settings.Lifetime), LifetimeLabel(cfg), "##akb_lifetime_row");
        Gw.Tooltip(Loc.T(L.Settings.LifetimeHelp));
        Gw.EndRow();
    }

    private static string StopAfterLabel(int value)
    {
        if (value == stopAfterValue && stopAfterLanguage == Loc.Current)
        {
            return stopAfterText;
        }
        stopAfterValue = value;
        stopAfterLanguage = Loc.Current;
        stopAfterText = ((value == 0) ? Loc.T(L.Settings.StopAfterNever) : Loc.Plural(L.Settings.StopAfterMatchesCount, value));
        return stopAfterText;
    }

    private static string LifetimeLabel(Configuration cfg)
    {
        if (cfg.LifetimeMatches == lifetimeMatches && cfg.LifetimeMgc == lifetimeMgc && cfg.LifetimeMgp == lifetimeMgp && lifetimeLanguage == Loc.Current)
        {
            return lifetimeText;
        }
        lifetimeMatches = cfg.LifetimeMatches;
        lifetimeMgc = cfg.LifetimeMgc;
        lifetimeMgp = cfg.LifetimeMgp;
        lifetimeLanguage = Loc.Current;
        lifetimeText = Loc.Plural(L.Settings.LifetimeValue, lifetimeMatches, Formatting.Amount(lifetimeMgc), Formatting.Amount(lifetimeMgp));
        return lifetimeText;
    }

    private static int SpeedIndex(TypingSpeed speed)
    {
        return Math.Max(0, Array.IndexOf(TypingPaces.All, speed));
    }

    private static int OrderIndex(TargetOrder order)
    {
        return Math.Max(0, Array.IndexOf(TargetPicker.Orders, order));
    }
}
