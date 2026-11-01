using System;
using System.Numerics;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using AutoKeyboundBrawler.Windows.Sections;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Shell;

internal static class HeaderBar
{
    private const float MinimumPillRoom = 80f;

    private static readonly string closeGlyph = FontAwesomeIcon.Times.ToIconString();

    private static readonly string minimizeGlyph = FontAwesomeIcon.ChevronDown.ToIconString();

    private static readonly string restoreGlyph = FontAwesomeIcon.ChevronUp.ToIconString();

    private static float pillHeight = 24f;

    private const string Title = "Keybound Brawler Autopilot";

    private const float PadX = 16f;

    private const float IconBox = 26f;

    private const float ButtonGap = 6f;

    private const int ButtonCount = 2;

    private const float ButtonAllowance = 30f;

    public const float MinimumWidth = 136f;

    public static float ButtonsWidth()
    {
        return GlassworkSizes.Height(ButtonSize.Regular) * ButtonCount + ButtonGap * ImGuiHelpers.GlobalScale;
    }

    public static bool HandleDrag(Vector2 windowPos, float width, float height)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float dragWidth = width - PadX * globalScale - ButtonsWidth() - 8f * globalScale;
        ImGui.SetCursorScreenPos(windowPos);
        ImGui.InvisibleButton("##akb_drag", new Vector2(MathF.Max(1f, dragWidth), height));
        bool doubleClicked = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
        if (!ImGui.IsItemActive())
        {
            return doubleClicked;
        }
        Vector2 mouseDelta = ImGui.GetIO().MouseDelta;
        if (mouseDelta != Vector2.Zero)
        {
            ImGui.SetWindowPos(ImGui.GetWindowPos() + mouseDelta, ImGuiCond.Always);
        }
        return doubleClicked;
    }

    public static void Draw(AppWindow window, Plugin plugin, Vector2 origin, float width, float height, float windowRounding, bool compact)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        Vector2 bottomRight = origin + new Vector2(width, height);
        float padX = PadX * globalScale;
        float midY = origin.Y + height * 0.5f;
        Dock.Bar(windowDrawList, origin, bottomRight, new Corners(windowRounding, windowRounding, compact ? windowRounding : 0f, compact ? windowRounding : 0f));
        if (!compact)
        {
            Paint.Hairline(windowDrawList, new Vector2(origin.X, bottomRight.Y - 0.5f), new Vector2(bottomRight.X, bottomRight.Y - 0.5f));
        }
        float iconSize = IconBox * globalScale;
        Vector2 iconMin = new Vector2(origin.X + padX, midY - iconSize * 0.5f);
        AppIcon.Draw(windowDrawList, iconMin, iconMin + new Vector2(iconSize, iconSize), 7f * globalScale);
        float rightLimit = bottomRight.X - padX - ButtonsWidth();
        float cursorX = iconMin.X + iconSize + 12f * globalScale;
        using (Fonts.PushHeadline())
        {
            Vector2 titleSize = TextDraw.Measure(Title);
            if (cursorX + titleSize.X <= rightLimit)
            {
                TextDraw.At(Title, new Vector2(cursorX, midY - titleSize.Y * 0.5f), Styling.TextStrong);
                cursorX += titleSize.X + 14f * globalScale;
            }
        }
        float pillRight = DrawStatusPill(ReadyState.Resolve(plugin.Typer), cursorX, rightLimit, midY);
        if (pillRight > cursorX)
        {
            cursorX = pillRight + 14f * globalScale;
        }
        if (compact)
        {
            DrawCompactInfo(plugin, cursorX, rightLimit - 14f * globalScale, midY);
        }
        DrawButtons(window, bottomRight, midY, compact);
    }

    private static void DrawButtons(AppWindow window, Vector2 end, float midY, bool compact)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float padX = PadX * globalScale;
        float buttonHeight = GlassworkSizes.Height(ButtonSize.Regular);
        float buttonStep = buttonHeight + ButtonGap * globalScale;
        float buttonY = midY - buttonHeight * 0.5f;
        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonHeight, buttonY));
        if (Gw.IconButton(closeGlyph, ButtonStyle.Plain, ButtonSize.Regular, Loc.T(L.Common.Close)))
        {
            window.IsOpen = false;
        }
        ImGui.SetCursorScreenPos(new Vector2(end.X - padX - buttonHeight - buttonStep, buttonY));
        if (Gw.IconButton(compact ? restoreGlyph : minimizeGlyph, ButtonStyle.Plain, ButtonSize.Regular, Loc.T(compact ? L.Shell.Restore : L.Shell.Minimize)))
        {
            window.ToggleCompact();
        }
    }

    private static float DrawStatusPill(ReadyState.Info info, float x, float rightLimit, float midY)
    {
        if (rightLimit - x < MinimumPillRoom * ImGuiHelpers.GlobalScale)
        {
            return x;
        }
        ImGui.SetCursorScreenPos(new Vector2(x, midY - pillHeight * 0.5f));
        Gw.Badge(ReadyState.ShortLabel(info.Kind), Styling.ToGlasswork(info.Accent));
        pillHeight = ImGui.GetItemRectSize().Y;
        return ImGui.GetItemRectMax().X;
    }

    private static void DrawCompactInfo(Plugin plugin, float x, float rightX, float midY)
    {
        AutoTyper typer = plugin.Typer;
        string label = (typer.Running ? Loc.T(L.Shell.CompactRunning, ReadyState.PhaseLabel(typer.Phase), Formatting.Phrases(typer.Session.PhrasesSubmitted)) : Loc.T(L.Shell.CompactIdle));
        float room = rightX - x;
        if (room <= 0f)
        {
            return;
        }
        using (Fonts.PushCaption())
        {
            Vector2 labelSize = TextDraw.Measure(label);
            TextDraw.At(TextDraw.Truncate(label, room), new Vector2(x, midY - labelSize.Y * 0.5f), typer.Running ? Styling.TextSecondary : Styling.TextDim);
        }
    }
}
