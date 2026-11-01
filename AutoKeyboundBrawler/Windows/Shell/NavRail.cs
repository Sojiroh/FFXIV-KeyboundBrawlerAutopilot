using System.Numerics;
using AutoKeyboundBrawler.Core;
using AutoKeyboundBrawler.Core.Localization;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Shell;

internal static class NavRail
{
    private readonly record struct Entry(AppWindow.Page Page, string Glyph, LocString Label);

    private const float TopPad = 12f;

    private const float Gap = 8f;

    private const float BadgeOffset = 0.32f;

    private const float BadgeRadius = 3.5f;

    private const float BadgeRing = 1.5f;

    private static readonly Entry[] entries = new Entry[5]
    {
        new Entry(AppWindow.Page.Brawl, FontAwesomeIcon.Keyboard.ToIconString(), L.Shell.NavBrawl),
        new Entry(AppWindow.Page.Settings, FontAwesomeIcon.SlidersH.ToIconString(), L.Shell.NavSettings),
        new Entry(AppWindow.Page.Log, FontAwesomeIcon.Terminal.ToIconString(), L.Shell.NavLog),
        new Entry(AppWindow.Page.Changelog, FontAwesomeIcon.Newspaper.ToIconString(), L.Shell.NavChangelog),
        new Entry(AppWindow.Page.About, FontAwesomeIcon.InfoCircle.ToIconString(), L.Shell.NavAbout)
    };

    public static AppWindow.Page? Draw(AppWindow.Page current, Plugin plugin)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float buttonSize = GlassworkSizes.Height(ButtonSize.Large);
        float gap = Gap * globalScale;
        Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
        float availWidth = ImGui.GetContentRegionAvail().X;
        float buttonX = cursorScreenPos.X + (availWidth - buttonSize) * 0.5f;
        float top = cursorScreenPos.Y + TopPad * globalScale;
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        bool running = plugin.Typer.Running;
        AppWindow.Page? clicked = null;
        for (int i = 0; i < entries.Length; i++)
        {
            Entry entry = entries[i];
            float buttonY = top + (buttonSize + gap) * (float)i;
            ImGui.SetCursorScreenPos(new Vector2(buttonX, buttonY));
            ButtonStyle style = ((entry.Page == current) ? ButtonStyle.Tinted : ButtonStyle.Plain);
            if (Gw.IconButton(entry.Glyph, style, ButtonSize.Large, Loc.T(entry.Label)))
            {
                clicked = entry.Page;
            }
            DrawBadge(windowDrawList, entry.Page, current, new Vector2(buttonX + buttonSize * 0.5f, buttonY + buttonSize * 0.5f), buttonSize, running);
        }
        ImGui.SetCursorScreenPos(cursorScreenPos);
        ImGui.Dummy(new Vector2(availWidth, TopPad * globalScale + (buttonSize + gap) * (float)entries.Length));
        return clicked;
    }

    private static void DrawBadge(ImDrawListPtr dl, AppWindow.Page page, AppWindow.Page current, Vector2 center, float button, bool running)
    {
        Vector4 color;
        if ((page == AppWindow.Page.Brawl) & running)
        {
            color = Styling.PulseColor(Styling.AccentTeal, Styling.AccentTealSoft);
        }
        else if (page == AppWindow.Page.Changelog && current != AppWindow.Page.Changelog && Plugin.Cfg.HasUnseenChangelog)
        {
            color = Styling.PulseColor(Styling.AccentTeal, Styling.AccentTealSoft);
        }
        else if (page == AppWindow.Page.Log && current != AppWindow.Page.Log && RunLog.Unseen is { } level)
        {
            color = level == RunLogLevel.Error ? Styling.AccentRose : Styling.AccentAmber;
        }
        else
        {
            return;
        }
        float globalScale = ImGuiHelpers.GlobalScale;
        Vector2 badgeCenter = center + new Vector2(button * BadgeOffset, -button * BadgeOffset);
        float badgeRadius = BadgeRadius * globalScale;
        dl.AddCircleFilled(badgeCenter, badgeRadius + BadgeRing * globalScale, Paint.Col(Styling.WindowBg with { W = 1f }));
        dl.AddCircleFilled(badgeCenter, badgeRadius, Paint.Col(color));
    }
}
