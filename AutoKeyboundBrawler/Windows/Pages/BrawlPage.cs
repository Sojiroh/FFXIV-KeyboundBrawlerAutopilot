using System;
using System.Numerics;
using AutoKeyboundBrawler.Windows.Sections;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace AutoKeyboundBrawler.Windows.Pages;

internal sealed class BrawlPage
{
    private const float SwitchRevealMs = 320f;

    private const float SectionGap = 16f;

    private const float BoardShare = 0.6f;

    private const float TwoColumnMinWidth = 760f;

    public void Draw(Plugin plugin)
    {
        var typer = plugin.Typer;
        using (Motion.PushSwitch("##akb_brawl_state", typer.Running, SwitchRevealMs))
        {
            StatusHero.Draw(typer);
            Styling.VSpace(SectionGap);
            var availWidth = ImGui.GetContentRegionAvail().X;
            if (availWidth < TwoColumnMinWidth * ImGuiHelpers.GlobalScale)
            {
                LiveBoard.Draw(typer);
                Styling.VSpace(SectionGap);
                QuickSettings.Draw(plugin);
            }
            else
            {
                DrawColumns(plugin, availWidth);
            }
        }
    }

    private static void DrawColumns(Plugin plugin, float width)
    {
        var gap = SectionGap * ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetCursorScreenPos();
        var boardWidth = MathF.Floor((width - gap) * BoardShare);
        var window = ImGuiP.GetCurrentWindow();
        var savedWorkMaxX = window.WorkRect.Max.X;
        var savedContentMaxX = window.ContentRegionRect.Max.X;
        window.WorkRect.Max.X = origin.X + boardWidth;
        window.ContentRegionRect.Max.X = origin.X + boardWidth;
        ImGui.BeginGroup();
        LiveBoard.Draw(plugin.Typer);
        ImGui.EndGroup();
        var boardBottom = ImGui.GetItemRectMax().Y;
        window.WorkRect.Max.X = savedWorkMaxX;
        window.ContentRegionRect.Max.X = savedContentMaxX;
        ImGui.SetCursorScreenPos(new Vector2(origin.X + boardWidth + gap, origin.Y));
        ImGui.BeginGroup();
        QuickSettings.Draw(plugin);
        ImGui.EndGroup();
        var settingsBottom = ImGui.GetItemRectMax().Y;
        ImGui.SetCursorScreenPos(new Vector2(origin.X, MathF.Max(boardBottom, settingsBottom)));
        ImGui.Dummy(new Vector2(width, 0f));
    }
}
