using System.Numerics;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using AutoKeyboundBrawler.Windows.Components;
using AutoKeyboundBrawler.Windows.Sections;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Shell;

internal static class MiniPlayer
{
    private const float PadX = 18f;

    private static readonly string stopGlyph = FontAwesomeIcon.Stop.ToIconString();

    public static bool Draw(Plugin plugin, Vector2 size, float windowRounding)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
        Vector2 bottomRight = cursorScreenPos + size;
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        AutoTyper typer = plugin.Typer;
        var ready = ReadyState.Resolve(typer);
        Dock.Background(windowDrawList, cursorScreenPos, bottomRight, windowRounding);
        float padX = PadX * globalScale;
        float buttonHeight = GlassworkSizes.Height(ButtonSize.Regular);
        ImGui.SetCursorScreenPos(cursorScreenPos);
        Hit.Result hit = Hit.Area("##akb_mini_open", new Vector2(size.X - padX - buttonHeight - 8f * globalScale, size.Y));
        float hoverAmount = Motion.Hover(Motion.Key("##akb_mini_open"), hit.Hovered);
        if (hoverAmount > 0.01f)
        {
            Paint.Fill(windowDrawList, cursorScreenPos, bottomRight, Styling.WithAlpha(Styling.Surface2, 0.35f * hoverAmount), windowRounding, ImDrawFlags.RoundCornersBottom);
        }
        float centerY = cursorScreenPos.Y + size.Y * 0.5f;
        Paint.Dot(windowDrawList, new Vector2(cursorScreenPos.X + padX + 4f * globalScale, centerY), 4f * globalScale, Styling.PulseColor(ready.Accent, ready.AccentSoft));
        float textLeft = cursorScreenPos.X + padX + 22f * globalScale;
        float textRight = bottomRight.X - padX - buttonHeight - 16f * globalScale;
        string label = ReadyState.PhaseLabel(typer.Phase);
        Vector2 labelSize = TextDraw.SmallCapsSize(label);
        float textLineHeight = ImGui.GetTextLineHeight();
        float lineGap = 2f * globalScale;
        float textTop = centerY - (labelSize.Y + lineGap + textLineHeight) * 0.5f;
        TextDraw.SmallCaps(label, new Vector2(textLeft, textTop), ready.AccentSoft);
        TextDraw.At(TextDraw.Truncate(Loc.T(L.Shell.MiniStatus, ready.Title, Formatting.Phrases(typer.Session.PhrasesSubmitted)), textRight - textLeft), new Vector2(textLeft, textTop + labelSize.Y + lineGap), Styling.TextStrong);
        ImGui.SetCursorScreenPos(new Vector2(bottomRight.X - padX - buttonHeight, centerY - buttonHeight * 0.5f));
        if (Gw.IconButton(stopGlyph, ButtonStyle.Destructive, ButtonSize.Regular, Loc.T(L.Shell.StopTyping)))
        {
            typer.Stop();
        }
        ImGui.SetCursorScreenPos(cursorScreenPos);
        ImGui.Dummy(size);
        return hit.Clicked;
    }
}
