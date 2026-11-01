using System;
using System.Globalization;
using System.Numerics;
using AutoKeyboundBrawler.Core.Game;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using AutoKeyboundBrawler.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Sections;

internal static class LiveBoard
{
    private readonly record struct InputBadge(string Label, Vector4 Accent);

    private const float CardPadding = 16f;

    private const float RowGap = 12f;

    private const float ChipGap = 8f;

    private const float ChipPadX = 12f;

    private const float ChipPadY = 6f;

    private const float MarkerGap = 6f;

    private const float StatusGap = 14f;

    private const float StatusDotRadius = 3.5f;

    private const float LegendGap = 16f;

    private const float EmptyHeightLines = 3f;

    private const int CachedHitCounts = 21;

    private const float CastBarWidth = 36f;

    private const float CastBarHeight = 4f;

    private static readonly string[] hitCounts = BuildHitCounts();

    private static readonly string cameraGlyph = FontAwesomeIcon.Camera.ToIconString();

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
        BattleSnapshot snapshot = typer.Snapshot;
        Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
        float availWidth = ImGui.GetContentRegionAvail().X;
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        float chipHeight = ImGui.GetTextLineHeight() + ChipPadY * 2f * globalScale;
        float cursorY = cursorScreenPos.Y + DrawHeader(windowDrawList, snapshot, cursorScreenPos, availWidth) + RowGap * globalScale;
        if (snapshot.CastCount > 0)
        {
            DrawCasts(windowDrawList, snapshot, new Vector2(cursorScreenPos.X, cursorY), availWidth, chipHeight);
            cursorY += chipHeight + RowGap * globalScale;
        }
        if (snapshot.PhraseCount == 0)
        {
            cursorY += DrawEmpty(new Vector2(cursorScreenPos.X, cursorY), availWidth);
        }
        else
        {
            cursorY += DrawPhrases(windowDrawList, snapshot, typer.Running ? typer.Target : string.Empty, new Vector2(cursorScreenPos.X, cursorY), availWidth, chipHeight);
        }
        cursorY += RowGap * globalScale;
        cursorY += DrawLegend(new Vector2(cursorScreenPos.X, cursorY));
        ImGui.SetCursorScreenPos(cursorScreenPos);
        ImGui.Dummy(new Vector2(availWidth, cursorY - cursorScreenPos.Y));
    }

    private static float DrawHeader(ImDrawListPtr dl, BattleSnapshot snapshot, Vector2 origin, float width)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        Look look = Frame.Look;
        float buttonSize = MathF.Max(look.ControlHeight, look.MinHit) * globalScale;
        string label = Loc.T(L.Brawl.Board);
        Vector2 titleSize = TextDraw.SectionTitleSize(label);
        float rowHeight = MathF.Max(titleSize.Y, buttonSize);
        float midY = origin.Y + rowHeight * 0.5f;
        TextDraw.SectionTitle(label, new Vector2(origin.X, midY - titleSize.Y * 0.5f), Styling.TextStrong);
        float right = origin.X + width;
        ImGui.SetCursorScreenPos(new Vector2(right - buttonSize, origin.Y + (rowHeight - buttonSize) * 0.5f));
        if (Gw.IconButton(cameraGlyph, ButtonStyle.Plain, ButtonSize.Regular, Loc.T(L.Brawl.SnapshotHint)))
        {
            UiSnapshot.Write("requested from the Brawl page");
            Gw.Toast(Loc.T(L.Brawl.SnapshotDone), ToastKind.Success);
        }
        InputBadge inputBadge = Badge(snapshot);
        Vector2 badgeSize = TextDraw.Measure(inputBadge.Label);
        float badgeX = right - buttonSize - StatusGap * globalScale - badgeSize.X;
        if (badgeX - StatusGap * globalScale > origin.X + titleSize.X)
        {
            TextDraw.At(inputBadge.Label, new Vector2(badgeX, midY - badgeSize.Y * 0.5f), Styling.TextDim);
            Paint.Dot(dl, new Vector2(badgeX - 8.75f * globalScale, midY), StatusDotRadius * globalScale, inputBadge.Accent);
        }
        return rowHeight;
    }

    private static InputBadge Badge(BattleSnapshot snapshot)
    {
        if (!snapshot.InputVisible)
        {
            return new InputBadge(Loc.T(L.Brawl.InputClosed), Styling.TextMuted);
        }
        if (snapshot.InputFocused)
        {
            return new InputBadge(Loc.T(L.Brawl.InputFocused), Styling.AccentMint);
        }
        if (!snapshot.OtherInputFocused)
        {
            return new InputBadge(Loc.T(L.Brawl.InputUnfocused), Styling.AccentAmber);
        }
        return new InputBadge(Loc.T(L.Brawl.InputOther), Styling.AccentAmber);
    }

    private static float DrawEmpty(Vector2 origin, float width)
    {
        float height = ImGui.GetTextLineHeight() * EmptyHeightLines;
        string label = TextDraw.Truncate(Loc.T(L.Brawl.BoardEmpty), width);
        Vector2 labelSize = TextDraw.Measure(label);
        TextDraw.At(label, new Vector2(origin.X + (width - labelSize.X) * 0.5f, origin.Y + (height - labelSize.Y) * 0.5f), Styling.TextMuted);
        return height;
    }

    private static float DrawLegend(Vector2 origin)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float x = origin.X;
        float textLineHeight;
        using (Fonts.PushCaption())
        {
            textLineHeight = ImGui.GetTextLineHeight();
            x = DrawLegendItem(FontAwesomeIcon.ShieldAlt, Styling.AccentBlue, Loc.T(L.Brawl.LegendBarrier), x, origin.Y, textLineHeight) + LegendGap * globalScale;
            x = DrawLegendItem(FontAwesomeIcon.Bolt, Styling.AccentAmber, Loc.T(L.Brawl.LegendCasting), x, origin.Y, textLineHeight) + LegendGap * globalScale;
            DrawLegendItem(FontAwesomeIcon.Lock, Styling.AccentRose, Loc.T(L.Brawl.LegendLetterlock), x, origin.Y, textLineHeight);
        }
        return textLineHeight;
    }

    private static float DrawLegendItem(FontAwesomeIcon icon, Vector4 accent, string label, float x, float y, float height)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float iconSize = height * 0.8f;
        ProgressRing.CenterIcon(new Vector2(x + iconSize * 0.5f, y + height * 0.5f), icon, accent, iconSize);
        x += iconSize + MarkerGap * globalScale;
        TextDraw.At(label, new Vector2(x, y), Styling.TextDim);
        return x + TextDraw.Measure(label).X;
    }

    private static void DrawCasts(ImDrawListPtr dl, BattleSnapshot snapshot, Vector2 origin, float width, float chipHeight)
    {
        float cursorX = origin.X;
        float right = origin.X + width;
        for (int i = 0; i < snapshot.CastCount; i++)
        {
            float chipRight = DrawCastChip(dl, snapshot, i, new Vector2(cursorX, origin.Y), chipHeight, right);
            if (chipRight < 0f)
            {
                return;
            }
            cursorX = chipRight + ChipGap * ImGuiHelpers.GlobalScale;
        }
    }

    private static float DrawCastChip(ImDrawListPtr dl, BattleSnapshot snapshot, int cast, Vector2 min, float chipHeight, float right)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        string name = ((snapshot.CastNames[cast].Length > 0) ? snapshot.CastNames[cast] : "?");
        int hits = snapshot.CastHits[cast];
        FontAwesomeIcon icon = (LetterlockSpells.Matches(snapshot.CastNames[cast]) ? FontAwesomeIcon.Lock : ((hits > 0) ? FontAwesomeIcon.Bolt : FontAwesomeIcon.HourglassHalf));
        Vector4 accent = ((icon == FontAwesomeIcon.Lock) ? Styling.AccentRose : Styling.AccentAmber);
        string hitText = ((hits > 0) ? HitCount(hits) : string.Empty);
        float padX = ChipPadX * globalScale;
        float gap = MarkerGap * globalScale;
        Vector2 iconSize = TextDraw.IconSize(icon);
        float nameWidth = TextDraw.Measure(name).X;
        float hitWidth = ((hitText.Length > 0) ? (TextDraw.Measure(hitText).X + gap) : 0f);
        float barWidth = CastBarWidth * globalScale;
        Vector2 max = min + new Vector2(padX * 2f + iconSize.X + gap + nameWidth + gap + hitWidth + barWidth, chipHeight);
        if (max.X > right)
        {
            return -1f;
        }
        float midY = min.Y + chipHeight * 0.5f;
        Paint.Pill(dl, min, max, Styling.WithAlpha(accent, 0.12f), Styling.WithAlpha(accent, 0.55f));
        float cursorX = min.X + padX;
        TextDraw.Icon(icon, new Vector2(cursorX, midY - iconSize.Y * 0.5f), accent);
        cursorX += iconSize.X + gap;
        TextDraw.At(name, new Vector2(cursorX, midY - ImGui.GetTextLineHeight() * 0.5f), Styling.TextStrong);
        cursorX += nameWidth + gap;
        if (hitText.Length > 0)
        {
            TextDraw.At(hitText, new Vector2(cursorX, midY - ImGui.GetTextLineHeight() * 0.5f), accent);
            cursorX += hitWidth;
        }
        float barHeight = CastBarHeight * globalScale;
        Paint.Bar(dl, new Vector2(cursorX, midY - barHeight * 0.5f), barWidth, barHeight, snapshot.CastProgress[cast], accent);
        return max.X;
    }

    private static float DrawPhrases(ImDrawListPtr dl, BattleSnapshot snapshot, string target, Vector2 origin, float width, float chipHeight)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float gap = ChipGap * globalScale;
        float cursorX = 0f;
        float rowY = origin.Y;
        bool targetMarked = false;
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            float chipWidth = ChipWidth(snapshot, i, width);
            if (cursorX > 0f && cursorX + chipWidth > width)
            {
                cursorX = 0f;
                rowY += chipHeight + gap;
            }
            string phrase = snapshot.Phrases[i];
            bool isTarget = !targetMarked && target.Length > 0 && phrase.Equals(target, StringComparison.OrdinalIgnoreCase);
            targetMarked |= isTarget;
            DrawPhraseChip(dl, snapshot, i, isTarget, new Vector2(origin.X + cursorX, rowY), chipWidth, chipHeight);
            cursorX += chipWidth + gap;
        }
        return rowY + chipHeight - origin.Y;
    }

    private static void DrawPhraseChip(ImDrawListPtr dl, BattleSnapshot snapshot, int index, bool isTarget, Vector2 min, float chipWidth, float chipHeight)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        Vector2 max = min + new Vector2(chipWidth, chipHeight);
        Vector4 accent = MarkerAccent(snapshot, index) ?? (isTarget ? Styling.AccentTeal : Styling.BorderDim);
        float fillAlpha = (isTarget ? 0.2f : 0.1f);
        float borderAlpha = (isTarget ? 0.7f : 0.45f);
        Paint.Pill(dl, min, max, Styling.WithAlpha(accent, fillAlpha), Styling.WithAlpha(accent, borderAlpha));
        float y = min.Y + (chipHeight - ImGui.GetTextLineHeight()) * 0.5f;
        float textX = DrawMarker(snapshot, index, min.X + ChipPadX * globalScale, min.Y + chipHeight * 0.5f, accent);
        string phrase = snapshot.Phrases[index];
        float maxWidth = max.X - ChipPadX * globalScale - textX;
        string typedText = ((isTarget && string.Equals(snapshot.PhraseTyped[index], phrase, StringComparison.Ordinal)) ? snapshot.InputText : string.Empty);
        int typedLength = ((typedText.Length > 0 && phrase.StartsWith(typedText, StringComparison.OrdinalIgnoreCase)) ? typedText.Length : 0);
        if (typedLength == 0)
        {
            TextDraw.At(TextDraw.Truncate(phrase, maxWidth), new Vector2(textX, y), isTarget ? Styling.TextStrong : Styling.TextSecondary);
            return;
        }
        string typedPart = phrase.Substring(0, typedLength);
        TextDraw.At(typedPart, new Vector2(textX, y), Styling.AccentTealSoft);
        TextDraw.At(phrase.Substring(typedLength), new Vector2(textX + TextDraw.Measure(typedPart).X, y), Styling.TextStrong);
    }

    private static float DrawMarker(BattleSnapshot snapshot, int index, float x, float midY, Vector4 accent)
    {
        if (!HasMarker(snapshot, index))
        {
            return x;
        }
        float globalScale = ImGuiHelpers.GlobalScale;
        FontAwesomeIcon icon = MarkerIcon(snapshot, index);
        Vector2 iconSize = TextDraw.IconSize(icon);
        TextDraw.Icon(icon, new Vector2(x, midY - iconSize.Y * 0.5f), accent);
        x += iconSize.X + MarkerGap * globalScale;
        if (!ShowsHits(snapshot, index))
        {
            return x;
        }
        string hitText = HitCount(snapshot.CastHits[snapshot.PhraseCast[index]]);
        Vector2 hitSize = TextDraw.Measure(hitText);
        TextDraw.At(hitText, new Vector2(x, midY - hitSize.Y * 0.5f), accent);
        return x + hitSize.X + MarkerGap * globalScale;
    }

    private static bool HasMarker(BattleSnapshot snapshot, int index)
    {
        if (!snapshot.PhraseOnBarrier[index])
        {
            return snapshot.IsCasting(index);
        }
        return true;
    }

    private static bool ShowsHits(BattleSnapshot snapshot, int index)
    {
        if (!snapshot.PhraseOnBarrier[index])
        {
            return snapshot.IsInterruptible(index);
        }
        return false;
    }

    private static FontAwesomeIcon MarkerIcon(BattleSnapshot snapshot, int index)
    {
        if (!snapshot.PhraseOnBarrier[index])
        {
            return FontAwesomeIcon.Bolt;
        }
        return FontAwesomeIcon.ShieldAlt;
    }

    private static Vector4? MarkerAccent(BattleSnapshot snapshot, int index)
    {
        if (snapshot.PhraseOnBarrier[index])
        {
            return Styling.AccentBlue;
        }
        if (!snapshot.IsCasting(index))
        {
            return null;
        }
        return Styling.AccentAmber;
    }

    private static float ChipWidth(BattleSnapshot snapshot, int index, float maxWidth)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float width = TextDraw.Measure(snapshot.Phrases[index]).X + ChipPadX * 2f * globalScale;
        if (HasMarker(snapshot, index))
        {
            width += TextDraw.IconSize(MarkerIcon(snapshot, index)).X + MarkerGap * globalScale;
            if (ShowsHits(snapshot, index))
            {
                width += TextDraw.Measure(HitCount(snapshot.CastHits[snapshot.PhraseCast[index]])).X + MarkerGap * globalScale;
            }
        }
        return MathF.Min(maxWidth, width);
    }

    private static string HitCount(int hits)
    {
        if (hits >= CachedHitCounts)
        {
            return Formatting.Number(hits);
        }
        return hitCounts[hits];
    }

    private static string[] BuildHitCounts()
    {
        string[] counts = new string[CachedHitCounts];
        for (int i = 0; i < CachedHitCounts; i++)
        {
            counts[i] = i.ToString(CultureInfo.InvariantCulture);
        }
        return counts;
    }
}
