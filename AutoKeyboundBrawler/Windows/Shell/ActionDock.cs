using System;
using System.Numerics;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using AutoKeyboundBrawler.Windows.Sections.Config;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Shell;

internal static class ActionDock
{
    private const float PadX = 18f;

    private const float DetailInset = 22f;

    private const float DetailAlpha = 0.8f;

    private static readonly string startGlyph = FontAwesomeIcon.Play.ToIconString();

    private static readonly string stopGlyph = FontAwesomeIcon.Stop.ToIconString();

    public static void Draw(Plugin plugin, Vector2 size, float windowRounding)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        Dock.Background(windowDrawList, cursorScreenPos, cursorScreenPos + size, windowRounding);
        float padX = PadX * globalScale;
        float buttonHeight = GlassworkSizes.Height(ButtonSize.Large);
        float width = (size.X - padX * 2f) / globalScale;
        ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + padX, cursorScreenPos.Y + (size.Y - buttonHeight) * 0.5f));
        AutoTyper typer = plugin.Typer;
        bool running = typer.Running;
        string label = Loc.T(running ? L.Shell.Stop : L.Shell.Start);
        ButtonStyle style = (running ? ButtonStyle.Destructive : ButtonStyle.Prominent);
        if (Gw.Button(label, style, ButtonSize.Large, running ? stopGlyph : startGlyph, width))
        {
            if (running)
            {
                typer.Stop();
            }
            else
            {
                typer.Start();
            }
        }
        DrawDetail(windowDrawList, Detail(plugin), ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        ImGui.SetCursorScreenPos(cursorScreenPos);
        ImGui.Dummy(size);
    }

    private static string Detail(Plugin plugin)
    {
        AutoTyper typer = plugin.Typer;
        if (!typer.Running)
        {
            return Loc.T(L.Shell.StopSub, TypingSettings.SpeedName(plugin.Configuration.Speed), TypingSettings.OrderName(plugin.Configuration.Order));
        }
        TypingSession session = typer.Session;
        return Loc.T(L.Shell.StopSub, Formatting.Phrases(session.PhrasesSubmitted), Formatting.Clock(session.ElapsedMs(Environment.TickCount64)));
    }

    private static void DrawDetail(ImDrawListPtr dl, string detail, Vector2 min, Vector2 max)
    {
        Vector4 color = Styling.WithAlpha(Frame.Look.OnAccent.ToVector4(), DetailAlpha);
        Vector2 textSize = TextDraw.Measure(detail);
        float right = max.X - DetailInset * ImGuiHelpers.GlobalScale;
        if (right - textSize.X <= min.X + (max.X - min.X) * 0.5f)
        {
            return;
        }
        TextDraw.At(detail, new Vector2(right - textSize.X, (min.Y + max.Y - textSize.Y) * 0.5f), color);
    }
}
