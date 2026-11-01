using System;
using System.Numerics;
using AutoKeyboundBrawler.Core.Game;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using AutoKeyboundBrawler.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Sections;

internal static class StatusHero
{
    private readonly record struct RingState(float Fraction, bool Sweeping, Vector4 Accent, FontAwesomeIcon Icon);

    private const float CardPadding = 18f;

    private const float RingSize = 64f;

    private const float RingThickness = 5f;

    private const float RingGap = 18f;

    private const float LineGap = 4f;

    private const float MetricGap = 26f;

    private const float MinimumTextWidth = 220f;

    private const double SweepPeriodMs = 1400.0;

    private const float SweepArc = 0.28f;

    private const float SweepHeadAlpha = 0.9f;

    private const float IconHeightShare = 0.36f;

    private const int TypingMetricCount = 3;

    private const int MaxMetricCount = 5;

    private static readonly string[] metricValues = new string[MaxMetricCount];

    private static readonly string[] metricLabels = new string[MaxMetricCount];

    private static int metricCount = TypingMetricCount;

    public static void Draw(AutoTyper typer)
    {
        if (Gw.BeginCard(CardPadding))
        {
            DrawContent(typer);
        }
        Gw.EndCard();
    }

    private static void DrawContent(AutoTyper typer)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
        float availWidth = ImGui.GetContentRegionAvail().X;
        float ringSize = RingSize * globalScale;
        float textLeft = cursorScreenPos.X + ringSize + RingGap * globalScale;
        FillMetrics(typer);
        Vector2 metricsSize = MetricsSize();
        bool metricsBeside = cursorScreenPos.X + availWidth - metricsSize.X - MetricGap * globalScale - textLeft >= MinimumTextWidth * globalScale;
        float textRight = (metricsBeside ? (cursorScreenPos.X + availWidth - metricsSize.X - MetricGap * globalScale) : (cursorScreenPos.X + availWidth));
        var ready = ReadyState.Resolve(typer);
        RingState state = Ring(typer, ready);
        DrawRing(state, cursorScreenPos, ringSize);
        float textHeight = MeasureText(typer, ready);
        float rowHeight = MathF.Max(ringSize, textHeight);
        DrawText(typer, ready, new Vector2(textLeft, cursorScreenPos.Y + (rowHeight - textHeight) * 0.5f), textRight - textLeft, state.Accent);
        if (metricsBeside)
        {
            DrawMetrics(new Vector2(cursorScreenPos.X + availWidth - metricsSize.X, cursorScreenPos.Y + (rowHeight - metricsSize.Y) * 0.5f));
        }
        else
        {
            float metricsGap = 18f * globalScale;
            DrawMetrics(new Vector2(textLeft, cursorScreenPos.Y + rowHeight + metricsGap));
            rowHeight += metricsGap + metricsSize.Y;
        }
        ImGui.SetCursorScreenPos(cursorScreenPos);
        ImGui.Dummy(new Vector2(availWidth, rowHeight));
    }

    private static RingState Ring(AutoTyper typer, ReadyState.Info info)
    {
        if (!typer.Running)
        {
            return new RingState(0f, Sweeping: false, Styling.AccentMint, FontAwesomeIcon.Keyboard);
        }
        return typer.Phase switch
        {
            TyperPhase.Typing or TyperPhase.Clearing => new RingState(TypedFraction(typer), Sweeping: false, info.Accent, FontAwesomeIcon.Keyboard),
            TyperPhase.HoldingForCast => new RingState(typer.HoldRemaining, Sweeping: false, LetterlockSpells.Matches(typer.HoldingSpell) ? Styling.AccentRose : info.Accent, LetterlockSpells.Matches(typer.HoldingSpell) ? FontAwesomeIcon.Lock : FontAwesomeIcon.HourglassHalf),
            TyperPhase.Stalled => new RingState(0f, Sweeping: false, info.Accent, FontAwesomeIcon.ExclamationTriangle),
            TyperPhase.WaitingForFocus => new RingState(0f, Sweeping: true, info.Accent, FontAwesomeIcon.MousePointer),
            _ => new RingState(0f, Sweeping: true, info.Accent, FontAwesomeIcon.HourglassHalf),
        };
    }

    private static void DrawRing(RingState state, Vector2 origin, float ring)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        ImGui.SetCursorScreenPos(origin);
        Gw.ProgressRing(state.Fraction, RingSize, RingThickness, Styling.ToGlasswork(state.Accent));
        Vector2 center = origin + new Vector2(ring * 0.5f);
        if (state.Sweeping)
        {
            ProgressRing.Sweep(center, (ring - RingThickness * globalScale) * 0.5f, RingThickness * globalScale, state.Accent, SweepPeriodMs, SweepArc, SweepHeadAlpha);
        }
        ProgressRing.CenterIcon(center, state.Icon, state.Accent, ring * IconHeightShare);
    }

    private static float MeasureText(AutoTyper typer, ReadyState.Info info)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float textLineHeight;
        using (Fonts.PushTitle())
        {
            textLineHeight = ImGui.GetTextLineHeight();
        }
        float height = TextDraw.SmallCapsSize(ReadyState.ShortLabel(info.Kind)).Y + LineGap * globalScale + textLineHeight;
        if (!HasDetail(typer, info))
        {
            return height;
        }
        return height + LineGap * globalScale + ImGui.GetTextLineHeight();
    }

    private static void DrawText(AutoTyper typer, ReadyState.Info info, Vector2 origin, float width, Vector4 accent)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        string label = ReadyState.ShortLabel(info.Kind);
        TextDraw.SmallCaps(label, origin, accent);
        float lineY = origin.Y + TextDraw.SmallCapsSize(label).Y + LineGap * globalScale;
        float textLineHeight;
        using (Fonts.PushTitle())
        {
            textLineHeight = ImGui.GetTextLineHeight();
            if (IsTyping(typer))
            {
                DrawTypedPhrase(typer, new Vector2(origin.X, lineY), width);
            }
            else
            {
                TextDraw.At(TextDraw.Truncate(info.Title, width), new Vector2(origin.X, lineY), Styling.TextStrong);
            }
        }
        if (!HasDetail(typer, info))
        {
            return;
        }
        lineY += textLineHeight + LineGap * globalScale;
        TextDraw.At(TextDraw.Truncate(info.Detail, width), new Vector2(origin.X, lineY), Styling.TextDim);
    }

    private static void DrawTypedPhrase(AutoTyper typer, Vector2 position, float width)
    {
        string target = typer.Target;
        int typedCount = TypedCount(typer, out string typedForm);
        if (typedCount == 0 || !string.Equals(typedForm, target, StringComparison.Ordinal) || TextDraw.Measure(target).X > width)
        {
            TextDraw.At(TextDraw.Truncate(target, width), position, Styling.TextStrong);
            return;
        }
        string typedPart = target.Substring(0, typedCount);
        TextDraw.At(typedPart, position, Styling.AccentTealSoft);
        TextDraw.At(target.Substring(typedCount), position + new Vector2(TextDraw.Measure(typedPart).X, 0f), Styling.TextStrong);
    }

    private static float TypedFraction(AutoTyper typer)
    {
        if (typer.SubmitsDirectly)
        {
            return typer.SubmitProgress;
        }
        int typedCount = TypedCount(typer, out string typedForm);
        if (typedForm.Length != 0)
        {
            return (float)typedCount / (float)typedForm.Length;
        }
        return 0f;
    }

    private static int TypedCount(AutoTyper typer, out string typedForm)
    {
        BattleSnapshot snapshot = typer.Snapshot;
        typedForm = typer.Target;
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (string.Equals(snapshot.Phrases[i], typedForm, StringComparison.OrdinalIgnoreCase))
            {
                typedForm = snapshot.PhraseTyped[i];
                break;
            }
        }
        if (typer.SubmitsDirectly)
        {
            return (int)(typer.SubmitProgress * (float)typedForm.Length);
        }
        string inputText = snapshot.InputText;
        if (inputText.Length <= 0 || inputText.Length > typedForm.Length || !typedForm.StartsWith(inputText, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }
        return inputText.Length;
    }

    private static bool IsTyping(AutoTyper typer)
    {
        return typer.Running && typer.Target.Length > 0 && typer.Phase is TyperPhase.Typing or TyperPhase.Clearing;
    }

    private static bool HasDetail(AutoTyper typer, ReadyState.Info info)
    {
        if (!IsTyping(typer))
        {
            return info.Detail.Length > 0;
        }
        return false;
    }

    private static void FillMetrics(AutoTyper typer)
    {
        TypingSession session = typer.Session;
        long tickCount = Environment.TickCount64;
        metricValues[0] = Formatting.Number(session.PhrasesSubmitted);
        metricValues[1] = Loc.T(L.Brawl.TileSpeedValue, Formatting.Number(session.WordsPerMinute(tickCount)));
        metricValues[2] = Formatting.Clock(session.ElapsedMs(tickCount));
        metricLabels[0] = Loc.T(L.Brawl.TilePhrases);
        metricLabels[1] = Loc.T(L.Brawl.TileSpeed);
        metricLabels[2] = Loc.T(L.Brawl.TileSession);
        metricCount = TypingMetricCount;
        if (!typer.Queue.Enabled && session.MatchesCompleted == 0)
        {
            return;
        }
        metricValues[3] = Formatting.Amount(session.MgcEarned);
        metricValues[4] = Formatting.Amount(session.MgpEarned);
        metricLabels[3] = Loc.T(L.Brawl.TileMgc);
        metricLabels[4] = Loc.T(L.Brawl.TileMgp);
        metricCount = MaxMetricCount;
    }

    private static Vector2 MetricsSize()
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float width = 0f;
        float height = 0f;
        for (int i = 0; i < metricCount; i++)
        {
            width += MetricWidth(i) + ((i > 0) ? (MetricGap * globalScale) : 0f);
        }
        using (Fonts.PushHeadline())
        {
            height = ImGui.GetTextLineHeight();
        }
        return new Vector2(width, height + LineGap * globalScale + TextDraw.SmallCapsSize(metricLabels[0]).Y);
    }

    private static float MetricWidth(int index)
    {
        float valueWidth;
        using (Fonts.PushHeadline())
        {
            valueWidth = TextDraw.Measure(metricValues[index]).X;
        }
        return MathF.Max(valueWidth, TextDraw.SmallCapsSize(metricLabels[index]).X);
    }

    private static void DrawMetrics(Vector2 origin)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float cursorX = origin.X;
        for (int i = 0; i < metricCount; i++)
        {
            float textLineHeight;
            using (Fonts.PushHeadline())
            {
                textLineHeight = ImGui.GetTextLineHeight();
                TextDraw.At(metricValues[i], new Vector2(cursorX, origin.Y), Styling.TextStrong);
            }
            TextDraw.SmallCaps(metricLabels[i], new Vector2(cursorX, origin.Y + textLineHeight + LineGap * globalScale), Styling.TextDim);
            cursorX += MetricWidth(i) + MetricGap * globalScale;
        }
    }
}
